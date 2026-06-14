using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.EnableDomainDkim;

internal sealed partial class EnableDomainDkimCommandValidator : AbstractValidator<EnableDomainDkimCommand>
{
    private readonly ILogger _logger;

    public EnableDomainDkimCommandValidator(ILogger<EnableDomainDkimCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Domain)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<EnableDomainDkimCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "EnableDomainDkimCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
