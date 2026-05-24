using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutDynamoDbItem;

internal sealed partial class PutDynamoDbItemCommandValidator : AbstractValidator<PutDynamoDbItemCommand>
{
    private readonly ILogger _logger;

    public PutDynamoDbItemCommandValidator(ILogger<PutDynamoDbItemCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TableName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ItemJson)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(BeJsonObject)
                .WithMessage("Item must be a JSON object.");
    }

    private static bool BeJsonObject(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutDynamoDbItemCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutDynamoDbItemCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
