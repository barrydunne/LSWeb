using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sns;
using Foundation.Domain.Sns;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class SnsResourceSourceTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();

    private SnsResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsSns()
        => CreateSut().ServiceKey.Should().Be("sns");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsTopicsToSearchEntries()
    {
        // Arrange
        IReadOnlyList<SnsTopic> topics =
        [
            new("orders topic", "arn:aws:sns:eu-west-1:000000000000:orders topic"),
        ];
        _client
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(topics)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("sns");
        entry.ResourceId.Should().Be("orders topic");
        entry.DisplayName.Should().Be("orders topic");
        entry.Route.Should().Be("/services/sns/arn%3Aaws%3Asns%3Aeu-west-1%3A000000000000%3Aorders%20topic");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SnsTopic>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
