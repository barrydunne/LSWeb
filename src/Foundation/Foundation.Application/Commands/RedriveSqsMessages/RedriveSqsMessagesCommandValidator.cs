using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RedriveSqsMessages;

internal sealed partial class RedriveSqsMessagesCommandValidator : AbstractValidator<RedriveSqsMessagesCommand>
{
    private readonly ILogger _logger;

    public RedriveSqsMessagesCommandValidator(ILogger<RedriveSqsMessagesCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<RedriveSqsMessagesCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "RedriveSqsMessagesCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
