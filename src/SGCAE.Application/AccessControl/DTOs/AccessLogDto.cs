using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.AccessControl.DTOs;

public class AccessLogDto
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeFullName { get; set; }
    public string? EmployeeIdentificationNumber { get; set; }
    public Guid? CredentialId { get; set; }
    public string CredentialCodePresented { get; set; } = string.Empty;
    public Guid AccessPointId { get; set; }
    public string AccessPointName { get; set; } = string.Empty;
    public MovementType MovementType { get; set; }
    public string MovementTypeDescription => MovementType.ToString();
    public AccessResult Result { get; set; }
    public string ResultDescription => Result.ToString();
    public string? RejectionReason { get; set; }
    public DateTime Timestamp { get; set; }

    public static AccessLogDto FromEntity(AccessLog entity)
    {
        return new AccessLogDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeFullName = entity.Employee?.FullName,
            EmployeeIdentificationNumber = entity.Employee?.IdentificationNumber,
            CredentialId = entity.CredentialId,
            CredentialCodePresented = entity.CredentialCodePresented,
            AccessPointId = entity.AccessPointId,
            AccessPointName = entity.AccessPoint?.Name ?? string.Empty,
            MovementType = entity.MovementType,
            Result = entity.Result,
            RejectionReason = entity.RejectionReason,
            Timestamp = entity.Timestamp
        };
    }
}

