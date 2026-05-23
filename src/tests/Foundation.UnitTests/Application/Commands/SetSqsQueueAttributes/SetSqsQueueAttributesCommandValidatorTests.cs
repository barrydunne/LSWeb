using Foundation.Application.Commands.SetSqsQueueAttributes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetSqsQueueAttributes;

public class SetSqsQueueAttributesCommandValidatorTests
{
    private readonly SetSqsQueueAttributesCommandValidator _sut =
        new(NullLogger<SetSqsQueueAttributesCommandValidator>.Instance);

    private static SetSqsQueueAttributesCommand Command(
        string queueName = "orders",
        int visibilityTimeoutSeconds = 30,
        int messageRetentionPeriodSeconds = 345600,
        int delaySeconds = 0,
        int receiveMessageWaitTimeSeconds = 0)
        => new(
            queueName,
            visibilityTimeoutSeconds,
            messageRetentionPeriodSeconds,
            delaySeconds,
            receiveMessageWaitTimeSeconds);

    [Fact]
    public async Task ValidateAsync_WhenAllValuesValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Command(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameEmpty_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            Command(queueName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetSqsQueueAttributesCommand.QueueName));
    }

    [Fact]
    public async Task ValidateAsync_WhenVisibilityTimeoutTooHigh_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Command(visibilityTimeoutSeconds: 43201), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ =>
            _.PropertyName == nameof(SetSqsQueueAttributesCommand.VisibilityTimeoutSeconds));
    }

    [Fact]
    public async Task ValidateAsync_WhenVisibilityTimeoutNegative_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Command(visibilityTimeoutSeconds: -1), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ =>
            _.PropertyName == nameof(SetSqsQueueAttributesCommand.VisibilityTimeoutSeconds));
    }

    [Fact]
    public async Task ValidateAsync_WhenRetentionTooLow_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Command(messageRetentionPeriodSeconds: 59), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ =>
            _.PropertyName == nameof(SetSqsQueueAttributesCommand.MessageRetentionPeriodSeconds));
    }

    [Fact]
    public async Task ValidateAsync_WhenRetentionTooHigh_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Command(messageRetentionPeriodSeconds: 1209601), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ =>
            _.PropertyName == nameof(SetSqsQueueAttributesCommand.MessageRetentionPeriodSeconds));
    }

    [Fact]
    public async Task ValidateAsync_WhenDelayTooHigh_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Command(delaySeconds: 901), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetSqsQueueAttributesCommand.DelaySeconds));
    }

    [Fact]
    public async Task ValidateAsync_WhenWaitTimeTooHigh_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Command(receiveMessageWaitTimeSeconds: 21), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ =>
            _.PropertyName == nameof(SetSqsQueueAttributesCommand.ReceiveMessageWaitTimeSeconds));
    }
}
