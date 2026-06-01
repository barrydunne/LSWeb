using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.ListChangeSets;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListChangeSets;

public class ListChangeSetsQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private ListChangeSetsQueryHandler CreateSut()
        => new(_client, NullLogger<ListChangeSetsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsChangeSets()
    {
        // Arrange
        const string stackName = "orders-stack";
        IReadOnlyList<ChangeSetSummary> summaries =
        [
            new(
                "arn:changeset/add-queue",
                "add-queue",
                stackName,
                "CREATE_COMPLETE",
                null,
                "AVAILABLE",
                "Adds a queue",
                DateTime.UtcNow),
        ];
        var success = Ok(summaries);
        _client
            .ListChangeSetsAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(success));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListChangeSetsQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ChangeSets.Should().BeSameAs(summaries);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        const string stackName = "orders-stack";
        _client
            .ListChangeSetsAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<ChangeSetSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListChangeSetsQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
