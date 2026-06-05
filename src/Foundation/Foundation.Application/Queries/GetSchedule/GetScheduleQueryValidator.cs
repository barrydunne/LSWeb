using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSchedule;

internal sealed partial class GetScheduleQueryValidator : AbstractValidator<GetScheduleQuery>
{
    private readonly ILogger _logger;

    public GetScheduleQueryValidator(ILogger<GetScheduleQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.GroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<GetScheduleQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "GetScheduleQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
