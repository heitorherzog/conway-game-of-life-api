namespace ConwayGameLifeApi.Services;

public interface IGameOfLifeService
{
    Task<string> CreateBoardAsync(IEnumerable<IEnumerable<int>> cells, CancellationToken cancellationToken);

    Task<BoardState> GetNextStateAsync(string boardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BoardState>> AdvanceBoardAsync(string boardId, int steps, CancellationToken cancellationToken);

    Task<BoardState> GetFinalStateAsync(string boardId, int maxIterations, CancellationToken cancellationToken);
}
