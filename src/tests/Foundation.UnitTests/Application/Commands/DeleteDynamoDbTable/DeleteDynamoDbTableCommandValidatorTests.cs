using Foundation.Application.Commands.DeleteDynamoDbTable;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteDynamoDbTable;

public class DeleteDynamoDbTableCommandValidatorTests
{
    private readonly DeleteDynamoDbTableCommandValidator _sut =
        new(NullLogger<DeleteDynamoDbTableCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValidName_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteDynamoDbTableCommand("orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForTableName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteDynamoDbTableCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteDynamoDbTableCommand.TableName));
    }
}
