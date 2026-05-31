using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetIamGroup;

internal sealed partial class GetIamGroupQueryHandler : IQueryHandler<GetIamGroupQuery, GetIamGroupQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public GetIamGroupQueryHandler(IIamClient client, ILogger<GetIamGroupQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetIamGroupQueryResult>> Handle(GetIamGroupQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.GroupName);
        var group = await _client.GetGroupAsync(request.GroupName, cancellationToken);
        LogHandled(group.IsSuccess);

        if (!group.IsSuccess)
        {
            Result<GetIamGroupQueryResult> failure = group.Error!.Value;
            return failure;
        }

        return new GetIamGroupQueryResult(group.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting IAM group {GroupName}.")]
    private partial void LogHandling(string groupName);

    [LoggerMessage(LogLevel.Trace, "IAM group retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
