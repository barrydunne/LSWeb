using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.ListStackResources;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListStackResources;

public class ListStackResourcesQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private ListStackResourcesQueryHandler CreateSut()
        => new(_client, NullLogger<ListStackResourcesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsResources()
    {
        // Arrange
        const string stackName = "orders-stack";
        IReadOnlyList<StackResource> resources =
        [
            new(
                "OrdersQueue",
                "orders-queue",
                "AWS::SQS::Queue",
                "CREATE_COMPLETE",
                null,
                DateTime.UtcNow),
        ];
        _client
            .ListStackResourcesAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(resources)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListStackResourcesQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Resources.Should().BeSameAs(resources);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        const string stackName = "orders-stack";
        _client
            .ListStackResourcesAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<StackResource>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListStackResourcesQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
