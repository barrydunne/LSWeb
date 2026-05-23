using Foundation.Application.Commands.DeleteSqsMessage;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteSqsMessage;

public class DeleteSqsMessageCommandValidatorTests
{
    private readonly DeleteSqsMessageCommandValidator _sut =
        new(NullLogger<DeleteSqsMessageCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSqsMessageCommand("orders", "receipt-1"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameEmpty_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSqsMessageCommand(string.Empty, "receipt-1"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteSqsMessageCommand.QueueName));
    }

    [Fact]
    public async Task ValidateAsync_WhenReceiptHandleEmpty_ReturnsErrorForReceiptHandle()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSqsMessageCommand("orders", string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteSqsMessageCommand.ReceiptHandle));
    }
}
