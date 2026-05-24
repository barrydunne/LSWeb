using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateLogGroup;

internal sealed partial class CreateLogGroupCommandValidator : AbstractValidator<CreateLogGroupCommand>
{
    private const int MaxNameLength = 512;

    private readonly ILogger _logger;

    public CreateLogGroupCommandValidator(ILogger<CreateLogGroupCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.LogGroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Log group names may only contain letters, digits, '_', '/', '.', '#', and '-'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateLogGroupCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateLogGroupCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[\.\-_/#A-Za-z0-9]+$")]
    private static partial Regex NamePattern();
}
