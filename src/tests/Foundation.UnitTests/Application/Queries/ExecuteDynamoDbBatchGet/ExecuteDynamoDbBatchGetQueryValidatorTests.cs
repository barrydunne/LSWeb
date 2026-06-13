using Foundation.Application.Queries.ExecuteDynamoDbBatchGet;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ExecuteDynamoDbBatchGet;

public class ExecuteDynamoDbBatchGetQueryValidatorTests
{
    private readonly ExecuteDynamoDbBatchGetQueryValidator _sut =
        new(NullLogger<ExecuteDynamoDbBatchGetQueryValidator>.Instance);

    private static ExecuteDynamoDbBatchGetQuery Build(params DynamoDbBatchGetKey[] keys)
        => new(keys);

    private static DynamoDbBatchGetKey ValidKey(
        string tableName = "orders",
        string json = "{\"pk\":{\"S\":\"a\"}}")
        => new(tableName, json);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Build(ValidKey()), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNoKeys_ReturnsError()
    {
        var result = await _sut.ValidateAsync(Build(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteDynamoDbBatchGetQuery.Keys));
    }

    [Fact]
    public async Task ValidateAsync_WhenTooManyKeys_ReturnsError()
    {
        var keys = Enumerable.Range(0, 101).Select(_ => ValidKey()).ToArray();
        var result = await _sut.ValidateAsync(Build(keys), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteDynamoDbBatchGetQuery.Keys));
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidKey(tableName: string.Empty)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenJsonEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidKey(json: string.Empty)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }
}
