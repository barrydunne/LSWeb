using Foundation.Application.Commands.SetSnsSubscriptionFilterPolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetSnsSubscriptionFilterPolicy;

public class SetSnsSubscriptionFilterPolicyCommandValidatorTests
{
    private readonly SetSnsSubscriptionFilterPolicyCommandValidator _sut =
        new(NullLogger<SetSnsSubscriptionFilterPolicyCommandValidator>.Instance);

    private static SetSnsSubscriptionFilterPolicyCommand Command(
        string subscriptionArn = "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f",
        string filterPolicy = "{\"store\":[\"example_corp\"]}")
        => new(subscriptionArn, filterPolicy);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Command(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFilterPolicyEmpty_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Command(filterPolicy: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSubscriptionArnEmpty_ReturnsErrorForSubscriptionArn()
    {
        var result = await _sut.ValidateAsync(
            Command(subscriptionArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            _ => _.PropertyName == nameof(SetSnsSubscriptionFilterPolicyCommand.SubscriptionArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenFilterPolicyNull_ReturnsErrorForFilterPolicy()
    {
        var result = await _sut.ValidateAsync(
            Command(filterPolicy: null!), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            _ => _.PropertyName == nameof(SetSnsSubscriptionFilterPolicyCommand.FilterPolicy));
    }
}
