using Foundation.Application.Commands.DeleteS3ObjectVersion;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteS3ObjectVersion;

public class DeleteS3ObjectVersionCommandValidatorTests
{
    private readonly DeleteS3ObjectVersionCommandValidator _sut =
        new(NullLogger<DeleteS3ObjectVersionCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3ObjectVersionCommand("docs", "report.pdf", "v1"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3ObjectVersionCommand(string.Empty, "report.pdf", "v1"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteS3ObjectVersionCommand.BucketName));
    }

    [Fact]
    public async Task ValidateAsync_WhenKeyEmpty_ReturnsErrorForKey()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3ObjectVersionCommand("docs", string.Empty, "v1"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteS3ObjectVersionCommand.Key));
    }

    [Fact]
    public async Task ValidateAsync_WhenVersionIdEmpty_ReturnsErrorForVersionId()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3ObjectVersionCommand("docs", "report.pdf", string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteS3ObjectVersionCommand.VersionId));
    }
}
