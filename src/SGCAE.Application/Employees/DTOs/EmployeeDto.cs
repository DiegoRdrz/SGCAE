using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.Employees.DTOs;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string IdentificationType { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public EmploymentStatus Status { get; set; }
    public string StatusDescription => Status.ToString();
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }

    public static EmployeeDto FromEntity(Employee entity)
    {
        return new EmployeeDto
        {
            Id = entity.Id,
            IdentificationType = entity.IdentificationType,
            IdentificationNumber = entity.IdentificationNumber,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            FullName = entity.FullName,
            Email = entity.Email.Value,
            JobTitle = entity.JobTitle,
            Department = entity.Department,
            Status = entity.Status,
            HireDate = entity.HireDate,
            TerminationDate = entity.TerminationDate
        };
    }
}

