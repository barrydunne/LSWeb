using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Application.Queries.GetDynamoDbItem;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetDynamoDbItem;

public class GetDynamoDbItemQueryHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private GetDynamoDbItemQueryHandler CreateSut()
        => new(_client, NullLogger<GetDynamoDbItemQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsItem()
    {
        // Arrange
        _client
            .GetItemAsync("orders", "{\"id\":\"a\"}", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbItem>>(new DynamoDbItem("{\"id\":\"a\",\"name\":\"x\"}")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetDynamoDbItemQuery("orders", "{\"id\":\"a\"}"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Item.Json.Should().Be("{\"id\":\"a\",\"name\":\"x\"}");
        await _client.Received(1).GetItemAsync("orders", "{\"id\":\"a\"}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbItem>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetDynamoDbItemQuery("orders", "{\"id\":\"a\"}"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
