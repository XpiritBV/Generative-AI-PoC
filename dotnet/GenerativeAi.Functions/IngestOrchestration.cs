using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace GenerativeAi.Functions;

public class IngestOrchestration
{
    [FunctionName("BlobTrigger")]
    public static async Task Run([BlobTrigger("ingestion-documents/{name}")] Stream document,
                                 string name,
                                 [DurableClient] IDurableOrchestrationClient starter,
                                 ILogger log)
    {
        await starter.StartNewAsync(nameof(AnalyzeDocumentOrchestrator), input:name);
    }

    [FunctionName(nameof(AnalyzeDocumentOrchestrator))]
    public static async Task AnalyzeDocumentOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var fileName = context.GetInput<string>();

        var chunks = await context.CallActivityAsync<IEnumerable<Chunk>>(nameof(AnalyzeFunction.AnalyzeDocument), new AnalyzeFunction.AnalyzeDocumentRequest(new FileName(fileName)));
        var tasks = chunks.Select(chunk => context.CallActivityAsync(nameof(ProcessFunction.ProcessChunk),
                                                                     new ProcessFunction.ProcessChunkRequest(chunk)));
        await Task.WhenAll(tasks);
    }
}