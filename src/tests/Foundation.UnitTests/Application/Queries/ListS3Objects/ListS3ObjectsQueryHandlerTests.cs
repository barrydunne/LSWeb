using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListS3Objects;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListS3Objects;

public class ListS3ObjectsQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private ListS3ObjectsQueryHandler CreateSut()
        => new(_client, NullLogger<ListS3ObjectsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsPrefixesAndObjects()
    {
        // Arrange
        var listing = new S3ObjectListing(
            ["orders/2026/"],
            [new S3Object("orders/readme.txt", 12, "2026-01-02T03:04:05.0000000Z")]);
        _client
            .ListObjectsAsync("data", "orders/", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(listing)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListS3ObjectsQuery("data", "orders/"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Prefixes.Should().ContainSingle(_ => _ == "orders/2026/");
        result.Value.Objects.Should().ContainSingle(_ => _.Key == "orders/readme.txt");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListObjectsAsync("data", string.Empty, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectListing>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListS3ObjectsQuery("data", string.Empty), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
