using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSecretVersions;
using Foundation.Application.SecretsManager;
using Foundation.Domain.SecretsManager;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSecretVersions;

public class ListSecretVersionsQueryHandlerTests
{
    private readonly ISecretsManagerClient _client = Substitute.For<ISecretsManagerClient>();

    private ListSecretVersionsQueryHandler CreateSut()
        => new(_client, NullLogger<ListSecretVersionsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsVersions()
    {
        // Arrange
        var versions = new SecretVersionList(
            "db-password",
            "arn:db-password",
            [
                new("v2", ["AWSCURRENT"], null, null),
                new("v1", ["AWSPREVIOUS"], null, null),
            ]);
        _client
            .ListSecretVersionsAsync("db-password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(versions)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSecretVersionsQuery("db-password"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("db-password");
        result.Value.Arn.Should().Be("arn:db-password");
        result.Value.Versions.Should().ContainSingle(_ => _.VersionId == "v2" && _.Stages.Contains("AWSCURRENT"));
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListSecretVersionsAsync("db-password", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SecretVersionList>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSecretVersionsQuery("db-password"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
