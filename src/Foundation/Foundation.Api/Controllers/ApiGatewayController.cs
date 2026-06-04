using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Queries.ListRestApis;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides read-only access to AWS API Gateway: listing the REST APIs available on the backend.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/apigateway")]
public partial class ApiGatewayController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGatewayController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries.</param>
    /// <param name="logger">The logger.</param>
    public ApiGatewayController(ISender sender, ILogger<ApiGatewayController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the API Gateway REST APIs available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the REST API summaries.</returns>
    [HttpGet("restapis")]
    [ProducesResponseType(typeof(RestApiListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRestApis(CancellationToken cancellationToken)
    {
        LogHandlingListRestApis();
        var result = await _sender.Send(new ListRestApisQuery(), cancellationToken);
        LogListRestApisHandled(result.IsSuccess);
        return result.Match(
            restApis => Results.Ok(new RestApiListResponse(
                restApis.RestApis
                    .Select(restApi => new RestApiSummaryResponse(
                        restApi.Id,
                        restApi.Name,
                        restApi.Description,
                        restApi.CreatedDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST APIs.")]
    private partial void LogHandlingListRestApis();

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API list handled. Success: {Success}")]
    private partial void LogListRestApisHandled(bool success);
}
