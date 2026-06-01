using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.GetDriftStatus;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetDriftStatus;

public class GetDriftStatusQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private GetDriftStatusQueryHandler CreateSut()
        => new(_client, NullLogger<GetDriftStatusQueryHandler>.Instance);

    private static StackDriftStatus BuildStatus()
        => new(
            "drift-123",
            "arn:stack/orders-stack",
            "DETECTION_COMPLETE",
            null,
            "DRIFTED",
            2,
            DateTimeOffset.UtcNow);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStatus()
    {
        // Arrange
        var status = BuildStatus();
        _client
            .DescribeStackDriftDetectionStatusAsync("drift-123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<StackDriftStatus>>(status));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetDriftStatusQuery("drift-123"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().BeSameAs(status);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .DescribeStackDriftDetectionStatusAsync("drift-123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<StackDriftStatus>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetDriftStatusQuery("drift-123"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
