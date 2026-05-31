using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateAccountPasswordPolicy;

internal sealed partial class UpdateAccountPasswordPolicyCommandValidator : AbstractValidator<UpdateAccountPasswordPolicyCommand>
{
    private readonly ILogger _logger;

    public UpdateAccountPasswordPolicyCommandValidator(ILogger<UpdateAccountPasswordPolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.MinimumPasswordLength)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(6, 128);

        RuleFor(_ => _.MaxPasswordAge!.Value)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 1095)
            .When(_ => _.MaxPasswordAge.HasValue)
            .OverridePropertyName(nameof(UpdateAccountPasswordPolicyCommand.MaxPasswordAge));

        RuleFor(_ => _.PasswordReusePrevention!.Value)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 24)
            .When(_ => _.PasswordReusePrevention.HasValue)
            .OverridePropertyName(nameof(UpdateAccountPasswordPolicyCommand.PasswordReusePrevention));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateAccountPasswordPolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateAccountPasswordPolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
