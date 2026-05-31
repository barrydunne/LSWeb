using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.StepFunctions;
using Foundation.Domain.StepFunctions;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class StepFunctionsResourceSourceTests
{
    private readonly IStepFunctionsClient _client = Substitute.For<IStepFunctionsClient>();

    private StepFunctionsResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsStepFunctions()
        => CreateSut().ServiceKey.Should().Be("step-functions");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsStateMachinesToSearchEntries()
    {
        // Arrange
        IReadOnlyList<StateMachineSummary> stateMachines =
        [
            new(
                "orders workflow",
                "arn:aws:states:eu-west-1:000000000000:stateMachine:orders workflow",
                "STANDARD",
                DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListStateMachinesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stateMachines)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("step-functions");
        entry.ResourceId.Should().Be("orders workflow");
        entry.DisplayName.Should().Be("orders workflow");
        entry.Route.Should().Be(
            "/services/step-functions/arn%3Aaws%3Astates%3Aeu-west-1%3A000000000000%3AstateMachine%3Aorders%20workflow");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListStateMachinesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<StateMachineSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
