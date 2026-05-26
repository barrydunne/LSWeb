using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.BrowseParameters;

internal sealed partial class BrowseParametersQueryValidator : AbstractValidator<BrowseParametersQuery>
{
    private readonly ILogger _logger;

    public BrowseParametersQueryValidator(ILogger<BrowseParametersQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Path)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(path => path.StartsWith('/'))
            .WithMessage("Path must start with '/'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<BrowseParametersQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "BrowseParametersQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
