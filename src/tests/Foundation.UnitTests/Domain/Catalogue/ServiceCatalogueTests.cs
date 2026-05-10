using Foundation.Domain.Catalogue;

namespace Foundation.UnitTests.Domain.Catalogue;

public class ServiceCatalogueTests
{
    [Fact]
    public void Services_IsNotEmpty() =>
        ServiceCatalogue.Services.Should().NotBeEmpty();

    [Fact]
    public void Services_HaveUniqueKeys() =>
        ServiceCatalogue.Services.Select(service => service.Key).Should().OnlyHaveUniqueItems();

    [Fact]
    public void Services_HaveUniqueRoutes() =>
        ServiceCatalogue.Services.Select(service => service.Route).Should().OnlyHaveUniqueItems();

    [Fact]
    public void Services_EachRouteMatchesItsKey() =>
        ServiceCatalogue.Services.Should().AllSatisfy(service =>
            service.Route.Should().Be($"/services/{service.Key}"));

    [Fact]
    public void Services_EachKeyIsLowercaseAndNonEmpty() =>
        ServiceCatalogue.Services.Should().AllSatisfy(service =>
        {
            service.Key.Should().NotBeNullOrWhiteSpace();
            service.Key.Should().Be(service.Key.ToLowerInvariant());
        });

    [Fact]
    public void Services_EachHasDisplayNameAndIconHint() =>
        ServiceCatalogue.Services.Should().AllSatisfy(service =>
        {
            service.DisplayName.Should().NotBeNullOrWhiteSpace();
            service.IconHint.Should().NotBeNullOrWhiteSpace();
        });

    [Fact]
    public void Services_EachCategoryIsDefined() =>
        ServiceCatalogue.Services.Should().AllSatisfy(service =>
            Enum.IsDefined(service.Category).Should().BeTrue());

    [Fact]
    public void Services_ContainTheMinimumManagedSet() =>
        ServiceCatalogue.Services.Select(service => service.Key).Should().Contain(
        [
            "cloudwatch-logs",
            "dynamodb",
            "lambda",
            "s3",
            "secrets-manager",
            "sns",
            "sqs",
            "ssm-parameter-store",
            "step-functions",
        ]);
}
