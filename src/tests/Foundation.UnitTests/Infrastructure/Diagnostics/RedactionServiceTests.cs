using Foundation.Domain.Configuration;
using Foundation.Infrastructure.Diagnostics;

namespace Foundation.UnitTests.Infrastructure.Diagnostics;

public class RedactionServiceTests
{
    private static RedactionService CreateSut(bool allowReveal)
        => new(new RedactionSettings(allowReveal));

    [Fact]
    public void CanReveal_WhenRevealPermitted_ReturnsTrue()
    {
        // Arrange
        var sut = CreateSut(allowReveal: true);

        // Act
        var result = sut.CanReveal;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanReveal_WhenRevealNotPermitted_ReturnsFalse()
    {
        // Arrange
        var sut = CreateSut(allowReveal: false);

        // Act
        var result = sut.CanReveal;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Resolve_WhenValueNotSensitive_ReturnsRawValueRegardlessOfReveal()
    {
        // Arrange
        var sut = CreateSut(allowReveal: false);
        var value = new ConfigValue("ServiceUrl", "http://localhost:4566", ConfigSource.Default, IsSensitive: false);

        // Act
        var result = sut.Resolve(value, reveal: false);

        // Assert
        result.Should().Be("http://localhost:4566");
    }

    [Fact]
    public void Resolve_WhenSensitiveAndRevealNotRequested_ReturnsMaskedValue()
    {
        // Arrange
        var sut = CreateSut(allowReveal: true);
        var value = new ConfigValue("SecretKey", "super-secret", ConfigSource.EnvironmentVariable, IsSensitive: true);

        // Act
        var result = sut.Resolve(value, reveal: false);

        // Assert
        result.Should().Be(value.Display);
        result.Should().NotContain("super-secret");
    }

    [Fact]
    public void Resolve_WhenSensitiveAndRevealRequestedButNotPermitted_ReturnsMaskedValue()
    {
        // Arrange
        var sut = CreateSut(allowReveal: false);
        var value = new ConfigValue("SecretKey", "super-secret", ConfigSource.EnvironmentVariable, IsSensitive: true);

        // Act
        var result = sut.Resolve(value, reveal: true);

        // Assert
        result.Should().Be(value.Display);
        result.Should().NotContain("super-secret");
    }

    [Fact]
    public void Resolve_WhenSensitiveAndRevealRequestedAndPermitted_ReturnsRawValue()
    {
        // Arrange
        var sut = CreateSut(allowReveal: true);
        var value = new ConfigValue("SecretKey", "super-secret", ConfigSource.EnvironmentVariable, IsSensitive: true);

        // Act
        var result = sut.Resolve(value, reveal: true);

        // Assert
        result.Should().Be("super-secret");
    }
}
