using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Route53;
using Foundation.Domain.Route53;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class Route53ResourceSourceTests
{
    private readonly IRoute53Client _client = Substitute.For<IRoute53Client>();

    private Route53ResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsRoute53()
        => CreateSut().ServiceKey.Should().Be("route53");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsHostedZonesToSearchEntries()
    {
        // Arrange
        IReadOnlyList<HostedZone> hostedZones =
        [
            new("/hostedzone/Z123", "example.com.", 4, false),
        ];
        _client
            .ListHostedZonesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(hostedZones)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("route53");
        entry.ResourceId.Should().Be("/hostedzone/Z123");
        entry.DisplayName.Should().Be("example.com.");
        entry.Route.Should().Be("/services/route53/%2Fhostedzone%2FZ123");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListHostedZonesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<HostedZone>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
