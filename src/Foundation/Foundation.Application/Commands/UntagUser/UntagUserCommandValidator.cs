using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UntagUser;

internal sealed partial class UntagUserCommandValidator : AbstractValidator<UntagUserCommand>
{
    private readonly ILogger _logger;

    public UntagUserCommandValidator(ILogger<UntagUserCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
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
        ValidationContext<UntagUserCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UntagUserCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
