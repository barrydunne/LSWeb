using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetParameterValue;

internal sealed partial class GetParameterValueQueryValidator : AbstractValidator<GetParameterValueQuery>
{
    private readonly ILogger _logger;

    public GetParameterValueQueryValidator(ILogger<GetParameterValueQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<GetParameterValueQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "GetParameterValueQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
