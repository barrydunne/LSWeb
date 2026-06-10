using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateHttpApi;
using Foundation.Application.Commands.CreateHttpAuthorizer;
using Foundation.Application.Commands.CreateHttpIntegration;
using Foundation.Application.Commands.CreateHttpRoute;
using Foundation.Application.Commands.CreateHttpStage;
using Foundation.Application.Commands.DeleteHttpApi;
using Foundation.Application.Commands.DeleteHttpAuthorizer;
using Foundation.Application.Commands.DeleteHttpIntegration;
using Foundation.Application.Commands.DeleteHttpRoute;
using Foundation.Application.Commands.DeleteHttpStage;
using Foundation.Application.Commands.TestHttpRoute;
using Foundation.Application.Commands.UpdateHttpApi;
using Foundation.Application.Commands.UpdateHttpAuthorizer;
using Foundation.Application.Commands.UpdateHttpIntegration;
using Foundation.Application.Commands.UpdateHttpRoute;
using Foundation.Application.Commands.UpdateHttpStage;
using Foundation.Application.Queries.GetHttpApi;
using Foundation.Application.Queries.GetHttpAuthorizer;
using Foundation.Application.Queries.GetHttpRoute;
using Foundation.Application.Queries.GetHttpStage;
using Foundation.Application.Queries.ListHttpApis;
using Foundation.Application.Queries.ListHttpAuthorizers;
using Foundation.Application.Queries.ListHttpIntegrations;
using Foundation.Application.Queries.ListHttpRoutes;
using Foundation.Application.Queries.ListHttpStages;
using Foundation.Domain.ApiGatewayV2;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class ApiGatewayV2ControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<ApiGatewayV2Controller> _logger =
        Substitute.For<ILogger<ApiGatewayV2Controller>>();

    private ApiGatewayV2Controller CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListApis_WhenQuerySucceeds_ReturnsOkWithApis()
    {
        // Arrange
        IReadOnlyList<HttpApiSummary> apis =
        [
            new("abc123", "orders", "HTTP", "https://abc123.execute-api", DateTimeOffset.UnixEpoch),
        ];
        _sender
            .Send(Arg.Any<ListHttpApisQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpApisQueryResult>>(
                new ListHttpApisQueryResult(apis)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListApis(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpApiListResponse>>().Subject;
        var api = ok.Value!.Apis.Should().ContainSingle().Subject;
        api.ApiId.Should().Be("abc123");
        api.Name.Should().Be("orders");
        api.ProtocolType.Should().Be("HTTP");
        api.ApiEndpoint.Should().Be("https://abc123.execute-api");
        api.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task ListApis_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListHttpApisQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpApisQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListApis(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetApi_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsId()
    {
        // Arrange
        var detail = new HttpApiDetail(
            "abc123",
            "orders",
            "HTTP",
            "https://abc123.execute-api",
            "Order API",
            "1.0",
            "$request.method $request.path",
            new HttpApiCorsConfiguration(
                true, ["x-custom"], ["GET"], ["https://app"], ["x-exposed"], 600),
            DateTimeOffset.UnixEpoch);
        GetHttpApiQuery? captured = null;
        _sender
            .Send(Arg.Do<GetHttpApiQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpApiQueryResult>>(
                new GetHttpApiQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetApi("abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpApiDetailResponse>>().Subject;
        ok.Value!.ApiId.Should().Be("abc123");
        ok.Value.Name.Should().Be("orders");
        ok.Value.ProtocolType.Should().Be("HTTP");
        ok.Value.ApiEndpoint.Should().Be("https://abc123.execute-api");
        ok.Value.Description.Should().Be("Order API");
        ok.Value.Version.Should().Be("1.0");
        ok.Value.RouteSelectionExpression.Should().Be("$request.method $request.path");
        ok.Value.CorsConfiguration.Should().NotBeNull();
        ok.Value.CorsConfiguration!.AllowCredentials.Should().BeTrue();
        ok.Value.CorsConfiguration.AllowHeaders.Should().ContainSingle().Which.Should().Be("x-custom");
        ok.Value.CorsConfiguration.AllowMethods.Should().ContainSingle().Which.Should().Be("GET");
        ok.Value.CorsConfiguration.AllowOrigins.Should().ContainSingle().Which.Should().Be("https://app");
        ok.Value.CorsConfiguration.ExposeHeaders.Should().ContainSingle().Which.Should().Be("x-exposed");
        ok.Value.CorsConfiguration.MaxAge.Should().Be(600);
        ok.Value.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
    }

    [Fact]
    public async Task GetApi_WhenCorsConfigurationNull_ReturnsNullCors()
    {
        // Arrange
        var detail = new HttpApiDetail(
            "abc123", "orders", "HTTP", null, null, null, null, null, null);
        _sender
            .Send(Arg.Any<GetHttpApiQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpApiQueryResult>>(
                new GetHttpApiQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetApi("abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpApiDetailResponse>>().Subject;
        ok.Value!.CorsConfiguration.Should().BeNull();
    }

    [Fact]
    public async Task GetApi_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetHttpApiQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpApiQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetApi("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateApi_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        var request = new HttpApiCreateRequest("orders", "HTTP", "Order API", "1.0", null);
        CreateHttpApiCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateHttpApiCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("abc123"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateApi(request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<HttpApiCreatedResponse>>().Subject;
        created.Value!.ApiId.Should().Be("abc123");
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("orders");
        captured.ProtocolType.Should().Be("HTTP");
        captured.Description.Should().Be("Order API");
        captured.Version.Should().Be("1.0");
        captured.RouteSelectionExpression.Should().BeNull();
    }

    [Fact]
    public async Task CreateApi_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpApiCreateRequest("orders", "HTTP", null, null, null);
        _sender
            .Send(Arg.Any<CreateHttpApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateApi(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateApi_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        var request = new HttpApiUpdateRequest("orders", "HTTP", "Order API", "1.0", null);
        UpdateHttpApiCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateHttpApiCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateApi("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.Name.Should().Be("orders");
        captured.ProtocolType.Should().Be("HTTP");
        captured.Description.Should().Be("Order API");
        captured.Version.Should().Be("1.0");
        captured.RouteSelectionExpression.Should().BeNull();
    }

    [Fact]
    public async Task UpdateApi_WhenCorsProvided_ForwardsCorsConfiguration()
    {
        // Arrange
        var request = new HttpApiUpdateRequest(
            "orders", "HTTP", null, null, null,
            new HttpApiCorsRequest(true, ["content-type"], ["GET", "POST"], ["*"], [], 600));
        UpdateHttpApiCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateHttpApiCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        await sut.UpdateApi("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.CorsConfiguration.Should().NotBeNull();
        captured.CorsConfiguration!.AllowCredentials.Should().BeTrue();
        captured.CorsConfiguration.AllowHeaders.Should().BeEquivalentTo("content-type");
        captured.CorsConfiguration.AllowMethods.Should().BeEquivalentTo("GET", "POST");
        captured.CorsConfiguration.AllowOrigins.Should().BeEquivalentTo("*");
        captured.CorsConfiguration.ExposeHeaders.Should().BeEmpty();
        captured.CorsConfiguration.MaxAge.Should().Be(600);
    }

    [Fact]
    public async Task UpdateApi_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpApiUpdateRequest("orders", "HTTP", null, null, null);
        _sender
            .Send(Arg.Any<UpdateHttpApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateApi("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteApi_WhenCommandSucceeds_ReturnsNoContentAndForwardsId()
    {
        // Arrange
        DeleteHttpApiCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteHttpApiCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteApi("abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
    }

    [Fact]
    public async Task DeleteApi_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteHttpApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteApi("abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task TestInvokeRoute_WhenCommandSucceeds_ReturnsOkAndForwardsFields()
    {
        // Arrange
        var request = new HttpRouteTestRequest("$default", "GET", "/orders", "token-123", "{\"a\":1}");
        TestHttpRouteCommand? captured = null;
        _sender
            .Send(Arg.Do<TestHttpRouteCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpRouteInvocationResult>>(
                new HttpRouteInvocationResult(
                    200,
                    true,
                    9,
                    new Dictionary<string, string> { ["Content-Type"] = "application/json" },
                    "{\"ok\":true}")));
        var sut = CreateSut();

        // Act
        var result = await sut.TestInvokeRoute("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpRouteInvocationResponse>>().Subject;
        ok.Value!.StatusCode.Should().Be(200);
        ok.Value.Authorized.Should().BeTrue();
        ok.Value.LatencyMilliseconds.Should().Be(9);
        ok.Value.Body.Should().Be("{\"ok\":true}");
        ok.Value.Headers.Should().ContainKey("Content-Type");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.Stage.Should().Be("$default");
        captured.Method.Should().Be("GET");
        captured.Path.Should().Be("/orders");
        captured.Token.Should().Be("token-123");
        captured.Body.Should().Be("{\"a\":1}");
    }

    [Fact]
    public async Task TestInvokeRoute_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpRouteTestRequest("$default", "GET", "/orders", null, null);
        _sender
            .Send(Arg.Any<TestHttpRouteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpRouteInvocationResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.TestInvokeRoute("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListRoutes_WhenQuerySucceeds_ReturnsOkWithRoutes()
    {
        // Arrange
        IReadOnlyList<HttpRouteSummary> routes =
        [
            new("route1", "GET /items", "integrations/int1", "NONE"),
        ];
        ListHttpRoutesQuery? captured = null;
        _sender
            .Send(Arg.Do<ListHttpRoutesQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpRoutesQueryResult>>(
                new ListHttpRoutesQueryResult(routes)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRoutes("abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpRouteListResponse>>().Subject;
        var route = ok.Value!.Routes.Should().ContainSingle().Subject;
        route.RouteId.Should().Be("route1");
        route.RouteKey.Should().Be("GET /items");
        route.Target.Should().Be("integrations/int1");
        route.AuthorizationType.Should().Be("NONE");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
    }

    [Fact]
    public async Task ListRoutes_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListHttpRoutesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpRoutesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRoutes("abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetRoute_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsIds()
    {
        // Arrange
        var detail = new HttpRouteDetail(
            "route1", "GET /items", "integrations/int1", "JWT", "auth1", ["scope.read"], true);
        GetHttpRouteQuery? captured = null;
        _sender
            .Send(Arg.Do<GetHttpRouteQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpRouteQueryResult>>(
                new GetHttpRouteQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRoute("abc123", "route1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpRouteDetailResponse>>().Subject;
        ok.Value!.RouteId.Should().Be("route1");
        ok.Value.RouteKey.Should().Be("GET /items");
        ok.Value.Target.Should().Be("integrations/int1");
        ok.Value.AuthorizationType.Should().Be("JWT");
        ok.Value.AuthorizerId.Should().Be("auth1");
        ok.Value.AuthorizationScopes.Should().ContainSingle().Which.Should().Be("scope.read");
        ok.Value.ApiKeyRequired.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.RouteId.Should().Be("route1");
    }

    [Fact]
    public async Task GetRoute_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetHttpRouteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpRouteQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRoute("abc123", "missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateRoute_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        var request = new HttpRouteCreateRequest(
            "GET /items", "integrations/int1", "JWT", "auth1", ["scope.read"]);
        CreateHttpRouteCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateHttpRouteCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("route1"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRoute("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<HttpRouteCreatedResponse>>().Subject;
        created.Value!.RouteId.Should().Be("route1");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.RouteKey.Should().Be("GET /items");
        captured.Target.Should().Be("integrations/int1");
        captured.AuthorizationType.Should().Be("JWT");
        captured.AuthorizerId.Should().Be("auth1");
        captured.AuthorizationScopes.Should().ContainSingle().Which.Should().Be("scope.read");
    }

    [Fact]
    public async Task CreateRoute_WhenAuthorizationScopesNull_ForwardsEmptyScopes()
    {
        // Arrange
        var request = new HttpRouteCreateRequest("GET /items", null, "NONE", null, null);
        CreateHttpRouteCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateHttpRouteCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("route1"));
        var sut = CreateSut();

        // Act
        await sut.CreateRoute("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.AuthorizationScopes.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateRoute_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpRouteCreateRequest("GET /items", null, "NONE", null, null);
        _sender
            .Send(Arg.Any<CreateHttpRouteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRoute("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateRoute_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        var request = new HttpRouteUpdateRequest(
            "GET /items", "integrations/int1", "JWT", "auth1", ["scope.read"]);
        UpdateHttpRouteCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateHttpRouteCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRoute("abc123", "route1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.RouteId.Should().Be("route1");
        captured.RouteKey.Should().Be("GET /items");
        captured.Target.Should().Be("integrations/int1");
        captured.AuthorizationType.Should().Be("JWT");
        captured.AuthorizerId.Should().Be("auth1");
        captured.AuthorizationScopes.Should().ContainSingle().Which.Should().Be("scope.read");
    }

    [Fact]
    public async Task UpdateRoute_WhenAuthorizationScopesNull_ForwardsEmptyScopes()
    {
        // Arrange
        var request = new HttpRouteUpdateRequest("GET /items", null, "NONE", null, null);
        UpdateHttpRouteCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateHttpRouteCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        await sut.UpdateRoute("abc123", "route1", request, TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.AuthorizationScopes.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateRoute_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpRouteUpdateRequest("GET /items", null, "NONE", null, null);
        _sender
            .Send(Arg.Any<UpdateHttpRouteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRoute("abc123", "route1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRoute_WhenCommandSucceeds_ReturnsNoContentAndForwardsIds()
    {
        // Arrange
        DeleteHttpRouteCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteHttpRouteCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRoute("abc123", "route1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.RouteId.Should().Be("route1");
    }

    [Fact]
    public async Task DeleteRoute_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteHttpRouteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRoute("abc123", "route1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListIntegrations_WhenQuerySucceeds_ReturnsOkWithIntegrations()
    {
        // Arrange
        IReadOnlyList<HttpIntegrationSummary> integrations =
        [
            new("int1", "HTTP_PROXY", "GET", "https://example.test", "1.0", "proxy"),
        ];
        ListHttpIntegrationsQuery? captured = null;
        _sender
            .Send(Arg.Do<ListHttpIntegrationsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpIntegrationsQueryResult>>(
                new ListHttpIntegrationsQueryResult(integrations)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListIntegrations("abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpIntegrationListResponse>>().Subject;
        var integration = ok.Value!.Integrations.Should().ContainSingle().Subject;
        integration.IntegrationId.Should().Be("int1");
        integration.IntegrationType.Should().Be("HTTP_PROXY");
        integration.IntegrationMethod.Should().Be("GET");
        integration.IntegrationUri.Should().Be("https://example.test");
        integration.PayloadFormatVersion.Should().Be("1.0");
        integration.Description.Should().Be("proxy");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
    }

    [Fact]
    public async Task ListIntegrations_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListHttpIntegrationsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpIntegrationsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListIntegrations("abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateIntegration_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        var request = new HttpIntegrationCreateRequest(
            "HTTP_PROXY", "GET", "https://example.test", "1.0", "proxy");
        CreateHttpIntegrationCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateHttpIntegrationCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("int1"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateIntegration("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<HttpIntegrationCreatedResponse>>().Subject;
        created.Value!.IntegrationId.Should().Be("int1");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.IntegrationType.Should().Be("HTTP_PROXY");
        captured.IntegrationMethod.Should().Be("GET");
        captured.IntegrationUri.Should().Be("https://example.test");
        captured.PayloadFormatVersion.Should().Be("1.0");
        captured.Description.Should().Be("proxy");
    }

    [Fact]
    public async Task CreateIntegration_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpIntegrationCreateRequest("HTTP_PROXY", null, null, null, null);
        _sender
            .Send(Arg.Any<CreateHttpIntegrationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateIntegration("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateIntegration_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        var request = new HttpIntegrationUpdateRequest(
            "HTTP_PROXY", "POST", "https://updated.test", "2.0", "updated");
        UpdateHttpIntegrationCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateHttpIntegrationCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateIntegration("abc123", "int1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.IntegrationId.Should().Be("int1");
        captured.IntegrationType.Should().Be("HTTP_PROXY");
        captured.IntegrationMethod.Should().Be("POST");
        captured.IntegrationUri.Should().Be("https://updated.test");
        captured.PayloadFormatVersion.Should().Be("2.0");
        captured.Description.Should().Be("updated");
    }

    [Fact]
    public async Task UpdateIntegration_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpIntegrationUpdateRequest("HTTP_PROXY", null, "https://updated.test", null, null);
        _sender
            .Send(Arg.Any<UpdateHttpIntegrationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateIntegration("abc123", "int1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteIntegration_WhenCommandSucceeds_ReturnsNoContentAndForwardsIds()
    {
        // Arrange
        DeleteHttpIntegrationCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteHttpIntegrationCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteIntegration("abc123", "int1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.IntegrationId.Should().Be("int1");
    }

    [Fact]
    public async Task DeleteIntegration_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteHttpIntegrationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteIntegration("abc123", "int1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListAuthorizers_WhenQuerySucceeds_ReturnsOkWithAuthorizers()
    {
        // Arrange
        IReadOnlyList<HttpAuthorizerSummary> authorizers =
        [
            new("auth1", "jwt-authorizer", "JWT"),
        ];
        ListHttpAuthorizersQuery? captured = null;
        _sender
            .Send(Arg.Do<ListHttpAuthorizersQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpAuthorizersQueryResult>>(
                new ListHttpAuthorizersQueryResult(authorizers)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListAuthorizers("abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpAuthorizerListResponse>>().Subject;
        var authorizer = ok.Value!.Authorizers.Should().ContainSingle().Subject;
        authorizer.AuthorizerId.Should().Be("auth1");
        authorizer.Name.Should().Be("jwt-authorizer");
        authorizer.AuthorizerType.Should().Be("JWT");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
    }

    [Fact]
    public async Task ListAuthorizers_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListHttpAuthorizersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpAuthorizersQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListAuthorizers("abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetAuthorizer_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsIds()
    {
        // Arrange
        var detail = new HttpAuthorizerDetail(
            "auth1",
            "jwt-authorizer",
            "JWT",
            ["$request.header.Authorization"],
            "https://example.com/issuer",
            ["client1"]);
        GetHttpAuthorizerQuery? captured = null;
        _sender
            .Send(Arg.Do<GetHttpAuthorizerQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpAuthorizerQueryResult>>(
                new GetHttpAuthorizerQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAuthorizer("abc123", "auth1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpAuthorizerDetailResponse>>().Subject;
        ok.Value!.AuthorizerId.Should().Be("auth1");
        ok.Value.Name.Should().Be("jwt-authorizer");
        ok.Value.AuthorizerType.Should().Be("JWT");
        ok.Value.IdentitySource.Should().ContainSingle().Which.Should().Be("$request.header.Authorization");
        ok.Value.JwtIssuer.Should().Be("https://example.com/issuer");
        ok.Value.JwtAudience.Should().ContainSingle().Which.Should().Be("client1");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.AuthorizerId.Should().Be("auth1");
    }

    [Fact]
    public async Task GetAuthorizer_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetHttpAuthorizerQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpAuthorizerQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAuthorizer("abc123", "missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateAuthorizer_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        var request = new HttpAuthorizerCreateRequest(
            "jwt-authorizer",
            "JWT",
            ["$request.header.Authorization"],
            "https://example.com/issuer",
            ["client1"]);
        CreateHttpAuthorizerCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateHttpAuthorizerCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("auth1"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAuthorizer("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<HttpAuthorizerCreatedResponse>>().Subject;
        created.Value!.AuthorizerId.Should().Be("auth1");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.Name.Should().Be("jwt-authorizer");
        captured.AuthorizerType.Should().Be("JWT");
        captured.IdentitySource.Should().ContainSingle().Which.Should().Be("$request.header.Authorization");
        captured.JwtIssuer.Should().Be("https://example.com/issuer");
        captured.JwtAudience.Should().ContainSingle().Which.Should().Be("client1");
    }

    [Fact]
    public async Task CreateAuthorizer_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpAuthorizerCreateRequest(
            "jwt-authorizer", "JWT", ["$request.header.Authorization"], "https://example.com/issuer", ["client1"]);
        _sender
            .Send(Arg.Any<CreateHttpAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAuthorizer("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateAuthorizer_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        var request = new HttpAuthorizerUpdateRequest(
            "jwt-authorizer",
            "JWT",
            ["$request.header.Authorization"],
            "https://example.com/issuer",
            ["client1"]);
        UpdateHttpAuthorizerCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateHttpAuthorizerCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAuthorizer("abc123", "auth1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.AuthorizerId.Should().Be("auth1");
        captured.Name.Should().Be("jwt-authorizer");
        captured.AuthorizerType.Should().Be("JWT");
        captured.IdentitySource.Should().ContainSingle().Which.Should().Be("$request.header.Authorization");
        captured.JwtIssuer.Should().Be("https://example.com/issuer");
        captured.JwtAudience.Should().ContainSingle().Which.Should().Be("client1");
    }

    [Fact]
    public async Task UpdateAuthorizer_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpAuthorizerUpdateRequest(
            "jwt-authorizer", "JWT", ["$request.header.Authorization"], "https://example.com/issuer", ["client1"]);
        _sender
            .Send(Arg.Any<UpdateHttpAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAuthorizer("abc123", "auth1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteAuthorizer_WhenCommandSucceeds_ReturnsNoContentAndForwardsIds()
    {
        // Arrange
        DeleteHttpAuthorizerCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteHttpAuthorizerCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAuthorizer("abc123", "auth1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.AuthorizerId.Should().Be("auth1");
    }

    [Fact]
    public async Task DeleteAuthorizer_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteHttpAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAuthorizer("abc123", "auth1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListStages_WhenQuerySucceeds_ReturnsOkWithStages()
    {
        // Arrange
        IReadOnlyList<HttpStageSummary> stages =
        [
            new("dev", true, "deploy1", DateTimeOffset.UnixEpoch),
        ];
        ListHttpStagesQuery? captured = null;
        _sender
            .Send(Arg.Do<ListHttpStagesQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpStagesQueryResult>>(
                new ListHttpStagesQueryResult(stages)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStages("abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpStageListResponse>>().Subject;
        var stage = ok.Value!.Stages.Should().ContainSingle().Subject;
        stage.StageName.Should().Be("dev");
        stage.AutoDeploy.Should().BeTrue();
        stage.DeploymentId.Should().Be("deploy1");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
    }

    [Fact]
    public async Task ListStages_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListHttpStagesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListHttpStagesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStages("abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetStage_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsIds()
    {
        // Arrange
        var detail = new HttpStageDetail(
            "dev",
            true,
            "deploy1",
            "Development stage",
            100,
            50.0,
            new Dictionary<string, string> { ["color"] = "blue" },
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddDays(1));
        GetHttpStageQuery? captured = null;
        _sender
            .Send(Arg.Do<GetHttpStageQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpStageQueryResult>>(
                new GetHttpStageQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetStage("abc123", "dev", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<HttpStageDetailResponse>>().Subject;
        ok.Value!.StageName.Should().Be("dev");
        ok.Value.AutoDeploy.Should().BeTrue();
        ok.Value.DeploymentId.Should().Be("deploy1");
        ok.Value.Description.Should().Be("Development stage");
        ok.Value.DefaultRouteThrottlingBurstLimit.Should().Be(100);
        ok.Value.DefaultRouteThrottlingRateLimit.Should().Be(50.0);
        ok.Value.StageVariables.Should().ContainKey("color").WhoseValue.Should().Be("blue");
        ok.Value.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value.LastUpdatedDate.Should().Be(DateTimeOffset.UnixEpoch.AddDays(1));
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.StageName.Should().Be("dev");
    }

    [Fact]
    public async Task GetStage_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetHttpStageQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetHttpStageQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetStage("abc123", "missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateStage_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        var request = new HttpStageCreateRequest(
            "dev",
            true,
            "Development stage",
            100,
            50.0,
            new Dictionary<string, string> { ["color"] = "blue" });
        CreateHttpStageCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateHttpStageCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("dev"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateStage("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<HttpStageCreatedResponse>>().Subject;
        created.Value!.StageName.Should().Be("dev");
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.StageName.Should().Be("dev");
        captured.AutoDeploy.Should().BeTrue();
        captured.Description.Should().Be("Development stage");
        captured.DefaultRouteThrottlingBurstLimit.Should().Be(100);
        captured.DefaultRouteThrottlingRateLimit.Should().Be(50.0);
        captured.StageVariables.Should().ContainKey("color").WhoseValue.Should().Be("blue");
    }

    [Fact]
    public async Task CreateStage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpStageCreateRequest(
            "dev", true, "Development stage", 100, 50.0, new Dictionary<string, string>());
        _sender
            .Send(Arg.Any<CreateHttpStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateStage("abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateStage_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        var request = new HttpStageUpdateRequest(
            true,
            "Development stage",
            100,
            50.0,
            new Dictionary<string, string> { ["color"] = "blue" });
        UpdateHttpStageCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateHttpStageCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateStage("abc123", "dev", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.StageName.Should().Be("dev");
        captured.AutoDeploy.Should().BeTrue();
        captured.Description.Should().Be("Development stage");
        captured.DefaultRouteThrottlingBurstLimit.Should().Be(100);
        captured.DefaultRouteThrottlingRateLimit.Should().Be(50.0);
        captured.StageVariables.Should().ContainKey("color").WhoseValue.Should().Be("blue");
    }

    [Fact]
    public async Task UpdateStage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new HttpStageUpdateRequest(
            true, "Development stage", 100, 50.0, new Dictionary<string, string>());
        _sender
            .Send(Arg.Any<UpdateHttpStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateStage("abc123", "dev", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteStage_WhenCommandSucceeds_ReturnsNoContentAndForwardsIds()
    {
        // Arrange
        DeleteHttpStageCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteHttpStageCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteStage("abc123", "dev", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.ApiId.Should().Be("abc123");
        captured.StageName.Should().Be("dev");
    }

    [Fact]
    public async Task DeleteStage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteHttpStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteStage("abc123", "dev", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
