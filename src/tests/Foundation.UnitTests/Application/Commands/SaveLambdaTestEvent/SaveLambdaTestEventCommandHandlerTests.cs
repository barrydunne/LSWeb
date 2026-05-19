using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Commands.SaveLambdaTestEvent;
using Foundation.Application.Lambda;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SaveLambdaTestEvent;

public class SaveLambdaTestEventCommandHandlerTests
{
    private readonly ITestEventStore _store = Substitute.For<ITestEventStore>();

    private SaveLambdaTestEventCommandHandler CreateSut()
        => new(_store, NullLogger<SaveLambdaTestEventCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenStoreSucceeds_SavesEventAndReturnsSuccess()
    {
        // Arrange
        _store
            .SaveEventAsync("orders", Arg.Any<LambdaTestEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new SaveLambdaTestEventCommand("orders", "first", "{\"a\":1}"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _store.Received(1).SaveEventAsync(
            "orders",
            Arg.Is<LambdaTestEvent>(testEvent => testEvent.Name == "first" && testEvent.Payload == "{\"a\":1}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStoreFails_ReturnsError()
    {
        // Arrange
        _store
            .SaveEventAsync("orders", Arg.Any<LambdaTestEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("save boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new SaveLambdaTestEventCommand("orders", "first", "{}"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("save boom");
    }
}
