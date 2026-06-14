using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutS3BucketNotifications;

internal sealed partial class PutS3BucketNotificationsCommandValidator : AbstractValidator<PutS3BucketNotificationsCommand>
{
    private static readonly Dictionary<string, string> _typeTokens = new(StringComparer.Ordinal)
    {
        ["Lambda"] = ":lambda:",
        ["Queue"] = ":sqs:",
        ["Topic"] = ":sns:",
    };

    private readonly ILogger _logger;

    public PutS3BucketNotificationsCommandValidator(ILogger<PutS3BucketNotificationsCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleForEach(_ => _.Notifications).ChildRules(rule =>
        {
            rule.RuleFor(_ => _.Type)
                .Must(_typeTokens.ContainsKey)
                .WithMessage("Notification type must be Lambda, Queue or Topic.");

            rule.RuleFor(_ => _.TargetArn)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .Must(MatchesType)
                .WithMessage("The target ARN does not match the selected destination type.");

            rule.RuleFor(_ => _.Events)
                .NotNull()
                .NotEmpty()
                .WithMessage("Each notification rule must specify at least one event.");
        });
    }

    private static bool MatchesType(Foundation.Domain.S3.S3NotificationConfiguration notification, string targetArn)
        => !_typeTokens.TryGetValue(notification.Type, out var token)
            || targetArn.Contains(token, StringComparison.Ordinal);

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutS3BucketNotificationsCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutS3BucketNotificationsCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
