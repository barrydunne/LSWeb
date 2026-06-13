using Foundation.Application.Commands.CreateEventBridgeEventBus;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateEventBridgeEventBus;

public class CreateEventBridgeEventBusCommandValidatorTests
{
    private readonly CreateEventBridgeEventBusCommandValidator _sut =
        new(NullLogger<CreateEventBridgeEventBusCommandValidator>.Instance);

    private static CreateEventBridgeEventBusCommand Build(string name = "orders-bus")
        => new(name);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Build(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(Build(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateEventBridgeEventBusCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameHasInvalidCharacters_ReturnsError()
    {
        var result = await _sut.ValidateAsync(Build("bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameIsDefault_ReturnsError()
    {
        var result = await _sut.ValidateAsync(Build("default"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Build(new string('a', 257)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }
}
