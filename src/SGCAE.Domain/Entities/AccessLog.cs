using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.Entities;

public class AccessLog : BaseEntity
{
    public Guid? EmployeeId { get; private set; }
    public virtual Employee? Employee { get; private set; }

    public Guid? CredentialId { get; private set; }
    public virtual Credential? Credential { get; private set; }

    public Guid AccessPointId { get; private set; }
    public virtual AccessPoint AccessPoint { get; private set; } = null!;

    public string CredentialCodePresented { get; private set; } = string.Empty;
    public MovementType MovementType { get; private set; }
    public AccessResult Result { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime Timestamp { get; private set; }

    protected AccessLog() { } // EF Core

    private AccessLog(
        Guid? employeeId,
        Guid? credentialId,
        Guid accessPointId,
        string credentialCodePresented,
        MovementType movementType,
        AccessResult result,
        string? rejectionReason)
    {
        EmployeeId = employeeId;
        CredentialId = credentialId;
        AccessPointId = accessPointId;
        CredentialCodePresented = credentialCodePresented.Trim().ToUpperInvariant();
        MovementType = movementType;
        Result = result;
        RejectionReason = rejectionReason?.Trim();
        Timestamp = DateTime.UtcNow;
    }

    public static AccessLog CreateGranted(
        Guid employeeId,
        Guid credentialId,
        Guid accessPointId,
        string credentialCode,
        MovementType movementType)
    {
        return new AccessLog(
            employeeId,
            credentialId,
            accessPointId,
            credentialCode,
            movementType,
            AccessResult.Granted,
            rejectionReason: null);
    }

    public static AccessLog CreateDenied(
        Guid? employeeId,
        Guid? credentialId,
        Guid accessPointId,
        string credentialCode,
        MovementType movementType,
        string rejectionReason)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new DomainException("A denied access log must contain a rejection reason.");

        return new AccessLog(
            employeeId,
            credentialId,
            accessPointId,
            credentialCode,
            movementType,
            AccessResult.Denied,
            rejectionReason);
    }
}

