using Foundation.Application.Commands.ExecuteDynamoDbTransaction;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ExecuteDynamoDbTransaction;

public class ExecuteDynamoDbTransactionCommandValidatorTests
{
    private readonly ExecuteDynamoDbTransactionCommandValidator _sut =
        new(NullLogger<ExecuteDynamoDbTransactionCommandValidator>.Instance);

    private static ExecuteDynamoDbTransactionCommand Build(params DynamoDbTransactionAction[] actions)
        => new(actions);

    private static DynamoDbTransactionAction ValidAction(
        string operation = "Put",
        string tableName = "orders",
        string json = "{\"pk\":{\"S\":\"a\"}}")
        => new(operation, tableName, json);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Build(ValidAction()), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDeleteAction_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidAction(operation: "Delete")), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNoActions_ReturnsError()
    {
        var result = await _sut.ValidateAsync(Build(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteDynamoDbTransactionCommand.Actions));
    }

    [Fact]
    public async Task ValidateAsync_WhenTooManyActions_ReturnsError()
    {
        var actions = Enumerable.Range(0, 101).Select(_ => ValidAction()).ToArray();
        var result = await _sut.ValidateAsync(Build(actions), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteDynamoDbTransactionCommand.Actions));
    }

    [Fact]
    public async Task ValidateAsync_WhenOperationInvalid_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidAction(operation: "Update")), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenTableNameEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidAction(tableName: string.Empty)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenJsonEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(ValidAction(json: string.Empty)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }
}
