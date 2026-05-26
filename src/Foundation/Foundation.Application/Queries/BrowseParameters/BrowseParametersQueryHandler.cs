using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Ssm;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.BrowseParameters;

internal sealed partial class BrowseParametersQueryHandler : IQueryHandler<BrowseParametersQuery, BrowseParametersQueryResult>
{
    private readonly ISsmClient _client;
    private readonly ILogger _logger;

    public BrowseParametersQueryHandler(ISsmClient client, ILogger<BrowseParametersQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<BrowseParametersQueryResult>> Handle(BrowseParametersQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Path, request.Recursive);
        var parameters = await _client.GetParametersByPathAsync(request.Path, request.Recursive, cancellationToken);
        LogHandled(parameters.IsSuccess);

        if (!parameters.IsSuccess)
        {
            Result<BrowseParametersQueryResult> failure = parameters.Error!.Value;
            return failure;
        }

        return new BrowseParametersQueryResult(request.Path, parameters.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Browsing SSM parameters under {Path}. Recursive: {Recursive}")]
    private partial void LogHandling(string path, bool recursive);

    [LoggerMessage(LogLevel.Trace, "SSM parameter browse handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
