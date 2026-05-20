using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListS3Buckets;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListS3Buckets;

public class ListS3BucketsQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private ListS3BucketsQueryHandler CreateSut()
        => new(_client, NullLogger<ListS3BucketsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsBuckets()
    {
        // Arrange
        IReadOnlyList<S3Bucket> buckets =
        [
            new("orders", "2026-01-02T03:04:05.0000000Z"),
        ];
        _client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListS3BucketsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Buckets.Should().ContainSingle(_ => _.Name == "orders");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<S3Bucket>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListS3BucketsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
