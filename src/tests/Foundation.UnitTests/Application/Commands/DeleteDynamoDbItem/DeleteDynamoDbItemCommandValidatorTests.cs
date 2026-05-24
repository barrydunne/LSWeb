using Foundation.Application.Commands.DeleteDynamoDbItem;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteDynamoDbItem;

public class DeleteDynamoDbItemCommandValidatorTests
{
    private readonly DeleteDynamoDbItemCommandValidator _sut =
        new(NullLogger<DeleteDynamoDbItemCommandValidator>.Instance);

    private static DeleteDynamoDbItemCommand Valid(
        string tableName = "orders",
        string keyJson = "{\"id\":\"a\"}")
        => new(tableName, keyJson);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteDynamoDbItemCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenKeyJsonEmpty_ReturnsErrorForKeyJson()
    {
        var result = await _sut.ValidateAsync(
            Valid(keyJson: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteDynamoDbItemCommand.KeyJson));
    }

    [Fact]
    public async Task ValidateAsync_WhenKeyJsonIsNotAnObject_ReturnsErrorForKeyJson()
    {
        var result = await _sut.ValidateAsync(
            Valid(keyJson: "[1,2,3]"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteDynamoDbItemCommand.KeyJson));
    }

    [Fact]
    public async Task ValidateAsync_WhenKeyJsonIsNotValidJson_ReturnsErrorForKeyJson()
    {
        var result = await _sut.ValidateAsync(
            Valid(keyJson: "not-json"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteDynamoDbItemCommand.KeyJson));
    }
}
