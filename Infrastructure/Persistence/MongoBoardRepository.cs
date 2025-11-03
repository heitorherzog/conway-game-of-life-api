using ConwayGameLifeApi.Infrastructure.Persistence.Documents;
using MongoDB.Bson;

namespace ConwayGameLifeApi.Infrastructure.Persistence;

public sealed class MongoBoardRepository : IBoardRepository
{
    private readonly IMongoCollection<BoardDocument> _boards;
    private readonly IMongoCollection<BoardStateDocument> _states;
    private readonly ILogger<MongoBoardRepository> _logger;

    public MongoBoardRepository(IMongoClient client, IOptions<MongoDbSettings> settings, ILogger<MongoBoardRepository> logger)
    {
        _logger = logger;
        var mongoSettings = settings.Value;
        var database = client.GetDatabase(mongoSettings.DatabaseName);
        _boards = database.GetCollection<BoardDocument>(mongoSettings.BoardsCollectionName);
        _states = database.GetCollection<BoardStateDocument>(mongoSettings.StatesCollectionName);
    }

    public async Task<string> CreateBoardAsync(BoardState initialState, CancellationToken cancellationToken)
    {
        var boardId = ObjectId.GenerateNewId().ToString();
        var boardDocument = new BoardDocument
        {
            Id = boardId,
            Rows = initialState.Rows,
            Columns = initialState.Columns,
            CreatedAt = DateTime.UtcNow
        };

        var initialStateDocument = new BoardStateDocument
        {
            Id = ObjectId.GenerateNewId().ToString(),
            BoardId = boardId,
            Generation = initialState.Generation,
            Cells = ToDocumentCells(initialState.Cells),
            CreatedAt = DateTime.UtcNow
        };

        await _boards.InsertOneAsync(boardDocument, cancellationToken: cancellationToken);
        await _states.InsertOneAsync(initialStateDocument, cancellationToken: cancellationToken);
        _logger.LogInformation("Board {BoardId} created with initial generation {Generation}.", boardId, initialState.Generation);

        return boardId;
    }

    public async Task<BoardState?> GetLatestStateAsync(string boardId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(boardId, out _))
        {
            return null;
        }

        var filter = Builders<BoardStateDocument>.Filter.Eq(s => s.BoardId, boardId);
        var stateDocument = await _states
            .Find(filter)
            .SortByDescending(s => s.Generation)
            .FirstOrDefaultAsync(cancellationToken);

        return stateDocument is null
            ? null
            : BoardState.FromPersistence(boardId, stateDocument.Generation, stateDocument.Cells);
    }

    public async Task<IReadOnlyList<BoardState>> GetStatesAsync(string boardId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(boardId, out _))
        {
            return Array.Empty<BoardState>();
        }

        var filter = Builders<BoardStateDocument>.Filter.Eq(s => s.BoardId, boardId);
        var stateDocuments = await _states
            .Find(filter)
            .SortBy(s => s.Generation)
            .ToListAsync(cancellationToken);

        return stateDocuments
            .Select(doc => BoardState.FromPersistence(boardId, doc.Generation, doc.Cells))
            .ToList();
    }

    public async Task SaveStateAsync(BoardState state, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(state.BoardId, out _))
        {
            throw new BoardNotFoundException(state.BoardId);
        }

        var document = new BoardStateDocument
        {
            Id = ObjectId.GenerateNewId().ToString(),
            BoardId = state.BoardId,
            Generation = state.Generation,
            Cells = ToDocumentCells(state.Cells),
            CreatedAt = DateTime.UtcNow
        };

        await _states.InsertOneAsync(document, cancellationToken: cancellationToken);
        _logger.LogInformation("Persisted generation {Generation} for board {BoardId}.", state.Generation, state.BoardId);
    }

    private static List<List<int>> ToDocumentCells(IReadOnlyList<IReadOnlyList<int>> cells)
    {
        var result = new List<List<int>>(cells.Count);
        foreach (var row in cells)
        {
            result.Add(row.ToList());
        }

        return result;
    }
}
