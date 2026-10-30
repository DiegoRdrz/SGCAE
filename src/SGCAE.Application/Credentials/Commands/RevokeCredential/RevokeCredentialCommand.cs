using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Application.Credentials.DTOs;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.Credentials.Commands.RevokeCredential;

public record RevokeCredentialCommand(string Code, string Reason) : IRequest<CredentialDto>;

public class RevokeCredentialCommandValidator : AbstractValidator<RevokeCredentialCommand>
{
    public RevokeCredentialCommandValidator()
    {
        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Credential code is required.");

        RuleFor(v => v.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(250).WithMessage("Reason cannot exceed 250 characters.");
    }
}

public class RevokeCredentialCommandHandler : IRequestHandler<RevokeCredentialCommand, CredentialDto>
{
    private readonly IApplicationDbContext _context;

    public RevokeCredentialCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CredentialDto> Handle(RevokeCredentialCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var credential = await _context.Credentials
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

        if (credential is null)
            throw new NotFoundException(nameof(Credential), code);

        credential.Revoke(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);

        return CredentialDto.FromEntity(credential);
    }
}

