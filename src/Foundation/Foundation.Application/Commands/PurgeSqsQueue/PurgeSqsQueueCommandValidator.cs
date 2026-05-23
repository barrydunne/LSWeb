using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PurgeSqsQueue;

internal sealed partial class PurgeSqsQueueCommandValidator : AbstractValidator<PurgeSqsQueueCommand>
{
    private readonly ILogger _logger;

    public PurgeSqsQueueCommandValidator(ILogger<PurgeSqsQueueCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PurgeSqsQueueCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PurgeSqsQueueCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
