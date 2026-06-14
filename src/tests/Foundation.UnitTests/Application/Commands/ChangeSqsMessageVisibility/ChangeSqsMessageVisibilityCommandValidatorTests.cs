using Foundation.Application.Commands.ChangeSqsMessageVisibility;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ChangeSqsMessageVisibility;

public class ChangeSqsMessageVisibilityCommandValidatorTests
{
    private readonly ChangeSqsMessageVisibilityCommandValidator _sut =
        new(NullLogger<ChangeSqsMessageVisibilityCommandValidator>.Instance);

    private static ChangeSqsMessageVisibilityCommand Valid(
        string queueName = "orders", string receiptHandle = "rh", int seconds = 30)
        => new(queueName, receiptHandle, seconds);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameEmpty_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            Valid(queueName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ChangeSqsMessageVisibilityCommand.QueueName));
    }

    [Fact]
    public async Task ValidateAsync_WhenReceiptHandleEmpty_ReturnsErrorForReceiptHandle()
    {
        var result = await _sut.ValidateAsync(
            Valid(receiptHandle: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ChangeSqsMessageVisibilityCommand.ReceiptHandle));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(43201)]
    public async Task ValidateAsync_WhenTimeoutOutOfRange_ReturnsErrorForTimeout(int seconds)
    {
        var result = await _sut.ValidateAsync(
            Valid(seconds: seconds), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ChangeSqsMessageVisibilityCommand.VisibilityTimeoutSeconds));
    }
}
