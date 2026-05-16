using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.AddFavourite;

internal sealed partial class AddFavouriteCommandValidator : AbstractValidator<AddFavouriteCommand>
{
    private readonly ILogger _logger;

    public AddFavouriteCommandValidator(ILogger<AddFavouriteCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Reference)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<AddFavouriteCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "AddFavouriteCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
