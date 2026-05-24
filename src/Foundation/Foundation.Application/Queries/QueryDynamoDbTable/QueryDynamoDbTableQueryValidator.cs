using FluentValidation;
using FluentValidation.Results;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.QueryDynamoDbTable;

internal sealed partial class QueryDynamoDbTableQueryValidator : AbstractValidator<QueryDynamoDbTableQuery>
{
    private static readonly IReadOnlySet<string> _partitionOperators = new HashSet<string> { "=" };

    private static readonly IReadOnlySet<string> _sortOperators =
        new HashSet<string> { "=", "<", "<=", ">", ">=", "begins_with", "between" };

    private static readonly IReadOnlySet<string> _filterOperators =
        new HashSet<string> { "=", "<>", "<", "<=", ">", ">=", "begins_with", "contains", "between" };

    private static readonly IReadOnlySet<string> _valueTypes = new HashSet<string> { "S", "N", "BOOL" };

    private readonly ILogger _logger;

    public QueryDynamoDbTableQueryValidator(ILogger<QueryDynamoDbTableQueryValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Request.TableName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Request.Limit)
            .GreaterThan(0);

        RuleFor(_ => _.Request.PartitionKey)
            .NotNull()
                .When(_ => !_.Request.Scan)
                .WithMessage("A partition key condition is required for a query.");

        RuleFor(_ => _.Request.PartitionKey!)
            .ChildRules(condition => DefineConditionRules(condition, _partitionOperators))
                .When(_ => _.Request.PartitionKey is not null);

        RuleFor(_ => _.Request.SortKey!)
            .ChildRules(condition => DefineConditionRules(condition, _sortOperators))
                .When(_ => _.Request.SortKey is not null);

        RuleForEach(_ => _.Request.Filters)
            .ChildRules(condition => DefineConditionRules(condition, _filterOperators));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<QueryDynamoDbTableQuery> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    private static void DefineConditionRules(
        InlineValidator<DynamoDbCondition> validator, IReadOnlySet<string> allowedOperators)
    {
        validator.RuleFor(_ => _.AttributeName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        validator.RuleFor(_ => _.Operator)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(allowedOperators.Contains)
                .WithMessage("Operator '{PropertyValue}' is not supported for this condition.");

        validator.RuleFor(_ => _.ValueType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(_valueTypes.Contains)
                .WithMessage("Value type '{PropertyValue}' is not supported. Use S, N or BOOL.");

        validator.RuleFor(_ => _.Value)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        validator.RuleFor(_ => _.SecondValue)
            .NotEmpty()
                .When(_ => _.Operator == "between")
                .WithMessage("A second value is required for the 'between' operator.");
    }

    [LoggerMessage(LogLevel.Warning, "QueryDynamoDbTableQuery validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
