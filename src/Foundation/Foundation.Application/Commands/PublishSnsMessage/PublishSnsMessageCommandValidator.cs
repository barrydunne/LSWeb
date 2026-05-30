using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PublishSnsMessage;

internal sealed partial class PublishSnsMessageCommandValidator : AbstractValidator<PublishSnsMessageCommand>
{
    private readonly ILogger _logger;

    public PublishSnsMessageCommandValidator(ILogger<PublishSnsMessageCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TopicArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Message)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PublishSnsMessageCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PublishSnsMessageCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
