using Foundation.Application.Commands.SubscribeSnsTopic;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SubscribeSnsTopic;

public class SubscribeSnsTopicCommandValidatorTests
{
    private readonly SubscribeSnsTopicCommandValidator _sut =
        new(NullLogger<SubscribeSnsTopicCommandValidator>.Instance);

    private static SubscribeSnsTopicCommand Valid(
        string topicArn = "arn:aws:sns:eu-west-1:000000000000:topic",
        string protocol = "sqs",
        string endpoint = "arn:aws:sqs:eu-west-1:000000000000:q")
        => new(topicArn, protocol, endpoint);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("sqs")]
    [InlineData("lambda")]
    [InlineData("email")]
    [InlineData("https")]
    public async Task ValidateAsync_WhenProtocolSupported_IsValid(string protocol)
    {
        var result = await _sut.ValidateAsync(
            Valid(protocol: protocol), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTopicArnEmpty_ReturnsErrorForTopicArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(topicArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SubscribeSnsTopicCommand.TopicArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenProtocolUnsupported_ReturnsErrorForProtocol()
    {
        var result = await _sut.ValidateAsync(
            Valid(protocol: "carrier-pigeon"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SubscribeSnsTopicCommand.Protocol));
    }

    [Fact]
    public async Task ValidateAsync_WhenEndpointEmpty_ReturnsErrorForEndpoint()
    {
        var result = await _sut.ValidateAsync(
            Valid(endpoint: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SubscribeSnsTopicCommand.Endpoint));
    }
}
