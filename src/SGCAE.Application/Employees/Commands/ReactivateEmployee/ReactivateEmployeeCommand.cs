using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Employees.DTOs;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.Employees.Commands.ReactivateEmployee;

public record ReactivateEmployeeCommand(Guid Id) : IRequest<EmployeeDto>;

public class ReactivateEmployeeCommandHandler : IRequestHandler<ReactivateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public ReactivateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(ReactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee is null)
            throw new NotFoundException(nameof(Employee), request.Id);

        employee.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return EmployeeDto.FromEntity(employee);
    }
}

