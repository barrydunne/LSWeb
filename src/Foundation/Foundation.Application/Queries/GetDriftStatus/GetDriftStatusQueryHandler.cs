using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetDriftStatus;

internal sealed partial class GetDriftStatusQueryHandler
    : IQueryHandler<GetDriftStatusQuery, GetDriftStatusQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public GetDriftStatusQueryHandler(
        ICloudFormationClient client, ILogger<GetDriftStatusQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetDriftStatusQueryResult>> Handle(
        GetDriftStatusQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.DriftDetectionId);
        var status = await _client.DescribeStackDriftDetectionStatusAsync(
            request.DriftDetectionId, cancellationToken);
        LogHandled(status.IsSuccess);

        if (!status.IsSuccess)
        {
            Result<GetDriftStatusQueryResult> failure = status.Error!.Value;
            return failure;
        }

        return new GetDriftStatusQueryResult(status.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting CloudFormation drift detection status {DriftDetectionId}.")]
    private partial void LogHandling(string driftDetectionId);

    [LoggerMessage(LogLevel.Trace, "CloudFormation drift detection status handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
