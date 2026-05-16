using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.AddFavourite;
using Foundation.Application.Commands.RecordRecentlyViewed;
using Foundation.Application.Commands.RemoveFavourite;
using Foundation.Application.Queries.GetFavourites;
using Foundation.Application.Queries.GetRecentlyViewed;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides endpoints for tracking recently-viewed resources and user-pinned favourites.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/user")]
public partial class UserController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public UserController(ISender sender, ILogger<UserController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the resource references the user has recently viewed, most recent first.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the recently-viewed references.</returns>
    [HttpGet("recently-viewed")]
    [ProducesResponseType(typeof(ReferenceListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetRecentlyViewed(CancellationToken cancellationToken)
    {
        LogHandlingGetRecentlyViewed();
        var result = await _sender.Send(new GetRecentlyViewedQuery(), cancellationToken);
        LogGetRecentlyViewedHandled(result.IsSuccess);
        return result.Match(
            recents => Results.Ok(new ReferenceListResponse(recents.References)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Records that the user opened a resource, adding it to the front of the recently-viewed list.
    /// </summary>
    /// <param name="request">The reference of the resource that was opened.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result acknowledging that the resource was recorded.</returns>
    [HttpPost("recently-viewed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RecordRecentlyViewed([FromBody] ReferenceRequest request, CancellationToken cancellationToken)
    {
        LogHandlingRecordRecentlyViewed();
        var result = await _sender.Send(new RecordRecentlyViewedCommand(request.Reference), cancellationToken);
        LogRecordRecentlyViewedHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the resource references the user has pinned as favourites.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the pinned favourite references.</returns>
    [HttpGet("favourites")]
    [ProducesResponseType(typeof(ReferenceListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetFavourites(CancellationToken cancellationToken)
    {
        LogHandlingGetFavourites();
        var result = await _sender.Send(new GetFavouritesQuery(), cancellationToken);
        LogGetFavouritesHandled(result.IsSuccess);
        return result.Match(
            favourites => Results.Ok(new ReferenceListResponse(favourites.References)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Pins a resource as a favourite.
    /// </summary>
    /// <param name="request">The reference of the resource to pin.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result acknowledging that the resource was pinned.</returns>
    [HttpPut("favourites")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> AddFavourite([FromBody] ReferenceRequest request, CancellationToken cancellationToken)
    {
        LogHandlingAddFavourite();
        var result = await _sender.Send(new AddFavouriteCommand(request.Reference), cancellationToken);
        LogAddFavouriteHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Unpins a resource from the favourites list.
    /// </summary>
    /// <param name="request">The reference of the resource to unpin.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result acknowledging that the resource was unpinned.</returns>
    [HttpDelete("favourites")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RemoveFavourite([FromBody] ReferenceRequest request, CancellationToken cancellationToken)
    {
        LogHandlingRemoveFavourite();
        var result = await _sender.Send(new RemoveFavouriteCommand(request.Reference), cancellationToken);
        LogRemoveFavouriteHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling get recently-viewed request.")]
    private partial void LogHandlingGetRecentlyViewed();

    [LoggerMessage(LogLevel.Trace, "Get recently-viewed request handled. Success: {Success}")]
    private partial void LogGetRecentlyViewedHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling record recently-viewed request.")]
    private partial void LogHandlingRecordRecentlyViewed();

    [LoggerMessage(LogLevel.Trace, "Record recently-viewed request handled. Success: {Success}")]
    private partial void LogRecordRecentlyViewedHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling get favourites request.")]
    private partial void LogHandlingGetFavourites();

    [LoggerMessage(LogLevel.Trace, "Get favourites request handled. Success: {Success}")]
    private partial void LogGetFavouritesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling add favourite request.")]
    private partial void LogHandlingAddFavourite();

    [LoggerMessage(LogLevel.Trace, "Add favourite request handled. Success: {Success}")]
    private partial void LogAddFavouriteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling remove favourite request.")]
    private partial void LogHandlingRemoveFavourite();

    [LoggerMessage(LogLevel.Trace, "Remove favourite request handled. Success: {Success}")]
    private partial void LogRemoveFavouriteHandled(bool success);
}
