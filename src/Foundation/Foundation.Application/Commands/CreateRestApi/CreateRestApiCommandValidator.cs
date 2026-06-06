using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateRestApi;

internal sealed partial class CreateRestApiCommandValidator : AbstractValidator<CreateRestApiCommand>
{
    private const int MaxNameLength = 1024;

    private static readonly string[] _allowedEndpointTypes = ["EDGE", "REGIONAL", "PRIVATE"];

    private readonly ILogger _logger;

    public CreateRestApiCommandValidator(ILogger<CreateRestApiCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength);

        RuleFor(_ => _.EndpointConfigurationTypes)
            .NotNull();

        RuleForEach(_ => _.EndpointConfigurationTypes)
            .Must(type => _allowedEndpointTypes.Contains(type))
                .WithMessage("Endpoint configuration type must be EDGE, REGIONAL or PRIVATE.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateRestApiCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateRestApiCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
