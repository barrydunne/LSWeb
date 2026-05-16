using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.AddFavourite;
using Foundation.Application.Commands.RecordRecentlyViewed;
using Foundation.Application.Commands.RemoveFavourite;
using Foundation.Application.Queries.GetFavourites;
using Foundation.Application.Queries.GetRecentlyViewed;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class UserControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<UserController> _logger = Substitute.For<ILogger<UserController>>();

    [Fact]
    public async Task GetRecentlyViewed_WhenQuerySucceeds_ReturnsOkWithReferences()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetRecentlyViewedQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRecentlyViewedQueryResult>>(
                new GetRecentlyViewedQueryResult(["sns://topic", "sqs://queue"])));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.GetRecentlyViewed(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ReferenceListResponse>>().Subject;
        ok.Value!.References.Should().ContainInOrder("sns://topic", "sqs://queue");
    }

    [Fact]
    public async Task GetRecentlyViewed_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetRecentlyViewedQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRecentlyViewedQueryResult>>(new Error("boom")));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.GetRecentlyViewed(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RecordRecentlyViewed_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RecordRecentlyViewedCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.RecordRecentlyViewed(new ReferenceRequest("sns://topic"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task RecordRecentlyViewed_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RecordRecentlyViewedCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new InvalidOperationException("boom")));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.RecordRecentlyViewed(new ReferenceRequest("sns://topic"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetFavourites_WhenQuerySucceeds_ReturnsOkWithReferences()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetFavouritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetFavouritesQueryResult>>(
                new GetFavouritesQueryResult(["s3://bucket"])));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.GetFavourites(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ReferenceListResponse>>().Subject;
        ok.Value!.References.Should().ContainSingle().Which.Should().Be("s3://bucket");
    }

    [Fact]
    public async Task GetFavourites_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetFavouritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetFavouritesQueryResult>>(new Error("boom")));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.GetFavourites(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task AddFavourite_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<AddFavouriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.AddFavourite(new ReferenceRequest("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task AddFavourite_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<AddFavouriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new InvalidOperationException("boom")));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.AddFavourite(new ReferenceRequest("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RemoveFavourite_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RemoveFavouriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.RemoveFavourite(new ReferenceRequest("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task RemoveFavourite_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RemoveFavouriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new InvalidOperationException("boom")));
        var sut = new UserController(_sender, _logger);

        // Act
        var result = await sut.RemoveFavourite(new ReferenceRequest("s3://bucket"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
