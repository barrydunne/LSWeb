using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SendSqsMessage;

internal sealed partial class SendSqsMessageCommandValidator : AbstractValidator<SendSqsMessageCommand>
{
    private const string FifoSuffix = ".fifo";

    private readonly ILogger _logger;

    public SendSqsMessageCommandValidator(ILogger<SendSqsMessageCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Body)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.MessageGroupId)
            .NotEmpty()
            .When(_ => _.QueueName?.EndsWith(FifoSuffix, StringComparison.Ordinal) == true,
                ApplyConditionTo.CurrentValidator)
            .WithMessage("FIFO queues require a message group id.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SendSqsMessageCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SendSqsMessageCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
