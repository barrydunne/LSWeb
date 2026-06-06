using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateHttpApi;

internal sealed partial class UpdateHttpApiCommandValidator : AbstractValidator<UpdateHttpApiCommand>
{
    private const int MaxNameLength = 128;

    private static readonly string[] _allowedProtocolTypes = ["HTTP", "WEBSOCKET"];

    private readonly ILogger _logger;

    public UpdateHttpApiCommandValidator(ILogger<UpdateHttpApiCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.ApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength);

        RuleFor(_ => _.ProtocolType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(protocolType => _allowedProtocolTypes.Contains(protocolType))
                .WithMessage("Protocol type must be HTTP or WEBSOCKET.");

        RuleFor(_ => _.RouteSelectionExpression)
            .NotEmpty()
            .When(_ => string.Equals(_.ProtocolType, "WEBSOCKET", StringComparison.Ordinal))
                .WithMessage("A route selection expression is required for WEBSOCKET APIs.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateHttpApiCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateHttpApiCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
