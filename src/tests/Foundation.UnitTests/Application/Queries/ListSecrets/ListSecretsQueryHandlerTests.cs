using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSecrets;
using Foundation.Application.SecretsManager;
using Foundation.Domain.SecretsManager;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSecrets;

public class ListSecretsQueryHandlerTests
{
    private readonly ISecretsManagerClient _client = Substitute.For<ISecretsManagerClient>();

    private ListSecretsQueryHandler CreateSut()
        => new(_client, NullLogger<ListSecretsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsSecrets()
    {
        // Arrange
        IReadOnlyList<Secret> secrets =
        [
            new("db-password", "arn:db-password", "primary db", null, null),
        ];
        _client
            .ListSecretsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(secrets)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSecretsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Secrets.Should().ContainSingle(_ => _.Name == "db-password");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListSecretsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<Secret>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSecretsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
