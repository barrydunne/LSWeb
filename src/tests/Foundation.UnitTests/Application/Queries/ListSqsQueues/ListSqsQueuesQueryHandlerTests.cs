using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSqsQueues;
using Foundation.Application.Sqs;
using Foundation.Domain.Sqs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSqsQueues;

public class ListSqsQueuesQueryHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();

    private ListSqsQueuesQueryHandler CreateSut()
        => new(_client, NullLogger<ListSqsQueuesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsQueues()
    {
        // Arrange
        IReadOnlyList<SqsQueue> queues =
        [
            new("orders", "http://localhost:4566/000000000000/orders", 3, 1, 0),
        ];
        _client
            .ListQueuesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(queues)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListSqsQueuesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Queues.Should().ContainSingle(_ => _.Name == "orders");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListQueuesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SqsQueue>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListSqsQueuesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
