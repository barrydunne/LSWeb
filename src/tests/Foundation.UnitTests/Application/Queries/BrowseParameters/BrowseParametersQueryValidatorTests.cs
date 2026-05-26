using Foundation.Application.Queries.BrowseParameters;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.BrowseParameters;

public class BrowseParametersQueryValidatorTests
{
    private readonly BrowseParametersQueryValidator _sut =
        new(NullLogger<BrowseParametersQueryValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new BrowseParametersQuery("/app", true), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenPathEmpty_ReturnsErrorForPath()
    {
        var result = await _sut.ValidateAsync(
            new BrowseParametersQuery(string.Empty, false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(BrowseParametersQuery.Path));
    }

    [Fact]
    public async Task ValidateAsync_WhenPathDoesNotStartWithSlash_ReturnsErrorForPath()
    {
        var result = await _sut.ValidateAsync(
            new BrowseParametersQuery("app/config", false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(BrowseParametersQuery.Path));
    }
}
