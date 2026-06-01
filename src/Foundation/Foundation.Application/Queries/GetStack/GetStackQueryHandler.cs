using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetStack;

internal sealed partial class GetStackQueryHandler
    : IQueryHandler<GetStackQuery, GetStackQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public GetStackQueryHandler(
        ICloudFormationClient client, ILogger<GetStackQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetStackQueryResult>> Handle(
        GetStackQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName);
        var stack = await _client.DescribeStackAsync(request.StackName, cancellationToken);
        LogHandled(stack.IsSuccess);

        if (!stack.IsSuccess)
        {
            Result<GetStackQueryResult> failure = stack.Error!.Value;
            return failure;
        }

        return new GetStackQueryResult(stack.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Describing CloudFormation stack {StackName}.")]
    private partial void LogHandling(string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack describe handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
