using Foundation.Application.Commands.PutUserPermissionsBoundary;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutUserPermissionsBoundary;

public class PutUserPermissionsBoundaryCommandValidatorTests
{
    private readonly PutUserPermissionsBoundaryCommandValidator _sut =
        new(NullLogger<PutUserPermissionsBoundaryCommandValidator>.Instance);

    private static PutUserPermissionsBoundaryCommand Valid(
        string userName = "alice", string permissionsBoundaryArn = "arn:aws:iam::aws:policy/Boundary")
        => new(userName, permissionsBoundaryArn);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserNameEmpty_ReturnsErrorForUserName()
    {
        var result = await _sut.ValidateAsync(
            Valid(userName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutUserPermissionsBoundaryCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenArnEmpty_ReturnsErrorForArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(permissionsBoundaryArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutUserPermissionsBoundaryCommand.PermissionsBoundaryArn));
    }

    [Theory]
    [InlineData("Boundary")]
    [InlineData("aws:iam::policy")]
    public async Task ValidateAsync_WhenArnNotArn_ReturnsErrorForArn(string permissionsBoundaryArn)
    {
        var result = await _sut.ValidateAsync(
            Valid(permissionsBoundaryArn: permissionsBoundaryArn), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutUserPermissionsBoundaryCommand.PermissionsBoundaryArn));
    }
}
