using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGCAE.Domain.Entities;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Infrastructure.Persistence;

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        if (!await _context.AccessPoints.AnyAsync())
        {
            var accessPoints = new List<AccessPoint>
            {
                new("Main Reception", "Main pedestrian entrance", "Central Building - 1st Floor", isEnabled: true),
                new("Storage Warehouse", "Loading and unloading area", "Industrial Bay A", isEnabled: true),
                new("Employee Parking", "Vehicular access gate", "North Perimeter", isEnabled: true),
                new("Data Center", "Server rooms and networking hub", "Basement 1", isEnabled: true),
                new("Maintenance Workshop", "Technical repair shop", "Backyard", isEnabled: false)
            };

            await _context.AccessPoints.AddRangeAsync(accessPoints);
            await _context.SaveChangesAsync();
        }

        if (!await _context.Employees.AnyAsync())
        {
            var emp1 = new Employee(
                identificationType: "ID",
                identificationNumber: "EMP-1001",
                firstName: "John",
                lastName: "Doe",
                email: Email.Create("john.doe@company.com"),
                jobTitle: "Logistics Specialist",
                department: "Operations",
                hireDate: DateTime.UtcNow.AddYears(-2));

            emp1.AssignCredential("CRD-1001");

            var emp2 = new Employee(
                identificationType: "ID",
                identificationNumber: "EMP-1002",
                firstName: "Jane",
                lastName: "Smith",
                email: Email.Create("jane.smith@company.com"),
                jobTitle: "Security Coordinator",
                department: "Security",
                hireDate: DateTime.UtcNow.AddMonths(-10));

            emp2.AssignCredential("CRD-1002");

            var emp3 = new Employee(
                identificationType: "ID",
                identificationNumber: "EMP-1003",
                firstName: "Robert",
                lastName: "Brown",
                email: Email.Create("robert.brown@company.com"),
                jobTitle: "Warehouse Assistant",
                department: "Operations",
                hireDate: DateTime.UtcNow.AddYears(-1));

            emp3.AssignCredential("CRD-9999");
            emp3.Deactivate("Employment contract terminated.");

            var emp4 = new Employee(
                identificationType: "ID",
                identificationNumber: "EMP-1004",
                firstName: "Emily",
                lastName: "White",
                email: Email.Create("emily.white@company.com"),
                jobTitle: "Inventory Analyst",
                department: "Logistics",
                hireDate: DateTime.UtcNow.AddMonths(-6));

            var cred4 = emp4.AssignCredential("CRD-8888");
            cred4.Block("Reported temporarily misplaced.");

            await _context.Employees.AddRangeAsync(emp1, emp2, emp3, emp4);
            await _context.SaveChangesAsync();
        }
    }
}

