using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListHttpAuthorizers;

internal sealed partial class ListHttpAuthorizersQueryHandler : IQueryHandler<ListHttpAuthorizersQuery, ListHttpAuthorizersQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public ListHttpAuthorizersQueryHandler(IApiGatewayV2Client client, ILogger<ListHttpAuthorizersQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListHttpAuthorizersQueryResult>> Handle(ListHttpAuthorizersQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId);
        var authorizers = await _client.ListAuthorizersAsync(request.ApiId, cancellationToken);
        LogHandled(authorizers.IsSuccess);

        if (!authorizers.IsSuccess)
        {
            Result<ListHttpAuthorizersQueryResult> failure = authorizers.Error!.Value;
            return failure;
        }

        return new ListHttpAuthorizersQueryResult(authorizers.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway v2 authorizers for {ApiId}.")]
    private partial void LogHandling(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 authorizer listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
