using Foundation.Application.Configuration;
using Foundation.Application.Connectivity;
using Foundation.Application.Diagnostics;
using Foundation.Application.Queries.GetDiagnostics;
using Foundation.Domain.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetDiagnostics;

public class GetDiagnosticsQueryHandlerTests
{
    private readonly IConfigProvider _configProvider = Substitute.For<IConfigProvider>();
    private readonly IConnectivityProbe _connectivityProbe = Substitute.For<IConnectivityProbe>();
    private readonly IRedactionService _redactionService = Substitute.For<IRedactionService>();

    public GetDiagnosticsQueryHandlerTests()
    {
        _configProvider.GetSnapshot().Returns(new ConfigSnapshot(
            new ConfigValue("AccessKey", "live-access", ConfigSource.EnvironmentVariable, IsSensitive: true),
            new ConfigValue("SecretKey", "live-secret", ConfigSource.EnvironmentVariable, IsSensitive: true),
            new ConfigValue("ServiceUrl", "http://localhost:4566", ConfigSource.Default, IsSensitive: false),
            new ConfigValue("Region", "eu-west-1", ConfigSource.Default, IsSensitive: false)));

        _redactionService
            .Resolve(Arg.Any<ConfigValue>(), Arg.Any<bool>())
            .Returns(call => call.Arg<ConfigValue>().Display);
    }

    private GetDiagnosticsQueryHandler CreateSut()
        => new(_configProvider, _connectivityProbe, _redactionService, NullLogger<GetDiagnosticsQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenBackendReachable_ReturnsConnectedWithoutError()
    {
        // Arrange
        _connectivityProbe
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new ConnectivityProbeResult(true, null));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetDiagnosticsQuery(Reveal: false), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ConnectivityStatus.Should().Be("Connected");
        result.Value.ConnectivityError.Should().BeNull();
        result.Value.Endpoint.Should().Be("http://localhost:4566");
        result.Value.Region.Should().Be("eu-west-1");
    }

    [Fact]
    public async Task Handle_WhenBackendUnreachable_ReturnsDisconnectedWithError()
    {
        // Arrange
        _connectivityProbe
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new ConnectivityProbeResult(false, "connection refused"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetDiagnosticsQuery(Reveal: false), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ConnectivityStatus.Should().Be("Disconnected");
        result.Value.ConnectivityError.Should().Be("connection refused");
    }

    [Fact]
    public async Task Handle_BuildsConfigurationFromSnapshotWithSourceAndSensitivity()
    {
        // Arrange
        _connectivityProbe
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new ConnectivityProbeResult(true, null));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetDiagnosticsQuery(Reveal: false), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Configuration.Should().HaveCount(4);
        result.Value.Configuration[0].Should().BeEquivalentTo(
            new DiagnosticsConfigValue("AccessKey", "********", "EnvironmentVariable", true));
        result.Value.Configuration[2].Should().BeEquivalentTo(
            new DiagnosticsConfigValue("ServiceUrl", "http://localhost:4566", "Default", false));
    }

    [Fact]
    public async Task Handle_PassesRevealRequestToRedactionService()
    {
        // Arrange
        _connectivityProbe
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new ConnectivityProbeResult(true, null));
        var sut = CreateSut();

        // Act
        await sut.Handle(new GetDiagnosticsQuery(Reveal: true), TestContext.Current.CancellationToken);

        // Assert
        _redactionService.Received().Resolve(Arg.Any<ConfigValue>(), true);
    }

    [Fact]
    public async Task Handle_ReflectsRevealAllowedFromRedactionService()
    {
        // Arrange
        _connectivityProbe
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new ConnectivityProbeResult(true, null));
        _redactionService.CanReveal.Returns(true);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetDiagnosticsQuery(Reveal: false), TestContext.Current.CancellationToken);

        // Assert
        result.Value.RevealAllowed.Should().BeTrue();
    }
}
