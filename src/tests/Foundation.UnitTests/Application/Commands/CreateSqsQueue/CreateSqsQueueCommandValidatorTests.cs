using Foundation.Application.Commands.CreateSqsQueue;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateSqsQueue;

public class CreateSqsQueueCommandValidatorTests
{
    private readonly CreateSqsQueueCommandValidator _sut =
        new(NullLogger<CreateSqsQueueCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValidStandard_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CreateSqsQueueCommand("orders", false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenValidFifo_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CreateSqsQueueCommand("orders.fifo", true), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameEmpty_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            new CreateSqsQueueCommand(string.Empty, false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSqsQueueCommand.QueueName));
    }

    [Fact]
    public async Task ValidateAsync_WhenFifoNameMissingSuffix_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            new CreateSqsQueueCommand("orders", true), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSqsQueueCommand.QueueName));
    }
}
