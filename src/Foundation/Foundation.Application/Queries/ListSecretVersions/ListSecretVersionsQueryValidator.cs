using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSecretVersions;

internal sealed partial class ListSecretVersionsQueryValidator : AbstractValidator<ListSecretVersionsQuery>
{
    private readonly ILogger _logger;

    public ListSecretVersionsQueryValidator(ILogger<ListSecretVersionsQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.SecretId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ListSecretVersionsQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ListSecretVersionsQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
