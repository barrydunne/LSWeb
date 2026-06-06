using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateRestApi;
using Foundation.Application.Commands.CreateRestAuthorizer;
using Foundation.Application.Commands.CreateRestDeployment;
using Foundation.Application.Commands.CreateRestResource;
using Foundation.Application.Commands.CreateRestStage;
using Foundation.Application.Commands.DeleteRestApi;
using Foundation.Application.Commands.DeleteRestAuthorizer;
using Foundation.Application.Commands.DeleteRestMethod;
using Foundation.Application.Commands.DeleteRestResource;
using Foundation.Application.Commands.DeleteRestStage;
using Foundation.Application.Commands.PutRestMethod;
using Foundation.Application.Commands.UpdateRestApi;
using Foundation.Application.Commands.UpdateRestAuthorizer;
using Foundation.Application.Commands.UpdateRestStage;
using Foundation.Application.Queries.GetRestApi;
using Foundation.Application.Queries.GetRestAuthorizer;
using Foundation.Application.Queries.GetRestMethod;
using Foundation.Application.Queries.GetRestStage;
using Foundation.Application.Queries.ListRestApis;
using Foundation.Application.Queries.ListRestAuthorizers;
using Foundation.Application.Queries.ListRestDeployments;
using Foundation.Application.Queries.ListRestResources;
using Foundation.Application.Queries.ListRestStages;
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

    [Fact]
    public async Task GetRestApi_WhenQuerySucceeds_ReturnsOkWithDetail()
    {
        // Arrange
        var detail = new RestApiDetail(
            "api-1", "orders-api", "Orders API", "1.0", "HEADER",
            ["REGIONAL"], ["application/octet-stream"], DateTimeOffset.UnixEpoch);
        _sender
            .Send(Arg.Any<GetRestApiQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRestApiQueryResult>>(
                new GetRestApiQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRestApi("api-1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestApiDetailResponse>>().Subject;
        ok.Value!.Id.Should().Be("api-1");
        ok.Value!.Name.Should().Be("orders-api");
        ok.Value!.Description.Should().Be("Orders API");
        ok.Value!.Version.Should().Be("1.0");
        ok.Value!.ApiKeySource.Should().Be("HEADER");
        ok.Value!.EndpointConfigurationTypes.Should().ContainSingle().Which.Should().Be("REGIONAL");
        ok.Value!.BinaryMediaTypes.Should().ContainSingle();
        ok.Value!.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task GetRestApi_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetRestApiQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRestApiQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRestApi("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateRestApi_WhenCommandSucceeds_ReturnsCreatedWithId()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("api-9"));
        var request = new RestApiCreateRequest("orders", "desc", "1.0", "HEADER", ["REGIONAL"]);
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestApi(request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<RestApiCreatedResponse>>().Subject;
        created.Value!.Id.Should().Be("api-9");
        created.Location.Should().Be("/api/services/apigateway/restapis/api-9");
    }

    [Fact]
    public async Task CreateRestApi_WhenEndpointTypesNull_PassesEmptyList()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("api-9"));
        var request = new RestApiCreateRequest("orders", null, null, null, null);
        var sut = CreateSut();

        // Act
        await sut.CreateRestApi(request, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<CreateRestApiCommand>(command => command.EndpointConfigurationTypes.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRestApi_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var request = new RestApiCreateRequest("orders", null, null, null, ["REGIONAL"]);
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestApi(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateRestApi_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRestApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new RestApiUpdateRequest("orders-v2", "desc");
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRestApi("api-1", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpdateRestApiCommand>(command =>
                command.RestApiId == "api-1" && command.Name == "orders-v2"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRestApi_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRestApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var request = new RestApiUpdateRequest("orders-v2", "desc");
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRestApi("api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRestApi_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestApi("api-1", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteRestApiCommand>(command => command.RestApiId == "api-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRestApi_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestApiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestApi("api-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListRestResources_WhenQuerySucceeds_ReturnsOkWithResources()
    {
        // Arrange
        IReadOnlyList<RestResourceSummary> resources =
        [
            new("res-1", null, null, "/", []),
            new("res-2", "res-1", "items", "/items", ["GET", "POST"]),
        ];
        _sender
            .Send(Arg.Any<ListRestResourcesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestResourcesQueryResult>>(
                new ListRestResourcesQueryResult(resources)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestResources("api-1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestResourceListResponse>>().Subject;
        ok.Value!.Resources.Should().HaveCount(2);
        var child = ok.Value!.Resources.Single(_ => _.Id == "res-2");
        child.ParentId.Should().Be("res-1");
        child.PathPart.Should().Be("items");
        child.Path.Should().Be("/items");
        child.ResourceMethods.Should().Equal("GET", "POST");
    }

    [Fact]
    public async Task ListRestResources_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListRestResourcesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestResourcesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestResources("api-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateRestResource_WhenCommandSucceeds_ReturnsCreatedWithId()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestResourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("res-9"));
        var request = new RestResourceCreateRequest("res-1", "items");
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestResource("api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<RestResourceCreatedResponse>>().Subject;
        created.Value!.Id.Should().Be("res-9");
        created.Location.Should().Be("/api/services/apigateway/restapis/api-1/resources/res-9");
        await _sender.Received(1).Send(
            Arg.Is<CreateRestResourceCommand>(command =>
                command.RestApiId == "api-1"
                && command.ParentId == "res-1"
                && command.PathPart == "items"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRestResource_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestResourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var request = new RestResourceCreateRequest("res-1", "items");
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestResource("api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRestResource_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestResourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestResource("api-1", "res-2", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteRestResourceCommand>(command =>
                command.RestApiId == "api-1" && command.ResourceId == "res-2"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRestResource_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestResourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestResource("api-1", "res-2", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetRestMethod_WhenQuerySucceeds_ReturnsOkWithDetail()
    {
        // Arrange
        var method = new RestMethodDetail(
            "res-2", "GET", "COGNITO_USER_POOLS", "auth-9", true, ["scope-a"]);
        _sender
            .Send(Arg.Any<GetRestMethodQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRestMethodQueryResult>>(
                new GetRestMethodQueryResult(method)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRestMethod(
            "api-1", "res-2", "GET", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestMethodDetailResponse>>().Subject;
        ok.Value!.ResourceId.Should().Be("res-2");
        ok.Value!.HttpMethod.Should().Be("GET");
        ok.Value!.AuthorizationType.Should().Be("COGNITO_USER_POOLS");
        ok.Value!.AuthorizerId.Should().Be("auth-9");
        ok.Value!.ApiKeyRequired.Should().BeTrue();
        ok.Value!.AuthorizationScopes.Should().Equal("scope-a");
    }

    [Fact]
    public async Task GetRestMethod_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetRestMethodQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRestMethodQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRestMethod(
            "api-1", "res-2", "GET", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutRestMethod_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutRestMethodCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new RestMethodPutRequest("NONE", null, false, null);
        var sut = CreateSut();

        // Act
        var result = await sut.PutRestMethod(
            "api-1", "res-2", "GET", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<PutRestMethodCommand>(command =>
                command.RestApiId == "api-1"
                && command.ResourceId == "res-2"
                && command.HttpMethod == "GET"
                && command.AuthorizationScopes.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PutRestMethod_WhenScopesProvided_PassesScopes()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutRestMethodCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new RestMethodPutRequest(
            "COGNITO_USER_POOLS", "auth-9", true, ["scope-a", "scope-b"]);
        var sut = CreateSut();

        // Act
        await sut.PutRestMethod("api-1", "res-2", "POST", request, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<PutRestMethodCommand>(command =>
                command.AuthorizationType == "COGNITO_USER_POOLS"
                && command.AuthorizerId == "auth-9"
                && command.ApiKeyRequired
                && command.AuthorizationScopes.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PutRestMethod_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutRestMethodCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var request = new RestMethodPutRequest("NONE", null, false, null);
        var sut = CreateSut();

        // Act
        var result = await sut.PutRestMethod(
            "api-1", "res-2", "GET", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRestMethod_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestMethodCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestMethod(
            "api-1", "res-2", "GET", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteRestMethodCommand>(command =>
                command.RestApiId == "api-1"
                && command.ResourceId == "res-2"
                && command.HttpMethod == "GET"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRestMethod_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestMethodCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestMethod(
            "api-1", "res-2", "GET", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListRestAuthorizers_WhenQuerySucceeds_ReturnsOkWithAuthorizers()
    {
        // Arrange
        IReadOnlyList<RestAuthorizerSummary> authorizers =
        [
            new("auth-1", "pool-authorizer", "COGNITO_USER_POOLS"),
        ];
        _sender
            .Send(Arg.Any<ListRestAuthorizersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestAuthorizersQueryResult>>(
                new ListRestAuthorizersQueryResult(authorizers)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestAuthorizers("api-1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestAuthorizerListResponse>>().Subject;
        var authorizer = ok.Value!.Authorizers.Should().ContainSingle().Subject;
        authorizer.Id.Should().Be("auth-1");
        authorizer.Name.Should().Be("pool-authorizer");
        authorizer.Type.Should().Be("COGNITO_USER_POOLS");
    }

    [Fact]
    public async Task ListRestAuthorizers_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListRestAuthorizersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestAuthorizersQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestAuthorizers("api-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetRestAuthorizer_WhenQuerySucceeds_ReturnsOkWithDetail()
    {
        // Arrange
        var detail = new RestAuthorizerDetail(
            "auth-1", "pool-authorizer", "COGNITO_USER_POOLS",
            ["arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc"],
            "method.request.header.Authorization", "COGNITO_USER_POOLS");
        _sender
            .Send(Arg.Any<GetRestAuthorizerQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRestAuthorizerQueryResult>>(
                new GetRestAuthorizerQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRestAuthorizer(
            "api-1", "auth-1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestAuthorizerDetailResponse>>().Subject;
        ok.Value!.Id.Should().Be("auth-1");
        ok.Value!.Name.Should().Be("pool-authorizer");
        ok.Value!.Type.Should().Be("COGNITO_USER_POOLS");
        ok.Value!.ProviderARNs.Should().ContainSingle();
        ok.Value!.IdentitySource.Should().Be("method.request.header.Authorization");
        ok.Value!.AuthType.Should().Be("COGNITO_USER_POOLS");
    }

    [Fact]
    public async Task GetRestAuthorizer_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetRestAuthorizerQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRestAuthorizerQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRestAuthorizer(
            "api-1", "missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateRestAuthorizer_WhenCommandSucceeds_ReturnsCreatedWithId()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("auth-9"));
        var request = new RestAuthorizerCreateRequest(
            "pool-authorizer", "COGNITO_USER_POOLS",
            ["arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc"], null);
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestAuthorizer(
            "api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<RestAuthorizerCreatedResponse>>().Subject;
        created.Value!.Id.Should().Be("auth-9");
        created.Location.Should().Be("/api/services/apigateway/restapis/api-1/authorizers/auth-9");
        await _sender.Received(1).Send(
            Arg.Is<CreateRestAuthorizerCommand>(command =>
                command.RestApiId == "api-1"
                && command.Name == "pool-authorizer"
                && command.Type == "COGNITO_USER_POOLS"
                && command.ProviderARNs.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRestAuthorizer_WhenProviderArnsNull_PassesEmptyList()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("auth-9"));
        var request = new RestAuthorizerCreateRequest(
            "pool-authorizer", "COGNITO_USER_POOLS", null, null);
        var sut = CreateSut();

        // Act
        await sut.CreateRestAuthorizer("api-1", request, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<CreateRestAuthorizerCommand>(command => command.ProviderARNs.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRestAuthorizer_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var request = new RestAuthorizerCreateRequest(
            "pool-authorizer", "COGNITO_USER_POOLS", null, null);
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestAuthorizer(
            "api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateRestAuthorizer_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRestAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new RestAuthorizerUpdateRequest(
            "pool-authorizer", "COGNITO_USER_POOLS",
            ["arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc"],
            "method.request.header.Authorization");
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRestAuthorizer(
            "api-1", "auth-9", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpdateRestAuthorizerCommand>(command =>
                command.RestApiId == "api-1"
                && command.AuthorizerId == "auth-9"
                && command.Name == "pool-authorizer"
                && command.ProviderARNs.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRestAuthorizer_WhenProviderArnsNull_PassesEmptyList()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRestAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new RestAuthorizerUpdateRequest(
            "pool-authorizer", "COGNITO_USER_POOLS", null, null);
        var sut = CreateSut();

        // Act
        await sut.UpdateRestAuthorizer("api-1", "auth-9", request, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<UpdateRestAuthorizerCommand>(command => command.ProviderARNs.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRestAuthorizer_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRestAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var request = new RestAuthorizerUpdateRequest(
            "pool-authorizer", "COGNITO_USER_POOLS", null, null);
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRestAuthorizer(
            "api-1", "auth-9", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRestAuthorizer_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestAuthorizer(
            "api-1", "auth-9", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteRestAuthorizerCommand>(command =>
                command.RestApiId == "api-1" && command.AuthorizerId == "auth-9"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRestAuthorizer_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestAuthorizerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestAuthorizer(
            "api-1", "auth-9", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListRestStages_WhenQuerySucceeds_ReturnsOkWithStages()
    {
        // Arrange
        IReadOnlyList<RestStageSummary> stages =
        [
            new("dev", "deployment-1", DateTimeOffset.UnixEpoch),
        ];
        _sender
            .Send(Arg.Any<ListRestStagesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestStagesQueryResult>>(
                new ListRestStagesQueryResult(stages)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestStages("api-1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestStageListResponse>>().Subject;
        var stage = ok.Value!.Stages.Should().ContainSingle().Subject;
        stage.StageName.Should().Be("dev");
        stage.DeploymentId.Should().Be("deployment-1");
        stage.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task ListRestStages_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListRestStagesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestStagesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestStages("api-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetRestStage_WhenQuerySucceeds_ReturnsOkWithDetail()
    {
        // Arrange
        var detail = new RestStageDetail(
            "dev", "deployment-1", "Development stage", true,
            new Dictionary<string, string> { ["key"] = "value" },
            DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddDays(1));
        _sender
            .Send(Arg.Any<GetRestStageQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRestStageQueryResult>>(
                new GetRestStageQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRestStage(
            "api-1", "dev", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestStageDetailResponse>>().Subject;
        ok.Value!.StageName.Should().Be("dev");
        ok.Value!.DeploymentId.Should().Be("deployment-1");
        ok.Value!.Description.Should().Be("Development stage");
        ok.Value!.CacheClusterEnabled.Should().BeTrue();
        ok.Value!.Variables.Should().ContainKey("key").WhoseValue.Should().Be("value");
        ok.Value!.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value!.LastUpdatedDate.Should().Be(DateTimeOffset.UnixEpoch.AddDays(1));
    }

    [Fact]
    public async Task GetRestStage_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetRestStageQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetRestStageQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRestStage(
            "api-1", "missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListRestDeployments_WhenQuerySucceeds_ReturnsOkWithDeployments()
    {
        // Arrange
        IReadOnlyList<RestDeploymentSummary> deployments =
        [
            new("deployment-1", "Initial deployment", DateTimeOffset.UnixEpoch),
        ];
        _sender
            .Send(Arg.Any<ListRestDeploymentsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestDeploymentsQueryResult>>(
                new ListRestDeploymentsQueryResult(deployments)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestDeployments("api-1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RestDeploymentListResponse>>().Subject;
        var deployment = ok.Value!.Deployments.Should().ContainSingle().Subject;
        deployment.Id.Should().Be("deployment-1");
        deployment.Description.Should().Be("Initial deployment");
        deployment.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task ListRestDeployments_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListRestDeploymentsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListRestDeploymentsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRestDeployments("api-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateRestDeployment_WhenCommandSucceeds_ReturnsCreatedWithId()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestDeploymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("deployment-9"));
        var request = new RestDeploymentCreateRequest("dev", "Initial deployment");
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestDeployment(
            "api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<RestDeploymentCreatedResponse>>().Subject;
        created.Value!.Id.Should().Be("deployment-9");
        created.Location.Should().Be("/api/services/apigateway/restapis/api-1/deployments/deployment-9");
        await _sender.Received(1).Send(
            Arg.Is<CreateRestDeploymentCommand>(command =>
                command.RestApiId == "api-1"
                && command.StageName == "dev"
                && command.Description == "Initial deployment"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRestDeployment_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestDeploymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var request = new RestDeploymentCreateRequest(null, null);
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestDeployment(
            "api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateRestStage_WhenCommandSucceeds_ReturnsCreatedWithStageName()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("dev"));
        var request = new RestStageCreateRequest(
            "dev", "deployment-1", "Development stage",
            new Dictionary<string, string> { ["key"] = "value" });
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestStage(
            "api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<RestStageCreatedResponse>>().Subject;
        created.Value!.StageName.Should().Be("dev");
        created.Location.Should().Be("/api/services/apigateway/restapis/api-1/stages/dev");
        await _sender.Received(1).Send(
            Arg.Is<CreateRestStageCommand>(command =>
                command.RestApiId == "api-1"
                && command.StageName == "dev"
                && command.DeploymentId == "deployment-1"
                && command.Description == "Development stage"
                && command.Variables.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRestStage_WhenVariablesNull_PassesEmptyDictionary()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("dev"));
        var request = new RestStageCreateRequest("dev", "deployment-1", null, null);
        var sut = CreateSut();

        // Act
        await sut.CreateRestStage("api-1", request, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<CreateRestStageCommand>(command => command.Variables.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRestStage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRestStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var request = new RestStageCreateRequest("dev", "deployment-1", null, null);
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRestStage(
            "api-1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateRestStage_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRestStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new RestStageUpdateRequest(
            "Updated description",
            new Dictionary<string, string> { ["key"] = "value" });
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRestStage(
            "api-1", "dev", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpdateRestStageCommand>(command =>
                command.RestApiId == "api-1"
                && command.StageName == "dev"
                && command.Description == "Updated description"
                && command.Variables.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRestStage_WhenVariablesNull_PassesEmptyDictionary()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRestStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new RestStageUpdateRequest(null, null);
        var sut = CreateSut();

        // Act
        await sut.UpdateRestStage("api-1", "dev", request, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<UpdateRestStageCommand>(command => command.Variables.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRestStage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRestStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var request = new RestStageUpdateRequest(null, null);
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRestStage(
            "api-1", "dev", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRestStage_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestStage(
            "api-1", "dev", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteRestStageCommand>(command =>
                command.RestApiId == "api-1" && command.StageName == "dev"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRestStage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRestStageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRestStage(
            "api-1", "dev", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
