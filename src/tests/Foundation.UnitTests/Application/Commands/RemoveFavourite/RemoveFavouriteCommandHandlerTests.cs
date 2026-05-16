using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Commands.RemoveFavourite;
using Foundation.Application.Preferences;
using Foundation.Domain.Preferences;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RemoveFavourite;

public class RemoveFavouriteCommandHandlerTests
{
    private readonly IUserDataStore _userDataStore = Substitute.For<IUserDataStore>();

    [Fact]
    public async Task Handle_WhenStoreSucceeds_UnpinsAndSavesFavourite()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(new UserPreferences(["s3://bucket"], [])));
        _userDataStore
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = new RemoveFavouriteCommandHandler(_userDataStore, NullLogger<RemoveFavouriteCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new RemoveFavouriteCommand("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userDataStore
            .Received(1)
            .SavePreferencesAsync(
                Arg.Is<UserPreferences>(_ => !_.Favourites.Contains("s3://bucket")),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetFails_PropagatesErrorWithoutSaving()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(new InvalidOperationException("boom")));
        var sut = new RemoveFavouriteCommandHandler(_userDataStore, NullLogger<RemoveFavouriteCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new RemoveFavouriteCommand("s3://bucket"), TestContext.Current.CancellationToken);

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
            .Returns(Task.FromResult<Result<UserPreferences>>(new UserPreferences(["s3://bucket"], [])));
        _userDataStore
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new InvalidOperationException("save")));
        var sut = new RemoveFavouriteCommandHandler(_userDataStore, NullLogger<RemoveFavouriteCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new RemoveFavouriteCommand("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
