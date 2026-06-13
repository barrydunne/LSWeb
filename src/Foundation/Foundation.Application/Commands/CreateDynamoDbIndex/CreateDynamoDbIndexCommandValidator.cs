using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateDynamoDbIndex;

internal sealed partial class CreateDynamoDbIndexCommandValidator : AbstractValidator<CreateDynamoDbIndexCommand>
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 255;

    private static readonly string[] _scalarTypes = ["S", "N", "B"];
    private static readonly string[] _projectionTypes = ["ALL", "KEYS_ONLY"];

    private readonly ILogger _logger;

    public CreateDynamoDbIndexCommandValidator(ILogger<CreateDynamoDbIndexCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TableName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.IndexName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MinimumLength(MinNameLength)
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Index names may only contain letters, digits, '_', '.', and '-'.");

        RuleFor(_ => _.PartitionKeyName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PartitionKeyType)
            .Must(type => _scalarTypes.Contains(type))
                .WithMessage("Partition key type must be one of 'S', 'N', or 'B'.");

        RuleFor(_ => _.SortKeyType)
            .Must(type => _scalarTypes.Contains(type))
                .WithMessage("Sort key type must be one of 'S', 'N', or 'B'.")
            .When(_ => !string.IsNullOrEmpty(_.SortKeyName));

        RuleFor(_ => _.ProjectionType)
            .Must(type => _projectionTypes.Contains(type))
                .WithMessage("Projection type must be one of 'ALL' or 'KEYS_ONLY'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateDynamoDbIndexCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateDynamoDbIndexCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[\.\-_A-Za-z0-9]+$")]
    private static partial Regex NamePattern();
}
