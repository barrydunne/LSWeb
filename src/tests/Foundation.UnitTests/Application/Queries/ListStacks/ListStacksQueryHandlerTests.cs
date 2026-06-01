using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.ListStacks;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListStacks;

public class ListStacksQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private ListStacksQueryHandler CreateSut()
        => new(_client, NullLogger<ListStacksQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStacks()
    {
        // Arrange
        IReadOnlyList<CloudFormationStackSummary> stacks =
        [
            new(
                "orders-stack",
                "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc",
                "CREATE_COMPLETE",
                "Orders processing stack",
                DateTimeOffset.UnixEpoch,
                null),
        ];
        _client
            .ListStacksAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stacks)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListStacksQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Stacks.Should().ContainSingle(_ => _.StackName == "orders-stack");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListStacksAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<CloudFormationStackSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListStacksQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
