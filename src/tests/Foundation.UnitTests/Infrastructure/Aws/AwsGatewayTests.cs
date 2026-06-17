using System.Net;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Foundation.Domain.Capabilities;
using Foundation.Domain.Resilience;
using Foundation.Infrastructure.Aws;
using Foundation.Infrastructure.Capabilities;
using Foundation.Infrastructure.Configuration;
using Foundation.Infrastructure.Errors;
using Foundation.Infrastructure.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.CircuitBreaker;

namespace Foundation.UnitTests.Infrastructure.Aws;

public class AwsGatewayTests
{
    private const string ServiceKey = "s3";

    private static (AwsGateway Sut, CapabilityDetector Detector, CircuitBreakerMonitor Monitor) CreateSut()
    {
        var provider = new ConfigProvider(new AwsSettings { ServiceUrl = "http://localhost:4566", Region = "eu-west-1" });
        var factory = new AwsClientFactory(provider);
        var detector = new CapabilityDetector();
        var monitor = new CircuitBreakerMonitor();
        var sut = new AwsGateway(factory, new ErrorTranslator(), detector, monitor, NullLogger<AwsGateway>.Instance);
        return (sut, detector, monitor);
    }

    private static CapabilityStatus StatusFor(CapabilityDetector detector, string serviceKey)
        => detector.GetCapabilities().Find(serviceKey)!.Status;

    [Fact]
    public async Task ExecuteAsync_WhenOperationSucceeds_ReturnsResultValue()
    {
        // Arrange
        var (sut, detector, _) = CreateSut();

        // Act
        var result = await sut.ExecuteAsync<AmazonSecurityTokenServiceClient, int>(
            ServiceKey,
            (_, _) => Task.FromResult(42),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        StatusFor(detector, ServiceKey).Should().Be(CapabilityStatus.Supported);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperationFailsThenSucceeds_RetriesAndReturnsValue()
    {
        // Arrange
        var (sut, detector, _) = CreateSut();
        var attempts = 0;

        // Act
        var result = await sut.ExecuteAsync<AmazonSecurityTokenServiceClient, int>(
            ServiceKey,
            (_, _) =>
            {
                attempts++;
                return attempts < 3
                    ? throw new InvalidOperationException("transient")
                    : Task.FromResult(7);
            },
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
        attempts.Should().Be(3);
        StatusFor(detector, ServiceKey).Should().Be(CapabilityStatus.Supported);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperationAlwaysFails_ReturnsFriendlyFailureAfterMaxAttempts()
    {
        // Arrange
        var (sut, _, _) = CreateSut();
        var attempts = 0;

        // Act
        var result = await sut.ExecuteAsync<AmazonSecurityTokenServiceClient, int>(
            ServiceKey,
            (_, _) =>
            {
                attempts++;
                throw new InvalidOperationException("permanent");
            },
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("An unexpected error occurred.");
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBackendDoesNotSupportTheOperation_RecordsUnsupportedAndReturnsFriendlyError()
    {
        // Arrange
        var (sut, detector, _) = CreateSut();

        // Act
        var result = await sut.ExecuteAsync<AmazonSecurityTokenServiceClient, int>(
            ServiceKey,
            (_, _) => throw new AmazonServiceException("Operation is not implemented")
            {
                StatusCode = HttpStatusCode.NotImplemented,
            },
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("This service or operation is not supported by the current backend.");
        StatusFor(detector, ServiceKey).Should().Be(CapabilityStatus.Unsupported);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_PropagatesCancellation()
    {
        // Arrange
        var (sut, _, _) = CreateSut();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => sut.ExecuteAsync<AmazonSecurityTokenServiceClient, int>(
            ServiceKey,
            (_, _) => Task.FromResult(1),
            cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCircuitIsOpen_RecordsTheServiceAsSuspended()
    {
        // Arrange
        var (sut, _, monitor) = CreateSut();

        // Act
        var result = await sut.ExecuteAsync<AmazonSecurityTokenServiceClient, int>(
            ServiceKey,
            (_, _) => throw new BrokenCircuitException("The circuit is now open and is not allowing calls."),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        var status = monitor.GetStatus();
        status.IsOpen.Should().BeTrue();
        status.AffectedServices.Should().ContainSingle().Which.Should().Be(ServiceKey);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAServiceRecoversAfterBeingSuspended_ClearsTheSuspendedState()
    {
        // Arrange
        var (sut, _, monitor) = CreateSut();
        await sut.ExecuteAsync<AmazonSecurityTokenServiceClient, int>(
            ServiceKey,
            (_, _) => throw new BrokenCircuitException("The circuit is now open and is not allowing calls."),
            TestContext.Current.CancellationToken);
        monitor.GetStatus().IsOpen.Should().BeTrue();

        // Act
        var result = await sut.ExecuteAsync<AmazonSecurityTokenServiceClient, int>(
            ServiceKey,
            (_, _) => Task.FromResult(1),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var status = monitor.GetStatus();
        status.IsOpen.Should().BeFalse();
        status.AffectedServices.Should().BeEmpty();
    }
}
