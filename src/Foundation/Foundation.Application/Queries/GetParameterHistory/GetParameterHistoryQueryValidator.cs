using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetParameterHistory;

internal sealed partial class GetParameterHistoryQueryValidator : AbstractValidator<GetParameterHistoryQuery>
{
    private readonly ILogger _logger;

    public GetParameterHistoryQueryValidator(ILogger<GetParameterHistoryQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<GetParameterHistoryQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "GetParameterHistoryQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
