using Foundation.Application.Commands.CreateDynamoDbIndex;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateDynamoDbIndex;

public class CreateDynamoDbIndexCommandValidatorTests
{
    private readonly CreateDynamoDbIndexCommandValidator _sut =
        new(NullLogger<CreateDynamoDbIndexCommandValidator>.Instance);

    private static CreateDynamoDbIndexCommand Valid(
        string tableName = "orders",
        string indexName = "gsi-1",
        string partitionKeyName = "gpk",
        string partitionKeyType = "S",
        string? sortKeyName = null,
        string? sortKeyType = null,
        string projectionType = "ALL")
        => new(tableName, indexName, partitionKeyName, partitionKeyType, sortKeyName, sortKeyType, projectionType);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSortKeyProvidedWithValidType_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(sortKeyName: "gsk", sortKeyType: "N"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameEmpty_ReturnsErrorForTableName()
    {
        var result = await _sut.ValidateAsync(
            Valid(tableName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbIndexCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenIndexNameEmpty_ReturnsErrorForIndexName()
    {
        var result = await _sut.ValidateAsync(
            Valid(indexName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbIndexCommand.IndexName));
    }

    [Fact]
    public async Task ValidateAsync_WhenIndexNameTooShort_ReturnsErrorForIndexName()
    {
        var result = await _sut.ValidateAsync(
            Valid(indexName: "ab"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbIndexCommand.IndexName));
    }

    [Fact]
    public async Task ValidateAsync_WhenIndexNameHasInvalidCharacters_ReturnsErrorForIndexName()
    {
        var result = await _sut.ValidateAsync(
            Valid(indexName: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbIndexCommand.IndexName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyNameEmpty_ReturnsErrorForPartitionKeyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(partitionKeyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbIndexCommand.PartitionKeyName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyTypeInvalid_ReturnsErrorForPartitionKeyType()
    {
        var result = await _sut.ValidateAsync(
            Valid(partitionKeyType: "X"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbIndexCommand.PartitionKeyType));
    }

    [Fact]
    public async Task ValidateAsync_WhenSortKeyTypeInvalid_ReturnsErrorForSortKeyType()
    {
        var result = await _sut.ValidateAsync(
            Valid(sortKeyName: "gsk", sortKeyType: "X"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbIndexCommand.SortKeyType));
    }

    [Fact]
    public async Task ValidateAsync_WhenProjectionTypeInvalid_ReturnsErrorForProjectionType()
    {
        var result = await _sut.ValidateAsync(
            Valid(projectionType: "INCLUDE"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbIndexCommand.ProjectionType));
    }
}
