using Foundation.Application.Queries.ExecuteDynamoDbStatement;
using Foundation.Domain.DynamoDb;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ExecuteDynamoDbStatement;

public class ExecuteDynamoDbStatementQueryValidatorTests
{
    private readonly ExecuteDynamoDbStatementQueryValidator _sut =
        new(NullLogger<ExecuteDynamoDbStatementQueryValidator>.Instance);

    private static ExecuteDynamoDbStatementQuery Query(
        string statement = "SELECT * FROM \"orders\"", int limit = 25)
        => new(new DynamoDbStatementRequest(statement, limit, null));

    [Fact]
    public async Task ValidateAsync_WhenValidStatement_IsValid()
    {
        var result = await _sut.ValidateAsync(Query(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenStatementEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(statement: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.Contains("Statement"));
    }

    [Fact]
    public async Task ValidateAsync_WhenLimitNotPositive_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Query(limit: 0), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName.Contains("Limit"));
    }
}
