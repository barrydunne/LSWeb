using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateHttpIntegration;

internal sealed partial class CreateHttpIntegrationCommandValidator : AbstractValidator<CreateHttpIntegrationCommand>
{
    private static readonly string[] _allowedIntegrationTypes =
        ["AWS", "AWS_PROXY", "HTTP", "HTTP_PROXY", "MOCK"];

    private readonly ILogger _logger;

    public CreateHttpIntegrationCommandValidator(ILogger<CreateHttpIntegrationCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.ApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.IntegrationType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(integrationType => _allowedIntegrationTypes.Contains(integrationType))
                .WithMessage("Integration type must be AWS, AWS_PROXY, HTTP, HTTP_PROXY, or MOCK.");

        RuleFor(_ => _.IntegrationUri)
            .NotEmpty()
                .WithMessage("Integration URI is required unless the integration type is MOCK.")
            .When(_ => _.IntegrationType != "MOCK", ApplyConditionTo.CurrentValidator);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateHttpIntegrationCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateHttpIntegrationCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
