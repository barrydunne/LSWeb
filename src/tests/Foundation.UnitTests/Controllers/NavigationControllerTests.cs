using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Queries.ResolveReference;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class NavigationControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<NavigationController> _logger = Substitute.For<ILogger<NavigationController>>();

    [Fact]
    public async Task Resolve_WhenQuerySucceeds_ReturnsOkWithResolvedRoute()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ResolveReferenceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ResolveReferenceQueryResult>>(
                new ResolveReferenceQueryResult("sqs", "orders", "/services/sqs/orders")));
        var sut = new NavigationController(_sender, _logger);

        // Act
        var result = await sut.Resolve(
            "arn:aws:sqs:eu-west-1:000000000000:orders",
            null,
            TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ResolveReferenceResponse>>().Subject;
        ok.Value!.ServiceKey.Should().Be("sqs");
        ok.Value.ResourceId.Should().Be("orders");
        ok.Value.Route.Should().Be("/services/sqs/orders");
    }

    [Fact]
    public async Task Resolve_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ResolveReferenceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ResolveReferenceQueryResult>>(new Error("Unsupported service 'mystery'.")));
        var sut = new NavigationController(_sender, _logger);

        // Act
        var result = await sut.Resolve("mystery-id", "mystery", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
