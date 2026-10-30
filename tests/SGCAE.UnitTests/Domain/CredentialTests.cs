using FluentAssertions;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using Xunit;

namespace SGCAE.UnitTests.Domain;

public class CredentialTests
{
    [Fact]
    public void BlockCredential_ActiveCredential_ShouldChangeStatusToSuspended()
    {
        // Arrange
        var credential = new Credential(Guid.NewGuid(), "CRD-TEST");

        // Act
        credential.Block("Temporary loss");

        // Assert
        credential.Status.Should().Be(CredentialStatus.Suspended);
        credential.IsValidForAccess().Should().BeFalse();
        credential.StatusChangeReason.Should().Be("Temporary loss");
    }

    [Fact]
    public void ReactivateCredential_RevokedCredential_ShouldThrowDomainException()
    {
        // Arrange
        var credential = new Credential(Guid.NewGuid(), "CRD-TEST");
        credential.Revoke("Theft confirmed");

        // Act
        Action act = () => credential.Reactivate();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*revoked credential cannot be reactivated*");
    }

    [Fact]
    public void BlockCredential_WithoutReason_ShouldThrowDomainException()
    {
        // Arrange
        var credential = new Credential(Guid.NewGuid(), "CRD-TEST");

        // Act
        Action act = () => credential.Block("   ");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*reason*");
    }
}

