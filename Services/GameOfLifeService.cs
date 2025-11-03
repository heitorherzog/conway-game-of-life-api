namespace ConwayGameLifeApi.Services;

public sealed class GameOfLifeService : IGameOfLifeService
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<GameOfLifeService> _logger;

    public GameOfLifeService(IBoardRepository boardRepository, ILogger<GameOfLifeService> logger)
    {
        _boardRepository = boardRepository;
        _logger = logger;
    }

    public async Task<string> CreateBoardAsync(IEnumerable<IEnumerable<int>> cells, CancellationToken cancellationToken)
    {
        var initialState = BoardState.Create(string.Empty, 0, cells);
        var boardId = await _boardRepository.CreateBoardAsync(initialState, cancellationToken);
        _logger.LogInformation("Created board {BoardId} with initial generation {Generation}.", boardId, initialState.Generation);
        return boardId;
    }

    public async Task<BoardState> GetNextStateAsync(string boardId, CancellationToken cancellationToken)
    {
        var current = await _boardRepository.GetLatestStateAsync(boardId, cancellationToken)
            ?? throw new BoardNotFoundException(boardId);

        return await GenerateAndPersistNextAsync(current, cancellationToken);
    }

    public async Task<IReadOnlyList<BoardState>> AdvanceBoardAsync(string boardId, int steps, CancellationToken cancellationToken)
    {
        if (steps <= 0)
        {
            throw new InvalidBoardStateException("Steps must be greater than zero.");
        }

        var states = new List<BoardState>(steps);
        for (var i = 0; i < steps; i++)
        {
            var next = await GetNextStateAsync(boardId, cancellationToken);
            states.Add(next);
        }

        return new ReadOnlyCollection<BoardState>(states);
    }

    public async Task<BoardState> GetFinalStateAsync(string boardId, int maxIterations, CancellationToken cancellationToken)
    {
        if (maxIterations <= 0)
        {
            throw new InvalidBoardStateException("Max iterations must be greater than zero.");
        }

        var history = await _boardRepository.GetStatesAsync(boardId, cancellationToken);
        if (history.Count == 0)
        {
            throw new BoardNotFoundException(boardId);
        }

        var seenStates = new HashSet<string>(StringComparer.Ordinal);
        foreach (var state in history)
        {
            seenStates.Add(Hash(state.Cells));
        }

        var current = history[^1];
        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var next = await GenerateAndPersistNextAsync(current, cancellationToken);
            var hash = Hash(next.Cells);

            if (!seenStates.Add(hash) || AreEqual(current.Cells, next.Cells))
            {
                _logger.LogInformation("Final state reached for board {BoardId} at generation {Generation} after {Iteration} iterations.", boardId, next.Generation, iteration + 1);
                return next;
            }

            current = next;
        }

        throw new FinalStateNotReachedException(boardId, maxIterations);
    }

    private async Task<BoardState> GenerateAndPersistNextAsync(BoardState current, CancellationToken cancellationToken)
    {
        var nextCells = ComputeNext(current.Cells);
        var nextState = BoardState.Create(current.BoardId, current.Generation + 1, nextCells);
        await _boardRepository.SaveStateAsync(nextState, cancellationToken);
        return nextState;
    }

    private static IReadOnlyList<IReadOnlyList<int>> ComputeNext(IReadOnlyList<IReadOnlyList<int>> current)
    {
        var rows = current.Count;
        var columns = current[0].Count;
        var next = new List<IReadOnlyList<int>>(rows);

        for (var row = 0; row < rows; row++)
        {
            var nextRow = new int[columns];
            for (var column = 0; column < columns; column++)
            {
                var aliveNeighbours = CountAliveNeighbours(current, row, column);
                var isAlive = current[row][column] == 1;
                var nextValue = isAlive switch
                {
                    true when aliveNeighbours is 2 or 3 => 1,
                    false when aliveNeighbours == 3 => 1,
                    _ => 0
                };

                nextRow[column] = nextValue;
            }

            next.Add(new ReadOnlyCollection<int>(nextRow));
        }

        return new ReadOnlyCollection<IReadOnlyList<int>>(next);
    }

    private static int CountAliveNeighbours(IReadOnlyList<IReadOnlyList<int>> current, int row, int column)
    {
        var rows = current.Count;
        var columns = current[0].Count;
        var alive = 0;

        for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                if (rowOffset == 0 && columnOffset == 0)
                {
                    continue;
                }

                var neighbourRow = row + rowOffset;
                var neighbourColumn = column + columnOffset;

                if (neighbourRow >= 0 && neighbourRow < rows && neighbourColumn >= 0 && neighbourColumn < columns)
                {
                    alive += current[neighbourRow][neighbourColumn];
                }
            }
        }

        return alive;
    }

    private static bool AreEqual(IReadOnlyList<IReadOnlyList<int>> left, IReadOnlyList<IReadOnlyList<int>> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var row = 0; row < left.Count; row++)
        {
            var leftRow = left[row];
            var rightRow = right[row];

            if (leftRow.Count != rightRow.Count)
            {
                return false;
            }

            for (var column = 0; column < leftRow.Count; column++)
            {
                if (leftRow[column] != rightRow[column])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static string Hash(IReadOnlyList<IReadOnlyList<int>> cells)
    {
        var builder = new StringBuilder();
        foreach (var row in cells)
        {
            foreach (var cell in row)
            {
                builder.Append(cell);
            }

            builder.Append('|');
        }

        return builder.ToString();
    }
}
