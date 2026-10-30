using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.Entities;

public class Credential : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public virtual Employee Employee { get; private set; } = null!;
    public string Code { get; private set; } = string.Empty;
    public CredentialStatus Status { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? StatusUpdatedAt { get; private set; }
    public string? StatusChangeReason { get; private set; }

    private readonly List<AccessLog> _accessLogs = new();
    public virtual IReadOnlyCollection<AccessLog> AccessLogs => _accessLogs.AsReadOnly();

    protected Credential() { } // EF Core

    public Credential(Guid employeeId, string code)
    {
        if (employeeId == Guid.Empty)
            throw new DomainException("Employee ID is required to issue a credential.");

        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Credential code is required.");

        EmployeeId = employeeId;
        Code = code.Trim().ToUpperInvariant();
        Status = CredentialStatus.Active;
        IssuedAt = DateTime.UtcNow;
    }

    public void Block(string reason)
    {
        if (Status == CredentialStatus.Revoked)
            throw new DomainException("Cannot block a credential that has already been permanently revoked.");

        if (Status == CredentialStatus.Suspended)
            throw new DomainException("The credential is already suspended.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A reason must be provided to suspend the credential.");

        Status = CredentialStatus.Suspended;
        StatusUpdatedAt = DateTime.UtcNow;
        StatusChangeReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Revoke(string reason)
    {
        if (Status == CredentialStatus.Revoked)
            throw new DomainException("The credential is already revoked.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A reason must be provided to revoke the credential.");

        Status = CredentialStatus.Revoked;
        StatusUpdatedAt = DateTime.UtcNow;
        StatusChangeReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status == CredentialStatus.Revoked)
            throw new DomainException("A revoked credential cannot be reactivated. A new one must be issued.");

        if (Status == CredentialStatus.Active)
            throw new DomainException("The credential is already active.");

        Status = CredentialStatus.Active;
        StatusUpdatedAt = DateTime.UtcNow;
        StatusChangeReason = "Credential reactivated.";
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsValidForAccess() => Status == CredentialStatus.Active;
}

