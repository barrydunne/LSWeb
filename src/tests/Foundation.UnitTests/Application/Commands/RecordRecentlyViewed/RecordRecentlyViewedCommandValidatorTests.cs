using Foundation.Application.Commands.RecordRecentlyViewed;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RecordRecentlyViewed;

public class RecordRecentlyViewedCommandValidatorTests
{
    private readonly RecordRecentlyViewedCommandValidator _sut =
        new(NullLogger<RecordRecentlyViewedCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenReferenceProvided_IsValid()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new RecordRecentlyViewedCommand("sns://topic"), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenReferenceEmpty_ReturnsErrorForReference()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new RecordRecentlyViewedCommand(string.Empty), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RecordRecentlyViewedCommand.Reference));
    }
}
