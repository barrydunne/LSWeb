using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Domain.CloudFormation;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class CloudFormationResourceSourceTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private CloudFormationResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsCloudFormation()
        => CreateSut().ServiceKey.Should().Be("cloudformation");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsStacksToSearchEntries()
    {
        // Arrange
        IReadOnlyList<CloudFormationStackSummary> stacks =
        [
            new(
                "orders stack",
                "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders stack/abc",
                "CREATE_COMPLETE",
                "Orders processing stack",
                DateTimeOffset.UnixEpoch,
                null),
        ];
        _client
            .ListStacksAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stacks)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("cloudformation");
        entry.ResourceId.Should().Be("orders stack");
        entry.DisplayName.Should().Be("orders stack");
        entry.Route.Should().Be("/services/cloudformation/orders%20stack");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListStacksAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<CloudFormationStackSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
