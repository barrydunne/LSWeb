using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateUser;

internal sealed partial class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private const int MaxNameLength = 64;

    private readonly ILogger _logger;

    public CreateUserCommandValidator(ILogger<CreateUserCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("User names may only contain letters, digits, and the characters +=,.@_-.");

        When(_ => _.Path is not null, () =>
            RuleFor(_ => _.Path!)
                .Cascade(CascadeMode.Stop)
                .Must(path => PathPattern().IsMatch(path))
                    .WithMessage("Path must begin and end with a forward slash."));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateUserCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateUserCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9+=,.@_-]+$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"^/$|^/[\x21-\x7E]+/$")]
    private static partial Regex PathPattern();
}
