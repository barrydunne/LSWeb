using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateRestResource;

internal sealed partial class CreateRestResourceCommandValidator
    : AbstractValidator<CreateRestResourceCommand>
{
    private const int MaxPathPartLength = 512;

    private readonly ILogger _logger;

    public CreateRestResourceCommandValidator(ILogger<CreateRestResourceCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RestApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ParentId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PathPart)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxPathPartLength)
            .Must(part => !part.Contains('/', StringComparison.Ordinal))
                .WithMessage("Path part must not contain a slash.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateRestResourceCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateRestResourceCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
