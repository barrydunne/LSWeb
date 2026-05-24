using Foundation.Application.Commands.CreateLogGroup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateLogGroup;

public class CreateLogGroupCommandValidatorTests
{
    private readonly CreateLogGroupCommandValidator _sut =
        new(NullLogger<CreateLogGroupCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValidName_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CreateLogGroupCommand("/app/orders.log-1#2"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForLogGroupName()
    {
        var result = await _sut.ValidateAsync(
            new CreateLogGroupCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLogGroupCommand.LogGroupName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForLogGroupName()
    {
        var result = await _sut.ValidateAsync(
            new CreateLogGroupCommand(new string('a', 513)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLogGroupCommand.LogGroupName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForLogGroupName()
    {
        var result = await _sut.ValidateAsync(
            new CreateLogGroupCommand("bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateLogGroupCommand.LogGroupName));
    }
}
