using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateParameter;
using Foundation.Application.Commands.DeleteParameter;
using Foundation.Application.Commands.UpdateParameterValue;
using Foundation.Application.Queries.BrowseParameters;
using Foundation.Application.Queries.GetParameterHistory;
using Foundation.Application.Queries.GetParameterValue;
using Foundation.Domain.Ssm;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SsmControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SsmController> _logger = Substitute.For<ILogger<SsmController>>();

    private SsmController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task BrowseParameters_WhenQuerySucceeds_ReturnsOkWithParameters()
    {
        // Arrange
        var lastModified = new DateTimeOffset(2024, 2, 3, 4, 5, 6, TimeSpan.Zero);
        IReadOnlyList<Parameter> parameters =
        [
            new("/app/config/key", "String", 3, lastModified, "arn:/app/config/key"),
        ];
        _sender
            .Send(Arg.Any<BrowseParametersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<BrowseParametersQueryResult>>(
                new BrowseParametersQueryResult("/app", parameters)));
        var sut = CreateSut();

        // Act
        var result = await sut.BrowseParameters("/app", true, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ParameterListResponse>>().Subject;
        ok.Value!.Path.Should().Be("/app");
        var parameter = ok.Value.Parameters.Should().ContainSingle().Subject;
        parameter.Name.Should().Be("/app/config/key");
        parameter.Type.Should().Be("String");
        parameter.Version.Should().Be(3);
        parameter.LastModifiedDate.Should().Be(lastModified);
        parameter.Arn.Should().Be("arn:/app/config/key");
    }

    [Fact]
    public async Task BrowseParameters_WhenPathMissing_DefaultsToRoot()
    {
        // Arrange
        BrowseParametersQuery? captured = null;
        _sender
            .Send(Arg.Do<BrowseParametersQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<BrowseParametersQueryResult>>(
                new BrowseParametersQueryResult("/", [])));
        var sut = CreateSut();

        // Act
        await sut.BrowseParameters(null, false, TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.Path.Should().Be("/");
    }

    [Fact]
    public async Task BrowseParameters_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<BrowseParametersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<BrowseParametersQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.BrowseParameters("/app", false, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateParameter_WhenCommandSucceeds_ReturnsCreatedAndForwardsAllFields()
    {
        // Arrange
        CreateParameterCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateParameterCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateParameter(
            new ParameterCreateRequest("/app/config/key", "SecureString", "value", "primary config"),
            TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("/app/config/key");
        captured.Type.Should().Be("SecureString");
        captured.Value.Should().Be("value");
        captured.Description.Should().Be("primary config");
    }

    [Fact]
    public async Task CreateParameter_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateParameterCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateParameter(
            new ParameterCreateRequest("/app/config/key", "String", "value", null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteParameter_WhenCommandSucceeds_ReturnsNoContentAndForwardsName()
    {
        // Arrange
        DeleteParameterCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteParameterCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteParameter("/app/config/key", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("/app/config/key");
    }

    [Fact]
    public async Task DeleteParameter_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteParameterCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteParameter("/app/config/key", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetParameterValue_WhenQuerySucceeds_ReturnsOkWithValueAndForwardsReveal()
    {
        // Arrange
        GetParameterValueQuery? captured = null;
        _sender
            .Send(Arg.Do<GetParameterValueQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetParameterValueQueryResult>>(
                new GetParameterValueQueryResult(
                    "/app/db/password", "SecureString", 4, "********", "arn:/app/db/password", true, false)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetParameterValue("/app/db/password", true, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ParameterValueResponse>>().Subject;
        ok.Value!.Name.Should().Be("/app/db/password");
        ok.Value.Type.Should().Be("SecureString");
        ok.Value.Version.Should().Be(4);
        ok.Value.Value.Should().Be("********");
        ok.Value.IsSensitive.Should().BeTrue();
        ok.Value.RevealAllowed.Should().BeFalse();
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("/app/db/password");
        captured.Reveal.Should().BeTrue();
    }

    [Fact]
    public async Task GetParameterValue_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetParameterValueQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetParameterValueQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetParameterValue("/app/db/password", false, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateParameterValue_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        UpdateParameterValueCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateParameterValueCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateParameterValue(
            "/app/config/key",
            new ParameterValueUpdateRequest("new-value"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("/app/config/key");
        captured.Value.Should().Be("new-value");
    }

    [Fact]
    public async Task UpdateParameterValue_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateParameterValueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateParameterValue(
            "/app/config/key",
            new ParameterValueUpdateRequest("new-value"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetParameterHistory_WhenQuerySucceeds_ReturnsOkWithEntriesAndForwardsReveal()
    {
        // Arrange
        var modified = new DateTimeOffset(2024, 5, 6, 7, 8, 9, TimeSpan.Zero);
        GetParameterHistoryQuery? captured = null;
        _sender
            .Send(Arg.Do<GetParameterHistoryQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetParameterHistoryQueryResult>>(
                new GetParameterHistoryQueryResult(
                    "/app/db/password",
                    true,
                    [
                        new("SecureString", 2, "********", modified, "arn:user/admin", true),
                        new("SecureString", 1, "********", modified, "arn:user/admin", true),
                    ])));
        var sut = CreateSut();

        // Act
        var result = await sut.GetParameterHistory("/app/db/password", true, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ParameterHistoryResponse>>().Subject;
        ok.Value!.Name.Should().Be("/app/db/password");
        ok.Value.RevealAllowed.Should().BeTrue();
        ok.Value.Entries.Should().HaveCount(2);
        ok.Value.Entries[0].Version.Should().Be(2);
        ok.Value.Entries[0].Type.Should().Be("SecureString");
        ok.Value.Entries[0].Value.Should().Be("********");
        ok.Value.Entries[0].LastModifiedDate.Should().Be(modified);
        ok.Value.Entries[0].LastModifiedUser.Should().Be("arn:user/admin");
        ok.Value.Entries[0].IsSensitive.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("/app/db/password");
        captured.Reveal.Should().BeTrue();
    }

    [Fact]
    public async Task GetParameterHistory_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetParameterHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetParameterHistoryQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetParameterHistory("/app/db/password", false, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
