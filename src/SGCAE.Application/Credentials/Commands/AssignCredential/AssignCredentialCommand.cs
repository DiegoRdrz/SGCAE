using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Credentials.DTOs;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.Credentials.Commands.AssignCredential;

public record AssignCredentialCommand(Guid EmployeeId, string Code) : IRequest<CredentialDto>;

public class AssignCredentialCommandValidator : AbstractValidator<AssignCredentialCommand>
{
    public AssignCredentialCommandValidator()
    {
        RuleFor(v => v.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Credential code is required.")
            .MinimumLength(3).WithMessage("Code must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters.");
    }
}

public class AssignCredentialCommandHandler : IRequestHandler<AssignCredentialCommand, CredentialDto>
{
    private readonly IApplicationDbContext _context;

    public AssignCredentialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CredentialDto> Handle(AssignCredentialCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(e => e.Credentials)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            throw new NotFoundException(nameof(Employee), request.EmployeeId);

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeExists = await _context.Credentials
            .AnyAsync(c => c.Code == normalizedCode, cancellationToken);

        if (codeExists)
            throw new DomainException($"Credential code '{normalizedCode}' is already registered in the system.");

        var credential = employee.AssignCredential(normalizedCode);

        _context.Credentials.Add(credential);
        await _context.SaveChangesAsync(cancellationToken);

        return CredentialDto.FromEntity(credential);
    }
}

