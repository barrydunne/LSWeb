using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateSnsTopic;

internal sealed partial class CreateSnsTopicCommandValidator : AbstractValidator<CreateSnsTopicCommand>
{
    private const int MinNameLength = 1;
    private const int MaxNameLength = 256;

    private readonly ILogger _logger;

    public CreateSnsTopicCommandValidator(ILogger<CreateSnsTopicCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MinimumLength(MinNameLength)
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Topic names may only contain letters, digits, hyphens, and underscores.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateSnsTopicCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateSnsTopicCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9_-]+$")]
    private static partial Regex NamePattern();
}
