using Foundation.Application.Commands.ResetCircuitBreaker;
using Foundation.Application.Resilience;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ResetCircuitBreaker;

public class ResetCircuitBreakerCommandHandlerTests
{
    private readonly ICircuitBreakerReset _reset = Substitute.For<ICircuitBreakerReset>();

    [Fact]
    public async Task Handle_WhenInvoked_ResetsTheCircuitBreakerAndSucceeds()
    {
        // Arrange
        var sut = new ResetCircuitBreakerCommandHandler(_reset, NullLogger<ResetCircuitBreakerCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new ResetCircuitBreakerCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _reset.Received(1).ResetAsync(Arg.Any<CancellationToken>());
    }
}
