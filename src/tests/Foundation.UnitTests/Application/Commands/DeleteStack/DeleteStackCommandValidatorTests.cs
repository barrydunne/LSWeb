using Foundation.Application.Commands.DeleteStack;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteStack;

public class DeleteStackCommandValidatorTests
{
    private readonly DeleteStackCommandValidator _sut =
        new(NullLogger<DeleteStackCommandValidator>.Instance);

    private static DeleteStackCommand Valid(string stackName = "orders-stack")
        => new(stackName);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteStackCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameTooLong_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "a" + new string('b', 128)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteStackCommand.StackName));
    }

    [Fact]
    public async Task ValidateAsync_WhenStackNameHasInvalidCharacters_ReturnsErrorForStackName()
    {
        var result = await _sut.ValidateAsync(
            Valid(stackName: "1-bad-name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteStackCommand.StackName));
    }
}
