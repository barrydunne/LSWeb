using Foundation.Application.Commands.UnsubscribeSnsTopic;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UnsubscribeSnsTopic;

public class UnsubscribeSnsTopicCommandValidatorTests
{
    private readonly UnsubscribeSnsTopicCommandValidator _sut =
        new(NullLogger<UnsubscribeSnsTopicCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new UnsubscribeSnsTopicCommand("arn:aws:sns:eu-west-1:000000000000:topic:sub"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmpty_ReturnsErrorForSubscriptionArn()
    {
        var result = await _sut.ValidateAsync(
            new UnsubscribeSnsTopicCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UnsubscribeSnsTopicCommand.SubscriptionArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenNotAnArn_ReturnsErrorForSubscriptionArn()
    {
        var result = await _sut.ValidateAsync(
            new UnsubscribeSnsTopicCommand("PendingConfirmation"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UnsubscribeSnsTopicCommand.SubscriptionArn));
    }
}
