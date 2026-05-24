using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ExecuteDynamoDbStatement;

internal sealed partial class ExecuteDynamoDbStatementQueryValidator
    : AbstractValidator<ExecuteDynamoDbStatementQuery>
{
    private readonly ILogger _logger;

    public ExecuteDynamoDbStatementQueryValidator(
        ILogger<ExecuteDynamoDbStatementQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Request.Statement)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Request.Limit)
            .GreaterThan(0);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ExecuteDynamoDbStatementQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ExecuteDynamoDbStatementQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
