using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Foundation.Application.Queries.GetRecentlyViewed;
using Foundation.Domain.Preferences;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetRecentlyViewed;

public class GetRecentlyViewedQueryHandlerTests
{
    private readonly IUserDataStore _userDataStore = Substitute.For<IUserDataStore>();

    [Fact]
    public async Task Handle_WhenStoreSucceeds_ReturnsRecentlyViewedReferences()
    {
        // Arrange
        var preferences = new UserPreferences([], ["sns://topic", "sqs://queue"]);
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(preferences));
        var sut = new GetRecentlyViewedQueryHandler(_userDataStore, NullLogger<GetRecentlyViewedQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetRecentlyViewedQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.References.Should().ContainInOrder("sns://topic", "sqs://queue");
    }

    [Fact]
    public async Task Handle_WhenStoreFails_PropagatesError()
    {
        // Arrange
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(new InvalidOperationException("boom")));
        var sut = new GetRecentlyViewedQueryHandler(_userDataStore, NullLogger<GetRecentlyViewedQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetRecentlyViewedQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
