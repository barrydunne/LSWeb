using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Commands.RecordRecentlyViewed;
using Foundation.Application.Preferences;
using Foundation.Domain.Preferences;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RecordRecentlyViewed;

public class RecordRecentlyViewedCommandHandlerTests
{
    private readonly IUserDataStore _userDataStore = Substitute.For<IUserDataStore>();

    [Fact]
    public async Task Handle_WhenStoreSucceeds_RecordsAndSavesReference()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(UserPreferences.Empty));
        _userDataStore
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = new RecordRecentlyViewedCommandHandler(_userDataStore, NullLogger<RecordRecentlyViewedCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new RecordRecentlyViewedCommand("sns://topic"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userDataStore
            .Received(1)
            .SavePreferencesAsync(
                Arg.Is<UserPreferences>(_ => _.RecentlyViewed.Contains("sns://topic")),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetFails_PropagatesErrorWithoutSaving()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(new InvalidOperationException("boom")));
        var sut = new RecordRecentlyViewedCommandHandler(_userDataStore, NullLogger<RecordRecentlyViewedCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new RecordRecentlyViewedCommand("sns://topic"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _userDataStore
            .DidNotReceive()
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaveFails_PropagatesError()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(UserPreferences.Empty));
        _userDataStore
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new InvalidOperationException("save")));
        var sut = new RecordRecentlyViewedCommandHandler(_userDataStore, NullLogger<RecordRecentlyViewedCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new RecordRecentlyViewedCommand("sns://topic"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
