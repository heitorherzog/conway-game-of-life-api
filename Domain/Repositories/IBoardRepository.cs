namespace ConwayGameLifeApi.Domain.Repositories;

public interface IBoardRepository
{
    Task<string> CreateBoardAsync(BoardState initialState, CancellationToken cancellationToken);

    Task<BoardState?> GetLatestStateAsync(string boardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BoardState>> GetStatesAsync(string boardId, CancellationToken cancellationToken);

    Task SaveStateAsync(BoardState state, CancellationToken cancellationToken);
}
