using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteSqsMessage;

internal sealed partial class DeleteSqsMessageCommandValidator : AbstractValidator<DeleteSqsMessageCommand>
{
    private readonly ILogger _logger;

    public DeleteSqsMessageCommandValidator(ILogger<DeleteSqsMessageCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ReceiptHandle)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteSqsMessageCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteSqsMessageCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
