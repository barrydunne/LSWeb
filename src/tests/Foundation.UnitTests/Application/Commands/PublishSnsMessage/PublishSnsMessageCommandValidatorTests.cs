using Foundation.Application.Commands.PublishSnsMessage;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PublishSnsMessage;

public class PublishSnsMessageCommandValidatorTests
{
    private readonly PublishSnsMessageCommandValidator _sut =
        new(NullLogger<PublishSnsMessageCommandValidator>.Instance);

    private static PublishSnsMessageCommand Command(
        string topicArn = "arn:aws:sns:eu-west-1:000000000000:orders",
        string? subject = "Subject",
        string message = "hello")
        => new(topicArn, subject, message, new Dictionary<string, string>());

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Command(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSubjectNull_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Command(subject: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTopicArnEmpty_ReturnsErrorForTopicArn()
    {
        var result = await _sut.ValidateAsync(
            Command(topicArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PublishSnsMessageCommand.TopicArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenMessageEmpty_ReturnsErrorForMessage()
    {
        var result = await _sut.ValidateAsync(
            Command(message: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PublishSnsMessageCommand.Message));
    }
}
