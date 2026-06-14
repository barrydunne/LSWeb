using Foundation.Application.Commands.DeleteSesIdentity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteSesIdentity;

public class DeleteSesIdentityCommandValidatorTests
{
    private readonly DeleteSesIdentityCommandValidator _sut =
        new(NullLogger<DeleteSesIdentityCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSesIdentityCommand("sender@example.com"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmpty_ReturnsErrorForIdentity()
    {
        var result = await _sut.ValidateAsync(
            new DeleteSesIdentityCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteSesIdentityCommand.Identity));
    }
}
