namespace ConwayGameLifeApi.Domain.Exceptions;

public sealed class BoardNotFoundException : Exception
{
    public BoardNotFoundException(string boardId)
        : base($"Board with id '{boardId}' was not found.")
    {
        BoardId = boardId;
    }

    public string BoardId { get; }
}
