using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateRestAuthorizer;

internal sealed partial class CreateRestAuthorizerCommandValidator
    : AbstractValidator<CreateRestAuthorizerCommand>
{
    private const string CognitoUserPoolsType = "COGNITO_USER_POOLS";
    private const int MaxNameLength = 256;

    private readonly ILogger _logger;

    public CreateRestAuthorizerCommandValidator(
        ILogger<CreateRestAuthorizerCommandValidator> logger)
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

        RuleFor(_ => _.Type)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(type => type == CognitoUserPoolsType)
                .WithMessage("Authorizer type must be COGNITO_USER_POOLS.");

        RuleFor(_ => _.ProviderARNs)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arns => arns.All(IsCognitoUserPoolArn))
                .WithMessage("Each provider ARN must be a Cognito user pool ARN.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateRestAuthorizerCommand> context,
        CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    private static bool IsCognitoUserPoolArn(string arn)
        => !string.IsNullOrWhiteSpace(arn)
            && arn.Contains(":cognito-idp:", StringComparison.Ordinal)
            && arn.Contains(":userpool/", StringComparison.Ordinal);

    [LoggerMessage(LogLevel.Warning, "CreateRestAuthorizerCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
