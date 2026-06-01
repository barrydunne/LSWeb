using Foundation.Application.Commands.ExecuteChangeSet;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ExecuteChangeSet;

public class ExecuteChangeSetCommandValidatorTests
{
    private readonly ExecuteChangeSetCommandValidator _sut =
        new(NullLogger<ExecuteChangeSetCommandValidator>.Instance);

    private static ExecuteChangeSetCommand Valid(
        string stackName = "orders-stack",
        string changeSetName = "add-queue")
        => new(stackName, changeSetName);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameEmpty_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteChangeSetCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameHasInvalidCharacters_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "1-bad-name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteChangeSetCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetNameEmpty_ReturnsErrorForChangeSetName()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteChangeSetCommand.ChangeSetName));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetNameTooLong_ReturnsErrorForChangeSetName()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetName: "a" + new string('b', 128)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteChangeSetCommand.ChangeSetName));
    }
}
