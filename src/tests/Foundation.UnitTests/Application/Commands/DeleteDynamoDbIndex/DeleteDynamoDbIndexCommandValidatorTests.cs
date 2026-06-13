using Foundation.Application.Commands.DeleteDynamoDbIndex;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteDynamoDbIndex;

public class DeleteDynamoDbIndexCommandValidatorTests
{
    private readonly DeleteDynamoDbIndexCommandValidator _sut =
        new(NullLogger<DeleteDynamoDbIndexCommandValidator>.Instance);

    private static DeleteDynamoDbIndexCommand Valid(
        string tableName = "orders",
        string indexName = "gsi-1")
        => new(tableName, indexName);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteDynamoDbIndexCommand.TableName));
    }

    [Fact]
    public async Task ValidateAsync_WhenIndexNameEmpty_ReturnsErrorForIndexName()
    {
        var result = await _sut.ValidateAsync(
            Valid(indexName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteDynamoDbIndexCommand.IndexName));
    }
}
