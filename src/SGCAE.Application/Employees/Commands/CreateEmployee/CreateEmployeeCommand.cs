using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Employees.DTOs;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Application.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string IdentificationType,
    string IdentificationNumber,
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    string Department,
    DateTime? HireDate = null) : IRequest<EmployeeDto>;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(v => v.IdentificationType)
            .NotEmpty().WithMessage("Identification type is required.")
            .MaximumLength(10);

        RuleFor(v => v.IdentificationNumber)
            .NotEmpty().WithMessage("Identification number is required.")
            .MaximumLength(30);

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email address is required.")
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

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public CreateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var idExists = await _context.Employees
            .AnyAsync(e => e.IdentificationNumber == request.IdentificationNumber.Trim(), cancellationToken);

        if (idExists)
            throw new DomainException($"An employee with identification number '{request.IdentificationNumber}' already exists.");

        var emailVo = Email.Create(request.Email);
        var emailExists = await _context.Employees
            .AnyAsync(e => e.Email == emailVo, cancellationToken);

        if (emailExists)
            throw new DomainException($"An employee with email '{request.Email}' already exists.");
        var employee = new Employee(
            request.IdentificationType,
            request.IdentificationNumber,
            request.FirstName,
            request.LastName,
            emailVo,
            request.JobTitle,
            request.Department,
            request.HireDate);

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return EmployeeDto.FromEntity(employee);
    }
}

