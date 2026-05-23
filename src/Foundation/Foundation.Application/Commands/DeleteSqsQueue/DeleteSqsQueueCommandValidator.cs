using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteSqsQueue;

internal sealed partial class DeleteSqsQueueCommandValidator : AbstractValidator<DeleteSqsQueueCommand>
{
    private readonly ILogger _logger;

    public DeleteSqsQueueCommandValidator(ILogger<DeleteSqsQueueCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteSqsQueueCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteSqsQueueCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
