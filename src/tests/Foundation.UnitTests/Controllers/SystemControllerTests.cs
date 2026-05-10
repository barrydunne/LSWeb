using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Queries.GetConnectivity;
using Foundation.Application.Queries.GetLiveness;
using Foundation.Domain.Connectivity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SystemControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SystemController> _logger = Substitute.For<ILogger<SystemController>>();

    [Fact]
    public async Task Health_WhenQuerySucceeds_ReturnsOkWithStatus()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLivenessQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLivenessQueryResult>>(new GetLivenessQueryResult("Healthy")));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Health(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<GetLivenessQueryResult>>().Subject;
        ok.Value!.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task Health_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLivenessQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLivenessQueryResult>>(new InvalidOperationException("boom")));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Health(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Connectivity_WhenBackendConnected_ReturnsOkWithMaskedDetails()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetConnectivityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetConnectivityQueryResult>>(new GetConnectivityQueryResult(
                new ConnectionState(ConnectivityStatus.Connected, "http://localhost:4566", "eu-west-1", null))));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Connectivity(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ConnectivityResponse>>().Subject;
        ok.Value!.Status.Should().Be("Connected");
        ok.Value.Endpoint.Should().Be("http://localhost:4566");
        ok.Value.Region.Should().Be("eu-west-1");
        ok.Value.Error.Should().BeNull();
    }

    [Fact]
    public async Task Connectivity_WhenBackendDisconnected_ReturnsOkWithError()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetConnectivityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetConnectivityQueryResult>>(new GetConnectivityQueryResult(
                new ConnectionState(ConnectivityStatus.Disconnected, "http://localhost:4566", "eu-west-1", "connection refused"))));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Connectivity(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ConnectivityResponse>>().Subject;
        ok.Value!.Status.Should().Be("Disconnected");
        ok.Value.Error.Should().Be("connection refused");
    }

    [Fact]
    public async Task Connectivity_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetConnectivityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetConnectivityQueryResult>>(new InvalidOperationException("boom")));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Connectivity(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
