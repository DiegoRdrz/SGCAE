using FluentAssertions;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.ValueObjects;
using Xunit;

namespace SGCAE.UnitTests.Domain;

public class EmployeeTests
{
    [Fact]
    public void CreateEmployee_WithValidData_ShouldCreateActiveEmployee()
    {
        // Arrange & Act
        var email = Email.Create("employee@company.com");
        var employee = new Employee(
            "ID",
            "12345678",
            "John",
            "Doe",
            email,
            "Supervisor",
            "Operations");

        // Assert
        employee.Id.Should().NotBeEmpty();
        employee.Status.Should().Be(EmploymentStatus.Active);
        employee.CanAccess().Should().BeTrue();
        employee.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void AssignCredential_InactiveEmployee_ShouldThrowDomainException()
    {
        // Arrange
        var email = Email.Create("employee@company.com");
        var employee = new Employee("ID", "12345678", "John", "Doe", email, "Supervisor", "Operations");
        employee.Deactivate("Resignation");

        // Act
        Action act = () => employee.AssignCredential("CRD-001");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*inactive employee*");
    }

    [Fact]
    public void AssignCredential_EmployeeAlreadyHasActiveCredential_ShouldThrowDomainException()
    {
        // Arrange
        var email = Email.Create("employee@company.com");
        var employee = new Employee("ID", "12345678", "John", "Doe", email, "Supervisor", "Operations");
        employee.AssignCredential("CRD-001");

        // Act
        Action act = () => employee.AssignCredential("CRD-002");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already has an active credential*");
    }

    [Fact]
    public void DeactivateEmployee_ShouldAutomaticallyRevokeActiveCredentials()
    {
        // Arrange
        var email = Email.Create("employee@company.com");
        var employee = new Employee("ID", "12345678", "John", "Doe", email, "Supervisor", "Operations");
        var credential = employee.AssignCredential("CRD-001");

        // Act
        employee.Deactivate("Contract ended");

        // Assert
        employee.Status.Should().Be(EmploymentStatus.Inactive);
        employee.TerminationDate.Should().NotBeNull();
        credential.Status.Should().Be(CredentialStatus.Revoked);
        credential.IsValidForAccess().Should().BeFalse();
    }
}

