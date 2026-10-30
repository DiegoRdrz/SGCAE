using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.AccessPoints.DTOs;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.AccessPoints.Commands.UpdateAccessPoint;

public record UpdateAccessPointCommand(
    Guid Id,
    string Name,
    string? Description,
    string PhysicalLocation) : IRequest<AccessPointDto>;

public class UpdateAccessPointCommandValidator : AbstractValidator<UpdateAccessPointCommand>
{
    public UpdateAccessPointCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Access point ID is required.");

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

public class UpdateAccessPointCommandHandler : IRequestHandler<UpdateAccessPointCommand, AccessPointDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAccessPointCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccessPointDto> Handle(UpdateAccessPointCommand request, CancellationToken cancellationToken)
    {
        var accessPoint = await _context.AccessPoints
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (accessPoint is null)
            throw new NotFoundException(nameof(AccessPoint), request.Id);

        var nameExistsInOther = await _context.AccessPoints
            .AnyAsync(p => p.Id != request.Id && p.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (nameExistsInOther)
            throw new DomainException($"Another access point named '{request.Name}' already exists.");

        accessPoint.Update(request.Name, request.Description, request.PhysicalLocation);
        await _context.SaveChangesAsync(cancellationToken);

        return AccessPointDto.FromEntity(accessPoint);
    }
}

