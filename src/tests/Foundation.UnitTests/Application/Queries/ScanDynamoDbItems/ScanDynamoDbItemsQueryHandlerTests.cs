using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Application.Queries.ScanDynamoDbItems;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ScanDynamoDbItems;

public class ScanDynamoDbItemsQueryHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private ScanDynamoDbItemsQueryHandler CreateSut()
        => new(_client, NullLogger<ScanDynamoDbItemsQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsPage()
    {
        // Arrange
        var page = new DynamoDbItemPage([new("{\"id\":\"a\"}"), new("{\"id\":\"b\"}")], true);
        _client
            .ScanItemsAsync("orders", 25, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbItemPage>>(page));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ScanDynamoDbItemsQuery("orders", 25), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Items.Should().HaveCount(2);
        result.Value.Page.Truncated.Should().BeTrue();
        await _client.Received(1).ScanItemsAsync("orders", 25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ScanItemsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbItemPage>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ScanDynamoDbItemsQuery("orders", 25), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
