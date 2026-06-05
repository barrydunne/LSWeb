using Foundation.Application.Commands.PutScheduledRuleTargets;
using Foundation.Domain.EventBridge;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutScheduledRuleTargets;

public class PutScheduledRuleTargetsCommandValidatorTests
{
    private readonly PutScheduledRuleTargetsCommandValidator _sut =
        new(NullLogger<PutScheduledRuleTargetsCommandValidator>.Instance);

    private static EventBridgeTargetSpecification Target(string id = "t1", string arn = "arn:aws:sqs:us-east-1:000000000000:queue")
        => new(id, arn, null, null);

    private static PutScheduledRuleTargetsCommand Valid(
        string ruleName = "daily-rule",
        IReadOnlyList<EventBridgeTargetSpecification>? targets = null)
        => new(ruleName, null, targets ?? [Target()]);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenRuleNameEmpty_ReturnsErrorForRuleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(ruleName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleTargetsCommand.RuleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenRuleNameContainsInvalidCharacters_ReturnsErrorForRuleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(ruleName: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleTargetsCommand.RuleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTargetsEmpty_ReturnsErrorForTargets()
    {
        var result = await _sut.ValidateAsync(
            Valid(targets: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleTargetsCommand.Targets));
    }

    [Fact]
    public async Task ValidateAsync_WhenTargetIdEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(targets: [Target(id: string.Empty)]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.Contains("Id"));
    }

    [Fact]
    public async Task ValidateAsync_WhenTargetArnEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(targets: [Target(arn: string.Empty)]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.Contains("Arn"));
    }
}
