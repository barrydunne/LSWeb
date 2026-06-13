using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateDynamoDbTtl;

internal sealed partial class UpdateDynamoDbTtlCommandValidator : AbstractValidator<UpdateDynamoDbTtlCommand>
{
    private const int MaxAttributeNameLength = 255;

    private readonly ILogger _logger;

    public UpdateDynamoDbTtlCommandValidator(ILogger<UpdateDynamoDbTtlCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TableName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.AttributeName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxAttributeNameLength);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateDynamoDbTtlCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateDynamoDbTtlCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
