using Foundation.Application.Commands.CreateDynamoDbTable;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateDynamoDbTable;

public class CreateDynamoDbTableCommandValidatorTests
{
    private readonly CreateDynamoDbTableCommandValidator _sut =
        new(NullLogger<CreateDynamoDbTableCommandValidator>.Instance);

    private static CreateDynamoDbTableCommand Valid(
        string tableName = "orders",
        string partitionKeyName = "pk",
        string partitionKeyType = "S",
        string? sortKeyName = null,
        string? sortKeyType = null,
        string billingMode = "PAY_PER_REQUEST",
        long? readCapacityUnits = null,
        long? writeCapacityUnits = null)
        => new(tableName, partitionKeyName, partitionKeyType, sortKeyName, sortKeyType, billingMode, readCapacityUnits, writeCapacityUnits);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenProvisionedWithCapacity_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(billingMode: "PROVISIONED", readCapacityUnits: 5, writeCapacityUnits: 5),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSortKeyProvidedWithValidType_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(sortKeyName: "sk", sortKeyType: "N"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameEmpty_ReturnsErrorForTableName()
    {
        var result = await _sut.ValidateAsync(Valid(tableName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameTooShort_ReturnsErrorForTableName()
    {
        var result = await _sut.ValidateAsync(Valid(tableName: "ab"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameTooLong_ReturnsErrorForTableName()
    {
        var result = await _sut.ValidateAsync(Valid(tableName: new string('a', 256)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameContainsInvalidCharacters_ReturnsErrorForTableName()
    {
        var result = await _sut.ValidateAsync(Valid(tableName: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyNameEmpty_ReturnsErrorForPartitionKeyName()
    {
        var result = await _sut.ValidateAsync(Valid(partitionKeyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.PartitionKeyName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPartitionKeyTypeInvalid_ReturnsErrorForPartitionKeyType()
    {
        var result = await _sut.ValidateAsync(Valid(partitionKeyType: "X"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.PartitionKeyType));
    }

    [Fact]
    public async Task ValidateAsync_WhenSortKeyTypeInvalid_ReturnsErrorForSortKeyType()
    {
        var result = await _sut.ValidateAsync(
            Valid(sortKeyName: "sk", sortKeyType: "X"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.SortKeyType));
    }

    [Fact]
    public async Task ValidateAsync_WhenBillingModeInvalid_ReturnsErrorForBillingMode()
    {
        var result = await _sut.ValidateAsync(Valid(billingMode: "FREE"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.BillingMode));
    }

    [Fact]
    public async Task ValidateAsync_WhenProvisionedWithoutReadCapacity_ReturnsErrorForReadCapacityUnits()
    {
        var result = await _sut.ValidateAsync(
            Valid(billingMode: "PROVISIONED", readCapacityUnits: null, writeCapacityUnits: 5),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.ReadCapacityUnits));
    }

    [Fact]
    public async Task ValidateAsync_WhenProvisionedWithZeroReadCapacity_ReturnsErrorForReadCapacityUnits()
    {
        var result = await _sut.ValidateAsync(
            Valid(billingMode: "PROVISIONED", readCapacityUnits: 0, writeCapacityUnits: 5),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.ReadCapacityUnits));
    }

    [Fact]
    public async Task ValidateAsync_WhenProvisionedWithoutWriteCapacity_ReturnsErrorForWriteCapacityUnits()
    {
        var result = await _sut.ValidateAsync(
            Valid(billingMode: "PROVISIONED", readCapacityUnits: 5, writeCapacityUnits: null),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.WriteCapacityUnits));
    }

    [Fact]
    public async Task ValidateAsync_WhenProvisionedWithZeroWriteCapacity_ReturnsErrorForWriteCapacityUnits()
    {
        var result = await _sut.ValidateAsync(
            Valid(billingMode: "PROVISIONED", readCapacityUnits: 5, writeCapacityUnits: 0),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateDynamoDbTableCommand.WriteCapacityUnits));
    }
}
