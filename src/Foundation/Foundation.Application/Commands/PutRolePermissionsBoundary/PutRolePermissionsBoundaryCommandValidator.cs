using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutRolePermissionsBoundary;

internal sealed partial class PutRolePermissionsBoundaryCommandValidator : AbstractValidator<PutRolePermissionsBoundaryCommand>
{
    private readonly ILogger _logger;

    public PutRolePermissionsBoundaryCommandValidator(ILogger<PutRolePermissionsBoundaryCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
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
        ValidationContext<PutRolePermissionsBoundaryCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutRolePermissionsBoundaryCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
