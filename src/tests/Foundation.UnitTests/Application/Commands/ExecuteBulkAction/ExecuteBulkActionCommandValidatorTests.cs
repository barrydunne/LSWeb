using Foundation.Application.Commands.ExecuteBulkAction;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ExecuteBulkAction;

public class ExecuteBulkActionCommandValidatorTests
{
    private readonly ExecuteBulkActionCommandValidator _sut =
        new(NullLogger<ExecuteBulkActionCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenActionAndResourcesProvided_IsValid()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new ExecuteBulkActionCommand("delete", ["a"]), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenActionEmpty_ReturnsErrorForAction()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new ExecuteBulkActionCommand(string.Empty, ["a"]), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteBulkActionCommand.Action));
    }

    [Fact]
    public async Task ValidateAsync_WhenResourceIdsEmpty_ReturnsErrorForResourceIds()
    {
        // Act
        var result = await _sut.ValidateAsync(
            new ExecuteBulkActionCommand("delete", []), TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ExecuteBulkActionCommand.ResourceIds));
    }
}
