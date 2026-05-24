using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateDynamoDbTable;

internal sealed partial class CreateDynamoDbTableCommandValidator : AbstractValidator<CreateDynamoDbTableCommand>
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 255;

    private static readonly string[] _scalarTypes = ["S", "N", "B"];
    private static readonly string[] _billingModes = ["PAY_PER_REQUEST", "PROVISIONED"];

    private readonly ILogger _logger;

    public CreateDynamoDbTableCommandValidator(ILogger<CreateDynamoDbTableCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.TableName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MinimumLength(MinNameLength)
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Table names may only contain letters, digits, '_', '.', and '-'.");

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

        RuleFor(_ => _.BillingMode)
            .Must(mode => _billingModes.Contains(mode))
                .WithMessage("Billing mode must be one of 'PAY_PER_REQUEST' or 'PROVISIONED'.");

        RuleFor(_ => _.ReadCapacityUnits)
            .NotNull()
            .GreaterThan(0)
            .When(_ => _.BillingMode == "PROVISIONED");

        RuleFor(_ => _.WriteCapacityUnits)
            .NotNull()
            .GreaterThan(0)
            .When(_ => _.BillingMode == "PROVISIONED");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateDynamoDbTableCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateDynamoDbTableCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[\.\-_A-Za-z0-9]+$")]
    private static partial Regex NamePattern();
}
