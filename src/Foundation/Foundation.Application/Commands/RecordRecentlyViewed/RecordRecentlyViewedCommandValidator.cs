using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RecordRecentlyViewed;

internal sealed partial class RecordRecentlyViewedCommandValidator : AbstractValidator<RecordRecentlyViewedCommand>
{
    private readonly ILogger _logger;

    public RecordRecentlyViewedCommandValidator(ILogger<RecordRecentlyViewedCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Reference)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<RecordRecentlyViewedCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "RecordRecentlyViewedCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
