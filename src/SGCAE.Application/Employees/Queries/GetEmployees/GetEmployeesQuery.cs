using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Common.Models;
using SGCAE.Application.Employees.DTOs;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.Employees.Queries.GetEmployees;

public record GetEmployeesQuery(
    string? SearchTerm = null,
    string? Department = null,
    EmploymentStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<EmployeeDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

        var query = _context.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(e =>
                e.IdentificationNumber.ToLower().Contains(term) ||
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                ((string)(object)e.Email).ToLower().Contains(term) ||
                e.JobTitle.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            var dept = request.Department.Trim().ToLower();
            query = query.Where(e => e.Department.ToLower() == dept);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(EmployeeDto.FromEntity).ToList();

        return new PagedResult<EmployeeDto>(dtos, totalCount, pageNumber, pageSize);
    }
}

