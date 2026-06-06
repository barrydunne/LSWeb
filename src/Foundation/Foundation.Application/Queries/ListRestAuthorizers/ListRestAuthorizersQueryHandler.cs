using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListRestAuthorizers;

internal sealed partial class ListRestAuthorizersQueryHandler
    : IQueryHandler<ListRestAuthorizersQuery, ListRestAuthorizersQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public ListRestAuthorizersQueryHandler(
        IApiGatewayClient client, ILogger<ListRestAuthorizersQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListRestAuthorizersQueryResult>> Handle(
        ListRestAuthorizersQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RestApiId);
        var authorizers = await _client.ListAuthorizersAsync(request.RestApiId, cancellationToken);
        LogHandled(authorizers.IsSuccess);

        if (!authorizers.IsSuccess)
        {
            Result<ListRestAuthorizersQueryResult> failure = authorizers.Error!.Value;
            return failure;
        }

        return new ListRestAuthorizersQueryResult(authorizers.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST API authorizers for {RestApiId}.")]
    private partial void LogHandling(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API authorizers list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
