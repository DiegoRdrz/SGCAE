using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.Credentials.DTOs;

public class CredentialDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeIdentificationNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public CredentialStatus Status { get; set; }
    public string StatusDescription => Status.ToString();
    public DateTime IssuedAt { get; set; }
    public DateTime? StatusUpdatedAt { get; set; }
    public string? StatusChangeReason { get; set; }

    public static CredentialDto FromEntity(Credential entity)
    {
        return new CredentialDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeFullName = entity.Employee?.FullName ?? string.Empty,
            EmployeeIdentificationNumber = entity.Employee?.IdentificationNumber ?? string.Empty,
            Code = entity.Code,
            Status = entity.Status,
            IssuedAt = entity.IssuedAt,
            StatusUpdatedAt = entity.StatusUpdatedAt,
            StatusChangeReason = entity.StatusChangeReason
        };
    }
}

