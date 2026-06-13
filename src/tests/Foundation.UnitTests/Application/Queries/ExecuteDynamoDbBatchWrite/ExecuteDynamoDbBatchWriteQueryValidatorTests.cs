using Foundation.Application.Queries.ExecuteDynamoDbBatchWrite;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ExecuteDynamoDbBatchWrite;

public class ExecuteDynamoDbBatchWriteQueryValidatorTests
{
    private readonly ExecuteDynamoDbBatchWriteQueryValidator _sut =
        new(NullLogger<ExecuteDynamoDbBatchWriteQueryValidator>.Instance);

    private static ExecuteDynamoDbBatchWriteQuery Build(params DynamoDbBatchWriteItem[] items)
        => new(items);

    private static DynamoDbBatchWriteItem ValidItem(
        string operation = "Put",
        string tableName = "orders",
        string json = "{\"pk\":{\"S\":\"a\"}}")
        => new(operation, tableName, json);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Build(ValidItem()), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDeleteItem_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidItem(operation: "Delete")), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNoItems_ReturnsError()
    {
        var result = await _sut.ValidateAsync(Build(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteDynamoDbBatchWriteQuery.Items));
    }

    [Fact]
    public async Task ValidateAsync_WhenTooManyItems_ReturnsError()
    {
        var items = Enumerable.Range(0, 26).Select(_ => ValidItem()).ToArray();
        var result = await _sut.ValidateAsync(Build(items), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteDynamoDbBatchWriteQuery.Items));
    }

    [Fact]
    public async Task ValidateAsync_WhenOperationInvalid_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidItem(operation: "Update")), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidItem(tableName: string.Empty)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenJsonEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidItem(json: string.Empty)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }
}
