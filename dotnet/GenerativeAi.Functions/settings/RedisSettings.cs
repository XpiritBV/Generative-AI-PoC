namespace GenerativeAi.Functions.settings;

public class RedisSettings
{
    public int Database { get; set; }
    public string ConnectionString { get; set; }

    public Redis.RedisSettings AsSettings()
        => new(ConnectionString, Database);
}