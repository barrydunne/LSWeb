using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSnsTopics;
using Foundation.Application.Sns;
using Foundation.Domain.Sns;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSnsTopics;

public class ListSnsTopicsQueryHandlerTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();

    private ListSnsTopicsQueryHandler CreateSut()
        => new(_client, NullLogger<ListSnsTopicsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsTopics()
    {
        // Arrange
        IReadOnlyList<SnsTopic> topics =
        [
            new("orders-topic", "arn:aws:sns:eu-west-1:000000000000:orders-topic"),
        ];
        _client
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(topics)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListSnsTopicsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Topics.Should().ContainSingle(_ => _.Name == "orders-topic");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SnsTopic>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListSnsTopicsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
