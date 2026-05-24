using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Domain.DynamoDb;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class DynamoDbResourceSourceTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();

    private DynamoDbResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsDynamoDb()
        => CreateSut().ServiceKey.Should().Be("dynamodb");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsTablesToSearchEntries()
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
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("dynamodb");
        entry.ResourceId.Should().Be("orders");
        entry.DisplayName.Should().Be("orders");
        entry.Route.Should().Be("/services/dynamodb/orders");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListTablesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<DynamoDbTable>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
