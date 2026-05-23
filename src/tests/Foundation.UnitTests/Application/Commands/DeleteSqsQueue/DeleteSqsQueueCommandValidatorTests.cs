using Foundation.Application.Commands.DeleteSqsQueue;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteSqsQueue;

public class DeleteSqsQueueCommandValidatorTests
{
    private readonly DeleteSqsQueueCommandValidator _sut =
        new(NullLogger<DeleteSqsQueueCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSqsQueueCommand("orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameEmpty_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSqsQueueCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteSqsQueueCommand.QueueName));
    }
}
