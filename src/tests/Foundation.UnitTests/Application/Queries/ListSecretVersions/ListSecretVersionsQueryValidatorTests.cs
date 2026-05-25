using Foundation.Application.Queries.ListSecretVersions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSecretVersions;

public class ListSecretVersionsQueryValidatorTests
{
    private readonly ListSecretVersionsQueryValidator _sut =
        new(NullLogger<ListSecretVersionsQueryValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new ListSecretVersionsQuery("db-password"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSecretIdEmpty_ReturnsErrorForSecretId()
    {
        var result = await _sut.ValidateAsync(
            new ListSecretVersionsQuery(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ListSecretVersionsQuery.SecretId));
    }
}
