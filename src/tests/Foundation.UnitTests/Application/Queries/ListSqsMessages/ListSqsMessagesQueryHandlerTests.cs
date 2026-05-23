using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSqsMessages;
using Foundation.Application.Sqs;
using Foundation.Domain.Sqs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSqsMessages;

public class ListSqsMessagesQueryHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();

    private ListSqsMessagesQueryHandler CreateSut()
        => new(_client, NullLogger<ListSqsMessagesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsMessages()
    {
        // Arrange
        IReadOnlyList<SqsMessage> messages =
        [
            new("id-1", "receipt-1", "body", new Dictionary<string, string>(), new Dictionary<string, string>()),
        ];
        _client
            .ReceiveMessagesAsync("orders", SqsPollMode.Peek, 10, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(messages)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsMessagesQuery("orders", SqsPollMode.Peek, 10), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Messages.Should().ContainSingle(_ => _.MessageId == "id-1");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ReceiveMessagesAsync("orders", SqsPollMode.Consume, 5, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SqsMessage>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsMessagesQuery("orders", SqsPollMode.Consume, 5), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
