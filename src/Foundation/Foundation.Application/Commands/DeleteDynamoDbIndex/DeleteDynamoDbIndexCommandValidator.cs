using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteDynamoDbIndex;

internal sealed partial class DeleteDynamoDbIndexCommandValidator : AbstractValidator<DeleteDynamoDbIndexCommand>
{
    private readonly ILogger _logger;

    public DeleteDynamoDbIndexCommandValidator(ILogger<DeleteDynamoDbIndexCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TableName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.IndexName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteDynamoDbIndexCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteDynamoDbIndexCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
