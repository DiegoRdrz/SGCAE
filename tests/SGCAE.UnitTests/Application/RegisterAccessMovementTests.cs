using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.AccessControl.Commands.RegisterAccessMovement;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.ValueObjects;
using SGCAE.Infrastructure.Persistence;
using Xunit;

namespace SGCAE.UnitTests.Application;

public class RegisterAccessMovementTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task RegisterMovement_ValidConditions_ShouldGrantAccess()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var point = new AccessPoint("Main Reception", "Main gate", "Floor 1", isEnabled: true);
        context.AccessPoints.Add(point);

        var employee = new Employee("ID", "10203040", "John", "Doe", Email.Create("john@company.com"), "Operator", "Logistics");
        var credential = employee.AssignCredential("CRD-OK");
        context.Employees.Add(employee);
        context.Credentials.Add(credential);
        await context.SaveChangesAsync();

        var handler = new RegisterAccessMovementCommandHandler(context);
        var command = new RegisterAccessMovementCommand("CRD-OK", point.Id, MovementType.Entry);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(AccessResult.Granted);
        result.IsGranted.Should().BeTrue();
        result.RejectionReason.Should().BeNull();
        result.EmployeeName.Should().Be("John Doe");

        var logInDb = await context.AccessLogs.FirstOrDefaultAsync(l => l.Id == result.LogId);
        logInDb.Should().NotBeNull();
        logInDb!.Result.Should().Be(AccessResult.Granted);
    }

    [Fact]
    public async Task RegisterMovement_DisabledAccessPoint_ShouldDenyAccess()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var point = new AccessPoint("Maintenance Bay", "Under construction", "Sector B", isEnabled: false);
        context.AccessPoints.Add(point);

        var employee = new Employee("ID", "10203040", "John", "Doe", Email.Create("john@company.com"), "Operator", "Logistics");
        var credential = employee.AssignCredential("CRD-OK");
        context.Employees.Add(employee);
        context.Credentials.Add(credential);
        await context.SaveChangesAsync();

        var handler = new RegisterAccessMovementCommandHandler(context);
        var command = new RegisterAccessMovementCommand("CRD-OK", point.Id, MovementType.Entry);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Result.Should().Be(AccessResult.Denied);
        result.IsGranted.Should().BeFalse();
        result.RejectionReason.Should().Contain("disabled");
    }

    [Fact]
    public async Task RegisterMovement_UnknownCredential_ShouldDenyAccess()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var point = new AccessPoint("Main Reception", "Main gate", "Floor 1", isEnabled: true);
        context.AccessPoints.Add(point);
        await context.SaveChangesAsync();

        var handler = new RegisterAccessMovementCommandHandler(context);
        var command = new RegisterAccessMovementCommand("CRD-FAKE", point.Id, MovementType.Entry);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Result.Should().Be(AccessResult.Denied);
        result.IsGranted.Should().BeFalse();
        result.RejectionReason.Should().Contain("does not exist");
    }

    [Fact]
    public async Task RegisterMovement_SuspendedCredential_ShouldDenyAccess()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var point = new AccessPoint("Main Reception", "Main gate", "Floor 1", isEnabled: true);
        context.AccessPoints.Add(point);

        var employee = new Employee("ID", "10203040", "John", "Doe", Email.Create("john@company.com"), "Operator", "Logistics");
        var credential = employee.AssignCredential("CRD-SUSP");
        credential.Block("Reported lost");
        context.Employees.Add(employee);
        context.Credentials.Add(credential);
        await context.SaveChangesAsync();

        var handler = new RegisterAccessMovementCommandHandler(context);
        var command = new RegisterAccessMovementCommand("CRD-SUSP", point.Id, MovementType.Exit);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Result.Should().Be(AccessResult.Denied);
        result.IsGranted.Should().BeFalse();
        result.RejectionReason.Should().Contain("suspended");
    }

    [Fact]
    public async Task RegisterMovement_InactiveEmployee_ShouldDenyAccess()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var point = new AccessPoint("Main Reception", "Main gate", "Floor 1", isEnabled: true);
        context.AccessPoints.Add(point);

        var employee = new Employee("ID", "10203040", "John", "Doe", Email.Create("john@company.com"), "Operator", "Logistics");
        var credential = employee.AssignCredential("CRD-TERM");
        employee.Deactivate("Employment terminated");
        context.Employees.Add(employee);
        context.Credentials.Add(credential);
        await context.SaveChangesAsync();

        var handler = new RegisterAccessMovementCommandHandler(context);
        var command = new RegisterAccessMovementCommand("CRD-TERM", point.Id, MovementType.Entry);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Result.Should().Be(AccessResult.Denied);
        result.IsGranted.Should().BeFalse();
        result.RejectionReason.Should().Contain("revoked");
    }
}

