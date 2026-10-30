using SGCAE.Domain.Common;

namespace SGCAE.Domain.Entities;

public class AccessPoint : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string PhysicalLocation { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    private readonly List<AccessLog> _accessLogs = new();
    public virtual IReadOnlyCollection<AccessLog> AccessLogs => _accessLogs.AsReadOnly();

    protected AccessPoint() { } // EF Core

    public AccessPoint(string name, string? description, string physicalLocation, bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Access point name is required.");

        if (string.IsNullOrWhiteSpace(physicalLocation))
            throw new DomainException("Physical location is required.");

        Name = name.Trim();
        Description = description?.Trim();
        PhysicalLocation = physicalLocation.Trim();
        IsEnabled = isEnabled;
    }

    public void Update(string name, string? description, string physicalLocation)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Access point name cannot be empty.");

        if (string.IsNullOrWhiteSpace(physicalLocation))
            throw new DomainException("Physical location cannot be empty.");

        Name = name.Trim();
        Description = description?.Trim();
        PhysicalLocation = physicalLocation.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        if (IsEnabled)
            throw new DomainException("The access point is already enabled.");

        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        if (!IsEnabled)
            throw new DomainException("The access point is already disabled.");

        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

