using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Ses;
using Foundation.Domain.Ses;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class SesResourceSourceTests
{
    private readonly ISesClient _client = Substitute.For<ISesClient>();

    private SesResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsSes()
        => CreateSut().ServiceKey.Should().Be("ses");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsIdentitiesToSearchEntries()
    {
        // Arrange
        IReadOnlyList<SesIdentity> identities =
        [
            new("sender@example.com", "EmailAddress", "Success"),
        ];
        _client
            .ListIdentitiesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(identities)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("ses");
        entry.ResourceId.Should().Be("sender@example.com");
        entry.DisplayName.Should().Be("sender@example.com");
        entry.Route.Should().Be("/services/ses/sender%40example.com");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListIdentitiesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SesIdentity>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
