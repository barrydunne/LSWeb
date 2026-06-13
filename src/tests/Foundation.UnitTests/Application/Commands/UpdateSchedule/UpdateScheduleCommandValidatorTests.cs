using Foundation.Application.Commands.UpdateSchedule;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateSchedule;

public class UpdateScheduleCommandValidatorTests
{
    private readonly UpdateScheduleCommandValidator _sut =
        new(NullLogger<UpdateScheduleCommandValidator>.Instance);

    private static UpdateScheduleCommand Valid(
        string name = "daily-job",
        string groupName = "default",
        string scheduleExpression = "rate(5 minutes)",
        string targetArn = "arn:aws:sqs:us-east-1:000000000000:queue",
        string roleArn = "arn:aws:iam::000000000000:role/scheduler",
        string flexibleTimeWindowMode = "OFF",
        int? maximumWindowInMinutes = null,
        string state = "ENABLED")
        => new(
            name,
            groupName,
            scheduleExpression,
            null,
            null,
            null,
            null,
            targetArn,
            roleArn,
            flexibleTimeWindowMode,
            maximumWindowInMinutes,
            state,
            null);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFlexibleWithWindow_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(flexibleTimeWindowMode: "FLEXIBLE", maximumWindowInMinutes: 15),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 65)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenGroupNameEmpty_ReturnsErrorForGroupName()
    {
        var result = await _sut.ValidateAsync(
            Valid(groupName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.GroupName));
    }

    [Fact]
    public async Task ValidateAsync_WhenScheduleExpressionEmpty_ReturnsErrorForScheduleExpression()
    {
        var result = await _sut.ValidateAsync(
            Valid(scheduleExpression: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.ScheduleExpression));
    }

    [Fact]
    public async Task ValidateAsync_WhenTargetArnEmpty_ReturnsErrorForTargetArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(targetArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.TargetArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenRoleArnEmpty_ReturnsErrorForRoleArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(roleArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.RoleArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenModeEmpty_ReturnsErrorForMode()
    {
        var result = await _sut.ValidateAsync(
            Valid(flexibleTimeWindowMode: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.FlexibleTimeWindowMode));
    }

    [Fact]
    public async Task ValidateAsync_WhenModeNotAllowed_ReturnsErrorForMode()
    {
        var result = await _sut.ValidateAsync(
            Valid(flexibleTimeWindowMode: "SOMETIMES"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.FlexibleTimeWindowMode));
    }

    [Fact]
    public async Task ValidateAsync_WhenFlexibleWithoutWindow_ReturnsErrorForWindow()
    {
        var result = await _sut.ValidateAsync(
            Valid(flexibleTimeWindowMode: "FLEXIBLE", maximumWindowInMinutes: null),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.MaximumWindowInMinutes));
    }

    [Fact]
    public async Task ValidateAsync_WhenFlexibleWindowOutOfRange_ReturnsErrorForWindow()
    {
        var result = await _sut.ValidateAsync(
            Valid(flexibleTimeWindowMode: "FLEXIBLE", maximumWindowInMinutes: 5000),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateScheduleCommand.MaximumWindowInMinutes));
    }
}
