using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.AccessControl.DTOs;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Common.Models;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.AccessControl.Queries.GetAccessHistory;

public record GetAccessHistoryQuery(
    Guid? EmployeeId = null,
    Guid? AccessPointId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    AccessResult? Result = null,
    MovementType? MovementType = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<AccessLogDto>>;

public class GetAccessHistoryQueryHandler : IRequestHandler<GetAccessHistoryQuery, PagedResult<AccessLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAccessHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AccessLogDto>> Handle(GetAccessHistoryQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

        var query = _context.AccessLogs
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.AccessPoint)
            .Include(r => r.Credential)
            .AsQueryable();

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);
        }

        if (request.AccessPointId.HasValue)
        {
            query = query.Where(r => r.AccessPointId == request.AccessPointId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(r => r.Timestamp >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(r => r.Timestamp <= request.EndDate.Value);
        }

        if (request.Result.HasValue)
        {
            query = query.Where(r => r.Result == request.Result.Value);
        }

        if (request.MovementType.HasValue)
        {
            query = query.Where(r => r.MovementType == request.MovementType.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(AccessLogDto.FromEntity).ToList();

        return new PagedResult<AccessLogDto>(dtos, totalCount, pageNumber, pageSize);
    }
}

