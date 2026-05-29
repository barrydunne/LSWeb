using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.ListSqsConsumerLambdas;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSqsConsumerLambdas;

public class ListSqsConsumerLambdasQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private ListSqsConsumerLambdasQueryHandler CreateSut()
        => new(_client, NullLogger<ListSqsConsumerLambdasQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    private static LambdaFunctionSummary Function(string name)
        => new(name, "python3.12", string.Empty, "2024-01-01", 128, 30);

    private static LambdaEventSourceMapping Mapping(string eventSourceArn, string functionArn, string state)
        => new("uuid", eventSourceArn, functionArn, state, 10, "2024-01-01");

    [Fact]
    public async Task Handle_WhenFunctionConsumesQueue_ReturnsOrderedConsumers()
    {
        // Arrange
        IReadOnlyList<LambdaFunctionSummary> functions =
        [
            Function("zeta-consumer"),
            Function("alpha-consumer"),
            Function("unrelated"),
        ];
        _client
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(functions)));
        _client
            .ListEventSourceMappingsAsync("zeta-consumer", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaEventSourceMapping>>(
            [
                Mapping("arn:aws:sqs:eu-west-1:000000000000:orders", "arn:aws:lambda:eu-west-1:000000000000:function:zeta-consumer", "Enabled"),
            ])));
        _client
            .ListEventSourceMappingsAsync("alpha-consumer", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaEventSourceMapping>>(
            [
                Mapping("arn:aws:dynamodb:eu-west-1:000000000000:table/widgets/stream/x", "arn:aws:lambda:eu-west-1:000000000000:function:alpha-consumer", "Enabled"),
                Mapping("arn:aws:sqs:eu-west-1:000000000000:orders", "arn:aws:lambda:eu-west-1:000000000000:function:alpha-consumer", "Disabled"),
            ])));
        _client
            .ListEventSourceMappingsAsync("unrelated", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaEventSourceMapping>>(
            [
                Mapping("arn:aws:sqs:eu-west-1:000000000000:other-queue", "arn:aws:lambda:eu-west-1:000000000000:function:unrelated", "Enabled"),
            ])));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsConsumerLambdasQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Lambdas.Select(_ => _.FunctionName).Should().Equal("alpha-consumer", "zeta-consumer");
        result.Value.Lambdas.Should().ContainSingle(_ => _.FunctionName == "alpha-consumer" && _.State == "Disabled");
    }

    [Fact]
    public async Task Handle_WhenNoFunctionConsumesQueue_ReturnsEmptyList()
    {
        // Arrange
        IReadOnlyList<LambdaFunctionSummary> functions = [Function("unrelated")];
        _client
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(functions)));
        _client
            .ListEventSourceMappingsAsync("unrelated", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaEventSourceMapping>>(
            [
                Mapping("arn:aws:sqs:eu-west-1:000000000000:other-queue", "arn:aws:lambda:eu-west-1:000000000000:function:unrelated", "Enabled"),
            ])));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsConsumerLambdasQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Lambdas.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenListingFunctionsFails_PropagatesError()
    {
        // Arrange
        _client
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaFunctionSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsConsumerLambdasQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }

    [Fact]
    public async Task Handle_WhenListingMappingsFails_PropagatesError()
    {
        // Arrange
        IReadOnlyList<LambdaFunctionSummary> functions = [Function("consumer")];
        _client
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(functions)));
        _client
            .ListEventSourceMappingsAsync("consumer", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaEventSourceMapping>>>(new Error("mappings boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsConsumerLambdasQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("mappings boom");
    }
}
