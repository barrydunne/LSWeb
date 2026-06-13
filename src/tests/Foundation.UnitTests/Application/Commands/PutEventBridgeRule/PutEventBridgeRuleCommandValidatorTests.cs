using Foundation.Application.Commands.PutEventBridgeRule;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutEventBridgeRule;

public class PutEventBridgeRuleCommandValidatorTests
{
    private readonly PutEventBridgeRuleCommandValidator _sut =
        new(NullLogger<PutEventBridgeRuleCommandValidator>.Instance);

    private static PutEventBridgeRuleCommand Valid(
        string name = "orders-rule",
        string eventPattern = "{\"source\":[\"my.app\"]}",
        string state = "ENABLED")
        => new(name, eventPattern, state, null, null);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDisabled_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(state: "DISABLED"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeRuleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 65)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeRuleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenStateInvalid_ReturnsErrorForState()
    {
        var result = await _sut.ValidateAsync(Valid(state: "PAUSED"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeRuleCommand.State));
    }

    [Fact]
    public async Task ValidateAsync_WhenEventPatternEmpty_ReturnsErrorForEventPattern()
    {
        var result = await _sut.ValidateAsync(
            Valid(eventPattern: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeRuleCommand.EventPattern));
    }

    [Fact]
    public async Task ValidateAsync_WhenEventPatternNotObject_ReturnsErrorForEventPattern()
    {
        var result = await _sut.ValidateAsync(
            Valid(eventPattern: "[1,2]"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeRuleCommand.EventPattern));
    }

    [Fact]
    public async Task ValidateAsync_WhenEventPatternNotValidJson_ReturnsErrorForEventPattern()
    {
        var result = await _sut.ValidateAsync(
            Valid(eventPattern: "not-json"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeRuleCommand.EventPattern));
    }
}
