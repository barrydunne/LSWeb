using Foundation.Application.Commands.RemoveFavourite;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RemoveFavourite;

public class RemoveFavouriteCommandValidatorTests
{
    private readonly RemoveFavouriteCommandValidator _sut =
        new(NullLogger<RemoveFavouriteCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenReferenceProvided_IsValid()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new RemoveFavouriteCommand("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenReferenceEmpty_ReturnsErrorForReference()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new RemoveFavouriteCommand(string.Empty), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RemoveFavouriteCommand.Reference));
    }
}
