using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetSqsQueueRedrive;
using Foundation.Application.Sqs;
using Foundation.Domain.Sqs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSqsQueueRedrive;

public class GetSqsQueueRedriveQueryHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();

    private GetSqsQueueRedriveQueryHandler CreateSut()
        => new(_client, NullLogger<GetSqsQueueRedriveQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRedrive()
    {
        // Arrange
        var redrive = new SqsRedrive(
            new SqsRedriveTarget("arn:aws:sqs:eu-west-1:000000000000:orders-dlq", "orders-dlq", 5),
            []);
        _client
            .GetQueueRedriveAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(redrive)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSqsQueueRedriveQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Redrive.DeadLetterTarget!.QueueName.Should().Be("orders-dlq");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetQueueRedriveAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SqsRedrive>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSqsQueueRedriveQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
