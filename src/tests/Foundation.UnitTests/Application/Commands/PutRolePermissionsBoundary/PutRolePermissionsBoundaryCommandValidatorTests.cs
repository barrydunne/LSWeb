using Foundation.Application.Commands.PutRolePermissionsBoundary;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutRolePermissionsBoundary;

public class PutRolePermissionsBoundaryCommandValidatorTests
{
    private readonly PutRolePermissionsBoundaryCommandValidator _sut =
        new(NullLogger<PutRolePermissionsBoundaryCommandValidator>.Instance);

    private static PutRolePermissionsBoundaryCommand Valid(
        string roleName = "svc-role", string permissionsBoundaryArn = "arn:aws:iam::aws:policy/Boundary")
        => new(roleName, permissionsBoundaryArn);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRolePermissionsBoundaryCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenArnEmpty_ReturnsErrorForArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(permissionsBoundaryArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRolePermissionsBoundaryCommand.PermissionsBoundaryArn));
    }

    [Theory]
    [InlineData("Boundary")]
    [InlineData("aws:iam::policy")]
    public async Task ValidateAsync_WhenArnNotArn_ReturnsErrorForArn(string permissionsBoundaryArn)
    {
        var result = await _sut.ValidateAsync(
            Valid(permissionsBoundaryArn: permissionsBoundaryArn), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRolePermissionsBoundaryCommand.PermissionsBoundaryArn));
    }
}
