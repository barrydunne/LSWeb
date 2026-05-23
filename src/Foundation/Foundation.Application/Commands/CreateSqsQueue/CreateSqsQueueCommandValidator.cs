using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateSqsQueue;

internal sealed partial class CreateSqsQueueCommandValidator : AbstractValidator<CreateSqsQueueCommand>
{
    private const string FifoSuffix = ".fifo";

    private readonly ILogger _logger;

    public CreateSqsQueueCommandValidator(ILogger<CreateSqsQueueCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(name => name.EndsWith(FifoSuffix, StringComparison.Ordinal))
                .When(_ => _.FifoQueue, ApplyConditionTo.CurrentValidator)
                .WithMessage("FIFO queue names must end with '.fifo'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateSqsQueueCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateSqsQueueCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
