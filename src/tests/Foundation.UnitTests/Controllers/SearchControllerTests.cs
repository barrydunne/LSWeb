using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.RefreshSearch;
using Foundation.Application.Queries.GetSearchState;
using Foundation.Application.Queries.SearchResources;
using Foundation.Domain.Search;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SearchControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SearchController> _logger = Substitute.For<ILogger<SearchController>>();

    [Fact]
    public async Task Search_WhenQuerySucceeds_ReturnsOkWithMatches()
    {
        // Arrange
        var matches = new List<SearchEntry>
        {
            new("sqs", "orders", "Orders Queue", "/services/sqs/orders"),
        };
        _sender
            .Send(Arg.Any<SearchResourcesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SearchResourcesQueryResult>>(new SearchResourcesQueryResult(matches)));
        var sut = new SearchController(_sender, _logger);

        // Act
        var result = await sut.Search("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SearchResponse>>().Subject;
        var match = ok.Value!.Matches.Should().ContainSingle().Subject;
        match.ServiceKey.Should().Be("sqs");
        match.ResourceId.Should().Be("orders");
        match.DisplayName.Should().Be("Orders Queue");
        match.Route.Should().Be("/services/sqs/orders");
    }

    [Fact]
    public async Task Search_WhenQueryIsNull_DispatchesEmptyTerm()
    {
        // Arrange
        SearchResourcesQuery? captured = null;
        _sender
            .Send(Arg.Do<SearchResourcesQuery>(_ => captured = _), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SearchResourcesQueryResult>>(new SearchResourcesQueryResult([])));
        var sut = new SearchController(_sender, _logger);

        // Act
        var result = await sut.Search(null, TestContext.Current.CancellationToken);

        // Assert
        captured!.Query.Should().BeEmpty();
        var ok = result.Should().BeOfType<Ok<SearchResponse>>().Subject;
        ok.Value!.Matches.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SearchResourcesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SearchResourcesQueryResult>>(new Error("boom")));
        var sut = new SearchController(_sender, _logger);

        // Act
        var result = await sut.Search("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Refresh_WhenCommandSucceeds_ReturnsAccepted()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RefreshSearchCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = new SearchController(_sender, _logger);

        // Act
        var result = await sut.Refresh(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status202Accepted);
    }

    [Fact]
    public async Task Refresh_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RefreshSearchCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new InvalidOperationException("boom")));
        var sut = new SearchController(_sender, _logger);

        // Act
        var result = await sut.Refresh(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task State_WhenQuerySucceeds_ReturnsOkWithState()
    {
        // Arrange
        var builtAt = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        _sender
            .Send(Arg.Any<GetSearchStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSearchStateQueryResult>>(
                new GetSearchStateQueryResult(builtAt, 7, true)));
        var sut = new SearchController(_sender, _logger);

        // Act
        var result = await sut.State(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SearchStateResponse>>().Subject;
        ok.Value!.BuiltAt.Should().Be(builtAt);
        ok.Value.EntryCount.Should().Be(7);
        ok.Value.IsBuilding.Should().BeTrue();
    }

    [Fact]
    public async Task State_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSearchStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSearchStateQueryResult>>(new Error("boom")));
        var sut = new SearchController(_sender, _logger);

        // Act
        var result = await sut.State(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
