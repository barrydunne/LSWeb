using Foundation.Application.Commands.AddFavourite;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.AddFavourite;

public class AddFavouriteCommandValidatorTests
{
    private readonly AddFavouriteCommandValidator _sut =
        new(NullLogger<AddFavouriteCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenReferenceProvided_IsValid()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new AddFavouriteCommand("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenReferenceEmpty_ReturnsErrorForReference()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new AddFavouriteCommand(string.Empty), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(AddFavouriteCommand.Reference));
    }
}
