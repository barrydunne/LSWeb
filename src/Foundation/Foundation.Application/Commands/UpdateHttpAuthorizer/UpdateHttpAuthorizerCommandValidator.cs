using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateHttpAuthorizer;

internal sealed partial class UpdateHttpAuthorizerCommandValidator : AbstractValidator<UpdateHttpAuthorizerCommand>
{
    private const int MaxNameLength = 128;

    private readonly ILogger _logger;

    public UpdateHttpAuthorizerCommandValidator(ILogger<UpdateHttpAuthorizerCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.ApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.AuthorizerId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength);

        RuleFor(_ => _.AuthorizerType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Equal("JWT")
                .WithMessage("Authorizer type must be 'JWT'.");

        RuleFor(_ => _.JwtIssuer)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(issuer => Uri.TryCreate(issuer, UriKind.Absolute, out _))
                .WithMessage("JWT issuer must be a valid absolute URI.");

        RuleFor(_ => _.JwtAudience)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(audience => audience.Count > 0)
                .WithMessage("At least one JWT audience must be supplied.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateHttpAuthorizerCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateHttpAuthorizerCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
