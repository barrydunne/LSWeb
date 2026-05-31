using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateAccessKeyStatus;

internal sealed partial class UpdateAccessKeyStatusCommandValidator : AbstractValidator<UpdateAccessKeyStatusCommand>
{
    private static readonly string[] _allowedStatuses = ["Active", "Inactive"];

    private readonly ILogger _logger;

    public UpdateAccessKeyStatusCommandValidator(ILogger<UpdateAccessKeyStatusCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.AccessKeyId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Status)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(status => _allowedStatuses.Contains(status, StringComparer.Ordinal))
                .WithMessage("Status must be either 'Active' or 'Inactive'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateAccessKeyStatusCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateAccessKeyStatusCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
