using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.ListLambdaTestEvents;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListLambdaTestEvents;

public class ListLambdaTestEventsQueryHandlerTests
{
    private readonly ITestEventStore _store = Substitute.For<ITestEventStore>();

    private ListLambdaTestEventsQueryHandler CreateSut()
        => new(_store, NullLogger<ListLambdaTestEventsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenStoreSucceeds_ReturnsEventsOrderedByNameWithTemplates()
    {
        // Arrange
        IReadOnlyList<LambdaTestEvent> stored =
        [
            new("zeta", "{}"),
            new("alpha", "{}"),
        ];
        _store
            .GetEventsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stored)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaTestEventsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Events.Select(_ => _.Name).Should().ContainInOrder("alpha", "zeta");
        result.Value.Templates.Should().BeEquivalentTo(LambdaTestEventTemplates.Templates);
    }

    [Fact]
    public async Task Handle_WhenStoreFails_ReturnsError()
    {
        // Arrange
        _store
            .GetEventsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaTestEvent>>>(new Error("read boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaTestEventsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("read boom");
    }
}
