using Foundation.Application.Commands.SetSqsRedrivePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetSqsRedrivePolicy;

public class SetSqsRedrivePolicyCommandValidatorTests
{
    private readonly SetSqsRedrivePolicyCommandValidator _sut =
        new(NullLogger<SetSqsRedrivePolicyCommandValidator>.Instance);

    private static SetSqsRedrivePolicyCommand Valid(
        string queueName = "orders",
        string arn = "arn:aws:sqs:eu-west-1:000000000000:orders-dlq",
        int maxReceiveCount = 5)
        => new(queueName, arn, maxReceiveCount);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetSqsRedrivePolicyCommand.QueueName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("orders-dlq")]
    public async Task ValidateAsync_WhenTargetNotAnArn_ReturnsErrorForTarget(string arn)
    {
        var result = await _sut.ValidateAsync(
            Valid(arn: arn), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetSqsRedrivePolicyCommand.DeadLetterTargetArn));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public async Task ValidateAsync_WhenMaxReceiveCountOutOfRange_ReturnsErrorForMaxReceiveCount(int count)
    {
        var result = await _sut.ValidateAsync(
            Valid(maxReceiveCount: count), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetSqsRedrivePolicyCommand.MaxReceiveCount));
    }
}
