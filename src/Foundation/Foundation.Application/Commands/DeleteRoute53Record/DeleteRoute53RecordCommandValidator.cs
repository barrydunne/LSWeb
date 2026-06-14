using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteRoute53Record;

internal sealed partial class DeleteRoute53RecordCommandValidator : AbstractValidator<DeleteRoute53RecordCommand>
{
    private readonly ILogger _logger;

    public DeleteRoute53RecordCommandValidator(ILogger<DeleteRoute53RecordCommandValidator> logger)
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
            .NotEmpty();

        RuleFor(_ => _.Values)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteRoute53RecordCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteRoute53RecordCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
