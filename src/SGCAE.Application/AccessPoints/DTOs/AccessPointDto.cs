using SGCAE.Domain.Entities;

namespace SGCAE.Application.AccessPoints.DTOs;

public class AccessPointDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PhysicalLocation { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }

    public static AccessPointDto FromEntity(AccessPoint entity)
    {
        return new AccessPointDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            PhysicalLocation = entity.PhysicalLocation,
            IsEnabled = entity.IsEnabled,
            CreatedAt = entity.CreatedAt
        };
    }
}

