using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UntagRole;

internal sealed partial class UntagRoleCommandValidator : AbstractValidator<UntagRoleCommand>
{
    private readonly ILogger _logger;

    public UntagRoleCommandValidator(ILogger<UntagRoleCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.TagKeys)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleForEach(_ => _.TagKeys)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .WithMessage("Tag key must not be empty.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UntagRoleCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UntagRoleCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
