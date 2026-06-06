using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Domain.Cognito;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class CognitoResourceSourceTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();

    private CognitoResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsCognito()
        => CreateSut().ServiceKey.Should().Be("cognito");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsUserPoolsToSearchEntries()
    {
        // Arrange
        IReadOnlyList<UserPoolSummary> userPools =
        [
            new("eu-west-1_abc123", "customers", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListUserPoolsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(userPools)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("cognito");
        entry.ResourceId.Should().Be("eu-west-1_abc123");
        entry.DisplayName.Should().Be("customers");
        entry.Route.Should().Be("/services/cognito/eu-west-1_abc123");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListUserPoolsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<UserPoolSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
