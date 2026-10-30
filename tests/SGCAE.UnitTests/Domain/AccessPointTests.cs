using FluentAssertions;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using Xunit;

namespace SGCAE.UnitTests.Domain;

public class AccessPointTests
{
    [Fact]
    public void DisableAccessPoint_EnabledPoint_ShouldBecomeDisabled()
    {
        // Arrange
        var accessPoint = new AccessPoint("Main Bay", "Loading dock", "Bay 1", isEnabled: true);

        // Act
        accessPoint.Disable();

        // Assert
        accessPoint.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void EnableAccessPoint_AlreadyEnabled_ShouldThrowDomainException()
    {
        // Arrange
        var accessPoint = new AccessPoint("Main Bay", "Loading dock", "Bay 1", isEnabled: true);

        // Act
        Action act = () => accessPoint.Enable();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already enabled*");
    }
}

