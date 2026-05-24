using Foundation.Application.Commands.PutDynamoDbItem;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutDynamoDbItem;

public class PutDynamoDbItemCommandValidatorTests
{
    private readonly PutDynamoDbItemCommandValidator _sut =
        new(NullLogger<PutDynamoDbItemCommandValidator>.Instance);

    private static PutDynamoDbItemCommand Valid(
        string tableName = "orders",
        string itemJson = "{\"id\":\"a\"}")
        => new(tableName, itemJson);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameEmpty_ReturnsErrorForTableName()
    {
        var result = await _sut.ValidateAsync(
            Valid(tableName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutDynamoDbItemCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenItemJsonEmpty_ReturnsErrorForItemJson()
    {
        var result = await _sut.ValidateAsync(
            Valid(itemJson: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutDynamoDbItemCommand.ItemJson));
    }

    [Fact]
    public async Task ValidateAsync_WhenItemJsonIsNotAnObject_ReturnsErrorForItemJson()
    {
        var result = await _sut.ValidateAsync(
            Valid(itemJson: "[1,2,3]"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutDynamoDbItemCommand.ItemJson));
    }

    [Fact]
    public async Task ValidateAsync_WhenItemJsonIsNotValidJson_ReturnsErrorForItemJson()
    {
        var result = await _sut.ValidateAsync(
            Valid(itemJson: "not-json"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutDynamoDbItemCommand.ItemJson));
    }
}
