using Domain;

using StackExchange.Redis;

namespace Redis;

public class RedisFactory
{
    private readonly IDatabase _database;
    private readonly RedisSettings _settings;

    public RedisFactory(RedisSettings settings)
    {
        _settings = settings;
        _database = Database();
    }

    private IDatabase Database()
    {
        var redis = ConnectionMultiplexer.Connect(_settings.ConnectionString);
        return redis.GetDatabase(_settings.Database);
    }

    public Documents Documents() => new RedisDocuments(_database);

    public Task CreateIndex() => new RedisDocuments(_database).CreateIndex();
}