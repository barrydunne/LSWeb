using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Navigation;
using Foundation.Application.Queries.ResolveReference;
using Foundation.Domain.Navigation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ResolveReference;

public class ResolveReferenceQueryHandlerTests
{
    private readonly IReferenceResolver _resolver = Substitute.For<IReferenceResolver>();

    [Fact]
    public async Task Handle_WhenReferenceResolves_ReturnsResolvedRoute()
    {
        // Arrange
        _resolver
            .Resolve("arn:aws:sqs:eu-west-1:000000000000:orders", null)
            .Returns(new ResourceReference("sqs", "orders", "/services/sqs/orders"));
        var sut = new ResolveReferenceQueryHandler(_resolver, NullLogger<ResolveReferenceQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(
            new ResolveReferenceQuery("arn:aws:sqs:eu-west-1:000000000000:orders", null),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ServiceKey.Should().Be("sqs");
        result.Value.ResourceId.Should().Be("orders");
        result.Value.Route.Should().Be("/services/sqs/orders");
    }

    [Fact]
    public async Task Handle_WhenReferenceCannotBeResolved_ReturnsError()
    {
        // Arrange
        _resolver
            .Resolve("mystery-id", "mystery")
            .Returns(new Error("Unsupported service 'mystery'."));
        var sut = new ResolveReferenceQueryHandler(_resolver, NullLogger<ResolveReferenceQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(
            new ResolveReferenceQuery("mystery-id", "mystery"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("Unsupported service 'mystery'.");
    }
}
