using Foundation.Application.Queries.QueryDynamoDbTable;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.QueryDynamoDbTable;

public class QueryDynamoDbTableQueryValidatorTests
{
    private readonly QueryDynamoDbTableQueryValidator _sut =
        new(NullLogger<QueryDynamoDbTableQueryValidator>.Instance);

    private static readonly DynamoDbCondition _validPartition = new("pk", "=", "S", "a", null);

    private static QueryDynamoDbTableQuery Query(
        DynamoDbCondition? partitionKey = null,
        DynamoDbCondition? sortKey = null,
        IReadOnlyList<DynamoDbCondition>? filters = null,
        string tableName = "orders",
        bool scan = false,
        int limit = 25)
        => new(new DynamoDbQueryRequest(
            tableName, null, scan, partitionKey, sortKey, filters ?? [], limit, null));

    [Fact]
    public async Task ValidateAsync_WhenValidQuery_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: _validPartition), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenValidScan_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Query(scan: true), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: _validPartition, tableName: string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.Contains("TableName"));
    }

    [Fact]
    public async Task ValidateAsync_WhenLimitNotPositive_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: _validPartition, limit: 0), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.Contains("Limit"));
    }

    [Fact]
    public async Task ValidateAsync_WhenQueryMissingPartitionKey_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage == "A partition key condition is required for a query.");
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyOperatorNotAllowed_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: new("pk", "begins_with", "S", "a", null)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyAttributeNameEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: new(string.Empty, "=", "S", "a", null)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyOperatorEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: new("pk", string.Empty, "S", "a", null)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyValueTypeInvalid_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: new("pk", "=", "X", "a", null)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyValueTypeEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: new("pk", "=", string.Empty, "a", null)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyValueEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: new("pk", "=", "S", string.Empty, null)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenSortKeyValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: _validPartition, sortKey: new("sk", "begins_with", "S", "x", null)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSortKeyBetweenMissingSecondValue_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: _validPartition, sortKey: new("sk", "between", "N", "1", null)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage == "A second value is required for the 'between' operator.");
    }

    [Fact]
    public async Task ValidateAsync_WhenSortKeyBetweenWithSecondValue_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: _validPartition, sortKey: new("sk", "between", "N", "1", "9")),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFilterValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: _validPartition, filters: [new("status", "contains", "S", "OPEN", null)]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFilterOperatorNotAllowed_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(partitionKey: _validPartition, filters: [new("status", "invalid", "S", "x", null)]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }
}
