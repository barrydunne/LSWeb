using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Application.Queries.ListDynamoDbTables;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListDynamoDbTables;

public class ListDynamoDbTablesQueryHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private ListDynamoDbTablesQueryHandler CreateSut()
        => new(_client, NullLogger<ListDynamoDbTablesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsTables()
    {
        // Arrange
        IReadOnlyList<DynamoDbTable> tables =
        [
            new("orders"),
        ];
        _client
            .ListTablesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(tables)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListDynamoDbTablesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tables.Should().ContainSingle(_ => _.Name == "orders");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListTablesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<DynamoDbTable>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListDynamoDbTablesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
