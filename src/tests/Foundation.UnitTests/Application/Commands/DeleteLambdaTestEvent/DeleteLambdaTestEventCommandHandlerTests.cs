using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Commands.DeleteLambdaTestEvent;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteLambdaTestEvent;

public class DeleteLambdaTestEventCommandHandlerTests
{
    private readonly ITestEventStore _store = Substitute.For<ITestEventStore>();

    private DeleteLambdaTestEventCommandHandler CreateSut()
        => new(_store, NullLogger<DeleteLambdaTestEventCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenStoreSucceeds_DeletesEventAndReturnsSuccess()
    {
        // Arrange
        _store
            .DeleteEventAsync("orders", "first", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DeleteLambdaTestEventCommand("orders", "first"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _store.Received(1).DeleteEventAsync("orders", "first", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStoreFails_ReturnsError()
    {
        // Arrange
        _store
            .DeleteEventAsync("orders", "first", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("delete boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DeleteLambdaTestEventCommand("orders", "first"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("delete boom");
    }
}
