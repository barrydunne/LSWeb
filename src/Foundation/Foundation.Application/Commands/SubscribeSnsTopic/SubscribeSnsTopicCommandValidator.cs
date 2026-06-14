using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SubscribeSnsTopic;

internal sealed partial class SubscribeSnsTopicCommandValidator : AbstractValidator<SubscribeSnsTopicCommand>
{
    private static readonly HashSet<string> _protocols = new(StringComparer.Ordinal)
    {
        "sqs",
        "lambda",
        "email",
        "email-json",
        "http",
        "https",
        "sms",
        "application",
        "firehose",
    };

    private readonly ILogger _logger;

    public SubscribeSnsTopicCommandValidator(ILogger<SubscribeSnsTopicCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TopicArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Protocol)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(_protocols.Contains)
            .WithMessage("The subscription protocol is not supported.");

        RuleFor(_ => _.Endpoint)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SubscribeSnsTopicCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SubscribeSnsTopicCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
