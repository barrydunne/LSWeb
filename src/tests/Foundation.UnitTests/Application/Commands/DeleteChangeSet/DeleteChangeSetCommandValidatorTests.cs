using Foundation.Application.Commands.DeleteChangeSet;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteChangeSet;

public class DeleteChangeSetCommandValidatorTests
{
    private readonly DeleteChangeSetCommandValidator _sut =
        new(NullLogger<DeleteChangeSetCommandValidator>.Instance);

    private static DeleteChangeSetCommand Valid(
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteChangeSetCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameHasInvalidCharacters_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "1-bad-name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteChangeSetCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetNameEmpty_ReturnsErrorForChangeSetName()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteChangeSetCommand.ChangeSetName));
    }

    [Fact]
    public async Task ValidateAsync_WhenChangeSetNameTooLong_ReturnsErrorForChangeSetName()
    {
        var result = await _sut.ValidateAsync(
            Valid(changeSetName: "a" + new string('b', 128)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteChangeSetCommand.ChangeSetName));
    }
}
