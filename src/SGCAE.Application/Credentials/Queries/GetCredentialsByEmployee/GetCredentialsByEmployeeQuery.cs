using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Credentials.DTOs;

namespace SGCAE.Application.Credentials.Queries.GetCredentialsByEmployee;

public record GetCredentialsByEmployeeQuery(Guid EmployeeId) : IRequest<List<CredentialDto>>;

public class GetCredentialsByEmployeeQueryHandler : IRequestHandler<GetCredentialsByEmployeeQuery, List<CredentialDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCredentialsByEmployeeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CredentialDto>> Handle(GetCredentialsByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Credentials
            .AsNoTracking()
            .Include(c => c.Employee)
            .Where(c => c.EmployeeId == request.EmployeeId)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync(cancellationToken);

        return items.Select(CredentialDto.FromEntity).ToList();
    }
}

