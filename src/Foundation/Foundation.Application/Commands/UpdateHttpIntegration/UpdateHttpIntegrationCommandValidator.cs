using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateHttpIntegration;

internal sealed partial class UpdateHttpIntegrationCommandValidator : AbstractValidator<UpdateHttpIntegrationCommand>
{
    private static readonly string[] _allowedIntegrationTypes =
        ["AWS", "AWS_PROXY", "HTTP", "HTTP_PROXY", "MOCK"];

    private readonly ILogger _logger;

    public UpdateHttpIntegrationCommandValidator(ILogger<UpdateHttpIntegrationCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.ApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.IntegrationId)
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
        ValidationContext<UpdateHttpIntegrationCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateHttpIntegrationCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
