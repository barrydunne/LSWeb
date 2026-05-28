using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteSnsTopic;

internal sealed partial class DeleteSnsTopicCommandValidator : AbstractValidator<DeleteSnsTopicCommand>
{
    private readonly ILogger _logger;

    public DeleteSnsTopicCommandValidator(ILogger<DeleteSnsTopicCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TopicArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteSnsTopicCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteSnsTopicCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
