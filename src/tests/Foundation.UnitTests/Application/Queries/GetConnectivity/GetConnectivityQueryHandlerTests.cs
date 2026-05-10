using Foundation.Application.Configuration;
using Foundation.Application.Connectivity;
using Foundation.Application.Queries.GetConnectivity;
using Foundation.Domain.Configuration;
using Foundation.Domain.Connectivity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetConnectivity;

public class GetConnectivityQueryHandlerTests
{
    private readonly IConfigProvider _configProvider = Substitute.For<IConfigProvider>();
    private readonly IConnectivityProbe _connectivityProbe = Substitute.For<IConnectivityProbe>();

    public GetConnectivityQueryHandlerTests()
        => _configProvider.GetSnapshot().Returns(new ConfigSnapshot(
            new ConfigValue("AccessKey", "test", ConfigSource.Default, IsSensitive: true),
            new ConfigValue("SecretKey", "test", ConfigSource.Default, IsSensitive: true),
            new ConfigValue("ServiceUrl", "http://localhost:4566", ConfigSource.Default, IsSensitive: false),
            new ConfigValue("Region", "eu-west-1", ConfigSource.Default, IsSensitive: false)));

    private GetConnectivityQueryHandler CreateSut()
        => new(_configProvider, _connectivityProbe, NullLogger<GetConnectivityQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenBackendReachable_ReturnsConnectedState()
    {
        // Arrange
        _connectivityProbe
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new ConnectivityProbeResult(true, null));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetConnectivityQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Connection.Status.Should().Be(ConnectivityStatus.Connected);
        result.Value.Connection.Endpoint.Should().Be("http://localhost:4566");
        result.Value.Connection.Region.Should().Be("eu-west-1");
        result.Value.Connection.Error.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenBackendUnreachable_ReturnsDisconnectedStateWithError()
    {
        // Arrange
        _connectivityProbe
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new ConnectivityProbeResult(false, "connection refused"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetConnectivityQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Connection.Status.Should().Be(ConnectivityStatus.Disconnected);
        result.Value.Connection.Endpoint.Should().Be("http://localhost:4566");
        result.Value.Connection.Region.Should().Be("eu-west-1");
        result.Value.Connection.Error.Should().Be("connection refused");
    }
}
