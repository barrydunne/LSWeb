using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateDynamoDbTable;
using Foundation.Application.Commands.CreateDynamoDbIndex;
using Foundation.Application.Commands.DeleteDynamoDbIndex;
using Foundation.Application.Commands.DeleteDynamoDbItem;
using Foundation.Application.Commands.DeleteDynamoDbTable;
using Foundation.Application.Commands.ExecuteDynamoDbTransaction;
using Foundation.Application.Commands.PutDynamoDbItem;
using Foundation.Application.Commands.UpdateDynamoDbTtl;
using Foundation.Application.Queries.ExecuteDynamoDbBatchGet;
using Foundation.Application.Queries.ExecuteDynamoDbBatchWrite;
using Foundation.Application.Queries.ExecuteDynamoDbStatement;
using Foundation.Application.Queries.GetDynamoDbItem;
using Foundation.Application.Queries.GetDynamoDbTable;
using Foundation.Application.Queries.ListDynamoDbTables;
using Foundation.Application.Queries.QueryDynamoDbTable;
using Foundation.Application.Queries.ScanDynamoDbItems;
using Foundation.Domain.DynamoDb;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class DynamoDbControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<DynamoDbController> _logger =
        Substitute.For<ILogger<DynamoDbController>>();

    private DynamoDbController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListTables_WhenQuerySucceeds_ReturnsOkWithTables()
    {
        // Arrange
        IReadOnlyList<DynamoDbTable> tables =
        [
            new("orders"),
        ];
        _sender
            .Send(Arg.Any<ListDynamoDbTablesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListDynamoDbTablesQueryResult>>(
                new ListDynamoDbTablesQueryResult(tables)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTables(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<DynamoDbTableListResponse>>().Subject;
        var table = ok.Value!.Tables.Should().ContainSingle().Subject;
        table.Name.Should().Be("orders");
    }

    [Fact]
    public async Task ListTables_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListDynamoDbTablesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListDynamoDbTablesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTables(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetTable_WhenQuerySucceeds_ReturnsOkWithDetail()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var detail = new DynamoDbTableDetail(
            "orders",
            "arn:orders",
            "ACTIVE",
            5,
            1024,
            "PAY_PER_REQUEST",
            10,
            20,
            createdAt,
            [new("id", "HASH")],
            [new("id", "S")],
            [new("gsi-1", "ACTIVE", [new("gid", "HASH")])],
            [new("lsi-1", null, [new("lid", "RANGE")])],
            true,
            "NEW_AND_OLD_IMAGES",
            "arn:orders/stream/2024",
            "ENABLED",
            "expiresAt");
        _sender
            .Send(Arg.Any<GetDynamoDbTableQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetDynamoDbTableQueryResult>>(
                new GetDynamoDbTableQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetTable("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<DynamoDbTableDetailResponse>>().Subject;
        var response = ok.Value!;
        response.Name.Should().Be("orders");
        response.Arn.Should().Be("arn:orders");
        response.Status.Should().Be("ACTIVE");
        response.ItemCount.Should().Be(5);
        response.TableSizeBytes.Should().Be(1024);
        response.BillingMode.Should().Be("PAY_PER_REQUEST");
        response.ReadCapacityUnits.Should().Be(10);
        response.WriteCapacityUnits.Should().Be(20);
        response.CreatedAt.Should().Be(createdAt);
        response.KeySchema.Should().ContainSingle(_ => _.AttributeName == "id" && _.KeyType == "HASH");
        response.Attributes.Should().ContainSingle(_ => _.AttributeName == "id" && _.AttributeType == "S");
        var gsi = response.GlobalSecondaryIndexes.Should().ContainSingle().Subject;
        gsi.Name.Should().Be("gsi-1");
        gsi.Status.Should().Be("ACTIVE");
        gsi.KeySchema.Should().ContainSingle(_ => _.AttributeName == "gid");
        var lsi = response.LocalSecondaryIndexes.Should().ContainSingle().Subject;
        lsi.Name.Should().Be("lsi-1");
        lsi.Status.Should().BeNull();
        lsi.KeySchema.Should().ContainSingle(_ => _.AttributeName == "lid");
        response.StreamEnabled.Should().BeTrue();
        response.StreamViewType.Should().Be("NEW_AND_OLD_IMAGES");
        response.LatestStreamArn.Should().Be("arn:orders/stream/2024");
        response.TtlStatus.Should().Be("ENABLED");
        response.TtlAttributeName.Should().Be("expiresAt");
    }

    [Fact]
    public async Task GetTable_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetDynamoDbTableQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetDynamoDbTableQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetTable("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    private static DynamoDbTableCreateRequest BuildCreateRequest()
        => new("orders", "pk", "S", null, null, "PAY_PER_REQUEST", null, null);

    [Fact]
    public async Task CreateTable_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateDynamoDbTableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateTable(BuildCreateRequest(), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task CreateTable_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateDynamoDbTableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateTable(BuildCreateRequest(), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteTable_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteDynamoDbTableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteTable("orders", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task DeleteTable_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteDynamoDbTableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteTable("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutTtl_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateDynamoDbTtlCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PutTtl(
            "orders",
            new DynamoDbTtlUpdateRequest(true, "expiresAt"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task PutTtl_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateDynamoDbTtlCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutTtl(
            "orders",
            new DynamoDbTtlUpdateRequest(false, "expiresAt"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateIndex_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateDynamoDbIndexCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateIndex(
            "orders",
            new DynamoDbIndexCreateRequest("gsi-1", "gpk", "S", null, null, "ALL"),
            TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task CreateIndex_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateDynamoDbIndexCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateIndex(
            "orders",
            new DynamoDbIndexCreateRequest("gsi-1", "gpk", "S", "gsk", "N", "KEYS_ONLY"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteIndex_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteDynamoDbIndexCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteIndex(
            "orders", "gsi-1", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task DeleteIndex_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteDynamoDbIndexCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteIndex(
            "orders", "gsi-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ExecuteTransaction_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteDynamoDbTransactionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();
        var request = new DynamoDbTransactionRequestBody(
        [
            new DynamoDbTransactionActionRequest("Put", "orders", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var result = await sut.ExecuteTransaction(request, TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        await _sender.Received(1).Send(
            Arg.Is<ExecuteDynamoDbTransactionCommand>(command => command.Actions.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteTransaction_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteDynamoDbTransactionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();
        var request = new DynamoDbTransactionRequestBody(
        [
            new DynamoDbTransactionActionRequest("Delete", "orders", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var result = await sut.ExecuteTransaction(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task BatchWrite_WhenQuerySucceeds_ReturnsOkWithOutcome()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteDynamoDbBatchWriteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecuteDynamoDbBatchWriteQueryResult>>(
                new ExecuteDynamoDbBatchWriteQueryResult(
                    new DynamoDbBatchWriteResult(2, ["{\"pk\":{\"S\":\"a\"}}"]))));
        var sut = CreateSut();
        var request = new DynamoDbBatchWriteRequestBody(
        [
            new DynamoDbBatchWriteItemRequest("Put", "orders", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var result = await sut.BatchWrite(request, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<DynamoDbBatchWriteResponse>>().Subject;
        ok.Value!.Requested.Should().Be(2);
        ok.Value.UnprocessedItems.Should().ContainSingle();
    }

    [Fact]
    public async Task BatchWrite_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteDynamoDbBatchWriteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecuteDynamoDbBatchWriteQueryResult>>(new Error("boom")));
        var sut = CreateSut();
        var request = new DynamoDbBatchWriteRequestBody(
        [
            new DynamoDbBatchWriteItemRequest("Delete", "orders", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var result = await sut.BatchWrite(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task BatchGet_WhenQuerySucceeds_ReturnsOkWithItems()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteDynamoDbBatchGetQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecuteDynamoDbBatchGetQueryResult>>(
                new ExecuteDynamoDbBatchGetQueryResult(
                    new DynamoDbBatchGetResult(1, [new DynamoDbItem("{\"pk\":{\"S\":\"a\"}}")]))));
        var sut = CreateSut();
        var request = new DynamoDbBatchGetRequestBody(
        [
            new DynamoDbBatchGetKeyRequest("orders", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var result = await sut.BatchGet(request, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<DynamoDbBatchGetResponse>>().Subject;
        ok.Value!.Requested.Should().Be(1);
        ok.Value.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task BatchGet_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteDynamoDbBatchGetQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecuteDynamoDbBatchGetQueryResult>>(new Error("boom")));
        var sut = CreateSut();
        var request = new DynamoDbBatchGetRequestBody(
        [
            new DynamoDbBatchGetKeyRequest("orders", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var result = await sut.BatchGet(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    private void StubScan(int truncatedCount = 2, bool truncated = false)
    {
        IReadOnlyList<DynamoDbItem> items =
            Enumerable.Range(0, truncatedCount).Select(_ => new DynamoDbItem("{\"id\":\"a\"}")).ToList();
        _sender
            .Send(Arg.Any<ScanDynamoDbItemsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ScanDynamoDbItemsQueryResult>>(
                new ScanDynamoDbItemsQueryResult(new DynamoDbItemPage(items, truncated))));
    }

    [Fact]
    public async Task ScanItems_WhenQuerySucceeds_ReturnsOkWithItems()
    {
        // Arrange
        StubScan(truncatedCount: 2, truncated: true);
        var sut = CreateSut();

        // Act
        var result = await sut.ScanItems("orders", 10, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<DynamoDbItemListResponse>>().Subject;
        ok.Value!.Items.Should().HaveCount(2);
        ok.Value!.Truncated.Should().BeTrue();
    }

    [Fact]
    public async Task ScanItems_WhenLimitNotPositive_UsesDefaultLimit()
    {
        // Arrange
        StubScan();
        var sut = CreateSut();

        // Act
        await sut.ScanItems("orders", 0, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<ScanDynamoDbItemsQuery>(query => query.Limit == 25),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScanItems_WhenLimitWithinRange_UsesRequestedLimit()
    {
        // Arrange
        StubScan();
        var sut = CreateSut();

        // Act
        await sut.ScanItems("orders", 10, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<ScanDynamoDbItemsQuery>(query => query.Limit == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScanItems_WhenLimitExceedsMaximum_ClampsToMaximum()
    {
        // Arrange
        StubScan();
        var sut = CreateSut();

        // Act
        await sut.ScanItems("orders", 500, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<ScanDynamoDbItemsQuery>(query => query.Limit == 100),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScanItems_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ScanDynamoDbItemsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ScanDynamoDbItemsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ScanItems("orders", 10, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetItem_WhenQuerySucceeds_ReturnsOkWithItem()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetDynamoDbItemQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetDynamoDbItemQueryResult>>(
                new GetDynamoDbItemQueryResult(new DynamoDbItem("{\"id\":\"a\"}"))));
        var sut = CreateSut();

        // Act
        var result = await sut.GetItem("orders", "{\"id\":\"a\"}", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<DynamoDbItemResponse>>().Subject;
        ok.Value!.Json.Should().Be("{\"id\":\"a\"}");
    }

    [Fact]
    public async Task GetItem_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetDynamoDbItemQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetDynamoDbItemQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetItem("orders", "{\"id\":\"a\"}", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutItem_WhenCommandSucceeds_ReturnsOk()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutDynamoDbItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PutItem(
            "orders",
            new DynamoDbItemPutRequest("{\"id\":\"a\"}", "attribute_not_exists(id)"),
            TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        await _sender.Received(1).Send(
            Arg.Is<PutDynamoDbItemCommand>(command =>
                command.ConditionExpression == "attribute_not_exists(id)"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PutItem_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutDynamoDbItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutItem(
            "orders",
            new DynamoDbItemPutRequest("{\"id\":\"a\"}", null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteItem_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteDynamoDbItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteItem("orders", "{\"id\":\"a\"}", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task DeleteItem_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteDynamoDbItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteItem("orders", "{\"id\":\"a\"}", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    private void StubQuery(int itemCount = 2, string? nextToken = null)
    {
        IReadOnlyList<DynamoDbItem> items =
            Enumerable.Range(0, itemCount).Select(_ => new DynamoDbItem("{\"id\":\"a\"}")).ToList();
        _sender
            .Send(Arg.Any<QueryDynamoDbTableQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<QueryDynamoDbTableQueryResult>>(
                new QueryDynamoDbTableQueryResult(new DynamoDbQueryResult(items, nextToken))));
    }

    private static DynamoDbQueryRequestBody QueryBody(int limit = 10)
        => new(
            "gsi-1",
            false,
            new DynamoDbQueryConditionRequest("pk", "=", "S", "a", null),
            new DynamoDbQueryConditionRequest("sk", "between", "N", "1", "9"),
            [new DynamoDbQueryConditionRequest("status", "contains", "S", "OPEN", null)],
            limit,
            "tok");

    [Fact]
    public async Task QueryTable_WhenQuerySucceeds_ReturnsOkWithItemsAndToken()
    {
        // Arrange
        StubQuery(itemCount: 2, nextToken: "next");
        var sut = CreateSut();

        // Act
        var result = await sut.QueryTable("orders", QueryBody(), TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<DynamoDbQueryResultResponse>>().Subject;
        ok.Value!.Items.Should().HaveCount(2);
        ok.Value!.NextToken.Should().Be("next");
    }

    [Fact]
    public async Task QueryTable_WhenScanMode_SendsScanRequestWithoutKeyConditions()
    {
        // Arrange
        StubQuery();
        var sut = CreateSut();
        var body = new DynamoDbQueryRequestBody(null, true, null, null, null, 10, null);

        // Act
        await sut.QueryTable("orders", body, TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<QueryDynamoDbTableQuery>(query =>
                query.Request.Scan
                && query.Request.PartitionKey == null
                && query.Request.SortKey == null
                && query.Request.Filters.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryTable_WhenLimitNotPositive_UsesDefaultLimit()
    {
        // Arrange
        StubQuery();
        var sut = CreateSut();

        // Act
        await sut.QueryTable("orders", QueryBody(limit: 0), TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<QueryDynamoDbTableQuery>(query => query.Request.Limit == 25),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryTable_WhenLimitWithinRange_UsesRequestedLimit()
    {
        // Arrange
        StubQuery();
        var sut = CreateSut();

        // Act
        await sut.QueryTable("orders", QueryBody(limit: 10), TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<QueryDynamoDbTableQuery>(query => query.Request.Limit == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryTable_WhenLimitExceedsMaximum_ClampsToMaximum()
    {
        // Arrange
        StubQuery();
        var sut = CreateSut();

        // Act
        await sut.QueryTable("orders", QueryBody(limit: 500), TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<QueryDynamoDbTableQuery>(query => query.Request.Limit == 100),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryTable_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<QueryDynamoDbTableQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<QueryDynamoDbTableQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.QueryTable("orders", QueryBody(), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    private void StubStatement(int itemCount = 2, string? nextToken = null)
    {
        IReadOnlyList<DynamoDbItem> items =
            Enumerable.Range(0, itemCount).Select(_ => new DynamoDbItem("{\"id\":\"a\"}")).ToList();
        _sender
            .Send(Arg.Any<ExecuteDynamoDbStatementQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecuteDynamoDbStatementQueryResult>>(
                new ExecuteDynamoDbStatementQueryResult(new DynamoDbStatementResult(items, nextToken))));
    }

    private static DynamoDbStatementRequestBody StatementBody(int limit = 10)
        => new("SELECT * FROM \"orders\"", limit, "tok");

    [Fact]
    public async Task ExecuteStatement_WhenStatementSucceeds_ReturnsOkWithItemsAndToken()
    {
        // Arrange
        StubStatement(itemCount: 2, nextToken: "next");
        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteStatement(StatementBody(), TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<DynamoDbStatementResultResponse>>().Subject;
        ok.Value!.Items.Should().HaveCount(2);
        ok.Value!.NextToken.Should().Be("next");
    }

    [Fact]
    public async Task ExecuteStatement_WhenLimitNotPositive_UsesDefaultLimit()
    {
        // Arrange
        StubStatement();
        var sut = CreateSut();

        // Act
        await sut.ExecuteStatement(StatementBody(limit: 0), TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<ExecuteDynamoDbStatementQuery>(query => query.Request.Limit == 25),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteStatement_WhenLimitWithinRange_UsesRequestedLimit()
    {
        // Arrange
        StubStatement();
        var sut = CreateSut();

        // Act
        await sut.ExecuteStatement(StatementBody(limit: 10), TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<ExecuteDynamoDbStatementQuery>(query => query.Request.Limit == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteStatement_WhenLimitExceedsMaximum_ClampsToMaximum()
    {
        // Arrange
        StubStatement();
        var sut = CreateSut();

        // Act
        await sut.ExecuteStatement(StatementBody(limit: 500), TestContext.Current.CancellationToken);

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<ExecuteDynamoDbStatementQuery>(query => query.Request.Limit == 100),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteStatement_WhenStatementFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteDynamoDbStatementQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecuteDynamoDbStatementQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteStatement(StatementBody(), TestContext.Current.CancellationToken);

        // Assert
        var statementStatus = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statementStatus.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
