using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Application.Queries.ExecuteDynamoDbStatement;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ExecuteDynamoDbStatement;

public class ExecuteDynamoDbStatementQueryHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private static DynamoDbStatementRequest Request()
        => new("SELECT * FROM \"orders\"", 25, null);

    private ExecuteDynamoDbStatementQueryHandler CreateSut()
        => new(_client, NullLogger<ExecuteDynamoDbStatementQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsPage()
    {
        // Arrange
        var page = new DynamoDbStatementResult(
            [new("{\"id\":\"a\"}"), new("{\"id\":\"b\"}")], "next-token");
        _client
            .ExecuteStatementAsync(Arg.Any<DynamoDbStatementRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbStatementResult>>(page));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ExecuteDynamoDbStatementQuery(Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Result.Items.Should().HaveCount(2);
        result.Value.Result.NextToken.Should().Be("next-token");
        await _client.Received(1).ExecuteStatementAsync(
            Arg.Any<DynamoDbStatementRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ExecuteStatementAsync(Arg.Any<DynamoDbStatementRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbStatementResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ExecuteDynamoDbStatementQuery(Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
