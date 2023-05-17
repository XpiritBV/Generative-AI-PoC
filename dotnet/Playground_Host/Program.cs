using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;
using Azure.Identity;

using Microsoft.Extensions.Options;

using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;

using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DocumentAnalysisClient>(_ =>
                                                      {
                                                          var settings = builder.Configuration.GetSection("azure_cognitiveservices").Get<AzureCognativeServicesSettings>()!;
                                                          return new DocumentAnalysisClient(settings.EndPoint, new AzureKeyCredential(settings.Key));
                                                      });
builder.Services.AddSingleton<OpenAIClient>(_ =>
                                            {
                                                var settings = builder.Configuration.GetSection("azure_openai").Get<AzureOpenAiSettings>()!;
                                                return new OpenAIClient(settings.EndPoint, new AzureKeyCredential(settings.Key));
                                            });

builder.Configuration
       .AddJsonFile("appsettings.json", optional:true, reloadOnChange:true)
       .AddUserSecrets<Program>()
       .AddEnvironmentVariables()
       .AddCommandLine(args);

var app = builder.Build();

app.MapGet("/ingest",
           async (DocumentAnalysisClient documentAnalysisClient, OpenAIClient openAiClient) =>
           {
               var redis = ConnectionMultiplexer.Connect("localhost");
               var database = redis.GetDatabase();

               var fileUri = new Uri(@"https://www.ema.europa.eu/en/documents/product-information/apexxnar-epar-product-information_nl.pdf");

               var operation = await documentAnalysisClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-document", fileUri);

               await Parallel.ForEachAsync(operation.Value.Pages,
                                           async (page, token) =>
                                           {
                                               var content = string.Join(" ", page.Lines.Select(l => l.Content));
                                               var result = await openAiClient.GetEmbeddingsAsync("text-embedding-ada-002", new EmbeddingsOptions(content), token);

                                               if(!result.HasValue)
                                                   return;

                                               var embeddings = result.Value.Data[0].Embedding;
                                               var vectorBytes = new byte[embeddings.Count * sizeof(float)];
                                               Buffer.BlockCopy(embeddings.ToArray(), 0, vectorBytes, 0, vectorBytes.Length);

                                               var key = $"file:{fileUri.AbsoluteUri}_{page.PageNumber}";
                                               await database.KeyDeleteAsync(key);
                                               await database.HashSetAsync(key,
                                                                           new HashEntry[]
                                                                           {
                                                                               new("File", fileUri.AbsoluteUri),
                                                                               new("PageNumber", page.PageNumber),
                                                                               new("Content", content),
                                                                               new("Vectors", vectorBytes)
                                                                           });
                                           });
           });
app.MapGet("/search",
           async (IConfiguration configuration) =>
           {
               var redis = ConnectionMultiplexer.Connect("localhost");
               var database = redis.GetDatabase();

               var openAiEndpoint = configuration.GetValue<Uri>("azure_openai_endpoint") ?? throw new ArgumentNullException();
               var openAiKey = configuration.GetValue<string>("azure_openai_key") ?? throw new ArgumentNullException();
               var openAiClient = new OpenAIClient(openAiEndpoint, new AzureKeyCredential(openAiKey), new OpenAIClientOptions());

               const string question = "Wat zijn de bijwerkingen van Apexxnar?";
               var result = await openAiClient.GetEmbeddingsAsync("text-embedding-ada-002", new EmbeddingsOptions(question));

               var embeddings = result.Value.Data[0].Embedding;
               var vectorBytes = new byte[embeddings.Count * sizeof(float)];
               Buffer.BlockCopy(embeddings.ToArray(), 0, vectorBytes, 0, vectorBytes.Length);

               // Create Index
               ISearchCommands ft = database.FT();
               ft.DropIndex("myIndex");
               ft.Create("myIndex",
                         new FTCreateParams().On(IndexDataType.HASH)
                                             .Prefix("file:"),
                         new Schema().AddVectorField("Vectors",
                                                     Schema.VectorField.VectorAlgo.HNSW,
                                                     new Dictionary<string, object>
                                                     {
                                                         {"TYPE", "FLOAT32"},
                                                         {"DIM", 1536},
                                                         {"DISTANCE_METRIC", "COSINE"}
                                                     })
                                     .AddTextField("File"));
               var searchResult = ft.Search("myIndex",
                                            new Query("*=>[KNN 5 @Vectors $query_vector AS vector_score]")
                                                .AddParam("query_vector", vectorBytes)
                                                .ReturnFields("File", "PageNumber", "Content", "vector_score")
                                                .SetSortBy("vector_score", false)
                                                .Dialect(2));
               var results = searchResult.Documents
                                         .Select(d => new Result(d["File"].ToString(),
                                                                 Convert.ToInt32(d["PageNumber"].ToString()),
                                                                 d["Content"].ToString(),
                                                                 float.Parse(d["vector_score"].ToString())));
               var snippets = string.Join(" ", results.Select(r => $"File: {r.File} Page: {r.PageNumber} content: {r.Content}"));
               var promptTemplate = $@"""
                You are a specialist doctor.
                Your task is to assist other doctors find information about medical guidelines. The medical guidelines are defined by the following set of snippets identified by numbers in the form [1].  
                ------------  
                SNIPPETS  
                {snippets}  
                ------------  
                Your answer must be based solely on the SNIPPETS above. Every part of the answer must be supported only by the SNIPPETS above. If the answer consists of steps, provide a clear bullet point list. If you don't know the answer, just say that you don't know. Don't try to make up an answer. Be clear and concise and provide one final answer. NEVER provide questions in the answer.

                Provide the answer as a LIST of JSON formatted dictionaries with the following keys:
                - 'answer_sentence': str, // the answer in your own words
                - 'snippet_id': int,  // the snippet your answer is based on
                - 'relevant_substring': str, // a direct quote from the snippet that was most relevant in creating your answer. Use ellipses ... for substrings longer than 10 words.
                
                Please give you answer in Turkish.

                QUESTION: {question}?
                """;

               var chatResult = await openAiClient.GetChatCompletionsAsync("gpt-35-turbo",
                                                                           new ChatCompletionsOptions
                                                                           {
                                                                               Temperature = 0,
                                                                               MaxTokens = 1000,
                                                                               Messages =
                                                                               {
                                                                                   new ChatMessage(ChatRole.System, "You are a helpful assistant"),
                                                                                   new ChatMessage(ChatRole.User, promptTemplate)
                                                                               }
                                                                           });

           });

app.Run();

public record Result(string File, int PageNumber, string Content, float Score);
public record AzureOpenAiSettings(Uri EndPoint, string Key);
public record AzureCognativeServicesSettings(Uri EndPoint, string Key);