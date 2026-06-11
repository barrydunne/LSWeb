using Foundation.Application.Commands.CreateLogStream;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateLogStream;

public class CreateLogStreamCommandValidatorTests
{
    private readonly CreateLogStreamCommandValidator _sut =
        new(NullLogger<CreateLogStreamCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CreateLogStreamCommand("/app/orders", "2024/01/01/[$LATEST]abc"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenLogGroupNameEmpty_ReturnsErrorForLogGroupName()
    {
        var result = await _sut.ValidateAsync(
            new CreateLogStreamCommand(string.Empty, "stream-1"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLogStreamCommand.LogGroupName));
    }

    [Fact]
    public async Task ValidateAsync_WhenLogStreamNameEmpty_ReturnsErrorForLogStreamName()
    {
        var result = await _sut.ValidateAsync(
            new CreateLogStreamCommand("/app/orders", string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLogStreamCommand.LogStreamName));
    }

    [Fact]
    public async Task ValidateAsync_WhenLogStreamNameTooLong_ReturnsErrorForLogStreamName()
    {
        var result = await _sut.ValidateAsync(
            new CreateLogStreamCommand("/app/orders", new string('a', 513)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLogStreamCommand.LogStreamName));
    }

    [Theory]
    [InlineData("bad:name")]
    [InlineData("bad*name")]
    public async Task ValidateAsync_WhenLogStreamNameContainsInvalidCharacters_ReturnsErrorForLogStreamName(string streamName)
    {
        var result = await _sut.ValidateAsync(
            new CreateLogStreamCommand("/app/orders", streamName),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLogStreamCommand.LogStreamName));
    }
}
