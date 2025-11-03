namespace ConwayGameLifeApi.Domain.Exceptions;

public sealed class InvalidBoardStateException : Exception
{
    public InvalidBoardStateException(string message)
        : base(message)
    {
    }
}
