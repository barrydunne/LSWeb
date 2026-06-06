using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListRestDeployments;

internal sealed partial class ListRestDeploymentsQueryHandler
    : IQueryHandler<ListRestDeploymentsQuery, ListRestDeploymentsQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public ListRestDeploymentsQueryHandler(
        IApiGatewayClient client, ILogger<ListRestDeploymentsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListRestDeploymentsQueryResult>> Handle(
        ListRestDeploymentsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RestApiId);
        var deployments = await _client.ListDeploymentsAsync(request.RestApiId, cancellationToken);
        LogHandled(deployments.IsSuccess);

        if (!deployments.IsSuccess)
        {
            Result<ListRestDeploymentsQueryResult> failure = deployments.Error!.Value;
            return failure;
        }

        return new ListRestDeploymentsQueryResult(deployments.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST API deployments for {RestApiId}.")]
    private partial void LogHandling(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API deployments list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
