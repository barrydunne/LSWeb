using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetS3BucketConfiguration;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetS3BucketConfiguration;

public class GetS3BucketConfigurationQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private GetS3BucketConfigurationQueryHandler CreateSut()
        => new(_client, NullLogger<GetS3BucketConfigurationQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenConfigurationSucceeds_ReturnsSortedRulesAndNotifications()
    {
        // Arrange
        var configuration = new S3BucketConfiguration(
            "Enabled",
            "aws:kms",
            "key-1",
            new List<S3LifecycleRule>
            {
                new("expire-old", "Enabled", "logs/"),
                new("archive", "Disabled", "archive/"),
            },
            new List<S3NotificationConfiguration>
            {
                new("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:process", ["s3:ObjectCreated:*"]),
                new("Queue", "arn:aws:sqs:us-east-1:000000000000:events", ["s3:ObjectRemoved:*"]),
            },
            "{\"Version\":\"2012-10-17\"}");
        _client
            .GetBucketConfigurationAsync("data", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3BucketConfiguration>>(configuration));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetS3BucketConfigurationQuery("data"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.VersioningStatus.Should().Be("Enabled");
        result.Value.EncryptionAlgorithm.Should().Be("aws:kms");
        result.Value.EncryptionKeyId.Should().Be("key-1");
        result.Value.Policy.Should().Be("{\"Version\":\"2012-10-17\"}");
        result.Value.LifecycleRules.Should().Equal(
            new S3LifecycleRuleResult("archive", "Disabled", "archive/"),
            new S3LifecycleRuleResult("expire-old", "Enabled", "logs/"));
        result.Value.Notifications.Should().BeEquivalentTo(
            new[]
            {
                new S3NotificationResult("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:process", ["s3:ObjectCreated:*"]),
                new S3NotificationResult("Queue", "arn:aws:sqs:us-east-1:000000000000:events", ["s3:ObjectRemoved:*"]),
            },
            options => options.WithStrictOrdering());
        await _client.Received(1).GetBucketConfigurationAsync("data", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConfigurationEmpty_ReturnsEmptyCollections()
    {
        // Arrange
        var configuration = new S3BucketConfiguration(
            "Disabled",
            string.Empty,
            string.Empty,
            new List<S3LifecycleRule>(),
            new List<S3NotificationConfiguration>(),
            string.Empty);
        _client
            .GetBucketConfigurationAsync("data", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3BucketConfiguration>>(configuration));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetS3BucketConfigurationQuery("data"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.VersioningStatus.Should().Be("Disabled");
        result.Value.EncryptionAlgorithm.Should().BeEmpty();
        result.Value.EncryptionKeyId.Should().BeEmpty();
        result.Value.Policy.Should().BeEmpty();
        result.Value.LifecycleRules.Should().BeEmpty();
        result.Value.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenConfigurationFails_ReturnsError()
    {
        // Arrange
        _client
            .GetBucketConfigurationAsync("data", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3BucketConfiguration>>(new Error("configuration boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetS3BucketConfigurationQuery("data"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("configuration boom");
    }
}
