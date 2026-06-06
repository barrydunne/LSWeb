using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateRestApi;

internal sealed partial class UpdateRestApiCommandValidator : AbstractValidator<UpdateRestApiCommand>
{
    private const int MaxNameLength = 1024;

    private readonly ILogger _logger;

    public UpdateRestApiCommandValidator(ILogger<UpdateRestApiCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RestApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateRestApiCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateRestApiCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
