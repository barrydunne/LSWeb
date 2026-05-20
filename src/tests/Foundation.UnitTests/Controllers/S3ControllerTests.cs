using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateS3Bucket;
using Foundation.Application.Commands.DeleteS3Bucket;
using Foundation.Application.Queries.ListS3Buckets;
using Foundation.Domain.S3;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class S3ControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<S3Controller> _logger = Substitute.For<ILogger<S3Controller>>();

    private S3Controller CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListBuckets_WhenQuerySucceeds_ReturnsOkWithSummaries()
    {
        // Arrange
        IReadOnlyList<S3Bucket> buckets =
        [
            new("orders", "2026-01-02T03:04:05.0000000Z"),
        ];
        _sender
            .Send(Arg.Any<ListS3BucketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListS3BucketsQueryResult>>(
                new ListS3BucketsQueryResult(buckets)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListBuckets(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<S3BucketListResponse>>().Subject;
        var summary = ok.Value!.Buckets.Should().ContainSingle().Subject;
        summary.Name.Should().Be("orders");
        summary.CreationDate.Should().Be("2026-01-02T03:04:05.0000000Z");
    }

    [Fact]
    public async Task ListBuckets_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListS3BucketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListS3BucketsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListBuckets(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateBucket_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateS3BucketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateBucket(
            new S3BucketCreateRequest("orders"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created>().Subject;
        created.Location.Should().Be("/api/services/s3/buckets/orders");
    }

    [Fact]
    public async Task CreateBucket_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateS3BucketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateBucket(
            new S3BucketCreateRequest("orders"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteBucket_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteS3BucketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteBucket("orders", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task DeleteBucket_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteS3BucketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteBucket("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
