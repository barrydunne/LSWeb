using Foundation.Application.Commands.DeleteRole;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteRole;

public class DeleteRoleCommandValidatorTests
{
    private readonly DeleteRoleCommandValidator _sut =
        new(NullLogger<DeleteRoleCommandValidator>.Instance);

    private static DeleteRoleCommand Valid(string name = "deploy-role")
        => new(name);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForRoleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteRoleCommand.RoleName));
    }
}
