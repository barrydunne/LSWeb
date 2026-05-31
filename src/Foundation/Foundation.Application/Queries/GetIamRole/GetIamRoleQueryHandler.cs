using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetIamRole;

internal sealed partial class GetIamRoleQueryHandler : IQueryHandler<GetIamRoleQuery, GetIamRoleQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public GetIamRoleQueryHandler(IIamClient client, ILogger<GetIamRoleQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetIamRoleQueryResult>> Handle(GetIamRoleQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RoleName);
        var role = await _client.GetRoleAsync(request.RoleName, cancellationToken);
        LogHandled(role.IsSuccess);

        if (!role.IsSuccess)
        {
            Result<GetIamRoleQueryResult> failure = role.Error!.Value;
            return failure;
        }

        return new GetIamRoleQueryResult(role.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting IAM role {RoleName}.")]
    private partial void LogHandling(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM role retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
