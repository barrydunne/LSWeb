using Foundation.Application.Commands.SetS3BucketVersioning;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetS3BucketVersioning;

public class SetS3BucketVersioningCommandValidatorTests
{
    private readonly SetS3BucketVersioningCommandValidator _sut =
        new(NullLogger<SetS3BucketVersioningCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new SetS3BucketVersioningCommand("docs", true), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new SetS3BucketVersioningCommand(string.Empty, true), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetS3BucketVersioningCommand.BucketName));
    }
}
