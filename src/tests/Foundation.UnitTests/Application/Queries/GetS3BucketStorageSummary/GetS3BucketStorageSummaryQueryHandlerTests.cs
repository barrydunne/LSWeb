using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetS3BucketStorageSummary;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetS3BucketStorageSummary;

public class GetS3BucketStorageSummaryQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private GetS3BucketStorageSummaryQueryHandler CreateSut()
        => new(_client, NullLogger<GetS3BucketStorageSummaryQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSummarySucceeds_ReturnsCounts()
    {
        // Arrange
        var summary = new S3BucketStorageSummary(7, 4096);
        _client
            .GetBucketStorageSummaryAsync("data", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3BucketStorageSummary>>(summary));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetS3BucketStorageSummaryQuery("data"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ObjectCount.Should().Be(7);
        result.Value.TotalSizeBytes.Should().Be(4096);
        await _client.Received(1).GetBucketStorageSummaryAsync("data", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSummaryFails_ReturnsError()
    {
        // Arrange
        _client
            .GetBucketStorageSummaryAsync("data", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3BucketStorageSummary>>(new Error("summary boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetS3BucketStorageSummaryQuery("data"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("summary boom");
    }
}
