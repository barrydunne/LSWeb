using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ExecuteDynamoDbBatchGet;

internal sealed partial class ExecuteDynamoDbBatchGetQueryValidator
    : AbstractValidator<ExecuteDynamoDbBatchGetQuery>
{
    private const int MaxKeys = 100;

    private readonly ILogger _logger;

    public ExecuteDynamoDbBatchGetQueryValidator(
        ILogger<ExecuteDynamoDbBatchGetQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Keys)
            .NotNull()
            .NotEmpty()
                .WithMessage("A batch get must contain at least one key.")
            .Must(keys => keys.Count <= MaxKeys)
                .WithMessage($"A batch get may contain at most {MaxKeys} keys.");

        RuleForEach(_ => _.Keys)
            .ChildRules(key =>
            {
                key.RuleFor(_ => _.TableName)
                    .NotNull()
                    .NotEmpty()
                        .WithMessage("Batch get keys must specify a table name.");

                key.RuleFor(_ => _.Json)
                    .NotNull()
                    .NotEmpty()
                        .WithMessage("Batch get keys must specify a JSON payload.");
            });
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ExecuteDynamoDbBatchGetQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ExecuteDynamoDbBatchGetQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
