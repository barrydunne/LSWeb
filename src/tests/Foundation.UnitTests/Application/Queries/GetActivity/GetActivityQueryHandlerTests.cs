using Foundation.Application.Activity;
using Foundation.Application.Queries.GetActivity;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetActivity;

public class GetActivityQueryHandlerTests
{
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    [Fact]
    public async Task Handle_WhenInvoked_ReturnsRecordedEntries()
    {
        // Arrange
        var entries = new List<ActivityEntry>
        {
            new("op-2", "catalogue-refresh", OperationState.Succeeded, "Service catalogue refreshed.", DateTimeOffset.UtcNow),
            new("op-1", "catalogue-refresh", OperationState.Succeeded, "Service catalogue refreshed.", DateTimeOffset.UtcNow.AddMinutes(-1)),
        };
        _activityLog.GetEntries().Returns(entries);
        var sut = new GetActivityQueryHandler(_activityLog, NullLogger<GetActivityQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetActivityQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Entries.Should().BeEquivalentTo(entries);
    }
}
