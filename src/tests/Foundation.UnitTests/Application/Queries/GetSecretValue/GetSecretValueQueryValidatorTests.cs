using Foundation.Application.Queries.GetSecretValue;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSecretValue;

public class GetSecretValueQueryValidatorTests
{
    private readonly GetSecretValueQueryValidator _sut =
        new(NullLogger<GetSecretValueQueryValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new GetSecretValueQuery("db-password", false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSecretIdEmpty_ReturnsErrorForSecretId()
    {
        var result = await _sut.ValidateAsync(
            new GetSecretValueQuery(string.Empty, false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(GetSecretValueQuery.SecretId));
    }
}
