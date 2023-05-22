using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Storage.Blobs;

using GenerativeAi.Functions.ingestion;
using GenerativeAi.Functions.settings;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

[assembly:FunctionsStartup(typeof(Startup))]

namespace GenerativeAi.Functions.ingestion;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().SetBasePath(builder.GetContext().ApplicationRootPath)
                                                                                .AddJsonFile("appsettings.json", optional:false, reloadOnChange:true)
                                                                                .AddJsonFile($"appsettings.{builder.GetContext().EnvironmentName}.json", optional:true, reloadOnChange:true)
                                                                                .AddEnvironmentVariables()
                                                                                .AddUserSecrets<Startup>()
                                                                                .Build());
        builder.Services.AddSingleton(provider =>
                                      {
                                          var settings = provider.GetService<IConfiguration>()
                                                                 .GetSection("azure_cognitiveservices")
                                                                 .Get<AzureCognativeServicesSettings>();
                                          return new DocumentAnalysisClient(settings.EndPoint, new AzureKeyCredential(settings.Key));
                                      });
        builder.Services.AddSingleton(provider =>
                                      {
                                          var settings = provider.GetService<IConfiguration>()
                                                                 .GetSection("azure_openai")
                                                                 .Get<AzureOpenAiSettings>();
                                          return new OpenAIClient(settings.EndPoint, new AzureKeyCredential(settings.Key));
                                      });
        builder.Services.AddSingleton(provider =>
                                      {
                                          var settings = provider.GetService<IConfiguration>()
                                                                 .GetSection("azure_cache")
                                                                 .Get<RedisSettings>();
                                          var redis = ConnectionMultiplexer.Connect(settings.ConnectionString);
                                          return redis.GetDatabase(settings.Database);
                                      });
        builder.Services.AddSingleton(provider =>
                                      {
                                          if(builder.GetContext().EnvironmentName == "Development")
                                              return new BlobServiceClient("UseDevelopmentStorage=true");

                                          var settings = provider.GetService<IConfiguration>()
                                                                 .GetSection("azure_storage")
                                                                 .Get<AzureStorageSettings>();
                                          return new BlobServiceClient(settings.EndPoint, new DefaultAzureCredential());
                                          //new StorageSharedKeyCredential(settings.Name, settings.Key),
                                          //new BlobClientOptions
                                          //{
                                          //    Retry =
                                          //    {
                                          //        Mode = RetryMode.Exponential,
                                          //        MaxRetries = 10,
                                          //        Delay = TimeSpan.FromSeconds(5)
                                          //    }
                                          //});
                                      });
    }
}