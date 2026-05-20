using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class S3ResourceSourceTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private S3ResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsS3()
        => CreateSut().ServiceKey.Should().Be("s3");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsBucketsToSearchEntries()
    {
        // Arrange
        IReadOnlyList<S3Bucket> buckets =
        [
            new("orders", "2026-01-02T03:04:05.0000000Z"),
            new("invoices", "2026-01-03T03:04:05.0000000Z"),
        ];
        _client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buckets)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().HaveCount(2);
        entries[0].ServiceKey.Should().Be("s3");
        entries[0].ResourceId.Should().Be("orders");
        entries[0].DisplayName.Should().Be("orders");
        entries[0].Route.Should().Be("/services/s3/orders");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<S3Bucket>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
