using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetSnsSubscriptionFilterPolicy;

internal sealed partial class SetSnsSubscriptionFilterPolicyCommandValidator
    : AbstractValidator<SetSnsSubscriptionFilterPolicyCommand>
{
    private readonly ILogger _logger;

    public SetSnsSubscriptionFilterPolicyCommandValidator(
        ILogger<SetSnsSubscriptionFilterPolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.SubscriptionArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.FilterPolicy)
            .NotNull();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetSnsSubscriptionFilterPolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetSnsSubscriptionFilterPolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
