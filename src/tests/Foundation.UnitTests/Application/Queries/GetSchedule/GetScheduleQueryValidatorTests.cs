using Foundation.Application.Queries.GetSchedule;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSchedule;

public class GetScheduleQueryValidatorTests
{
    private readonly GetScheduleQueryValidator _sut =
        new(NullLogger<GetScheduleQueryValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new GetScheduleQuery("nightly", "default"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            new GetScheduleQuery(string.Empty, "default"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(GetScheduleQuery.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenGroupNameEmpty_ReturnsErrorForGroupName()
    {
        var result = await _sut.ValidateAsync(
            new GetScheduleQuery("nightly", string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(GetScheduleQuery.GroupName));
    }
}
