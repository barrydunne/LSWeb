using Foundation.Application.Commands.PurgeSqsQueue;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PurgeSqsQueue;

public class PurgeSqsQueueCommandValidatorTests
{
    private readonly PurgeSqsQueueCommandValidator _sut =
        new(NullLogger<PurgeSqsQueueCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new PurgeSqsQueueCommand("orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQueueNameEmpty_ReturnsErrorForQueueName()
    {
        var result = await _sut.ValidateAsync(
            new PurgeSqsQueueCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PurgeSqsQueueCommand.QueueName));
    }
}
