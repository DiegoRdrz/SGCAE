using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Credentials.DTOs;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.Credentials.Queries.GetCredentialByCode;

public record GetCredentialByCodeQuery(string Code) : IRequest<CredentialDto>;

public class GetCredentialByCodeQueryHandler : IRequestHandler<GetCredentialByCodeQuery, CredentialDto>
{
    private readonly IApplicationDbContext _context;

    public GetCredentialByCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CredentialDto> Handle(GetCredentialByCodeQuery request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var credential = await _context.Credentials
            .AsNoTracking()
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

        if (credential is null)
            throw new NotFoundException(nameof(Credential), code);

        return CredentialDto.FromEntity(credential);
    }
}

