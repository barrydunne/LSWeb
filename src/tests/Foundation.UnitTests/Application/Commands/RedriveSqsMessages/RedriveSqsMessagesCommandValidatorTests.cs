using Foundation.Application.Commands.RedriveSqsMessages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RedriveSqsMessages;

public class RedriveSqsMessagesCommandValidatorTests
{
    private readonly RedriveSqsMessagesCommandValidator _sut =
        new(NullLogger<RedriveSqsMessagesCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new RedriveSqsMessagesCommand("orders-dlq"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameEmpty_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            new RedriveSqsMessagesCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RedriveSqsMessagesCommand.QueueName));
    }
}
