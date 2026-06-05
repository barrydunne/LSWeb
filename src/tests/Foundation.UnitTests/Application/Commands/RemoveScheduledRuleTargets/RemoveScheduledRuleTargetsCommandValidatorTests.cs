using Foundation.Application.Commands.RemoveScheduledRuleTargets;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RemoveScheduledRuleTargets;

public class RemoveScheduledRuleTargetsCommandValidatorTests
{
    private readonly RemoveScheduledRuleTargetsCommandValidator _sut =
        new(NullLogger<RemoveScheduledRuleTargetsCommandValidator>.Instance);

    private static RemoveScheduledRuleTargetsCommand Valid(
        string ruleName = "daily-rule",
        IReadOnlyList<string>? targetIds = null)
        => new(ruleName, null, targetIds ?? ["t1"]);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RemoveScheduledRuleTargetsCommand.RuleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenRuleNameContainsInvalidCharacters_ReturnsErrorForRuleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(ruleName: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RemoveScheduledRuleTargetsCommand.RuleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTargetIdsEmpty_ReturnsErrorForTargetIds()
    {
        var result = await _sut.ValidateAsync(
            Valid(targetIds: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RemoveScheduledRuleTargetsCommand.TargetIds));
    }

    [Fact]
    public async Task ValidateAsync_WhenTargetIdEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(targetIds: [string.Empty]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }
}
