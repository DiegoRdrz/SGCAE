using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Common.Models;
using SGCAE.Application.Employees.Commands.CreateEmployee;
using SGCAE.Application.Employees.Commands.DeactivateEmployee;
using SGCAE.Application.Employees.Commands.ReactivateEmployee;
using SGCAE.Application.Employees.Commands.UpdateEmployee;
using SGCAE.Application.Employees.DTOs;
using SGCAE.Application.Employees.Queries.GetEmployeeById;
using SGCAE.Application.Employees.Queries.GetEmployees;
using SGCAE.Domain.Enums;

namespace SGCAE.WebApi.Controllers;

/// <summary>
/// Employee management and lifecycle operations (RF-01).
/// </summary>
public class EmployeesController : ApiControllerBase
{
    /// <summary>
    /// Registers a new employee in the system.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves paginated employees with optional search and filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] EmploymentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetEmployeesQuery(search, department, status, page, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves an employee by unique ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetEmployeeByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Updates employee profile and work assignment information.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
    {
        var command = new UpdateEmployeeCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.JobTitle,
            request.Department);

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates an employee and automatically revokes all active credentials.
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EmployeeDto>> Deactivate(Guid id, [FromBody] DeactivateEmployeeRequest? request)
    {
        var result = await Mediator.Send(new DeactivateEmployeeCommand(id, request?.Reason));
        return Ok(result);
    }

    /// <summary>
    /// Reactivates an employee.
    /// </summary>
    [HttpPatch("{id:guid}/reactivate")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> Reactivate(Guid id)
    {
        var result = await Mediator.Send(new ReactivateEmployeeCommand(id));
        return Ok(result);
    }
}

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    string Department);

public record DeactivateEmployeeRequest(string? Reason);

