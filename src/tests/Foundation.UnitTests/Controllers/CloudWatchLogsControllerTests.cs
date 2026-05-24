using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateLogGroup;
using Foundation.Application.Commands.DeleteLogGroup;
using Foundation.Application.Queries.FilterLogEvents;
using Foundation.Application.Queries.GetLogEvents;
using Foundation.Application.Queries.ListLogGroups;
using Foundation.Application.Queries.ListLogStreams;
using Foundation.Domain.CloudWatchLogs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class CloudWatchLogsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<CloudWatchLogsController> _logger =
        Substitute.For<ILogger<CloudWatchLogsController>>();

    private CloudWatchLogsController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListGroups_WhenQuerySucceeds_ReturnsOkWithSummaries()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        IReadOnlyList<LogGroup> groups =
        [
            new("/aws/lambda/orders", "arn:orders", 1024, 7, createdAt),
        ];
        _sender
            .Send(Arg.Any<ListLogGroupsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLogGroupsQueryResult>>(
                new ListLogGroupsQueryResult(groups)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListGroups(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LogGroupListResponse>>().Subject;
        var summary = ok.Value!.LogGroups.Should().ContainSingle().Subject;
        summary.Name.Should().Be("/aws/lambda/orders");
        summary.Arn.Should().Be("arn:orders");
        summary.StoredBytes.Should().Be(1024);
        summary.RetentionInDays.Should().Be(7);
        summary.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task ListGroups_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListLogGroupsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLogGroupsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListGroups(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListStreams_WhenQuerySucceeds_ReturnsOkWithSummaries()
    {
        // Arrange
        var lastEvent = new DateTimeOffset(2024, 5, 6, 7, 8, 9, TimeSpan.Zero);
        IReadOnlyList<LogStream> streams =
        [
            new("stream-1", lastEvent),
        ];
        _sender
            .Send(Arg.Any<ListLogStreamsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLogStreamsQueryResult>>(
                new ListLogStreamsQueryResult(streams)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStreams("/aws/lambda/orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LogStreamListResponse>>().Subject;
        var summary = ok.Value!.LogStreams.Should().ContainSingle().Subject;
        summary.Name.Should().Be("stream-1");
        summary.LastEventTimestamp.Should().Be(lastEvent);
    }

    [Fact]
    public async Task ListStreams_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListLogStreamsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLogStreamsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStreams("/aws/lambda/orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetEvents_WhenQuerySucceeds_ReturnsOkWithEvents()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2024, 7, 8, 9, 10, 11, TimeSpan.Zero);
        IReadOnlyList<LogEvent> events =
        [
            new(timestamp, "hello"),
        ];
        _sender
            .Send(Arg.Any<GetLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLogEventsQueryResult>>(
                new GetLogEventsQueryResult(events)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetEvents(
            "/aws/lambda/orders", "stream-1", 50, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LogEventListResponse>>().Subject;
        var logEvent = ok.Value!.Events.Should().ContainSingle().Subject;
        logEvent.Timestamp.Should().Be(timestamp);
        logEvent.Message.Should().Be("hello");
    }

    [Fact]
    public async Task GetEvents_WhenLimitPositive_SendsRequestedLimit()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLogEventsQueryResult>>(
                new GetLogEventsQueryResult([])));
        var sut = CreateSut();

        // Act
        await sut.GetEvents("/aws/lambda/orders", "stream-1", 25, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<GetLogEventsQuery>(query => query.Limit == 25),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetEvents_WhenLimitNotPositive_DefaultsToHundred()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLogEventsQueryResult>>(
                new GetLogEventsQueryResult([])));
        var sut = CreateSut();

        // Act
        await sut.GetEvents("/aws/lambda/orders", "stream-1", 0, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<GetLogEventsQuery>(query => query.Limit == 100),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetEvents_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLogEventsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetEvents(
            "/aws/lambda/orders", "stream-1", 50, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task FilterEvents_WhenQuerySucceeds_ReturnsOkWithEvents()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2024, 7, 8, 9, 10, 11, TimeSpan.Zero);
        IReadOnlyList<LogEvent> events =
        [
            new(timestamp, "hello"),
        ];
        _sender
            .Send(Arg.Any<FilterLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<FilterLogEventsQueryResult>>(
                new FilterLogEventsQueryResult(events)));
        var sut = CreateSut();

        // Act
        var result = await sut.FilterEvents(
            "/aws/lambda/orders", "ERROR", 1_700_000_000_000, 50, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LogEventListResponse>>().Subject;
        var logEvent = ok.Value!.Events.Should().ContainSingle().Subject;
        logEvent.Timestamp.Should().Be(timestamp);
        logEvent.Message.Should().Be("hello");
    }

    [Fact]
    public async Task FilterEvents_WhenStartTimePositive_SendsConvertedStartTime()
    {
        // Arrange
        _sender
            .Send(Arg.Any<FilterLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<FilterLogEventsQueryResult>>(
                new FilterLogEventsQueryResult([])));
        var sut = CreateSut();

        // Act
        await sut.FilterEvents(
            "/aws/lambda/orders", "ERROR", 1_700_000_000_000, 25, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<FilterLogEventsQuery>(query =>
                query.Limit == 25
                && query.FilterPattern == "ERROR"
                && query.StartTime == DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FilterEvents_WhenStartTimeNotPositive_SendsNullStartTimeAndDefaultLimit()
    {
        // Arrange
        _sender
            .Send(Arg.Any<FilterLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<FilterLogEventsQueryResult>>(
                new FilterLogEventsQueryResult([])));
        var sut = CreateSut();

        // Act
        await sut.FilterEvents(
            "/aws/lambda/orders", null, 0, 0, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<FilterLogEventsQuery>(query =>
                query.Limit == 100
                && query.FilterPattern == null
                && query.StartTime == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FilterEvents_WhenStartTimeNull_SendsNullStartTime()
    {
        // Arrange
        _sender
            .Send(Arg.Any<FilterLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<FilterLogEventsQueryResult>>(
                new FilterLogEventsQueryResult([])));
        var sut = CreateSut();

        // Act
        await sut.FilterEvents(
            "/aws/lambda/orders", "ERROR", null, 10, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<FilterLogEventsQuery>(query => query.StartTime == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FilterEvents_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<FilterLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<FilterLogEventsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.FilterEvents(
            "/aws/lambda/orders", "ERROR", 1_700_000_000_000, 50, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateGroup_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateLogGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateGroup(
            new LogGroupCreateRequest("/app/orders"), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created>();
        await _sender.Received(1).Send(
            Arg.Is<CreateLogGroupCommand>(command => command.LogGroupName == "/app/orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateGroup_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateLogGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateGroup(
            new LogGroupCreateRequest("/app/orders"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteGroup_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteLogGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteGroup("/app/orders", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteLogGroupCommand>(command => command.LogGroupName == "/app/orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteGroup_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteLogGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteGroup("/app/orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
