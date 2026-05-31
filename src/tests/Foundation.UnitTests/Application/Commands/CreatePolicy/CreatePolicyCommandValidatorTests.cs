using Foundation.Application.Commands.CreatePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreatePolicy;

public class CreatePolicyCommandValidatorTests
{
    private const string ValidDocument = "{\"Version\":\"2012-10-17\",\"Statement\":[]}";

    private readonly CreatePolicyCommandValidator _sut =
        new(NullLogger<CreatePolicyCommandValidator>.Instance);

    private static CreatePolicyCommand Valid(
        string policyName = "deploy-policy",
        string document = ValidDocument,
        string? description = null,
        string? path = null)
        => new(policyName, document, description, path);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("deploy-policy")]
    [InlineData("Policy-123")]
    [InlineData("policy+tag=a.b@c_d-e")]
    public async Task ValidateAsync_WhenNameUsesAllowedCharacters_IsValid(string name)
    {
        var result = await _sut.ValidateAsync(Valid(policyName: name), TestContext.Current.CancellationToken);
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
            Valid(description: "A deploy policy"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForPolicyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyCommand.PolicyName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForPolicyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyName: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyCommand.PolicyName));
    }

    [Theory]
    [InlineData("bad name")]
    [InlineData("bad/name")]
    [InlineData("bad!name")]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForPolicyName(string name)
    {
        var result = await _sut.ValidateAsync(Valid(policyName: name), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyCommand.PolicyName));
    }

    [Fact]
    public async Task ValidateAsync_WhenDocumentEmpty_ReturnsErrorForPolicyDocument()
    {
        var result = await _sut.ValidateAsync(
            Valid(document: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyCommand.PolicyDocument));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    public async Task ValidateAsync_WhenDocumentNotJsonObject_ReturnsErrorForPolicyDocument(string document)
    {
        var result = await _sut.ValidateAsync(Valid(document: document), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyCommand.PolicyDocument));
    }

    [Theory]
    [InlineData("team/")]
    [InlineData("/team")]
    [InlineData("team")]
    public async Task ValidateAsync_WhenPathInvalid_ReturnsErrorForPath(string path)
    {
        var result = await _sut.ValidateAsync(Valid(path: path), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyCommand.Path));
    }

    [Fact]
    public async Task ValidateAsync_WhenDescriptionTooLong_ReturnsErrorForDescription()
    {
        var result = await _sut.ValidateAsync(
            Valid(description: new string('a', 1001)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreatePolicyCommand.Description));
    }
}
