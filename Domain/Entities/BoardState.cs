namespace ConwayGameLifeApi.Domain.Entities;

public sealed class BoardState
{
    private readonly IReadOnlyList<IReadOnlyList<int>> _cells;

    private BoardState(string boardId, int generation, IReadOnlyList<IReadOnlyList<int>> cells)
    {
        BoardId = boardId ?? throw new ArgumentNullException(nameof(boardId));
        Generation = generation;
        _cells = cells;
    }

    public string BoardId { get; }

    public int Generation { get; }

    public IReadOnlyList<IReadOnlyList<int>> Cells => _cells;

    public int Rows => _cells.Count;

    public int Columns => _cells[0].Count;

    public static BoardState Create(string boardId, int generation, IEnumerable<IEnumerable<int>> cells)
    {
        var normalized = Normalize(cells);
        return new BoardState(boardId, generation, normalized);
    }

    public IReadOnlyList<IReadOnlyList<int>> CloneCells()
    {
        return CloneMatrix(_cells);
    }

    public static BoardState FromPersistence(string boardId, int generation, IEnumerable<IEnumerable<int>> cells)
    {
        return new BoardState(boardId, generation, CloneMatrix(cells));
    }

    private static IReadOnlyList<IReadOnlyList<int>> Normalize(IEnumerable<IEnumerable<int>> cells)
    {
        if (cells is null)
        {
            throw new InvalidBoardStateException("Board state cannot be null.");
        }

        var matrix = cells.Select(row => row?.ToList() ?? throw new InvalidBoardStateException("Row cannot be null.")).ToList();

        if (matrix.Count == 0)
        {
            throw new InvalidBoardStateException("Board must contain at least one row.");
        }

        var columnCount = matrix[0].Count;
        if (columnCount == 0)
        {
            throw new InvalidBoardStateException("Board must contain at least one column.");
        }

        foreach (var row in matrix)
        {
            if (row.Count != columnCount)
            {
                throw new InvalidBoardStateException("All rows must contain the same number of columns.");
            }

            if (row.Any(cell => cell is not 0 and not 1))
            {
                throw new InvalidBoardStateException("Cells must be either 0 or 1.");
            }
        }

        return AsReadOnly(matrix);
    }

    private static IReadOnlyList<IReadOnlyList<int>> CloneMatrix(IEnumerable<IEnumerable<int>> source)
    {
        if (source is null)
        {
            throw new InvalidBoardStateException("Board state cannot be null.");
        }

        var matrix = source.Select(row => row.ToList()).ToList();
        return AsReadOnly(matrix);
    }

    private static IReadOnlyList<IReadOnlyList<int>> AsReadOnly(List<List<int>> matrix)
    {
        var rows = new List<IReadOnlyList<int>>(matrix.Count);
        foreach (var row in matrix)
        {
            rows.Add(new ReadOnlyCollection<int>(row.ToArray()));
        }

        return new ReadOnlyCollection<IReadOnlyList<int>>(rows);
    }
}
