using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Employees.DTOs;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Application.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    string Department) : IRequest<EmployeeDto>;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(150);

        RuleFor(v => v.JobTitle)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(100);

        RuleFor(v => v.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100);
    }
}

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee is null)
            throw new NotFoundException(nameof(Employee), request.Id);

        var emailVo = Email.Create(request.Email);
        var emailExistsInOther = await _context.Employees
            .AnyAsync(e => e.Id != request.Id && e.Email == emailVo, cancellationToken);

        if (emailExistsInOther)
            throw new DomainException($"The email address '{request.Email}' is already in use by another employee.");
        employee.Update(
            request.FirstName,
            request.LastName,
            request.JobTitle,
            request.Department,
            emailVo);

        await _context.SaveChangesAsync(cancellationToken);

        return EmployeeDto.FromEntity(employee);
    }
}

