using Foundation.Application.Commands.UpdateDynamoDbTtl;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateDynamoDbTtl;

public class UpdateDynamoDbTtlCommandValidatorTests
{
    private readonly UpdateDynamoDbTtlCommandValidator _sut =
        new(NullLogger<UpdateDynamoDbTtlCommandValidator>.Instance);

    private static UpdateDynamoDbTtlCommand Valid(
        string tableName = "orders",
        bool enabled = true,
        string attributeName = "expiresAt")
        => new(tableName, enabled, attributeName);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDisabling_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(enabled: false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameEmpty_ReturnsErrorForTableName()
    {
        var result = await _sut.ValidateAsync(
            Valid(tableName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateDynamoDbTtlCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenAttributeNameEmpty_ReturnsErrorForAttributeName()
    {
        var result = await _sut.ValidateAsync(
            Valid(attributeName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateDynamoDbTtlCommand.AttributeName));
    }

    [Fact]
    public async Task ValidateAsync_WhenAttributeNameTooLong_ReturnsErrorForAttributeName()
    {
        var result = await _sut.ValidateAsync(
            Valid(attributeName: new string('a', 256)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateDynamoDbTtlCommand.AttributeName));
    }
}
