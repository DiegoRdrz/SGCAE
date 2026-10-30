using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Domain.Entities;

public class Employee : BaseEntity
{
    public string IdentificationType { get; private set; } = string.Empty;
    public string IdentificationNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string JobTitle { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public EmploymentStatus Status { get; private set; }
    public DateTime HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }

    private readonly List<Credential> _credentials = new();
    public virtual IReadOnlyCollection<Credential> Credentials => _credentials.AsReadOnly();

    private readonly List<AccessLog> _accessLogs = new();
    public virtual IReadOnlyCollection<AccessLog> AccessLogs => _accessLogs.AsReadOnly();

    protected Employee() { } // EF Core

    public Employee(
        string identificationType,
        string identificationNumber,
        string firstName,
        string lastName,
        Email email,
        string jobTitle,
        string department,
        DateTime? hireDate = null)
    {
        if (string.IsNullOrWhiteSpace(identificationType))
            throw new DomainException("Identification type is required.");

        if (string.IsNullOrWhiteSpace(identificationNumber))
            throw new DomainException("Identification number is required.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new DomainException("Job title is required.");

        if (string.IsNullOrWhiteSpace(department))
            throw new DomainException("Department is required.");

        IdentificationType = identificationType.Trim().ToUpperInvariant();
        IdentificationNumber = identificationNumber.Trim();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email ?? throw new DomainException("Email is required.");
        JobTitle = jobTitle.Trim();
        Department = department.Trim();
        Status = EmploymentStatus.Active;
        HireDate = hireDate ?? DateTime.UtcNow;
    }

    public void Update(
        string firstName,
        string lastName,
        string jobTitle,
        string department,
        Email email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be empty.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be empty.");

        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new DomainException("Job title cannot be empty.");

        if (string.IsNullOrWhiteSpace(department))
            throw new DomainException("Department cannot be empty.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        JobTitle = jobTitle.Trim();
        Department = department.Trim();
        Email = email ?? throw new DomainException("Email cannot be null.");
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(string? reason = null)
    {
        if (Status == EmploymentStatus.Inactive)
            throw new DomainException("The employee is already inactive.");

        Status = EmploymentStatus.Inactive;
        TerminationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Invariant: Deactivating an employee automatically revokes all active credentials
        foreach (var credential in _credentials.Where(c => c.Status == CredentialStatus.Active))
        {
            credential.Revoke($"Auto-revoked due to employment deactivation. {reason}".Trim());
        }
    }

    public void Reactivate()
    {
        if (Status == EmploymentStatus.Active)
            throw new DomainException("The employee is already active.");

        Status = EmploymentStatus.Active;
        TerminationDate = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public Credential AssignCredential(string code)
    {
        if (Status != EmploymentStatus.Active)
            throw new DomainException("Cannot assign a credential to an inactive employee.");

        if (_credentials.Any(c => c.Status == CredentialStatus.Active))
            throw new DomainException("The employee already has an active credential. It must be suspended or revoked first.");

        var newCredential = new Credential(Id, code);
        _credentials.Add(newCredential);
        UpdatedAt = DateTime.UtcNow;
        return newCredential;
    }

    public bool CanAccess() => Status == EmploymentStatus.Active;

    public string FullName => $"{FirstName} {LastName}";
}

