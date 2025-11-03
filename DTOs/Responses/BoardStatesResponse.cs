namespace ConwayGameLifeApi.DTOs.Responses;

public sealed class BoardStatesResponse
{
    public required string BoardId { get; init; }

    public required IReadOnlyList<BoardStateResponse> States { get; init; }
}
