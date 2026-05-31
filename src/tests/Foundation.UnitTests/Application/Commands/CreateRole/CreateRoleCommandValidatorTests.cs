using Foundation.Application.Commands.CreateRole;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateRole;

public class CreateRoleCommandValidatorTests
{
    private const string ValidTrust = "{\"Version\":\"2012-10-17\",\"Statement\":[]}";

    private readonly CreateRoleCommandValidator _sut =
        new(NullLogger<CreateRoleCommandValidator>.Instance);

    private static CreateRoleCommand Valid(
        string roleName = "deploy-role",
        string? path = null,
        string trust = ValidTrust,
        string? description = null,
        int? maxSessionDuration = null)
        => new(roleName, path, trust, description, maxSessionDuration);

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

    [Theory]
    [InlineData("/")]
    [InlineData("/team/")]
    [InlineData("/team/sub/")]
    public async Task ValidateAsync_WhenPathValid_IsValid(string path)
    {
        var result = await _sut.ValidateAsync(Valid(path: path), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDescriptionProvided_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(description: "A deploy role"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForRoleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(roleName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoleCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForRoleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(roleName: new string('a', 65)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoleCommand.RoleName));
    }

    [Theory]
    [InlineData("bad name")]
    [InlineData("bad/name")]
    [InlineData("bad!name")]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForRoleName(string name)
    {
        var result = await _sut.ValidateAsync(Valid(roleName: name), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoleCommand.RoleName));
    }

    [Theory]
    [InlineData("team/")]
    [InlineData("/team")]
    [InlineData("team")]
    public async Task ValidateAsync_WhenPathInvalid_ReturnsErrorForPath(string path)
    {
        var result = await _sut.ValidateAsync(Valid(path: path), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoleCommand.Path));
    }

    [Fact]
    public async Task ValidateAsync_WhenTrustPolicyEmpty_ReturnsErrorForAssumeRolePolicyDocument()
    {
        var result = await _sut.ValidateAsync(
            Valid(trust: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoleCommand.AssumeRolePolicyDocument));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    public async Task ValidateAsync_WhenTrustPolicyNotJsonObject_ReturnsErrorForAssumeRolePolicyDocument(string document)
    {
        var result = await _sut.ValidateAsync(
            Valid(trust: document), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoleCommand.AssumeRolePolicyDocument));
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRoleCommand.MaxSessionDuration));
    }
}
