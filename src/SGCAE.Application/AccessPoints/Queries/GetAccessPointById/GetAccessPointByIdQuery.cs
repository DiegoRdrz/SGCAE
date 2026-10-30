using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.AccessPoints.DTOs;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.AccessPoints.Queries.GetAccessPointById;

public record GetAccessPointByIdQuery(Guid Id) : IRequest<AccessPointDto>;

public class GetAccessPointByIdQueryHandler : IRequestHandler<GetAccessPointByIdQuery, AccessPointDto>
{
    private readonly IApplicationDbContext _context;

    public GetAccessPointByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccessPointDto> Handle(GetAccessPointByIdQuery request, CancellationToken cancellationToken)
    {
        var accessPoint = await _context.AccessPoints
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (accessPoint is null)
            throw new NotFoundException(nameof(AccessPoint), request.Id);

        return AccessPointDto.FromEntity(accessPoint);
    }
}

