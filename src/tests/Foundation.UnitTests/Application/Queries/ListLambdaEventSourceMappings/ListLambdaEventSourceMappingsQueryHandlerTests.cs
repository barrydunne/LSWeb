using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.ListLambdaEventSourceMappings;
using Foundation.Application.S3;
using Foundation.Domain.Lambda;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListLambdaEventSourceMappings;

public class ListLambdaEventSourceMappingsQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();
    private readonly IS3Client _s3Client = Substitute.For<IS3Client>();

    public ListLambdaEventSourceMappingsQueryHandlerTests()
    {
        // Default to no event source mappings, no policy triggers and no buckets so each test only
        // arranges the collaborators it cares about.
        _client
            .ListEventSourceMappingsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaEventSourceMapping>>([])));
        _client
            .ListS3TriggersAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaS3Trigger>>([])));
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<S3Bucket>>([])));
    }

    private ListLambdaEventSourceMappingsQueryHandler CreateSut()
        => new(_client, _s3Client, NullLogger<ListLambdaEventSourceMappingsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    private static S3BucketConfiguration ConfigurationWith(IReadOnlyList<S3NotificationConfiguration> notifications)
        => new(string.Empty, string.Empty, string.Empty, [], notifications, string.Empty);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsMappingsOrderedByEventSourceArn()
    {
        // Arrange
        IReadOnlyList<LambdaEventSourceMapping> stored =
        [
            new("uuid-z", "arn:zeta", "arn:fn", "Enabled", 10, "2026-01-02T03:04:05Z"),
            new("uuid-a", "arn:alpha", "arn:fn", "Disabled", 5, "2026-01-01T00:00:00Z"),
        ];
        IReadOnlyList<LambdaS3Trigger> triggers =
        [
            new("arn:aws:s3:::zeta-bucket"),
            new("arn:aws:s3:::alpha-bucket"),
        ];
        _client
            .ListEventSourceMappingsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stored)));
        _client
            .ListS3TriggersAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(triggers)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Mappings.Select(_ => _.EventSourceArn).Should().ContainInOrder("arn:alpha", "arn:zeta");
        var first = result.Value.Mappings[0];
        first.Uuid.Should().Be("uuid-a");
        first.EventSourceArn.Should().Be("arn:alpha");
        first.FunctionArn.Should().Be("arn:fn");
        first.State.Should().Be("Disabled");
        first.BatchSize.Should().Be(5);
        first.LastModified.Should().Be("2026-01-01T00:00:00Z");
        result.Value.S3Triggers.Select(_ => _.BucketArn)
            .Should().ContainInOrder("arn:aws:s3:::alpha-bucket", "arn:aws:s3:::zeta-bucket");
    }

    [Fact]
    public async Task Handle_WhenBucketNotificationTargetsFunction_IncludesTriggerEvenWhenPolicyHasNone()
    {
        // Arrange - the function policy lists no S3 triggers, but a bucket's notification
        // configuration targets the function by its full ARN. This is the case the user reported:
        // the bucket shows the notification while the lambda showed no event sources.
        IReadOnlyList<S3Bucket> buckets =
        [
            new("mango-orchard-uploads", "2026-01-01T00:00:00Z"),
            new("unrelated-bucket", "2026-01-01T00:00:00Z"),
        ];
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        _s3Client
            .GetBucketConfigurationAsync("mango-orchard-uploads", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(ConfigurationWith(
            [
                new("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:pineapple-weather-sync", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
            ]))));
        _s3Client
            .GetBucketConfigurationAsync("unrelated-bucket", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(ConfigurationWith(
            [
                new("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:some-other-function", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
                new("Queue", "arn:aws:sqs:us-east-1:000000000000:other-queue", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
            ]))));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("pineapple-weather-sync"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.S3Triggers.Select(_ => _.BucketArn)
            .Should().ContainSingle().Which.Should().Be("arn:aws:s3:::mango-orchard-uploads");
    }

    [Fact]
    public async Task Handle_WhenBucketNotificationTargetsFunctionByQualifiedArn_IncludesTrigger()
    {
        // Arrange - a versioned function ARN carries a trailing qualifier that must be ignored when
        // matching the function name.
        IReadOnlyList<S3Bucket> buckets = [new("alias-bucket", "2026-01-01T00:00:00Z")];
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        _s3Client
            .GetBucketConfigurationAsync("alias-bucket", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(ConfigurationWith(
            [
                new("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:orders:PROD", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
            ]))));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.S3Triggers.Select(_ => _.BucketArn).Should().ContainSingle().Which.Should().Be("arn:aws:s3:::alias-bucket");
    }

    [Fact]
    public async Task Handle_WhenBucketNotificationTargetsFunctionByBareName_IncludesTrigger()
    {
        // Arrange - some backends record the notification target as the bare function name rather
        // than a full ARN.
        IReadOnlyList<S3Bucket> buckets = [new("bare-name-bucket", "2026-01-01T00:00:00Z")];
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        _s3Client
            .GetBucketConfigurationAsync("bare-name-bucket", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(ConfigurationWith(
            [
                new("Lambda", "orders", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
            ]))));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.S3Triggers.Select(_ => _.BucketArn).Should().ContainSingle().Which.Should().Be("arn:aws:s3:::bare-name-bucket");
    }

    [Fact]
    public async Task Handle_WhenBucketNotificationTargetsAnotherFunction_ExcludesTrigger()
    {
        // Arrange
        IReadOnlyList<S3Bucket> buckets = [new("other-bucket", "2026-01-01T00:00:00Z")];
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        _s3Client
            .GetBucketConfigurationAsync("other-bucket", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(ConfigurationWith(
            [
                new("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:not-this-one", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
            ]))));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.S3Triggers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenLambdaNotificationTargetIsEmptyOrUnqualified_ExcludesTrigger()
    {
        // Arrange - a Lambda notification with an empty target ARN, and another whose target carries
        // no ":function:" segment and does not match the bare function name, are both ignored.
        IReadOnlyList<S3Bucket> buckets = [new("noisy-bucket", "2026-01-01T00:00:00Z")];
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        _s3Client
            .GetBucketConfigurationAsync("noisy-bucket", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(ConfigurationWith(
            [
                new("Lambda", string.Empty, ["s3:ObjectCreated:*"], string.Empty, string.Empty),
                new("Lambda", "not-an-arn-without-a-function-marker", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
            ]))));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.S3Triggers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSameBucketAppearsInPolicyAndNotification_DeduplicatesAndOrders()
    {
        // Arrange - the same bucket is discovered from both the function policy and the bucket
        // notification configuration; it must appear only once and be ordered with the others.
        IReadOnlyList<LambdaS3Trigger> policyTriggers =
        [
            new("arn:aws:s3:::shared-bucket"),
            new("arn:aws:s3:::zeta-bucket"),
        ];
        _client
            .ListS3TriggersAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(policyTriggers)));
        IReadOnlyList<S3Bucket> buckets =
        [
            new("shared-bucket", "2026-01-01T00:00:00Z"),
            new("alpha-bucket", "2026-01-01T00:00:00Z"),
        ];
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        _s3Client
            .GetBucketConfigurationAsync("shared-bucket", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(ConfigurationWith(
            [
                new("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:orders", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
            ]))));
        _s3Client
            .GetBucketConfigurationAsync("alpha-bucket", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(ConfigurationWith(
            [
                new("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:orders", ["s3:ObjectCreated:*"], string.Empty, string.Empty),
            ]))));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.S3Triggers.Select(_ => _.BucketArn).Should().ContainInOrder(
            "arn:aws:s3:::alpha-bucket",
            "arn:aws:s3:::shared-bucket",
            "arn:aws:s3:::zeta-bucket");
        result.Value.S3Triggers.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WhenClientFails_ReturnsError()
    {
        // Arrange
        _client
            .ListEventSourceMappingsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaEventSourceMapping>>>(new Error("list boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("list boom");
    }

    [Fact]
    public async Task Handle_WhenS3TriggerClientFails_ReturnsError()
    {
        // Arrange
        IReadOnlyList<LambdaEventSourceMapping> stored = [];
        _client
            .ListEventSourceMappingsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stored)));
        _client
            .ListS3TriggersAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaS3Trigger>>>(new Error("policy boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("policy boom");
    }

    [Fact]
    public async Task Handle_WhenListBucketsFails_ReturnsError()
    {
        // Arrange
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<S3Bucket>>>(new Error("buckets boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("buckets boom");
    }

    [Fact]
    public async Task Handle_WhenBucketConfigurationFails_ReturnsError()
    {
        // Arrange
        IReadOnlyList<S3Bucket> buckets = [new("broken-bucket", "2026-01-01T00:00:00Z")];
        _s3Client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        _s3Client
            .GetBucketConfigurationAsync("broken-bucket", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3BucketConfiguration>>(new Error("config boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("config boom");
    }
}
