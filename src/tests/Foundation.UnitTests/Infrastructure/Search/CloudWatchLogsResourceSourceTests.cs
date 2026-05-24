using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Foundation.Domain.CloudWatchLogs;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class CloudWatchLogsResourceSourceTests
{
    private readonly ICloudWatchLogsClient _client = Substitute.For<ICloudWatchLogsClient>();

    private CloudWatchLogsResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsCloudWatchLogs()
        => CreateSut().ServiceKey.Should().Be("cloudwatch-logs");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsGroupsToSearchEntries()
    {
        // Arrange
        IReadOnlyList<LogGroup> groups =
        [
            new("/aws/lambda/orders", "arn:orders", 0, null, null),
        ];
        _client
            .ListLogGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(groups)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("cloudwatch-logs");
        entry.ResourceId.Should().Be("/aws/lambda/orders");
        entry.DisplayName.Should().Be("/aws/lambda/orders");
        entry.Route.Should().Be("/services/cloudwatch-logs/%2Faws%2Flambda%2Forders");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListLogGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LogGroup>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
