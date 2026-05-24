using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Foundation.Application.Queries.FilterLogEvents;
using Foundation.Domain.CloudWatchLogs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.FilterLogEvents;

public class FilterLogEventsQueryHandlerTests
{
    private readonly ICloudWatchLogsClient _client = Substitute.For<ICloudWatchLogsClient>();

    private FilterLogEventsQueryHandler CreateSut()
        => new(_client, NullLogger<FilterLogEventsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsEvents()
    {
        // Arrange
        var startTime = DateTimeOffset.UnixEpoch;
        IReadOnlyList<LogEvent> events =
        [
            new(DateTimeOffset.UnixEpoch, "hello"),
        ];
        _client
            .FilterLogEventsAsync("/aws/lambda/orders", "ERROR", startTime, 50, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(events)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new FilterLogEventsQuery("/aws/lambda/orders", "ERROR", startTime, 50),
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
            .FilterLogEventsAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LogEvent>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new FilterLogEventsQuery("/aws/lambda/orders", null, null, 50),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
