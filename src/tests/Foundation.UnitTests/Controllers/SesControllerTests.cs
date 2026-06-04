using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Queries.ListSesIdentities;
using Foundation.Domain.Ses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SesControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SesController> _logger =
        Substitute.For<ILogger<SesController>>();

    private SesController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListIdentities_WhenQuerySucceeds_ReturnsOkWithIdentities()
    {
        // Arrange
        IReadOnlyList<SesIdentity> identities =
        [
            new("sender@example.com", "EmailAddress", "Success"),
        ];
        _sender
            .Send(Arg.Any<ListSesIdentitiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSesIdentitiesQueryResult>>(
                new ListSesIdentitiesQueryResult(identities)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListIdentities(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SesIdentityListResponse>>().Subject;
        var identity = ok.Value!.Identities.Should().ContainSingle().Subject;
        identity.Identity.Should().Be("sender@example.com");
        identity.IdentityType.Should().Be("EmailAddress");
        identity.VerificationStatus.Should().Be("Success");
    }

    [Fact]
    public async Task ListIdentities_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSesIdentitiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSesIdentitiesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListIdentities(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
