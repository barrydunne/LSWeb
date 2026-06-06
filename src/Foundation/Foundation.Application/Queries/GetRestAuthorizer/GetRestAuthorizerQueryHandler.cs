using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetRestAuthorizer;

internal sealed partial class GetRestAuthorizerQueryHandler
    : IQueryHandler<GetRestAuthorizerQuery, GetRestAuthorizerQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public GetRestAuthorizerQueryHandler(
        IApiGatewayClient client, ILogger<GetRestAuthorizerQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetRestAuthorizerQueryResult>> Handle(
        GetRestAuthorizerQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.AuthorizerId, request.RestApiId);
        var authorizer = await _client.GetAuthorizerAsync(
            request.RestApiId, request.AuthorizerId, cancellationToken);
        LogHandled(authorizer.IsSuccess);

        if (!authorizer.IsSuccess)
        {
            Result<GetRestAuthorizerQueryResult> failure = authorizer.Error!.Value;
            return failure;
        }

        return new GetRestAuthorizerQueryResult(authorizer.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading API Gateway REST API authorizer {AuthorizerId} of {RestApiId}.")]
    private partial void LogHandling(string authorizerId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API authorizer read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
