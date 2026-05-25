using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.SecretsManager;
using Foundation.Domain.SecretsManager;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class SecretsManagerResourceSourceTests
{
    private readonly ISecretsManagerClient _client = Substitute.For<ISecretsManagerClient>();

    private SecretsManagerResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsSecretsManager()
        => CreateSut().ServiceKey.Should().Be("secrets-manager");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsSecretsToSearchEntries()
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
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("secrets-manager");
        entry.ResourceId.Should().Be("db-password");
        entry.DisplayName.Should().Be("db-password");
        entry.Route.Should().Be("/services/secrets-manager/db-password");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListSecretsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<Secret>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
