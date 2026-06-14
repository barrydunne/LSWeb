using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListS3ObjectVersions;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListS3ObjectVersions;

public class ListS3ObjectVersionsQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private ListS3ObjectVersionsQueryHandler CreateSut()
        => new(_client, NullLogger<ListS3ObjectVersionsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsVersions()
    {
        // Arrange
        IReadOnlyList<S3ObjectVersion> versions =
            [new("report.pdf", "v2", true, false, 1024, "2026-01-02T03:04:05Z")];
        _client
            .ListObjectVersionsAsync("docs", "report", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<S3ObjectVersion>>(versions)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListS3ObjectVersionsQuery("docs", "report"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Versions.Should().ContainSingle();
        result.Value.Versions[0].VersionId.Should().Be("v2");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListObjectVersionsAsync("docs", "report", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<S3ObjectVersion>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListS3ObjectVersionsQuery("docs", "report"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
