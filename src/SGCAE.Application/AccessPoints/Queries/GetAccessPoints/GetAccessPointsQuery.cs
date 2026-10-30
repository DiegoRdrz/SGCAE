using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.AccessPoints.DTOs;
using SGCAE.Application.Common.Interfaces;

namespace SGCAE.Application.AccessPoints.Queries.GetAccessPoints;

public record GetAccessPointsQuery(bool? OnlyEnabled = null) : IRequest<List<AccessPointDto>>;

public class GetAccessPointsQueryHandler : IRequestHandler<GetAccessPointsQuery, List<AccessPointDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAccessPointsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccessPointDto>> Handle(GetAccessPointsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AccessPoints.AsNoTracking().AsQueryable();

        if (request.OnlyEnabled.HasValue)
        {
            query = query.Where(p => p.IsEnabled == request.OnlyEnabled.Value);
        }

        var items = await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return items.Select(AccessPointDto.FromEntity).ToList();
    }
}

