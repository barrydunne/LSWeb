using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetSqsQueueAttributes;
using Foundation.Application.Sqs;
using Foundation.Domain.Sqs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSqsQueueAttributes;

public class GetSqsQueueAttributesQueryHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();

    private GetSqsQueueAttributesQueryHandler CreateSut()
        => new(_client, NullLogger<GetSqsQueueAttributesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsAttributes()
    {
        // Arrange
        var attributes = new SqsQueueAttributes(
            30, 345600, 0, 0, 262144, "arn:aws:sqs:eu-west-1:000000000000:orders", false, 7, 3, 2);
        _client
            .GetQueueAttributesAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(attributes)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSqsQueueAttributesQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Attributes.VisibilityTimeoutSeconds.Should().Be(30);
        result.Value.Attributes.QueueArn.Should().Be("arn:aws:sqs:eu-west-1:000000000000:orders");
        result.Value.Attributes.ApproximateMessageCount.Should().Be(7);
        result.Value.Attributes.ApproximateInFlightCount.Should().Be(3);
        result.Value.Attributes.ApproximateDelayedCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetQueueAttributesAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SqsQueueAttributes>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSqsQueueAttributesQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
