using Foundation.Application.Commands.UpdateS3ObjectTags;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateS3ObjectTags;

public class UpdateS3ObjectTagsCommandValidatorTests
{
    private readonly UpdateS3ObjectTagsCommandValidator _sut =
        new(NullLogger<UpdateS3ObjectTagsCommandValidator>.Instance);

    private static UpdateS3ObjectTagsCommand Valid(IReadOnlyDictionary<string, string>? tags = null)
        => new("data", "orders/readme.txt", tags ?? new Dictionary<string, string> { ["stage"] = "prod" });

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTagsEmpty_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(new Dictionary<string, string>()), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketNameEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new UpdateS3ObjectTagsCommand(string.Empty, "orders/readme.txt", new Dictionary<string, string>()),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateS3ObjectTagsCommand.BucketName));
    }

    [Fact]
    public async Task ValidateAsync_WhenKeyEmpty_ReturnsErrorForKey()
    {
        var result = await _sut.ValidateAsync(
            new UpdateS3ObjectTagsCommand("data", string.Empty, new Dictionary<string, string>()),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateS3ObjectTagsCommand.Key));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagsNull_ReturnsErrorForTags()
    {
        var result = await _sut.ValidateAsync(
            new UpdateS3ObjectTagsCommand("data", "orders/readme.txt", null!),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateS3ObjectTagsCommand.Tags));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagKeyBlank_ReturnsErrorForTags()
    {
        var result = await _sut.ValidateAsync(
            Valid(new Dictionary<string, string> { [" "] = "value" }), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateS3ObjectTagsCommand.Tags));
    }
}
