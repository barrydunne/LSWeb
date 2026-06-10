using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateHttpAuthorizer;

internal sealed partial class CreateHttpAuthorizerCommandValidator : AbstractValidator<CreateHttpAuthorizerCommand>
{
    private const int MaxNameLength = 128;

    private readonly ILogger _logger;

    public CreateHttpAuthorizerCommandValidator(ILogger<CreateHttpAuthorizerCommandValidator> logger)
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

        RuleFor(_ => _.AuthorizerType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Equal("JWT")
                .WithMessage("Authorizer type must be 'JWT'.");

        RuleFor(_ => _.IdentitySource)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(source => source.Count > 0)
                .WithMessage("At least one identity source must be supplied.");

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
        ValidationContext<CreateHttpAuthorizerCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateHttpAuthorizerCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
