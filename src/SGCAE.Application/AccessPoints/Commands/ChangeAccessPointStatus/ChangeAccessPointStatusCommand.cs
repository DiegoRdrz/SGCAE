using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.AccessPoints.DTOs;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.AccessPoints.Commands.ChangeAccessPointStatus;

public record ChangeAccessPointStatusCommand(Guid Id, bool Enable) : IRequest<AccessPointDto>;

public class ChangeAccessPointStatusCommandHandler : IRequestHandler<ChangeAccessPointStatusCommand, AccessPointDto>
{
    private readonly IApplicationDbContext _context;

    public ChangeAccessPointStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccessPointDto> Handle(ChangeAccessPointStatusCommand request, CancellationToken cancellationToken)
    {
        var accessPoint = await _context.AccessPoints
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (accessPoint is null)
            throw new NotFoundException(nameof(AccessPoint), request.Id);

        if (request.Enable)
            accessPoint.Enable();
        else
            accessPoint.Disable();

        await _context.SaveChangesAsync(cancellationToken);

        return AccessPointDto.FromEntity(accessPoint);
    }
}

