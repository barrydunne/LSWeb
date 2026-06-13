using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ExecuteDynamoDbBatchWrite;

internal sealed partial class ExecuteDynamoDbBatchWriteQueryValidator
    : AbstractValidator<ExecuteDynamoDbBatchWriteQuery>
{
    private const int MaxItems = 25;

    private static readonly string[] _operations = ["Put", "Delete"];

    private readonly ILogger _logger;

    public ExecuteDynamoDbBatchWriteQueryValidator(
        ILogger<ExecuteDynamoDbBatchWriteQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Items)
            .NotNull()
            .NotEmpty()
                .WithMessage("A batch write must contain at least one request.")
            .Must(items => items.Count <= MaxItems)
                .WithMessage($"A batch write may contain at most {MaxItems} requests.");

        RuleForEach(_ => _.Items)
            .ChildRules(item =>
            {
                item.RuleFor(_ => _.Operation)
                    .Must(operation => _operations.Contains(operation))
                        .WithMessage("Batch write operations must be one of 'Put' or 'Delete'.");

                item.RuleFor(_ => _.TableName)
                    .NotNull()
                    .NotEmpty()
                        .WithMessage("Batch write requests must specify a table name.");

                item.RuleFor(_ => _.Json)
                    .NotNull()
                    .NotEmpty()
                        .WithMessage("Batch write requests must specify a JSON payload.");
            });
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ExecuteDynamoDbBatchWriteQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ExecuteDynamoDbBatchWriteQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
