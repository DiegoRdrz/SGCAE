using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Employees.DTOs;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.Employees.Commands.DeactivateEmployee;

public record DeactivateEmployeeCommand(Guid Id, string? Reason = null) : IRequest<EmployeeDto>;

public class DeactivateEmployeeCommandHandler : IRequestHandler<DeactivateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public DeactivateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(e => e.Credentials)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee is null)
            throw new NotFoundException(nameof(Employee), request.Id);

        employee.Deactivate(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);

        return EmployeeDto.FromEntity(employee);
    }
}

