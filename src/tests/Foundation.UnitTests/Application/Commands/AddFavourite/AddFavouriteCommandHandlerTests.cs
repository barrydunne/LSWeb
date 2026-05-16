using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Commands.AddFavourite;
using Foundation.Application.Preferences;
using Foundation.Domain.Preferences;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.AddFavourite;

public class AddFavouriteCommandHandlerTests
{
    private readonly IUserDataStore _userDataStore = Substitute.For<IUserDataStore>();

    [Fact]
    public async Task Handle_WhenStoreSucceeds_PinsAndSavesFavourite()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(UserPreferences.Empty));
        _userDataStore
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = new AddFavouriteCommandHandler(_userDataStore, NullLogger<AddFavouriteCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new AddFavouriteCommand("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userDataStore
            .Received(1)
            .SavePreferencesAsync(
                Arg.Is<UserPreferences>(_ => _.Favourites.Contains("s3://bucket")),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetFails_PropagatesErrorWithoutSaving()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(new InvalidOperationException("boom")));
        var sut = new AddFavouriteCommandHandler(_userDataStore, NullLogger<AddFavouriteCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new AddFavouriteCommand("s3://bucket"), TestContext.Current.CancellationToken);

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
        var sut = new AddFavouriteCommandHandler(_userDataStore, NullLogger<AddFavouriteCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new AddFavouriteCommand("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
