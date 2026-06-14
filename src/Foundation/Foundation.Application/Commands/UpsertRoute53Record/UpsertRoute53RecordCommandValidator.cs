using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpsertRoute53Record;

internal sealed partial class UpsertRoute53RecordCommandValidator : AbstractValidator<UpsertRoute53RecordCommand>
{
    private static readonly string[] _allowedTypes = ["A", "AAAA", "CNAME", "TXT", "MX"];

    private readonly ILogger _logger;

    public UpsertRoute53RecordCommandValidator(ILogger<UpsertRoute53RecordCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.HostedZoneId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Type)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(_allowedTypes.Contains)
            .WithMessage("Type must be one of A, AAAA, CNAME, TXT or MX.");

        RuleFor(_ => _.Ttl)
            .GreaterThan(0);

        RuleFor(_ => _.Values)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(values => values.All(value => !string.IsNullOrWhiteSpace(value)))
            .WithMessage("Every record value must be non-empty.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpsertRoute53RecordCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpsertRoute53RecordCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
