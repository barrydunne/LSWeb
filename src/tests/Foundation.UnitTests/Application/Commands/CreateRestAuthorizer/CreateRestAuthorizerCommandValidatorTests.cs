using Foundation.Application.Commands.CreateRestAuthorizer;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateRestAuthorizer;

public class CreateRestAuthorizerCommandValidatorTests
{
    private const string ValidArn =
        "arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc";

    private readonly CreateRestAuthorizerCommandValidator _sut =
        new(NullLogger<CreateRestAuthorizerCommandValidator>.Instance);

    private static CreateRestAuthorizerCommand Valid(
        string restApiId = "api-1",
        string name = "pool-authorizer",
        string type = "COGNITO_USER_POOLS",
        IReadOnlyList<string>? providerArns = null,
        string? identitySource = null)
        => new(restApiId, name, type, providerArns ?? [ValidArn], identitySource);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenRestApiIdEmpty_ReturnsErrorForRestApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(restApiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestAuthorizerCommand.RestApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestAuthorizerCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 257)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestAuthorizerCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenTypeEmpty_ReturnsErrorForType()
    {
        var result = await _sut.ValidateAsync(
            Valid(type: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestAuthorizerCommand.Type));
    }

    [Fact]
    public async Task ValidateAsync_WhenTypeNotCognito_ReturnsErrorForType()
    {
        var result = await _sut.ValidateAsync(
            Valid(type: "TOKEN"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestAuthorizerCommand.Type));
    }

    [Fact]
    public async Task ValidateAsync_WhenProviderArnsEmpty_ReturnsErrorForProviderArns()
    {
        var result = await _sut.ValidateAsync(
            Valid(providerArns: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestAuthorizerCommand.ProviderARNs));
    }

    [Fact]
    public async Task ValidateAsync_WhenProviderArnNotCognitoUserPool_ReturnsErrorForProviderArns()
    {
        var result = await _sut.ValidateAsync(
            Valid(providerArns: ["arn:aws:iam::000000000000:role/example"]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestAuthorizerCommand.ProviderARNs));
    }
}
