using Foundation.Application.Health;
using Foundation.Application.Queries.GetHealth;
using Foundation.Domain.Health;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetHealth;

public class GetHealthQueryHandlerTests
{
    private readonly IHealthStatusProvider _provider = Substitute.For<IHealthStatusProvider>();

    [Fact]
    public async Task Handle_WhenInvoked_ReturnsCurrentSnapshotServices()
    {
        // Arrange
        var snapshot = new HealthStatus(
        [
            new ServiceHealth("s3", ServiceAvailability.Available),
            new ServiceHealth("lambda", ServiceAvailability.Unavailable),
        ]);
        _provider.GetCurrent().Returns(snapshot);
        var sut = new GetHealthQueryHandler(_provider, NullLogger<GetHealthQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetHealthQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Services.Should().BeEquivalentTo(snapshot.Services);
    }
}
