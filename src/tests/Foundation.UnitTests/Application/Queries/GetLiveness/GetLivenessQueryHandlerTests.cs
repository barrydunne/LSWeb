using Foundation.Application.Queries.GetLiveness;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetLiveness;

public class GetLivenessQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenInvoked_ReturnsHealthyStatus()
    {
        // Arrange
        var sut = new GetLivenessQueryHandler(NullLogger<GetLivenessQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetLivenessQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Healthy");
    }
}
