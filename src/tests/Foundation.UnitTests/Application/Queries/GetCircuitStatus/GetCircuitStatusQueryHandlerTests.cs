using Foundation.Application.Queries.GetCircuitStatus;
using Foundation.Application.Resilience;
using Foundation.Domain.Resilience;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetCircuitStatus;

public class GetCircuitStatusQueryHandlerTests
{
    private readonly ICircuitBreakerStateProvider _provider = Substitute.For<ICircuitBreakerStateProvider>();

    [Fact]
    public async Task Handle_WhenCircuitIsOpen_ReturnsOpenStatusWithAffectedServices()
    {
        // Arrange
        _provider.GetStatus().Returns(new CircuitStatus(true, ["s3", "sqs"]));
        var sut = new GetCircuitStatusQueryHandler(_provider, NullLogger<GetCircuitStatusQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetCircuitStatusQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsOpen.Should().BeTrue();
        result.Value.AffectedServices.Should().Equal("s3", "sqs");
    }

    [Fact]
    public async Task Handle_WhenCircuitIsClosed_ReturnsClosedStatusWithNoServices()
    {
        // Arrange
        _provider.GetStatus().Returns(new CircuitStatus(false, []));
        var sut = new GetCircuitStatusQueryHandler(_provider, NullLogger<GetCircuitStatusQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetCircuitStatusQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsOpen.Should().BeFalse();
        result.Value.AffectedServices.Should().BeEmpty();
    }
}
