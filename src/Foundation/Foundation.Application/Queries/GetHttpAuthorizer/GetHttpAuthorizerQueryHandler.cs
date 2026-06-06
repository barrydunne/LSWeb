using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetHttpAuthorizer;

internal sealed partial class GetHttpAuthorizerQueryHandler : IQueryHandler<GetHttpAuthorizerQuery, GetHttpAuthorizerQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public GetHttpAuthorizerQueryHandler(IApiGatewayV2Client client, ILogger<GetHttpAuthorizerQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetHttpAuthorizerQueryResult>> Handle(GetHttpAuthorizerQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId, request.AuthorizerId);
        var authorizer = await _client.GetAuthorizerAsync(request.ApiId, request.AuthorizerId, cancellationToken);
        LogHandled(authorizer.IsSuccess);

        if (!authorizer.IsSuccess)
        {
            Result<GetHttpAuthorizerQueryResult> failure = authorizer.Error!.Value;
            return failure;
        }

        return new GetHttpAuthorizerQueryResult(authorizer.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading API Gateway v2 authorizer {AuthorizerId} for {ApiId}.")]
    private partial void LogHandling(string apiId, string authorizerId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 authorizer read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
