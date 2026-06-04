using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListHostedZones;
using Foundation.Application.Route53;
using Foundation.Domain.Route53;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListHostedZones;

public class ListHostedZonesQueryHandlerTests
{
    private readonly IRoute53Client _client = Substitute.For<IRoute53Client>();

    private ListHostedZonesQueryHandler CreateSut()
        => new(_client, NullLogger<ListHostedZonesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsHostedZones()
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
        var result = await sut.Handle(
            new ListHostedZonesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HostedZones.Should().ContainSingle(_ => _.Name == "example.com.");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListHostedZonesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<HostedZone>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHostedZonesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
