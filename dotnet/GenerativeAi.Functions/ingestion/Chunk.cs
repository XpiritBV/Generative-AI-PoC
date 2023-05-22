namespace GenerativeAi.Functions.ingestion;

public record Chunk(ChunkId Id,
                    Document Document,
                    string Content);