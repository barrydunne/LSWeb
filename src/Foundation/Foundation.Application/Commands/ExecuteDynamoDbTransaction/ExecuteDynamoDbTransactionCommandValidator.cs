using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ExecuteDynamoDbTransaction;

internal sealed partial class ExecuteDynamoDbTransactionCommandValidator
    : AbstractValidator<ExecuteDynamoDbTransactionCommand>
{
    private const int MaxActions = 100;

    private static readonly string[] _operations = ["Put", "Delete"];

    private readonly ILogger _logger;

    public ExecuteDynamoDbTransactionCommandValidator(
        ILogger<ExecuteDynamoDbTransactionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Actions)
            .NotNull()
            .NotEmpty()
                .WithMessage("A transaction must contain at least one action.")
            .Must(actions => actions.Count <= MaxActions)
                .WithMessage($"A transaction may contain at most {MaxActions} actions.");

        RuleForEach(_ => _.Actions)
            .ChildRules(action =>
            {
                action.RuleFor(_ => _.Operation)
                    .Must(operation => _operations.Contains(operation))
                        .WithMessage("Transaction operations must be one of 'Put' or 'Delete'.");

                action.RuleFor(_ => _.TableName)
                    .NotNull()
                    .NotEmpty()
                        .WithMessage("Transaction actions must specify a table name.");

                action.RuleFor(_ => _.Json)
                    .NotNull()
                    .NotEmpty()
                        .WithMessage("Transaction actions must specify a JSON payload.");
            });
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ExecuteDynamoDbTransactionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ExecuteDynamoDbTransactionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
