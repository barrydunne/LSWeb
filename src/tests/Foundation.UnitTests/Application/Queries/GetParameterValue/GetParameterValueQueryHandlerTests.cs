using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Diagnostics;
using Foundation.Application.Queries.GetParameterValue;
using Foundation.Application.Ssm;
using Foundation.Domain.Configuration;
using Foundation.Domain.Ssm;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetParameterValue;

public class GetParameterValueQueryHandlerTests
{
    private readonly ISsmClient _client = Substitute.For<ISsmClient>();
    private readonly IRedactionService _redaction = Substitute.For<IRedactionService>();

    private static Result<T> Ok<T>(T value) => value;

    private GetParameterValueQueryHandler CreateSut()
        => new(_client, _redaction, NullLogger<GetParameterValueQueryHandler>.Instance);

    private void StubRedaction(bool canReveal)
    {
        _redaction.CanReveal.Returns(canReveal);
        _redaction
            .Resolve(Arg.Any<ConfigValue>(), Arg.Any<bool>())
            .Returns(call =>
            {
                var value = call.Arg<ConfigValue>();
                var reveal = call.Arg<bool>();
                return value.IsSensitive && !reveal ? ConfigValue.Mask : value.Value;
            });
    }

    [Fact]
    public async Task Handle_WhenSecureStringAndRevealNotRequested_ReturnsMaskedValue()
    {
        // Arrange
        _client
            .GetParameterValueAsync("/app/db/password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(
                new ParameterValue("/app/db/password", "SecureString", 4, "s3cr3t", "arn:/app/db/password"))));
        StubRedaction(canReveal: true);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterValueQuery("/app/db/password", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("/app/db/password");
        result.Value.Type.Should().Be("SecureString");
        result.Value.Version.Should().Be(4);
        result.Value.Value.Should().Be(ConfigValue.Mask);
        result.Value.Arn.Should().Be("arn:/app/db/password");
        result.Value.IsSensitive.Should().BeTrue();
        result.Value.RevealAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSecureStringAndRevealRequestedAndAllowed_ReturnsRawValue()
    {
        // Arrange
        _client
            .GetParameterValueAsync("/app/db/password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(
                new ParameterValue("/app/db/password", "SecureString", 4, "s3cr3t", "arn:/app/db/password"))));
        StubRedaction(canReveal: true);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterValueQuery("/app/db/password", true),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("s3cr3t");
        result.Value.IsSensitive.Should().BeTrue();
        result.Value.RevealAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSecureStringAndRevealRequestedButDisallowed_ReturnsMaskedValue()
    {
        // Arrange
        _client
            .GetParameterValueAsync("/app/db/password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(
                new ParameterValue("/app/db/password", "SecureString", 4, "s3cr3t", "arn:/app/db/password"))));
        _redaction.CanReveal.Returns(false);
        _redaction
            .Resolve(Arg.Any<ConfigValue>(), Arg.Any<bool>())
            .Returns(ConfigValue.Mask);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterValueQuery("/app/db/password", true),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ConfigValue.Mask);
        result.Value.IsSensitive.Should().BeTrue();
        result.Value.RevealAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenStringParameterAndRevealNotRequested_ReturnsValueMaskedAndSensitive()
    {
        // Arrange
        ConfigValue? captured = null;
        _client
            .GetParameterValueAsync("/app/feature/flag", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(
                new ParameterValue("/app/feature/flag", "String", 1, "enabled", "arn:/app/feature/flag"))));
        _redaction.CanReveal.Returns(false);
        _redaction
            .Resolve(Arg.Do<ConfigValue>(value => captured = value), Arg.Any<bool>())
            .Returns(call => call.Arg<ConfigValue>().Display);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterValueQuery("/app/feature/flag", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ConfigValue.Mask);
        result.Value.IsSensitive.Should().BeTrue();
        result.Value.RevealAllowed.Should().BeFalse();
        captured.Should().NotBeNull();
        captured!.IsSensitive.Should().BeTrue();
        captured.Source.Should().Be(ConfigSource.Default);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetParameterValueAsync("/missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ParameterValue>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterValueQuery("/missing", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
