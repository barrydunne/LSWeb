using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Foundation.Application.Queries.ListLogStreams;
using Foundation.Domain.CloudWatchLogs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListLogStreams;

public class ListLogStreamsQueryHandlerTests
{
    private readonly ICloudWatchLogsClient _client = Substitute.For<ICloudWatchLogsClient>();

    private ListLogStreamsQueryHandler CreateSut()
        => new(_client, NullLogger<ListLogStreamsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStreams()
    {
        // Arrange
        IReadOnlyList<LogStream> streams =
        [
            new("stream-1", null),
        ];
        _client
            .ListLogStreamsAsync("/aws/lambda/orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(streams)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLogStreamsQuery("/aws/lambda/orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.LogStreams.Should().ContainSingle(_ => _.Name == "stream-1");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListLogStreamsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LogStream>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLogStreamsQuery("/aws/lambda/orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
