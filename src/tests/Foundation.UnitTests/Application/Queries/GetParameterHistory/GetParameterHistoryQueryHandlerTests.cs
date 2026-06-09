using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Diagnostics;
using Foundation.Application.Queries.GetParameterHistory;
using Foundation.Application.Ssm;
using Foundation.Domain.Configuration;
using Foundation.Domain.Ssm;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetParameterHistory;

public class GetParameterHistoryQueryHandlerTests
{
    private readonly ISsmClient _client = Substitute.For<ISsmClient>();
    private readonly IRedactionService _redaction = Substitute.For<IRedactionService>();

    private static Result<T> Ok<T>(T value) => value;

    private GetParameterHistoryQueryHandler CreateSut()
        => new(_client, _redaction, NullLogger<GetParameterHistoryQueryHandler>.Instance);

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
    public async Task Handle_WhenSecureStringAndRevealNotRequested_ReturnsMaskedValues()
    {
        // Arrange
        var modified = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        _client
            .GetParameterHistoryAsync("/app/db/password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(new ParameterHistoryList(
                "/app/db/password",
                [
                    new("SecureString", 2, "newer", modified, "arn:user/admin"),
                    new("SecureString", 1, "older", modified, "arn:user/admin"),
                ]))));
        StubRedaction(canReveal: true);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterHistoryQuery("/app/db/password", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("/app/db/password");
        result.Value.RevealAllowed.Should().BeTrue();
        result.Value.Entries.Should().HaveCount(2);
        result.Value.Entries[0].Version.Should().Be(2);
        result.Value.Entries[0].Value.Should().Be(ConfigValue.Mask);
        result.Value.Entries[0].Type.Should().Be("SecureString");
        result.Value.Entries[0].LastModifiedDate.Should().Be(modified);
        result.Value.Entries[0].LastModifiedUser.Should().Be("arn:user/admin");
        result.Value.Entries[0].IsSensitive.Should().BeTrue();
        result.Value.Entries[1].Value.Should().Be(ConfigValue.Mask);
    }

    [Fact]
    public async Task Handle_WhenSecureStringAndRevealRequestedAndAllowed_ReturnsRawValues()
    {
        // Arrange
        _client
            .GetParameterHistoryAsync("/app/db/password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(new ParameterHistoryList(
                "/app/db/password",
                [new("SecureString", 1, "s3cr3t", null, "arn:user/admin")]))));
        StubRedaction(canReveal: true);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterHistoryQuery("/app/db/password", true),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Entries.Should().ContainSingle();
        result.Value.Entries[0].Value.Should().Be("s3cr3t");
        result.Value.Entries[0].IsSensitive.Should().BeTrue();
        result.Value.RevealAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenStringParameterAndRevealNotRequested_ReturnsValuesMaskedAndSensitive()
    {
        // Arrange
        ConfigValue? captured = null;
        _client
            .GetParameterHistoryAsync("/app/feature/flag", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(new ParameterHistoryList(
                "/app/feature/flag",
                [new("String", 1, "enabled", null, "arn:user/admin")]))));
        _redaction.CanReveal.Returns(false);
        _redaction
            .Resolve(Arg.Do<ConfigValue>(value => captured = value), Arg.Any<bool>())
            .Returns(call => call.Arg<ConfigValue>().Display);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterHistoryQuery("/app/feature/flag", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Entries[0].Value.Should().Be(ConfigValue.Mask);
        result.Value.Entries[0].IsSensitive.Should().BeTrue();
        result.Value.RevealAllowed.Should().BeFalse();
        captured.Should().NotBeNull();
        captured!.IsSensitive.Should().BeTrue();
        captured.Source.Should().Be(ConfigSource.Default);
    }

    [Fact]
    public async Task Handle_WhenHistoryEmpty_ReturnsEmptyEntries()
    {
        // Arrange
        _client
            .GetParameterHistoryAsync("/app/empty", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(new ParameterHistoryList("/app/empty", []))));
        StubRedaction(canReveal: false);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterHistoryQuery("/app/empty", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Entries.Should().BeEmpty();
        result.Value.RevealAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetParameterHistoryAsync("/missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ParameterHistoryList>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetParameterHistoryQuery("/missing", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
