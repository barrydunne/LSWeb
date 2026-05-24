using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteDynamoDbTable;

internal sealed partial class DeleteDynamoDbTableCommandValidator : AbstractValidator<DeleteDynamoDbTableCommand>
{
    private readonly ILogger _logger;

    public DeleteDynamoDbTableCommandValidator(ILogger<DeleteDynamoDbTableCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TableName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteDynamoDbTableCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteDynamoDbTableCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
