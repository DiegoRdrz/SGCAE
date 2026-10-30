using SGCAE.Domain.Enums;

namespace SGCAE.Application.AccessControl.DTOs;

public class AccessResultDto
{
    public Guid LogId { get; set; }
    public AccessResult Result { get; set; }
    public bool IsGranted => Result == AccessResult.Granted;
    public string? RejectionReason { get; set; }
    public string CredentialCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? EmployeeIdentificationNumber { get; set; }
    public string AccessPointName { get; set; } = string.Empty;
    public MovementType MovementType { get; set; }
    public DateTime Timestamp { get; set; }
}

