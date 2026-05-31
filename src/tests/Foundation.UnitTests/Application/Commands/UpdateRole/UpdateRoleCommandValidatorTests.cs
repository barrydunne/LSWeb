using Foundation.Application.Commands.UpdateRole;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateRole;

public class UpdateRoleCommandValidatorTests
{
    private readonly UpdateRoleCommandValidator _sut =
        new(NullLogger<UpdateRoleCommandValidator>.Instance);

    private static UpdateRoleCommand Valid(
        string roleName = "deploy-role",
        string? description = "Deploy role",
        int? maxSessionDuration = null)
        => new(roleName, description, maxSessionDuration);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("deploy-role")]
    [InlineData("Role-123")]
    [InlineData("role+tag=a.b@c_d-e")]
    public async Task ValidateAsync_WhenNameUsesAllowedCharacters_IsValid(string name)
    {
        var result = await _sut.ValidateAsync(Valid(roleName: name), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForRoleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(roleName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRoleCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForRoleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(roleName: new string('a', 65)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRoleCommand.RoleName));
    }

    [Theory]
    [InlineData("bad name")]
    [InlineData("bad/name")]
    [InlineData("bad!name")]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForRoleName(string name)
    {
        var result = await _sut.ValidateAsync(Valid(roleName: name), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRoleCommand.RoleName));
    }

    [Theory]
    [InlineData(3600)]
    [InlineData(43200)]
    public async Task ValidateAsync_WhenMaxSessionDurationInRange_IsValid(int seconds)
    {
        var result = await _sut.ValidateAsync(
            Valid(maxSessionDuration: seconds), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(3599)]
    [InlineData(43201)]
    public async Task ValidateAsync_WhenMaxSessionDurationOutOfRange_ReturnsErrorForMaxSessionDuration(int seconds)
    {
        var result = await _sut.ValidateAsync(
            Valid(maxSessionDuration: seconds), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRoleCommand.MaxSessionDuration));
    }
}
