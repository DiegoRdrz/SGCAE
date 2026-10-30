using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.AccessPoints.DTOs;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.AccessPoints.Commands.CreateAccessPoint;

public record CreateAccessPointCommand(
    string Name,
    string? Description,
    string PhysicalLocation,
    bool IsEnabled = true) : IRequest<AccessPointDto>;

public class CreateAccessPointCommandValidator : AbstractValidator<CreateAccessPointCommand>
{
    public CreateAccessPointCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Access point name is required.")
            .MaximumLength(100);

        RuleFor(v => v.PhysicalLocation)
            .NotEmpty().WithMessage("Physical location is required.")
            .MaximumLength(150);

        RuleFor(v => v.Description)
            .MaximumLength(300);
    }
}

public class CreateAccessPointCommandHandler : IRequestHandler<CreateAccessPointCommand, AccessPointDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAccessPointCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccessPointDto> Handle(CreateAccessPointCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _context.AccessPoints
            .AnyAsync(p => p.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (nameExists)
            throw new DomainException($"An access point named '{request.Name}' already exists.");

        var accessPoint = new AccessPoint(
            request.Name,
            request.Description,
            request.PhysicalLocation,
            request.IsEnabled);

        _context.AccessPoints.Add(accessPoint);
        await _context.SaveChangesAsync(cancellationToken);

        return AccessPointDto.FromEntity(accessPoint);
    }
}

