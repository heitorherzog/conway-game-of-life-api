namespace ConwayGameLifeApi.Controllers;

[ApiController]
[Route("api/boards")]
public sealed class BoardsController : ControllerBase
{
    private readonly IGameOfLifeService _gameOfLifeService;

    public BoardsController(IGameOfLifeService gameOfLifeService)
    {
        _gameOfLifeService = gameOfLifeService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateBoardResponse>> CreateBoard([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
    {
        if (request?.Cells is null)
        {
            return BadRequest("Cells payload is required.");
        }

        var boardId = await _gameOfLifeService.CreateBoardAsync(request.Cells, cancellationToken);
        var response = new CreateBoardResponse { BoardId = boardId };
        return CreatedAtAction(nameof(GetNextState), new { boardId }, response);
    }

    [HttpGet("{boardId}/next")]
    public async Task<ActionResult<BoardStateResponse>> GetNextState(string boardId, CancellationToken cancellationToken)
    {
        var state = await _gameOfLifeService.GetNextStateAsync(boardId, cancellationToken);
        return Ok(ToResponse(state));
    }

    [HttpGet("{boardId}/states")]
    public async Task<ActionResult<BoardStatesResponse>> GetStates(string boardId, [FromQuery] int steps, CancellationToken cancellationToken)
    {
        if (steps <= 0)
        {
            return BadRequest("Steps must be greater than zero.");
        }

        var states = await _gameOfLifeService.AdvanceBoardAsync(boardId, steps, cancellationToken);
        var responses = states.Select(ToResponse).ToList();
        return Ok(new BoardStatesResponse { BoardId = boardId, States = responses });
    }

    [HttpGet("{boardId}/final")]
    public async Task<ActionResult<BoardStateResponse>> GetFinalState(string boardId, [FromQuery] int? maxIterations, CancellationToken cancellationToken)
    {
        var iterations = maxIterations ?? 500;

        if (iterations <= 0)
        {
            return BadRequest("Max iterations must be greater than zero.");
        }

        var finalState = await _gameOfLifeService.GetFinalStateAsync(boardId, iterations, cancellationToken);
        return Ok(ToResponse(finalState));
    }

    private static BoardStateResponse ToResponse(BoardState state) => new()
    {
        BoardId = state.BoardId,
        Generation = state.Generation,
        Cells = state.CloneCells()
    };
}
