using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Ssm;
using Foundation.Domain.Ssm;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class SsmResourceSourceTests
{
    private readonly ISsmClient _client = Substitute.For<ISsmClient>();

    private SsmResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsSsmParameterStore()
        => CreateSut().ServiceKey.Should().Be("ssm-parameter-store");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsParametersToSearchEntries()
    {
        // Arrange
        IReadOnlyList<Parameter> parameters =
        [
            new("/app/config/key", "String", 1, null, "arn:/app/config/key"),
        ];
        _client
            .GetParametersByPathAsync("/", true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(parameters)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("ssm-parameter-store");
        entry.ResourceId.Should().Be("/app/config/key");
        entry.DisplayName.Should().Be("/app/config/key");
        entry.Route.Should().Be("/services/ssm-parameter-store/%2Fapp%2Fconfig%2Fkey");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .GetParametersByPathAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<Parameter>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
