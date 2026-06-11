using Foundation.Application.Commands.DeleteLogStream;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteLogStream;

public class DeleteLogStreamCommandValidatorTests
{
    private readonly DeleteLogStreamCommandValidator _sut =
        new(NullLogger<DeleteLogStreamCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLogStreamCommand("/app/orders", "stream-1"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenLogGroupNameEmpty_ReturnsErrorForLogGroupName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLogStreamCommand(string.Empty, "stream-1"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteLogStreamCommand.LogGroupName));
    }

    [Fact]
    public async Task ValidateAsync_WhenLogStreamNameEmpty_ReturnsErrorForLogStreamName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteLogStreamCommand("/app/orders", string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteLogStreamCommand.LogStreamName));
    }
}
