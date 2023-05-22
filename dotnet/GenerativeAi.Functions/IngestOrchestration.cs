using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace GenerativeAi.Functions;

public record DocumentRequest(FileName Name, string Type);

public class IngestOrchestration
{
    [FunctionName(nameof(Document))]
    public static async Task<HttpResponseMessage> Document([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage request,
                                                           [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
        var data = await request.Content.ReadAsAsync<DocumentRequest>();

        var instanceId = await orchestrationClient.StartNewAsync(nameof(AnalyzeDocumentOrchestrator), data);

        return orchestrationClient.CreateCheckStatusResponse(request, instanceId);
    }

    [FunctionName(nameof(AnalyzeDocumentOrchestrator))]
    public static async Task AnalyzeDocumentOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var request = context.GetInput<DocumentRequest>();

        var chunks = await context.CallActivityAsync<IEnumerable<Chunk>>(nameof(AnalyzeFunction.AnalyzeDocument), new AnalyzeFunction.AnalyzeDocumentRequest(request.Name, request.Type));
        var tasks = chunks.Select(chunk => context.CallActivityAsync(nameof(ProcessFunction.ProcessChunk),
                                                                     new ProcessFunction.ProcessChunkRequest(chunk)));
        await Task.WhenAll(tasks);
    }
}