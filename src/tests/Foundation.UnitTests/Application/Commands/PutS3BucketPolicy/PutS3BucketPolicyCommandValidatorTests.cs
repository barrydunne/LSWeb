using Foundation.Application.Commands.PutS3BucketPolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutS3BucketPolicy;

public class PutS3BucketPolicyCommandValidatorTests
{
    private readonly PutS3BucketPolicyCommandValidator _sut =
        new(NullLogger<PutS3BucketPolicyCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new PutS3BucketPolicyCommand("docs", "{\"Version\":\"2012-10-17\",\"Statement\":[]}"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new PutS3BucketPolicyCommand(string.Empty, "{\"Version\":\"2012-10-17\",\"Statement\":[]}"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutS3BucketPolicyCommand.BucketName));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("{\"Statement\":[]}")]
    [InlineData("{\"Version\":\"2012-10-17\"}")]
    public async Task ValidateAsync_WhenPolicyInvalid_ReturnsErrorForPolicy(string policy)
    {
        var result = await _sut.ValidateAsync(
            new PutS3BucketPolicyCommand("docs", policy), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutS3BucketPolicyCommand.Policy));
    }
}
