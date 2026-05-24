using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Application.Queries.QueryDynamoDbTable;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.QueryDynamoDbTable;

public class QueryDynamoDbTableQueryHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private static DynamoDbQueryRequest Request(bool scan = false)
        => new(
            "orders",
            "orders-by-status",
            scan,
            scan ? null : new DynamoDbCondition("pk", "=", "S", "a", null),
            null,
            [],
            25,
            null);

    private QueryDynamoDbTableQueryHandler CreateSut()
        => new(_client, NullLogger<QueryDynamoDbTableQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsPage()
    {
        // Arrange
        var page = new DynamoDbQueryResult(
            [new("{\"id\":\"a\"}"), new("{\"id\":\"b\"}")], "next-token");
        DynamoDbQueryRequest? forwarded = null;
        _client
            .QueryTableAsync(
                Arg.Do<DynamoDbQueryRequest>(request => forwarded = request),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbQueryResult>>(page));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new QueryDynamoDbTableQuery(Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Items.Should().HaveCount(2);
        result.Value.Page.NextToken.Should().Be("next-token");
        forwarded!.IndexName.Should().Be("orders-by-status");
        await _client.Received(1).QueryTableAsync(
            Arg.Any<DynamoDbQueryRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .QueryTableAsync(Arg.Any<DynamoDbQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new QueryDynamoDbTableQuery(Request(scan: true)), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
