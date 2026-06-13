using Foundation.Application.Commands.DeleteEventBridgeEventBus;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteEventBridgeEventBus;

public class DeleteEventBridgeEventBusCommandValidatorTests
{
    private readonly DeleteEventBridgeEventBusCommandValidator _sut =
        new(NullLogger<DeleteEventBridgeEventBusCommandValidator>.Instance);

    private static DeleteEventBridgeEventBusCommand Build(string name = "orders-bus")
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteEventBridgeEventBusCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameIsDefault_ReturnsError()
    {
        var result = await _sut.ValidateAsync(Build("default"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
    }
}
