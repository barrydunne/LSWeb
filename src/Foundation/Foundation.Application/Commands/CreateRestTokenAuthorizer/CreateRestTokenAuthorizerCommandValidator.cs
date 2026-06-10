using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateRestTokenAuthorizer;

internal sealed partial class CreateRestTokenAuthorizerCommandValidator
    : AbstractValidator<CreateRestTokenAuthorizerCommand>
{
    private const string IdentitySourcePrefix = "method.request.";
    private const int MaxNameLength = 256;

    private readonly ILogger _logger;

    public CreateRestTokenAuthorizerCommandValidator(
        ILogger<CreateRestTokenAuthorizerCommandValidator> logger)
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

        RuleFor(_ => _.Issuer)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(IsHttpsUrl)
                .WithMessage("Issuer must be an absolute https URL.");

        RuleFor(_ => _.Audience)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.IdentitySource)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(IsRequestIdentitySource)
                .WithMessage("Identity source must reference a request value, for example method.request.header.Authorization.");

        RuleFor(_ => _.AuthorizerUri)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateRestTokenAuthorizerCommand> context,
        CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    private static bool IsHttpsUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;

    private static bool IsRequestIdentitySource(string value)
        => value.StartsWith(IdentitySourcePrefix, StringComparison.Ordinal)
            && value.Length > IdentitySourcePrefix.Length;

    [LoggerMessage(LogLevel.Warning, "CreateRestTokenAuthorizerCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
