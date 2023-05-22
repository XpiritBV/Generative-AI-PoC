using System.Threading.Tasks;

namespace GenerativeAi.Functions.ingestion;

public interface Embed
{
    Task<Embedding> Embedding(string content);
}