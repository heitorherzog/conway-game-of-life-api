namespace ConwayGameLifeApi.Domain.Exceptions;

public sealed class FinalStateNotReachedException : Exception
{
    public FinalStateNotReachedException(string boardId, int iterations)
        : base($"Final state was not reached for board '{boardId}' after {iterations} iterations.")
    {
        BoardId = boardId;
        Iterations = iterations;
    }

    public string BoardId { get; }

    public int Iterations { get; }
}
