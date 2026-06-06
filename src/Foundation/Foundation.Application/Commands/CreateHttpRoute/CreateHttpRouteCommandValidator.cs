using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateHttpRoute;

internal sealed partial class CreateHttpRouteCommandValidator : AbstractValidator<CreateHttpRouteCommand>
{
    private const int MaxRouteKeyLength = 256;

    private readonly ILogger _logger;

    public CreateHttpRouteCommandValidator(ILogger<CreateHttpRouteCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.ApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.RouteKey)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxRouteKeyLength)
            .Must(routeKey => RouteKeyRegex().IsMatch(routeKey))
                .WithMessage("Route key must be '$default' or in the form 'METHOD /path' (for example 'GET /items').");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateHttpRouteCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [GeneratedRegex(@"^(\$default|(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|ANY) /.*)$", RegexOptions.CultureInvariant)]
    private static partial Regex RouteKeyRegex();

    [LoggerMessage(LogLevel.Warning, "CreateHttpRouteCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
