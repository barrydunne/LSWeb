using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Application.Queries.ExecuteDynamoDbBatchGet;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ExecuteDynamoDbBatchGet;

public class ExecuteDynamoDbBatchGetQueryHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private static ExecuteDynamoDbBatchGetQuery BuildQuery()
        => new([new DynamoDbBatchGetKey("orders", "{\"pk\":{\"S\":\"a\"}}")]);

    private ExecuteDynamoDbBatchGetQueryHandler CreateSut()
        => new(_client, NullLogger<ExecuteDynamoDbBatchGetQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenBatchSucceeds_ReturnsItems()
    {
        // Arrange
        _client
            .ExecuteBatchGetAsync(Arg.Any<IReadOnlyList<DynamoDbBatchGetKey>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbBatchGetResult>>(
                new DynamoDbBatchGetResult(1, [new DynamoDbItem("{\"pk\":{\"S\":\"a\"}}")])));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Result.Requested.Should().Be(1);
        result.Value.Result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WhenBatchFails_ReturnsError()
    {
        // Arrange
        _client
            .ExecuteBatchGetAsync(Arg.Any<IReadOnlyList<DynamoDbBatchGetKey>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbBatchGetResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
