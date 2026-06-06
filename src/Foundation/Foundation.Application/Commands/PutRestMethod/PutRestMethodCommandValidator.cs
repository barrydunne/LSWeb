using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutRestMethod;

internal sealed partial class PutRestMethodCommandValidator : AbstractValidator<PutRestMethodCommand>
{
    private static readonly string[] _allowedHttpMethods =
        ["GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "ANY"];

    private static readonly string[] _allowedAuthorizationTypes =
        ["NONE", "AWS_IAM", "CUSTOM", "COGNITO_USER_POOLS"];

    private readonly ILogger _logger;

    public PutRestMethodCommandValidator(ILogger<PutRestMethodCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RestApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ResourceId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.HttpMethod)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(method => _allowedHttpMethods.Contains(method))
                .WithMessage("HTTP method must be one of GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS or ANY.");

        RuleFor(_ => _.AuthorizationType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(type => _allowedAuthorizationTypes.Contains(type))
                .WithMessage("Authorization type must be one of NONE, AWS_IAM, CUSTOM or COGNITO_USER_POOLS.");

        RuleFor(_ => _.AuthorizationScopes)
            .NotNull();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutRestMethodCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutRestMethodCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
