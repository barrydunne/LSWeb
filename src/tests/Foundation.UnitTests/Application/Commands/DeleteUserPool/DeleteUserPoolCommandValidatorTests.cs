using Foundation.Application.Commands.DeleteUserPool;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteUserPool;

public class DeleteUserPoolCommandValidatorTests
{
    private readonly DeleteUserPoolCommandValidator _sut =
        new(NullLogger<DeleteUserPoolCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteUserPoolCommand("eu-west-1_abc123"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenIdEmpty_ReturnsErrorForId()
    {
        var result = await _sut.ValidateAsync(
            new DeleteUserPoolCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteUserPoolCommand.Id));
    }
}
