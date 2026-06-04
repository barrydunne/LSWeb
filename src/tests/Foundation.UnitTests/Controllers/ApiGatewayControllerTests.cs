using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Queries.ListRestApis;
using Foundation.Domain.ApiGateway;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class ApiGatewayControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<ApiGatewayController> _logger =
        Substitute.For<ILogger<ApiGatewayController>>();

    private ApiGatewayController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListRestApis_WhenQuerySucceeds_ReturnsOkWithRestApis()
    {
        // Arrange
        var created = DateTimeOffset.UnixEpoch;
        IReadOnlyList<RestApi> restApis =
        [
            new("api-1", "orders-api", "Orders API", created),
        ];
        _sender
            .Send(Arg.Any<ListRestApisQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestApisQueryResult>>(
                new ListRestApisQueryResult(restApis)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestApis(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestApiListResponse>>().Subject;
        var restApi = ok.Value!.RestApis.Should().ContainSingle().Subject;
        restApi.Id.Should().Be("api-1");
        restApi.Name.Should().Be("orders-api");
        restApi.Description.Should().Be("Orders API");
        restApi.CreatedDate.Should().Be(created);
    }

    [Fact]
    public async Task ListRestApis_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListRestApisQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestApisQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestApis(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
