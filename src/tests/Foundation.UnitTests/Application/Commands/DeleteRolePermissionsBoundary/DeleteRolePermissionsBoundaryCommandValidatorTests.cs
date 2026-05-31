using Foundation.Application.Commands.DeleteRolePermissionsBoundary;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteRolePermissionsBoundary;

public class DeleteRolePermissionsBoundaryCommandValidatorTests
{
    private readonly DeleteRolePermissionsBoundaryCommandValidator _sut =
        new(NullLogger<DeleteRolePermissionsBoundaryCommandValidator>.Instance);

    private static DeleteRolePermissionsBoundaryCommand Valid(string roleName = "svc-role")
        => new(roleName);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenRoleNameEmpty_ReturnsErrorForRoleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(roleName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteRolePermissionsBoundaryCommand.RoleName));
    }
}
