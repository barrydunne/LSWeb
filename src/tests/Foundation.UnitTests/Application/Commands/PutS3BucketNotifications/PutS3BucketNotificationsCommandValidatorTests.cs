using Foundation.Application.Commands.PutS3BucketNotifications;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutS3BucketNotifications;

public class PutS3BucketNotificationsCommandValidatorTests
{
    private readonly PutS3BucketNotificationsCommandValidator _sut =
        new(NullLogger<PutS3BucketNotificationsCommandValidator>.Instance);

    private static PutS3BucketNotificationsCommand Command(params S3NotificationConfiguration[] rules)
        => new("docs", rules);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Command(
                new("Lambda", "arn:aws:lambda:eu-west-1:000000000000:function:p", ["s3:ObjectCreated:*"], "", ""),
                new("Queue", "arn:aws:sqs:eu-west-1:000000000000:q", ["s3:ObjectRemoved:*"], "in/", ".json"),
                new("Topic", "arn:aws:sns:eu-west-1:000000000000:t", ["s3:ObjectCreated:Put"], "", "")),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new PutS3BucketNotificationsCommand(string.Empty, []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutS3BucketNotificationsCommand.BucketName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTypeUnknown_IsInvalid()
    {
        var result = await _sut.ValidateAsync(
            Command(new S3NotificationConfiguration("Email", "arn:aws:lambda:eu-west-1:000000000000:function:p", ["s3:ObjectCreated:*"], "", "")),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenArnDoesNotMatchType_IsInvalid()
    {
        var result = await _sut.ValidateAsync(
            Command(new S3NotificationConfiguration("Lambda", "arn:aws:sqs:eu-west-1:000000000000:q", ["s3:ObjectCreated:*"], "", "")),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenEventsEmpty_IsInvalid()
    {
        var result = await _sut.ValidateAsync(
            Command(new S3NotificationConfiguration("Lambda", "arn:aws:lambda:eu-west-1:000000000000:function:p", [], "", "")),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }
}
