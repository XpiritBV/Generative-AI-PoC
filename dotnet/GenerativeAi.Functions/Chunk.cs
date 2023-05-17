namespace GenerativeAi.Functions;

public record Chunk(ChunkId Id,
                    Document Document,
                    string Content);