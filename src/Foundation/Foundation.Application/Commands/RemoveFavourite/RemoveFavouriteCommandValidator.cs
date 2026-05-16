using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RemoveFavourite;

internal sealed partial class RemoveFavouriteCommandValidator : AbstractValidator<RemoveFavouriteCommand>
{
    private readonly ILogger _logger;

    public RemoveFavouriteCommandValidator(ILogger<RemoveFavouriteCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Reference)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<RemoveFavouriteCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "RemoveFavouriteCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
