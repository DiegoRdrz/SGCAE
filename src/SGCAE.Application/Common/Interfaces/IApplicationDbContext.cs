using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.Entities;

namespace SGCAE.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<Credential> Credentials { get; }
    DbSet<AccessPoint> AccessPoints { get; }
    DbSet<AccessLog> AccessLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

