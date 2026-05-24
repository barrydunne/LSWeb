using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Foundation.Application.Queries.ListLogGroups;
using Foundation.Domain.CloudWatchLogs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListLogGroups;

public class ListLogGroupsQueryHandlerTests
{
    private readonly ICloudWatchLogsClient _client = Substitute.For<ICloudWatchLogsClient>();

    private ListLogGroupsQueryHandler CreateSut()
        => new(_client, NullLogger<ListLogGroupsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsGroups()
    {
        // Arrange
        IReadOnlyList<LogGroup> groups =
        [
            new("/aws/lambda/orders", "arn:orders", 1024, 7, null),
        ];
        _client
            .ListLogGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(groups)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListLogGroupsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.LogGroups.Should().ContainSingle(_ => _.Name == "/aws/lambda/orders");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListLogGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LogGroup>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListLogGroupsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
