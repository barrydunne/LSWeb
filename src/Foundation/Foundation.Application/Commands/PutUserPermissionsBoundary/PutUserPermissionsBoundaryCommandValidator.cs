using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutUserPermissionsBoundary;

internal sealed partial class PutUserPermissionsBoundaryCommandValidator : AbstractValidator<PutUserPermissionsBoundaryCommand>
{
    private readonly ILogger _logger;

    public PutUserPermissionsBoundaryCommandValidator(ILogger<PutUserPermissionsBoundaryCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PermissionsBoundaryArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arn => arn.StartsWith("arn:", StringComparison.Ordinal))
                .WithMessage("Permissions boundary ARN must start with 'arn:'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutUserPermissionsBoundaryCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutUserPermissionsBoundaryCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
