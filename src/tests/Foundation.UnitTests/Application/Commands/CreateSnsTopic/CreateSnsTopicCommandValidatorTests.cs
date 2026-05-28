using Foundation.Application.Commands.CreateSnsTopic;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateSnsTopic;

public class CreateSnsTopicCommandValidatorTests
{
    private readonly CreateSnsTopicCommandValidator _sut =
        new(NullLogger<CreateSnsTopicCommandValidator>.Instance);

    private static CreateSnsTopicCommand Valid(string name = "orders-topic")
        => new(name);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("orders_topic")]
    [InlineData("Orders-Topic-123")]
    [InlineData("a")]
    public async Task ValidateAsync_WhenNameUsesAllowedCharacters_IsValid(string name)
    {
        var result = await _sut.ValidateAsync(
            Valid(name: name), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSnsTopicCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 257)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSnsTopicCommand.Name));
    }

    [Theory]
    [InlineData("bad name")]
    [InlineData("bad.name")]
    [InlineData("bad/name")]
    [InlineData("bad!name")]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName(string name)
    {
        var result = await _sut.ValidateAsync(
            Valid(name: name), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSnsTopicCommand.Name));
    }
}
