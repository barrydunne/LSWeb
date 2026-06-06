using Foundation.Application.Commands.UpdateUserPoolClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateUserPoolClient;

public class UpdateUserPoolClientCommandValidatorTests
{
    private readonly UpdateUserPoolClientCommandValidator _sut =
        new(NullLogger<UpdateUserPoolClientCommandValidator>.Instance);

    private static UpdateUserPoolClientCommand Valid(
        string userPoolId = "eu-west-1_abc123",
        string clientId = "client-1",
        string clientName = "web",
        IReadOnlyList<string>? allowedOAuthFlows = null)
        => new(
            userPoolId,
            clientId,
            clientName,
            [],
            allowedOAuthFlows ?? ["code"],
            [],
            [],
            false);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserPoolIdEmpty_ReturnsErrorForUserPoolId()
    {
        var result = await _sut.ValidateAsync(
            Valid(userPoolId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateUserPoolClientCommand.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientIdEmpty_ReturnsErrorForClientId()
    {
        var result = await _sut.ValidateAsync(
            Valid(clientId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateUserPoolClientCommand.ClientId));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientNameEmpty_ReturnsErrorForClientName()
    {
        var result = await _sut.ValidateAsync(
            Valid(clientName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateUserPoolClientCommand.ClientName));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientNameContainsInvalidCharacters_ReturnsErrorForClientName()
    {
        var result = await _sut.ValidateAsync(
            Valid(clientName: "bad/name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateUserPoolClientCommand.ClientName));
    }

    [Fact]
    public async Task ValidateAsync_WhenOAuthFlowInvalid_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(allowedOAuthFlows: ["password"]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage.Contains("Allowed OAuth flows"));
    }
}
