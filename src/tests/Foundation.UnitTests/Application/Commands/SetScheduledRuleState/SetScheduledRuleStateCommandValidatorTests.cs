using Foundation.Application.Commands.SetScheduledRuleState;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetScheduledRuleState;

public class SetScheduledRuleStateCommandValidatorTests
{
    private readonly SetScheduledRuleStateCommandValidator _sut =
        new(NullLogger<SetScheduledRuleStateCommandValidator>.Instance);

    private static SetScheduledRuleStateCommand Valid(
        string name = "daily-rule",
        string state = "ENABLED")
        => new(name, state, null);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDisabled_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(state: "DISABLED"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetScheduledRuleStateCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetScheduledRuleStateCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenStateEmpty_ReturnsErrorForState()
    {
        var result = await _sut.ValidateAsync(
            Valid(state: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetScheduledRuleStateCommand.State));
    }

    [Fact]
    public async Task ValidateAsync_WhenStateNotAllowed_ReturnsErrorForState()
    {
        var result = await _sut.ValidateAsync(
            Valid(state: "PAUSED"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetScheduledRuleStateCommand.State));
    }
}
