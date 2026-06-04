using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Queries.ListHostedZones;
using Foundation.Domain.Route53;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class Route53ControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<Route53Controller> _logger =
        Substitute.For<ILogger<Route53Controller>>();

    private Route53Controller CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListHostedZones_WhenQuerySucceeds_ReturnsOkWithHostedZones()
    {
        // Arrange
        IReadOnlyList<HostedZone> hostedZones =
        [
            new("/hostedzone/Z123", "example.com.", 4, true),
        ];
        _sender
            .Send(Arg.Any<ListHostedZonesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHostedZonesQueryResult>>(
                new ListHostedZonesQueryResult(hostedZones)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListHostedZones(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HostedZoneListResponse>>().Subject;
        var zone = ok.Value!.HostedZones.Should().ContainSingle().Subject;
        zone.Id.Should().Be("/hostedzone/Z123");
        zone.Name.Should().Be("example.com.");
        zone.RecordCount.Should().Be(4);
        zone.PrivateZone.Should().BeTrue();
    }

    [Fact]
    public async Task ListHostedZones_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListHostedZonesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHostedZonesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListHostedZones(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
