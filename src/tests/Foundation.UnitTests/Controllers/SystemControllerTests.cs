using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.RefreshCatalogue;
using Foundation.Application.Queries.GetActivity;
using Foundation.Application.Queries.GetCatalogue;
using Foundation.Application.Queries.GetConnectivity;
using Foundation.Application.Queries.GetHealth;
using Foundation.Application.Queries.GetLiveness;
using Foundation.Domain.Activity;
using Foundation.Domain.Capabilities;
using Foundation.Domain.Catalogue;
using Foundation.Domain.Connectivity;
using Foundation.Domain.Health;
using Foundation.Domain.Streaming;
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
    public async Task Liveness_WhenQuerySucceeds_ReturnsOkWithStatus()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLivenessQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLivenessQueryResult>>(new GetLivenessQueryResult("Healthy")));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Liveness(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<GetLivenessQueryResult>>().Subject;
        ok.Value!.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task Liveness_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLivenessQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLivenessQueryResult>>(new InvalidOperationException("boom")));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Liveness(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Health_WhenQuerySucceeds_ReturnsOkWithSnapshot()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetHealthQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHealthQueryResult>>(new GetHealthQueryResult(
            [
                new ServiceHealth("s3", ServiceAvailability.Available),
                new ServiceHealth("lambda", ServiceAvailability.Unavailable),
            ])));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Health(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HealthResponse>>().Subject;
        ok.Value!.Services.Should().HaveCount(2);
        ok.Value.Services[0].Key.Should().Be("s3");
        ok.Value.Services[0].Availability.Should().Be("Available");
        ok.Value.Services[1].Key.Should().Be("lambda");
        ok.Value.Services[1].Availability.Should().Be("Unavailable");
    }

    [Fact]
    public async Task Health_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetHealthQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHealthQueryResult>>(new InvalidOperationException("boom")));
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

    [Fact]
    public async Task Catalogue_WhenQuerySucceeds_ReturnsOkWithMappedServices()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetCatalogueQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetCatalogueQueryResult>>(new GetCatalogueQueryResult(
            [
                new ServiceDescriptor("s3", "S3", ServiceCategory.Storage, "archive", "/services/s3"),
            ],
            CapabilityMap.Empty)));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Catalogue(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CatalogueResponse>>().Subject;
        var service = ok.Value!.Services.Should().ContainSingle().Subject;
        service.Key.Should().Be("s3");
        service.DisplayName.Should().Be("S3");
        service.Category.Should().Be("Storage");
        service.IconHint.Should().Be("archive");
        service.Route.Should().Be("/services/s3");
        service.Supported.Should().BeTrue();
        service.SupportDetail.Should().BeNull();
    }

    [Fact]
    public async Task Catalogue_WhenServiceIsUnsupported_FlagsItAsNonActionable()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetCatalogueQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetCatalogueQueryResult>>(new GetCatalogueQueryResult(
            [
                new ServiceDescriptor("s3", "S3", ServiceCategory.Storage, "archive", "/services/s3"),
            ],
            new CapabilityMap(
            [
                new CapabilityEntry("s3", CapabilityStatus.Unsupported, "Not supported by the current backend."),
            ]))));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Catalogue(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CatalogueResponse>>().Subject;
        var service = ok.Value!.Services.Should().ContainSingle().Subject;
        service.Supported.Should().BeFalse();
        service.SupportDetail.Should().Be("Not supported by the current backend.");
    }

    [Fact]
    public async Task Catalogue_WhenCapabilityKnownAndSupported_FlagsItAsActionable()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetCatalogueQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetCatalogueQueryResult>>(new GetCatalogueQueryResult(
            [
                new ServiceDescriptor("s3", "S3", ServiceCategory.Storage, "archive", "/services/s3"),
            ],
            new CapabilityMap(
            [
                new CapabilityEntry("s3", CapabilityStatus.Supported, null),
            ]))));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Catalogue(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CatalogueResponse>>().Subject;
        var service = ok.Value!.Services.Should().ContainSingle().Subject;
        service.Supported.Should().BeTrue();
        service.SupportDetail.Should().BeNull();
    }

    [Fact]
    public async Task Catalogue_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetCatalogueQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetCatalogueQueryResult>>(new InvalidOperationException("boom")));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Catalogue(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RefreshCatalogue_WhenCommandSucceeds_ReturnsAccepted()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RefreshCatalogueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.RefreshCatalogue(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status202Accepted);
    }

    [Fact]
    public async Task RefreshCatalogue_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RefreshCatalogueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new InvalidOperationException("boom")));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.RefreshCatalogue(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Activity_WhenQuerySucceeds_ReturnsOkWithMappedEntries()
    {
        // Arrange
        var occurredAt = DateTimeOffset.UtcNow;
        _sender
            .Send(Arg.Any<GetActivityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetActivityQueryResult>>(new GetActivityQueryResult(
            [
                new ActivityEntry("op-1", "catalogue-refresh", OperationState.Succeeded, "Service catalogue refreshed.", occurredAt),
            ])));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Activity(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ActivityResponse>>().Subject;
        var entry = ok.Value!.Entries.Should().ContainSingle().Subject;
        entry.OperationId.Should().Be("op-1");
        entry.Operation.Should().Be("catalogue-refresh");
        entry.State.Should().Be("Succeeded");
        entry.Message.Should().Be("Service catalogue refreshed.");
        entry.OccurredAt.Should().Be(occurredAt);
    }

    [Fact]
    public async Task Activity_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetActivityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetActivityQueryResult>>(new InvalidOperationException("boom")));
        var sut = new SystemController(_sender, _logger);

        // Act
        var result = await sut.Activity(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
