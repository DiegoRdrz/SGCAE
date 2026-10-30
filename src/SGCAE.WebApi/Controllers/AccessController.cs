using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.AccessControl.Commands.RegisterAccessMovement;
using SGCAE.Application.AccessControl.DTOs;
using SGCAE.Application.AccessControl.Queries.GetAccessHistory;
using SGCAE.Application.Common.Models;
using SGCAE.Domain.Enums;

namespace SGCAE.WebApi.Controllers;

/// <summary>
/// Real-time access control and historical audit queries (RF-03 & RF-04).
/// </summary>
public class AccessController : ApiControllerBase
{
    /// <summary>
    /// Processes and validates an access attempt (Entry or Exit) at a physical point using a credential.
    /// Records both granted and denied access attempts with explicit reasons.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AccessResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessResultDto>> RegisterMovement([FromBody] RegisterAccessMovementCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Queries the access movement history with filtering and pagination.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(PagedResult<AccessLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AccessLogDto>>> GetHistory(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? accessPointId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] AccessResult? result,
        [FromQuery] MovementType? movementType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAccessHistoryQuery(
            employeeId,
            accessPointId,
            startDate,
            endDate,
            result,
            movementType,
            page,
            pageSize);

        var history = await Mediator.Send(query);
        return Ok(history);
    }
}

