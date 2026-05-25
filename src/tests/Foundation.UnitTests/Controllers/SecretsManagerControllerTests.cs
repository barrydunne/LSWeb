using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSecret;
using Foundation.Application.Commands.DeleteSecret;
using Foundation.Application.Commands.PutSecretValue;
using Foundation.Application.Queries.GetSecretValue;
using Foundation.Application.Queries.ListSecrets;
using Foundation.Application.Queries.ListSecretVersions;
using Foundation.Domain.SecretsManager;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SecretsManagerControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SecretsManagerController> _logger =
        Substitute.For<ILogger<SecretsManagerController>>();

    private SecretsManagerController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListSecrets_WhenQuerySucceeds_ReturnsOkWithSecrets()
    {
        // Arrange
        var createdDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var lastChanged = new DateTimeOffset(2024, 2, 3, 4, 5, 6, TimeSpan.Zero);
        IReadOnlyList<Secret> secrets =
        [
            new("db-password", "arn:db-password", "primary db", createdDate, lastChanged),
        ];
        _sender
            .Send(Arg.Any<ListSecretsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSecretsQueryResult>>(
                new ListSecretsQueryResult(secrets)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSecrets(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SecretListResponse>>().Subject;
        var secret = ok.Value!.Secrets.Should().ContainSingle().Subject;
        secret.Name.Should().Be("db-password");
        secret.Arn.Should().Be("arn:db-password");
        secret.Description.Should().Be("primary db");
        secret.CreatedDate.Should().Be(createdDate);
        secret.LastChangedDate.Should().Be(lastChanged);
    }

    [Fact]
    public async Task ListSecrets_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSecretsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSecretsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSecrets(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateSecret_WhenCommandSucceeds_ReturnsCreatedAndForwardsAllFields()
    {
        // Arrange
        CreateSecretCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateSecretCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateSecret(
            new SecretCreateRequest("db-password", "primary db", "s3cr3t"),
            TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("db-password");
        captured.Description.Should().Be("primary db");
        captured.SecretString.Should().Be("s3cr3t");
    }

    [Fact]
    public async Task CreateSecret_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateSecretCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateSecret(
            new SecretCreateRequest("db-password", "primary db", "s3cr3t"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteSecret_WhenCommandSucceeds_ReturnsNoContentAndForwardsSecretId()
    {
        // Arrange
        DeleteSecretCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteSecretCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteSecret("db-password", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.SecretId.Should().Be("db-password");
    }

    [Fact]
    public async Task DeleteSecret_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSecretCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteSecret("db-password", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetSecretValue_WhenQuerySucceeds_ReturnsOkWithValueAndForwardsReveal()
    {
        // Arrange
        GetSecretValueQuery? captured = null;
        _sender
            .Send(Arg.Do<GetSecretValueQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSecretValueQueryResult>>(
                new GetSecretValueQueryResult("db-password", "arn:db-password", "v1", "s3cr3t", true)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetSecretValue("db-password", true, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SecretValueResponse>>().Subject;
        ok.Value!.Name.Should().Be("db-password");
        ok.Value.Arn.Should().Be("arn:db-password");
        ok.Value.VersionId.Should().Be("v1");
        ok.Value.Value.Should().Be("s3cr3t");
        ok.Value.RevealAllowed.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.SecretId.Should().Be("db-password");
        captured.Reveal.Should().BeTrue();
    }

    [Fact]
    public async Task GetSecretValue_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSecretValueQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSecretValueQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetSecretValue("db-password", false, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateSecretValue_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        PutSecretValueCommand? captured = null;
        _sender
            .Send(Arg.Do<PutSecretValueCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateSecretValue(
            "db-password",
            new SecretValueUpdateRequest("new-value"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.SecretId.Should().Be("db-password");
        captured.SecretString.Should().Be("new-value");
    }

    [Fact]
    public async Task UpdateSecretValue_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutSecretValueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateSecretValue(
            "db-password",
            new SecretValueUpdateRequest("new-value"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListSecretVersions_WhenQuerySucceeds_ReturnsOkWithVersionsAndForwardsSecretId()
    {
        // Arrange
        var createdDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var lastAccessed = new DateTimeOffset(2024, 2, 3, 4, 5, 6, TimeSpan.Zero);
        ListSecretVersionsQuery? captured = null;
        _sender
            .Send(Arg.Do<ListSecretVersionsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSecretVersionsQueryResult>>(
                new ListSecretVersionsQueryResult(
                    "db-password",
                    "arn:db-password",
                    [new("v1", ["AWSCURRENT", "custom"], createdDate, lastAccessed)])));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSecretVersions("db-password", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SecretVersionListResponse>>().Subject;
        ok.Value!.Name.Should().Be("db-password");
        ok.Value.Arn.Should().Be("arn:db-password");
        var version = ok.Value.Versions.Should().ContainSingle().Subject;
        version.VersionId.Should().Be("v1");
        version.Stages.Should().Equal("AWSCURRENT", "custom");
        version.CreatedDate.Should().Be(createdDate);
        version.LastAccessedDate.Should().Be(lastAccessed);
        captured.Should().NotBeNull();
        captured!.SecretId.Should().Be("db-password");
    }

    [Fact]
    public async Task ListSecretVersions_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSecretVersionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSecretVersionsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSecretVersions("db-password", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
