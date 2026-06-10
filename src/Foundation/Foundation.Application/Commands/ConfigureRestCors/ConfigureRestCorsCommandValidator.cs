using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ConfigureRestCors;

internal sealed partial class ConfigureRestCorsCommandValidator
    : AbstractValidator<ConfigureRestCorsCommand>
{
    private static readonly HashSet<string> _allowedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "ANY",
    };

    private readonly ILogger _logger;

    public ConfigureRestCorsCommandValidator(
        ILogger<ConfigureRestCorsCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RestApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ResourceId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.AllowOrigins)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(origins => origins.All(origin => !string.IsNullOrWhiteSpace(origin)))
                .WithMessage("Allowed origins must not contain blank entries.");

        RuleFor(_ => _.AllowMethods)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(methods => methods.All(method => _allowedMethods.Contains(method)))
                .WithMessage("Allowed methods must be valid HTTP verbs.");

        RuleFor(_ => _.AllowHeaders)
            .NotNull();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ConfigureRestCorsCommand> context,
        CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ConfigureRestCorsCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
