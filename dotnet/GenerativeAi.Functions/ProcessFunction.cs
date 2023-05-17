using System;
using System.Linq;
using System.Threading.Tasks;

using Azure;
using Azure.AI.OpenAI;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

using StackExchange.Redis;

namespace GenerativeAi.Functions;

public class ProcessFunction
{
    private const string IndexPrefix = "file";
    private readonly IDatabase _database;
    private readonly OpenAIClient _openAiClient;

    public ProcessFunction(OpenAIClient openAiClient,
                           IDatabase database)
    {
        _openAiClient = openAiClient;
        _database = database;
    }

    [FunctionName(nameof(ProcessChunk))]
    public async Task ProcessChunk([ActivityTrigger] ProcessChunkRequest request, ExecutionContext context)
    {
        var result = await _openAiClient.GetEmbeddingsAsync(request.ModelId, new EmbeddingsOptions(request.Chunk.Content));
        await Store(request.Chunk, result);
    }

    private Task Store(Chunk chunk, NullableResponse<Embeddings> result)
    {
        var embeddings = result.Value.Data[0].Embedding;
        var vectorBytes = new byte[embeddings.Count * sizeof(float)];
        Buffer.BlockCopy(embeddings.ToArray(), 0, vectorBytes, 0, vectorBytes.Length);

        return _database.HashSetAsync(GenerateKey(chunk),
                                      new HashEntry[]
                                      {
                                          new("name", chunk.Document.Name.ToString()),
                                          new("version", chunk.Document.Version.ToString()),
                                          new("chunkId", chunk.Id.ToString()),
                                          new("content", chunk.Content),
                                          new("vectors", vectorBytes)
                                      });

        static string GenerateKey(Chunk chunk)
            => $"{IndexPrefix}:{chunk.Document.Name}_{chunk.Document.Version}_{chunk.Id}";
    }

    public record ProcessChunkRequest(Chunk Chunk, string ModelId = "text-embedding-ada-002");
}