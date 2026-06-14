using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutS3BucketPolicy;

internal sealed partial class PutS3BucketPolicyCommandValidator : AbstractValidator<PutS3BucketPolicyCommand>
{
    private readonly ILogger _logger;

    public PutS3BucketPolicyCommandValidator(ILogger<PutS3BucketPolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Policy)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(IsValidPolicy)
            .WithMessage("Policy must be a JSON object with 'Version' and 'Statement' properties.");
    }

    private static bool IsValidPolicy(string policy)
    {
        try
        {
            using var document = JsonDocument.Parse(policy);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("Version", out _)
                && document.RootElement.TryGetProperty("Statement", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutS3BucketPolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutS3BucketPolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
