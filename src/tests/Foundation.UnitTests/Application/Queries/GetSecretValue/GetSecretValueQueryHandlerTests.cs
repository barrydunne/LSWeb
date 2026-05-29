using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Diagnostics;
using Foundation.Application.Queries.GetSecretValue;
using Foundation.Application.SecretsManager;
using Foundation.Domain.Configuration;
using Foundation.Domain.SecretsManager;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSecretValue;

public class GetSecretValueQueryHandlerTests
{
    private readonly ISecretsManagerClient _client = Substitute.For<ISecretsManagerClient>();
    private readonly IRedactionService _redaction = Substitute.For<IRedactionService>();

    private static Result<T> Ok<T>(T value) => value;

    private GetSecretValueQueryHandler CreateSut()
        => new(_client, _redaction, NullLogger<GetSecretValueQueryHandler>.Instance);

    private void StubRedaction()
    {
        _redaction
            .ResolveUserSecret(Arg.Any<ConfigValue>(), Arg.Any<bool>())
            .Returns(call =>
            {
                var value = call.Arg<ConfigValue>();
                var reveal = call.Arg<bool>();
                return value.IsSensitive && !reveal ? ConfigValue.Mask : value.Value;
            });
    }

    [Fact]
    public async Task Handle_WhenRevealNotRequested_ReturnsMaskedValue()
    {
        // Arrange
        _client
            .GetSecretValueAsync("db-password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(new SecretValue("db-password", "arn:secret", "v1", "s3cr3t"))));
        StubRedaction();
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSecretValueQuery("db-password", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("db-password");
        result.Value.Arn.Should().Be("arn:secret");
        result.Value.VersionId.Should().Be("v1");
        result.Value.Value.Should().Be(ConfigValue.Mask);
        result.Value.RevealAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRevealRequested_ReturnsRawValue()
    {
        // Arrange
        _client
            .GetSecretValueAsync("db-password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(new SecretValue("db-password", "arn:secret", "v1", "s3cr3t"))));
        StubRedaction();
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSecretValueQuery("db-password", true),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("s3cr3t");
        result.Value.RevealAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AlwaysAllowsReveal_IndependentOfHostDiagnosticGate()
    {
        // Arrange
        _client
            .GetSecretValueAsync("db-password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(new SecretValue("db-password", "arn:secret", null, "s3cr3t"))));
        _redaction.CanReveal.Returns(false);
        StubRedaction();
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSecretValueQuery("db-password", true),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.VersionId.Should().BeNull();
        result.Value.Value.Should().Be("s3cr3t");
        result.Value.RevealAllowed.Should().BeTrue();
        _redaction.DidNotReceive().Resolve(Arg.Any<ConfigValue>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetSecretValueAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SecretValue>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSecretValueQuery("missing", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
