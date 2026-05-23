using Foundation.Application.Commands.SendSqsMessage;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SendSqsMessage;

public class SendSqsMessageCommandValidatorTests
{
    private readonly SendSqsMessageCommandValidator _sut =
        new(NullLogger<SendSqsMessageCommandValidator>.Instance);

    private static SendSqsMessageCommand Command(
        string queueName = "orders",
        string body = "hello",
        string? messageGroupId = null)
        => new(queueName, body, new Dictionary<string, string>(), messageGroupId, null);

    [Fact]
    public async Task ValidateAsync_WhenStandardQueueAndValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Command(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenFifoQueueWithGroupId_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Command("orders.fifo", messageGroupId: "group-1"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameEmpty_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            Command(queueName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SendSqsMessageCommand.QueueName));
    }

    [Fact]
    public async Task ValidateAsync_WhenBodyEmpty_ReturnsErrorForBody()
    {
        var result = await _sut.ValidateAsync(
            Command(body: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SendSqsMessageCommand.Body));
    }

    [Fact]
    public async Task ValidateAsync_WhenFifoQueueWithoutGroupId_ReturnsErrorForGroupId()
    {
        var result = await _sut.ValidateAsync(
            Command("orders.fifo"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SendSqsMessageCommand.MessageGroupId));
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameNull_DoesNotEvaluateFifoRule()
    {
        var result = await _sut.ValidateAsync(
            Command(queueName: null!), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SendSqsMessageCommand.QueueName));
        result.Errors.Should().NotContain(_ => _.PropertyName == nameof(SendSqsMessageCommand.MessageGroupId));
    }
}
