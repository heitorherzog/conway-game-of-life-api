namespace ConwayGameLifeApi.DTOs.Responses;

public sealed class BoardStateResponse
{
    public required string BoardId { get; init; }

    public required int Generation { get; init; }

    public required IReadOnlyList<IReadOnlyList<int>> Cells { get; init; }
}
