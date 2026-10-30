using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Credentials.Commands.AssignCredential;
using SGCAE.Application.Credentials.Commands.BlockCredential;
using SGCAE.Application.Credentials.Commands.RevokeCredential;
using SGCAE.Application.Credentials.DTOs;
using SGCAE.Application.Credentials.Queries.GetCredentialByCode;
using SGCAE.Application.Credentials.Queries.GetCredentialsByEmployee;

namespace SGCAE.WebApi.Controllers;

/// <summary>
/// Credential management and lifecycle operations (RF-02).
/// </summary>
public class CredentialsController : ApiControllerBase
{
    /// <summary>
    /// Issues and assigns a new credential to an active employee.
    /// </summary>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CredentialDto>> Assign([FromBody] AssignCredentialCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByCode), new { code = result.Code }, result);
    }

    /// <summary>
    /// Retrieves credential details by its alphanumeric code.
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredentialDto>> GetByCode(string code)
    {
        var result = await Mediator.Send(new GetCredentialByCodeQuery(code));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all credentials associated with a specific employee.
    /// </summary>
    [HttpGet("employee/{employeeId:guid}")]
    [ProducesResponseType(typeof(List<CredentialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CredentialDto>>> GetByEmployee(Guid employeeId)
    {
        var result = await Mediator.Send(new GetCredentialsByEmployeeQuery(employeeId));
        return Ok(result);
    }

    /// <summary>
    /// Suspends an active credential temporarily.
    /// </summary>
    [HttpPatch("{code}/block")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CredentialDto>> Block(string code, [FromBody] ChangeCredentialStatusRequest request)
    {
        var result = await Mediator.Send(new BlockCredentialCommand(code, request.Reason));
        return Ok(result);
    }

    /// <summary>
    /// Permanently revokes a credential.
    /// </summary>
    [HttpPatch("{code}/revoke")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CredentialDto>> Revoke(string code, [FromBody] ChangeCredentialStatusRequest request)
    {
        var result = await Mediator.Send(new RevokeCredentialCommand(code, request.Reason));
        return Ok(result);
    }
}

public record ChangeCredentialStatusRequest(string Reason);

