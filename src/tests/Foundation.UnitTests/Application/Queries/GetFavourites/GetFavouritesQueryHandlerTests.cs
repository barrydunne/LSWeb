using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Foundation.Application.Queries.GetFavourites;
using Foundation.Domain.Preferences;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetFavourites;

public class GetFavouritesQueryHandlerTests
{
    private readonly IUserDataStore _userDataStore = Substitute.For<IUserDataStore>();

    [Fact]
    public async Task Handle_WhenStoreSucceeds_ReturnsFavouriteReferences()
    {
        // Arrange
        var preferences = new UserPreferences(["s3://bucket", "sqs://queue"], []);
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(preferences));
        var sut = new GetFavouritesQueryHandler(_userDataStore, NullLogger<GetFavouritesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetFavouritesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.References.Should().ContainInOrder("s3://bucket", "sqs://queue");
    }

    [Fact]
    public async Task Handle_WhenStoreFails_PropagatesError()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(new InvalidOperationException("boom")));
        var sut = new GetFavouritesQueryHandler(_userDataStore, NullLogger<GetFavouritesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetFavouritesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
