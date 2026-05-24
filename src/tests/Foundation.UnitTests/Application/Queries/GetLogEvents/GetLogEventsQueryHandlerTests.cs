using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Foundation.Application.Queries.GetLogEvents;
using Foundation.Domain.CloudWatchLogs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetLogEvents;

public class GetLogEventsQueryHandlerTests
{
    private readonly ICloudWatchLogsClient _client = Substitute.For<ICloudWatchLogsClient>();

    private GetLogEventsQueryHandler CreateSut()
        => new(_client, NullLogger<GetLogEventsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsEvents()
    {
        // Arrange
        IReadOnlyList<LogEvent> events =
        [
            new(DateTimeOffset.UnixEpoch, "hello"),
        ];
        _client
            .GetLogEventsAsync("/aws/lambda/orders", "stream-1", 50, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(events)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLogEventsQuery("/aws/lambda/orders", "stream-1", 50),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Events.Should().ContainSingle(_ => _.Message == "hello");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetLogEventsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LogEvent>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLogEventsQuery("/aws/lambda/orders", "stream-1", 50),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
