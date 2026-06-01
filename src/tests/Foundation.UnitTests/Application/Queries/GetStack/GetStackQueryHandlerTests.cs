using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.GetStack;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetStack;

public class GetStackQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private GetStackQueryHandler CreateSut()
        => new(_client, NullLogger<GetStackQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStack()
    {
        // Arrange
        const string stackName = "orders-stack";
        var detail = new CloudFormationStackDetail(
            stackName,
            "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc",
            "CREATE_COMPLETE",
            null,
            "Orders processing stack",
            DateTimeOffset.UnixEpoch,
            null,
            [],
            [],
            [],
            []);
        _client
            .DescribeStackAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetStackQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Stack.Should().Be(detail);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        const string stackName = "orders-stack";
        _client
            .DescribeStackAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CloudFormationStackDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetStackQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
