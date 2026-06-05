using Foundation.Application.Commands.PutScheduledRule;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutScheduledRule;

public class PutScheduledRuleCommandValidatorTests
{
    private readonly PutScheduledRuleCommandValidator _sut =
        new(NullLogger<PutScheduledRuleCommandValidator>.Instance);

    private static PutScheduledRuleCommand Valid(
        string name = "daily-rule",
        string scheduleExpression = "rate(5 minutes)",
        string state = "ENABLED")
        => new(name, scheduleExpression, state, null, null);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenCronExpression_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(scheduleExpression: "cron(0 12 * * ? *)"), TestContext.Current.CancellationToken);
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 65)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenScheduleExpressionEmpty_ReturnsErrorForScheduleExpression()
    {
        var result = await _sut.ValidateAsync(
            Valid(scheduleExpression: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleCommand.ScheduleExpression));
    }

    [Fact]
    public async Task ValidateAsync_WhenScheduleExpressionNotRateOrCron_ReturnsErrorForScheduleExpression()
    {
        var result = await _sut.ValidateAsync(
            Valid(scheduleExpression: "every 5 minutes"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleCommand.ScheduleExpression));
    }

    [Fact]
    public async Task ValidateAsync_WhenStateEmpty_ReturnsErrorForState()
    {
        var result = await _sut.ValidateAsync(
            Valid(state: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleCommand.State));
    }

    [Fact]
    public async Task ValidateAsync_WhenStateNotAllowed_ReturnsErrorForState()
    {
        var result = await _sut.ValidateAsync(
            Valid(state: "PAUSED"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutScheduledRuleCommand.State));
    }
}
