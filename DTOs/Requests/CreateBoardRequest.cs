namespace ConwayGameLifeApi.DTOs.Requests;

public sealed class CreateBoardRequest
{
    public IEnumerable<IEnumerable<int>> Cells { get; init; } = Array.Empty<IEnumerable<int>>();
}
