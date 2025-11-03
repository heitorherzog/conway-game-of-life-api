using ConwayGameLifeApi.Infrastructure.Persistence.Documents;

namespace ConwayGameLifeApi.Infrastructure.Initialization;

public sealed class MongoDbInitializer
{
    private readonly IMongoCollection<BoardStateDocument> _states;
    private readonly ILogger<MongoDbInitializer> _logger;

    public MongoDbInitializer(IMongoClient client, IOptions<MongoDbSettings> settings, ILogger<MongoDbInitializer> logger)
    {
        _logger = logger;
        var mongoSettings = settings.Value;
        var database = client.GetDatabase(mongoSettings.DatabaseName);
        _states = database.GetCollection<BoardStateDocument>(mongoSettings.StatesCollectionName);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var indexKeys = Builders<BoardStateDocument>.IndexKeys.Ascending(state => state.BoardId).Ascending(state => state.Generation);
        var indexModel = new CreateIndexModel<BoardStateDocument>(indexKeys, new CreateIndexOptions { Unique = true });
        await _states.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
        _logger.LogInformation("MongoDB indexes ensured for board states.");
    }
}
