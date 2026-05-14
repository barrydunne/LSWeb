using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.RefreshSearch;
using Foundation.Application.Queries.GetSearchState;
using Foundation.Application.Queries.SearchResources;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides global resource search endpoints: free-text query, manual refresh, and index state.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/search")]
public partial class SearchController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public SearchController(ISender sender, ILogger<SearchController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Searches the most recently built index for resources matching a free-text term.
    /// </summary>
    /// <param name="query">The free-text term to match, supplied as the <c>q</c> query parameter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the matching resources, empty when the term is blank.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Search(
        [FromQuery(Name = "q")] string? query,
        CancellationToken cancellationToken)
    {
        LogHandlingSearch(query);
        var result = await _sender.Send(new SearchResourcesQuery(query ?? string.Empty), cancellationToken);
        LogSearchHandled(result.IsSuccess);
        return result.Match(
            matches => Results.Ok(new SearchResponse(
                matches.Matches
                    .Select(match => new SearchMatchResponse(
                        match.ServiceKey,
                        match.ResourceId,
                        match.DisplayName,
                        match.Route))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Requests an immediate rebuild of the search index, broadcasting progress to connected clients.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 202 result acknowledging that the refresh has been accepted.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IResult> Refresh(CancellationToken cancellationToken)
    {
        LogHandlingRefresh();
        var result = await _sender.Send(new RefreshSearchCommand(), cancellationToken);
        LogRefreshHandled(result.IsSuccess);
        return result.Match(
            () => Results.Accepted(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reports the current state of the search index: when it was last built, how many entries it holds, and whether a rebuild is in progress.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the index state.</returns>
    [HttpGet("state")]
    [ProducesResponseType(typeof(SearchStateResponse), StatusCodes.Status200OK)]
    public async Task<IResult> State(CancellationToken cancellationToken)
    {
        LogHandlingState();
        var result = await _sender.Send(new GetSearchStateQuery(), cancellationToken);
        LogStateHandled(result.IsSuccess);
        return result.Match(
            state => Results.Ok(new SearchStateResponse(
                state.BuiltAt,
                state.EntryCount,
                state.IsBuilding)),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling resource search for '{Query}'.")]
    private partial void LogHandlingSearch(string? query);

    [LoggerMessage(LogLevel.Trace, "Resource search handled. Success: {Success}")]
    private partial void LogSearchHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling search refresh request.")]
    private partial void LogHandlingRefresh();

    [LoggerMessage(LogLevel.Trace, "Search refresh request handled. Success: {Success}")]
    private partial void LogRefreshHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling search state request.")]
    private partial void LogHandlingState();

    [LoggerMessage(LogLevel.Trace, "Search state request handled. Success: {Success}")]
    private partial void LogStateHandled(bool success);
}
