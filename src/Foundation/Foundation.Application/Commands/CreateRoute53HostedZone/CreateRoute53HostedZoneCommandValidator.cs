using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateRoute53HostedZone;

internal sealed partial class CreateRoute53HostedZoneCommandValidator : AbstractValidator<CreateRoute53HostedZoneCommand>
{
    private readonly ILogger _logger;

    public CreateRoute53HostedZoneCommandValidator(ILogger<CreateRoute53HostedZoneCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(name => name.Contains('.', StringComparison.Ordinal))
            .WithMessage("Name must be a fully qualified domain name, for example 'example.com'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateRoute53HostedZoneCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateRoute53HostedZoneCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
