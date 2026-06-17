using Foundation.Infrastructure.Resilience;

namespace Foundation.UnitTests.Infrastructure.Resilience;

public class CircuitBreakerMonitorTests
{
    [Fact]
    public void GetStatus_WhenNothingRecorded_ReportsClosed()
    {
        // Arrange
        var sut = new CircuitBreakerMonitor();

        // Act
        var status = sut.GetStatus();

        // Assert
        status.IsOpen.Should().BeFalse();
        status.AffectedServices.Should().BeEmpty();
    }

    [Fact]
    public void GetStatus_WhenServicesSuspended_ReportsOpenWithSortedServices()
    {
        // Arrange
        var sut = new CircuitBreakerMonitor();
        sut.RecordSuspended("sqs");
        sut.RecordSuspended("lambda");
        sut.RecordSuspended("sqs");

        // Act
        var status = sut.GetStatus();

        // Assert
        status.IsOpen.Should().BeTrue();
        status.AffectedServices.Should().Equal("lambda", "sqs");
    }

    [Fact]
    public void RecordRecovered_WhenServiceWasSuspended_ClearsIt()
    {
        // Arrange
        var sut = new CircuitBreakerMonitor();
        sut.RecordSuspended("s3");
        sut.RecordSuspended("sqs");

        // Act
        sut.RecordRecovered("s3");

        // Assert
        var status = sut.GetStatus();
        status.IsOpen.Should().BeTrue();
        status.AffectedServices.Should().Equal("sqs");
    }

    [Fact]
    public void RecordRecovered_WhenServiceWasNotSuspended_IsANoOp()
    {
        // Arrange
        var sut = new CircuitBreakerMonitor();

        // Act
        sut.RecordRecovered("s3");

        // Assert
        sut.GetStatus().IsOpen.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RecordSuspended_WhenServiceKeyIsBlank_Throws(string serviceKey)
    {
        // Arrange
        var sut = new CircuitBreakerMonitor();

        // Act
        var act = () => sut.RecordSuspended(serviceKey);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RecordRecovered_WhenServiceKeyIsBlank_Throws(string serviceKey)
    {
        // Arrange
        var sut = new CircuitBreakerMonitor();

        // Act
        var act = () => sut.RecordRecovered(serviceKey);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
