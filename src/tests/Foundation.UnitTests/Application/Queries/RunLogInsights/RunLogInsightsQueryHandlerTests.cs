using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Foundation.Application.Queries.RunLogInsights;
using Foundation.Domain.CloudWatchLogs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.RunLogInsights;

public class RunLogInsightsQueryHandlerTests
{
    private readonly ICloudWatchLogsClient _client = Substitute.For<ICloudWatchLogsClient>();

    private RunLogInsightsQueryHandler CreateSut()
        => new(_client, NullLogger<RunLogInsightsQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsResult()
    {
        // Arrange
        var start = DateTimeOffset.UnixEpoch;
        var end = DateTimeOffset.UnixEpoch.AddHours(1);
        var insights = new LogInsightsResult(
            "Complete",
            [new LogInsightsRow([new LogInsightsField("@message", "hello")])],
            3,
            10);
        _client
            .RunInsightsQueryAsync("/app/orders", "fields @message", start, end, 100, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LogInsightsResult>>(insights));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new RunLogInsightsQuery("/app/orders", "fields @message", start, end, 100),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Result.Status.Should().Be("Complete");
        result.Value.Result.Rows.Should().ContainSingle();
        result.Value.Result.RecordsMatched.Should().Be(3);
        result.Value.Result.RecordsScanned.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .RunInsightsQueryAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LogInsightsResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new RunLogInsightsQuery("/app/orders", "fields @message", DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, 100),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
