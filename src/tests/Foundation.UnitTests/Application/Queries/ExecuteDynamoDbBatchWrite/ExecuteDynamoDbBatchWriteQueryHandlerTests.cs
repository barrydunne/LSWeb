using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Application.Queries.ExecuteDynamoDbBatchWrite;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ExecuteDynamoDbBatchWrite;

public class ExecuteDynamoDbBatchWriteQueryHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private static ExecuteDynamoDbBatchWriteQuery BuildQuery()
        => new([new DynamoDbBatchWriteItem("Put", "orders", "{\"pk\":{\"S\":\"a\"}}")]);

    private ExecuteDynamoDbBatchWriteQueryHandler CreateSut()
        => new(_client, NullLogger<ExecuteDynamoDbBatchWriteQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenBatchSucceeds_ReturnsResult()
    {
        // Arrange
        _client
            .ExecuteBatchWriteAsync(Arg.Any<IReadOnlyList<DynamoDbBatchWriteItem>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbBatchWriteResult>>(
                new DynamoDbBatchWriteResult(1, ["{\"pk\":{\"S\":\"a\"}}"])));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Result.Requested.Should().Be(1);
        result.Value.Result.UnprocessedItems.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WhenBatchFails_ReturnsError()
    {
        // Arrange
        _client
            .ExecuteBatchWriteAsync(Arg.Any<IReadOnlyList<DynamoDbBatchWriteItem>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbBatchWriteResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
