using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.AccessPoints.Commands.ChangeAccessPointStatus;
using SGCAE.Application.AccessPoints.Commands.CreateAccessPoint;
using SGCAE.Application.AccessPoints.Commands.UpdateAccessPoint;
using SGCAE.Application.AccessPoints.DTOs;
using SGCAE.Application.AccessPoints.Queries.GetAccessPointById;
using SGCAE.Application.AccessPoints.Queries.GetAccessPoints;

namespace SGCAE.WebApi.Controllers;

/// <summary>
/// Physical access points administration (RF-06).
/// </summary>
public class AccessPointsController : ApiControllerBase
{
    /// <summary>
    /// Registers a new physical access point in the facilities.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AccessPointDto>> Create([FromBody] CreateAccessPointCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lists access points with optional filter for enabled ones.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AccessPointDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccessPointDto>>> GetAll([FromQuery] bool? onlyEnabled)
    {
        var result = await Mediator.Send(new GetAccessPointsQuery(onlyEnabled));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves an access point by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessPointDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetAccessPointByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Updates access point configuration.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccessPointDto>> Update(Guid id, [FromBody] UpdateAccessPointRequest request)
    {
        var command = new UpdateAccessPointCommand(id, request.Name, request.Description, request.PhysicalLocation);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Enables an access point.
    /// </summary>
    [HttpPatch("{id:guid}/enable")]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessPointDto>> Enable(Guid id)
    {
        var result = await Mediator.Send(new ChangeAccessPointStatusCommand(id, Enable: true));
        return Ok(result);
    }

    /// <summary>
    /// Disables an access point (rejects new entry/exit attempts).
    /// </summary>
    [HttpPatch("{id:guid}/disable")]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessPointDto>> Disable(Guid id)
    {
        var result = await Mediator.Send(new ChangeAccessPointStatusCommand(id, Enable: false));
        return Ok(result);
    }
}

public record UpdateAccessPointRequest(string Name, string? Description, string PhysicalLocation);

