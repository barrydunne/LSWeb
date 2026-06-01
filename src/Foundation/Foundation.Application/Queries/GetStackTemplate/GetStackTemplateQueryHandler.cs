using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetStackTemplate;

internal sealed partial class GetStackTemplateQueryHandler
    : IQueryHandler<GetStackTemplateQuery, GetStackTemplateQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public GetStackTemplateQueryHandler(
        ICloudFormationClient client, ILogger<GetStackTemplateQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetStackTemplateQueryResult>> Handle(
        GetStackTemplateQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName);
        var template = await _client.GetTemplateAsync(request.StackName, cancellationToken);
        LogHandled(template.IsSuccess);

        if (!template.IsSuccess)
        {
            Result<GetStackTemplateQueryResult> failure = template.Error!.Value;
            return failure;
        }

        return new GetStackTemplateQueryResult(template.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting CloudFormation stack template {StackName}.")]
    private partial void LogHandling(string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack template get handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
