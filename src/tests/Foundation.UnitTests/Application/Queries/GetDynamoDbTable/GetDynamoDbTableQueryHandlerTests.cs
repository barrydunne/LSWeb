using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Application.Queries.GetDynamoDbTable;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetDynamoDbTable;

public class GetDynamoDbTableQueryHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private GetDynamoDbTableQueryHandler CreateSut()
        => new(_client, NullLogger<GetDynamoDbTableQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    private static DynamoDbTableDetail SampleTable()
        => new(
            "orders",
            "arn:orders",
            "ACTIVE",
            5,
            1024,
            "PAY_PER_REQUEST",
            null,
            null,
            null,
            [new("id", "HASH")],
            [new("id", "S")],
            [],
            [],
            false,
            null,
            null);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsTable()
    {
        // Arrange
        _client
            .DescribeTableAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(SampleTable())));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetDynamoDbTableQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Table.Name.Should().Be("orders");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .DescribeTableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DynamoDbTableDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetDynamoDbTableQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
