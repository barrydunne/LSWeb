using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ValidateTemplate;

internal sealed partial class ValidateTemplateQueryHandler
    : IQueryHandler<ValidateTemplateQuery, ValidateTemplateQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public ValidateTemplateQueryHandler(
        ICloudFormationClient client, ILogger<ValidateTemplateQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ValidateTemplateQueryResult>> Handle(
        ValidateTemplateQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var validation = await _client.ValidateTemplateAsync(
            request.TemplateBody, request.TemplateUrl, cancellationToken);
        LogHandled(validation.IsSuccess);

        if (!validation.IsSuccess)
        {
            Result<ValidateTemplateQueryResult> failure = validation.Error!.Value;
            return failure;
        }

        return new ValidateTemplateQueryResult(validation.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Validating CloudFormation template.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "CloudFormation template validation handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
