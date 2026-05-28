using Foundation.Application.Commands.DeleteSnsTopic;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteSnsTopic;

public class DeleteSnsTopicCommandValidatorTests
{
    private readonly DeleteSnsTopicCommandValidator _sut =
        new(NullLogger<DeleteSnsTopicCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSnsTopicCommand("arn:aws:sns:eu-west-1:000000000000:orders-topic"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenTopicArnEmpty_ReturnsErrorForTopicArn()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSnsTopicCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteSnsTopicCommand.TopicArn));
    }
}
