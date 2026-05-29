import { afterEach, describe, expect, it, vi } from 'vitest';
import {
  getActivity,
  getCatalogue,
  getCliSnippet,
  getConnectivity,
  getDiagnostics,
  getHealth,
  getLiveness,
  getSearch,
  getSearchState,
  refreshCatalogue,
  refreshSearch,
  resolveReference,
  getRecentlyViewed,
  recordRecentlyViewed,
  getFavourites,
  addFavourite,
  removeFavourite,
  executeBulkAction,
  getLambdaFunctions,
  getLambdaFunction,
  getLambdaEnvironment,
  updateLambdaEnvironment,
  invokeLambdaFunction,
  createLambdaFunction,
  updateLambdaFunction,
  deleteLambdaFunction,
  getLambdaTestEvents,
  saveLambdaTestEvent,
  deleteLambdaTestEvent,
  getLambdaEventSourceMappings,
  setLambdaEventSourceMappingState,
  getLambdaLogEvents,
  getLambdaInvocationInsights,
  getLambdaLayers,
  getS3Buckets,
  createS3Bucket,
  deleteS3Bucket,
  getSqsQueues,
  createSqsQueue,
  deleteSqsQueue,
  pollSqsMessages,
  deleteSqsMessage,
  purgeSqsQueue,
  sendSqsMessage,
  getSqsQueueSubscriptions,
  getSqsQueueConsumerLambdas,
  getSqsQueueAttributes,
  getSqsQueueRedrive,
  redriveSqsQueue,
  updateSqsQueueAttributes,
  getS3Objects,
  createS3Folder,
  uploadS3Object,
  deleteS3Object,
  s3ObjectDownloadUrl,
  getS3ObjectPreview,
  getS3PresignedUrl,
  getS3ObjectMetadata,
  updateS3ObjectTags,
  copyS3Object,
  moveS3Object,
  getS3BucketStorageSummary,
  getS3BucketConfiguration,
  getLogGroups,
  getLogStreams,
  getLogEvents,
  filterLogEvents,
  createLogGroup,
  deleteLogGroup,
  getDynamoDbTables,
  getDynamoDbTable,
  createDynamoDbTable,
  deleteDynamoDbTable,
  scanDynamoDbItems,
  getDynamoDbItem,
  putDynamoDbItem,
  deleteDynamoDbItem,
  queryDynamoDbTable,
  executeDynamoDbStatement,
  getSecrets,
  createSecret,
  deleteSecret,
  getSecretValue,
  putSecretValue,
  getSecretVersions,
  getParameters,
  createParameter,
  deleteParameter,
  getParameterValue,
  updateParameterValue,
  getParameterHistory,
  getSnsTopics,
  createSnsTopic,
  deleteSnsTopic,
  getSnsSubscriptions,
} from './client';
import type {
  DynamoDbQueryRequest,
  DynamoDbStatementRequest,
  SecretCreateRequest,
  SecretValueUpdateRequest,
  ParameterCreateRequest,
  SnsTopicCreateRequest,
} from './client';

describe('getLiveness', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed status when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ status: 'Healthy' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getLiveness();

    expect(result.status).toBe('Healthy');
    expect(fetchMock).toHaveBeenCalledWith('/api/system/liveness', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(getLiveness()).rejects.toThrow('Liveness request failed with status 503');
  });
});

describe('getHealth', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed services when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        services: [
          { key: 's3', availability: 'Available' },
          { key: 'lambda', availability: 'Unavailable' },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getHealth();

    expect(result.services).toHaveLength(2);
    expect(result.services[0].availability).toBe('Available');
    expect(fetchMock).toHaveBeenCalledWith('/api/system/health', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(getHealth()).rejects.toThrow('Health request failed with status 503');
  });
});

describe('getConnectivity', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed result when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        status: 'Connected',
        endpoint: 'http://localhost:4566',
        region: 'eu-west-1',
        error: null,
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getConnectivity();

    expect(result.status).toBe('Connected');
    expect(result.endpoint).toBe('http://localhost:4566');
    expect(result.region).toBe('eu-west-1');
    expect(result.error).toBeNull();
    expect(fetchMock).toHaveBeenCalledWith('/api/system/connectivity', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(getConnectivity()).rejects.toThrow('Connectivity request failed with status 503');
  });
});

describe('getCatalogue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed services when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        services: [
          {
            key: 's3',
            displayName: 'S3',
            category: 'Storage',
            iconHint: 'archive',
            route: '/services/s3',
            supported: true,
            supportDetail: null,
          },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getCatalogue();

    expect(result.services).toHaveLength(1);
    expect(result.services[0].key).toBe('s3');
    expect(result.services[0].supported).toBe(true);
    expect(result.services[0].supportDetail).toBeNull();
    expect(fetchMock).toHaveBeenCalledWith('/api/system/catalogue', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(getCatalogue()).rejects.toThrow('Catalogue request failed with status 503');
  });
});

describe('refreshCatalogue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts a refresh request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await refreshCatalogue();

    expect(fetchMock).toHaveBeenCalledWith('/api/system/catalogue/refresh', {
      method: 'POST',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(refreshCatalogue()).rejects.toThrow('Catalogue refresh request failed with status 503');
  });
});

describe('getActivity', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed entries when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        entries: [
          {
            operationId: 'op-1',
            operation: 'catalogue-refresh',
            state: 'Succeeded',
            message: 'Service catalogue refreshed.',
            occurredAt: '2026-01-02T03:04:05Z',
          },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getActivity();

    expect(result.entries).toHaveLength(1);
    expect(result.entries[0].operation).toBe('catalogue-refresh');
    expect(result.entries[0].state).toBe('Succeeded');
    expect(fetchMock).toHaveBeenCalledWith('/api/system/activity', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(getActivity()).rejects.toThrow('Activity request failed with status 503');
  });
});

describe('resolveReference', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('resolves an ARN without a service hint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ serviceKey: 'sqs', resourceId: 'orders', route: '/services/sqs/orders' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await resolveReference('arn:aws:sqs:eu-west-1:000000000000:orders');

    expect(result.serviceKey).toBe('sqs');
    expect(result.resourceId).toBe('orders');
    expect(result.route).toBe('/services/sqs/orders');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/navigation/resolve?ref=arn%3Aaws%3Asqs%3Aeu-west-1%3A000000000000%3Aorders',
      { signal: undefined },
    );
  });

  it('includes the service hint when provided', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ serviceKey: 'sqs', resourceId: 'orders', route: '/services/sqs/orders' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await resolveReference('orders', 'sqs');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/navigation/resolve?ref=orders&service=sqs',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 400 }),
    );

    await expect(resolveReference('not-an-arn')).rejects.toThrow(
      'Reference resolution failed with status 400',
    );
  });
});

describe('getSearch', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed matches when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        matches: [
          { serviceKey: 'sqs', resourceId: 'orders', displayName: 'orders', route: '/services/sqs/orders' },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getSearch('ord');

    expect(result.matches).toHaveLength(1);
    expect(result.matches[0].serviceKey).toBe('sqs');
    expect(result.matches[0].route).toBe('/services/sqs/orders');
    expect(fetchMock).toHaveBeenCalledWith('/api/search?q=ord', { signal: undefined });
  });

  it('encodes the query string', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ matches: [] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await getSearch('a b');

    expect(fetchMock).toHaveBeenCalledWith('/api/search?q=a+b', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(getSearch('ord')).rejects.toThrow('Search request failed with status 503');
  });
});

describe('getSearchState', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed state when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ builtAt: '2026-01-02T03:04:05Z', entryCount: 7, isBuilding: false }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getSearchState();

    expect(result.entryCount).toBe(7);
    expect(result.isBuilding).toBe(false);
    expect(result.builtAt).toBe('2026-01-02T03:04:05Z');
    expect(fetchMock).toHaveBeenCalledWith('/api/search/state', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(getSearchState()).rejects.toThrow('Search state request failed with status 503');
  });
});

describe('refreshSearch', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts a refresh request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await refreshSearch();

    expect(fetchMock).toHaveBeenCalledWith('/api/search/refresh', {
      method: 'POST',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 503 }),
    );

    await expect(refreshSearch()).rejects.toThrow('Search refresh request failed with status 503');
  });
});

describe('getRecentlyViewed', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed references when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ references: ['sqs://orders', 'sns://events'] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getRecentlyViewed();

    expect(result.references).toEqual(['sqs://orders', 'sns://events']);
    expect(fetchMock).toHaveBeenCalledWith('/api/user/recently-viewed', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getRecentlyViewed()).rejects.toThrow('Recently viewed request failed with status 503');
  });
});

describe('recordRecentlyViewed', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the reference when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await recordRecentlyViewed('sqs://orders');

    expect(fetchMock).toHaveBeenCalledWith('/api/user/recently-viewed', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reference: 'sqs://orders' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(recordRecentlyViewed('sqs://orders')).rejects.toThrow(
      'Record recently viewed request failed with status 503',
    );
  });
});

describe('getFavourites', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed references when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ references: ['s3://reports'] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getFavourites();

    expect(result.references).toEqual(['s3://reports']);
    expect(fetchMock).toHaveBeenCalledWith('/api/user/favourites', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getFavourites()).rejects.toThrow('Favourites request failed with status 503');
  });
});

describe('addFavourite', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the reference when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await addFavourite('s3://reports');

    expect(fetchMock).toHaveBeenCalledWith('/api/user/favourites', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reference: 's3://reports' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(addFavourite('s3://reports')).rejects.toThrow('Add favourite request failed with status 503');
  });
});

describe('removeFavourite', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('deletes the reference when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await removeFavourite('s3://reports');

    expect(fetchMock).toHaveBeenCalledWith('/api/user/favourites', {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reference: 's3://reports' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(removeFavourite('s3://reports')).rejects.toThrow('Remove favourite request failed with status 503');
  });
});

describe('getDiagnostics', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests masked values by default and returns the parsed result', async () => {
    const payload = {
      configuration: [{ name: 'Access key', value: '********', source: 'EnvironmentVariable', isSensitive: true }],
      endpoint: 'http://localhost:4566',
      region: 'eu-west-1',
      connectivityStatus: 'Connected',
      connectivityError: null,
      revealAllowed: false,
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getDiagnostics();

    expect(result).toEqual(payload);
    expect(fetchMock).toHaveBeenCalledWith('/api/system/diagnostics?reveal=false', { signal: undefined });
  });

  it('requests revealed values when reveal is true', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        configuration: [],
        endpoint: 'e',
        region: 'r',
        connectivityStatus: 'Connected',
        connectivityError: null,
        revealAllowed: true,
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await getDiagnostics(true);

    expect(fetchMock).toHaveBeenCalledWith('/api/system/diagnostics?reveal=true', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getDiagnostics()).rejects.toThrow('Diagnostics request failed with status 500');
  });
});

describe('getCliSnippet', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the request and returns the generated command', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ command: 'aws s3api list-buckets --endpoint-url http://localhost:4566 --region eu-west-1' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getCliSnippet({
      service: 's3api',
      operation: 'head-bucket',
      parameters: [{ name: 'bucket', value: 'my-bucket', isSensitive: false }],
    });

    expect(result.command).toContain('aws s3api list-buckets');
    expect(fetchMock).toHaveBeenCalledWith('/api/system/cli-snippet', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        service: 's3api',
        operation: 'head-bucket',
        parameters: [{ name: 'bucket', value: 'my-bucket', isSensitive: false }],
      }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(
      getCliSnippet({ service: 's3api', operation: 'list-buckets', parameters: [] }),
    ).rejects.toThrow('CLI snippet request failed with status 500');
  });
});

describe('executeBulkAction', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the resource ids and returns the per-item results', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        operationId: 'op-1',
        action: 'delete',
        totalCount: 2,
        succeededCount: 1,
        failedCount: 1,
        overallState: 'Failed',
        items: [
          { resourceId: 'a', succeeded: true, error: null },
          { resourceId: 'b', succeeded: false, error: 'boom' },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await executeBulkAction('delete', ['a', 'b']);

    expect(result.succeededCount).toBe(1);
    expect(result.failedCount).toBe(1);
    expect(result.items[1].error).toBe('boom');
    expect(fetchMock).toHaveBeenCalledWith('/api/bulk/delete', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ resourceIds: ['a', 'b'] }),
      signal: undefined,
    });
  });

  it('encodes the action in the request path', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        operationId: 'op-2',
        action: 'force delete',
        totalCount: 0,
        succeededCount: 0,
        failedCount: 0,
        overallState: 'Succeeded',
        items: [],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await executeBulkAction('force delete', []);

    expect(fetchMock).toHaveBeenCalledWith('/api/bulk/force%20delete', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ resourceIds: [] }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(executeBulkAction('delete', ['a'])).rejects.toThrow(
      'Bulk action request failed with status 500',
    );
  });
});

describe('getLambdaFunctions', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed functions when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        functions: [
          {
            functionName: 'process-orders',
            runtime: 'dotnet8',
            description: 'Order processor',
            lastModified: '2026-01-02T03:04:05Z',
            memorySize: 256,
            timeout: 30,
          },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getLambdaFunctions();

    expect(result.functions).toHaveLength(1);
    expect(result.functions[0].functionName).toBe('process-orders');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/lambda/functions', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getLambdaFunctions()).rejects.toThrow(
      'Lambda functions request failed with status 503',
    );
  });
});

describe('getLambdaFunction', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded function name and returns the parsed result', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        functionName: 'process orders',
        functionArn: 'arn:aws:lambda:eu-west-1:000000000000:function:process-orders',
        runtime: 'dotnet8',
        handler: 'Orders::Handler',
        description: 'Order processor',
        lastModified: '2026-01-02T03:04:05Z',
        memorySize: 256,
        timeout: 30,
        role: 'arn:aws:iam::000000000000:role/lambda-orders',
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getLambdaFunction('process orders');

    expect(result.handler).toBe('Orders::Handler');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/lambda/functions/process%20orders', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getLambdaFunction('missing')).rejects.toThrow(
      'Lambda function request failed with status 404',
    );
  });
});

describe('getLambdaEnvironment', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded function name with the reveal flag and returns the parsed result', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        variables: [{ name: 'API_KEY', value: '********', isSensitive: true }],
        revealAllowed: true,
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getLambdaEnvironment('process orders', true);

    expect(result.revealAllowed).toBe(true);
    expect(result.variables[0].name).toBe('API_KEY');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/environment?reveal=true',
      { signal: undefined },
    );
  });

  it('defaults the reveal flag to false', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ variables: [], revealAllowed: false }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await getLambdaEnvironment('orders');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/orders/environment?reveal=false',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getLambdaEnvironment('orders')).rejects.toThrow(
      'Lambda environment request failed with status 500',
    );
  });
});

describe('updateLambdaEnvironment', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the variables as a PUT body to the encoded endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await updateLambdaEnvironment('process orders', [{ name: 'STAGE', value: 'prod' }]);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/environment',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ variables: [{ name: 'STAGE', value: 'prod' }] }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 409 }));

    await expect(
      updateLambdaEnvironment('orders', [{ name: 'STAGE', value: 'prod' }]),
    ).rejects.toThrow('Lambda environment update request failed with status 409');
  });
});

describe('invokeLambdaFunction', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the payload to the encoded endpoint and returns the result', async () => {
    const invocation = {
      statusCode: 200,
      payload: '{"ok":true}',
      logTail: 'log',
      functionError: '',
      durationMs: 12,
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(invocation),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await invokeLambdaFunction('process orders', '{"id":1}');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/invocations',
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ payload: '{"id":1}' }),
        signal: undefined,
      },
    );
    expect(result).toEqual(invocation);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(invokeLambdaFunction('orders', '{}')).rejects.toThrow(
      'Lambda invoke request failed with status 500',
    );
  });
});

describe('createLambdaFunction', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const payload = {
    functionName: 'new-fn',
    runtime: 'dotnet8',
    handler: 'index.handler',
    role: 'arn:aws:iam::000000000000:role/lambda',
    description: 'A new function',
    memorySize: 256,
    timeout: 15,
    zipFileBase64: 'QkFTRTY0',
  };

  it('posts the payload to the functions endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createLambdaFunction(payload);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/lambda/functions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createLambdaFunction(payload)).rejects.toThrow(
      'Lambda create request failed with status 400',
    );
  });
});

describe('updateLambdaFunction', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const payload = {
    runtime: 'dotnet8',
    handler: 'index.handler',
    role: 'arn:aws:iam::000000000000:role/lambda',
    description: 'A new function',
    memorySize: 256,
    timeout: 15,
    zipFileBase64: null,
  };

  it('puts the payload to the encoded endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await updateLambdaFunction('process orders', payload);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(updateLambdaFunction('orders', payload)).rejects.toThrow(
      'Lambda update request failed with status 404',
    );
  });
});

describe('deleteLambdaFunction', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a DELETE to the encoded endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteLambdaFunction('process orders');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders',
      {
        method: 'DELETE',
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteLambdaFunction('orders')).rejects.toThrow(
      'Lambda delete request failed with status 404',
    );
  });
});

describe('getLambdaTestEvents', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded test-events endpoint and returns the payload', async () => {
    const result = { events: [{ name: 'a', payload: '{}' }], templates: [] };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(result),
    });
    vi.stubGlobal('fetch', fetchMock);

    await expect(getLambdaTestEvents('process orders')).resolves.toEqual(result);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/test-events',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getLambdaTestEvents('orders')).rejects.toThrow(
      'Lambda test events request failed with status 500',
    );
  });
});

describe('saveLambdaTestEvent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a PUT with the event name and payload', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await saveLambdaTestEvent('process orders', 'my event', '{"a":1}');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/test-events',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: 'my event', payload: '{"a":1}' }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(saveLambdaTestEvent('orders', 'name', '{}')).rejects.toThrow(
      'Lambda test event save request failed with status 400',
    );
  });
});

describe('deleteLambdaTestEvent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a DELETE to the encoded endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteLambdaTestEvent('process orders', 'my event');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/test-events/my%20event',
      {
        method: 'DELETE',
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteLambdaTestEvent('orders', 'name')).rejects.toThrow(
      'Lambda test event delete request failed with status 404',
    );
  });
});

describe('getLambdaEventSourceMappings', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded event-source-mappings endpoint and returns the payload', async () => {
    const result = {
      mappings: [
        {
          uuid: 'abc',
          eventSourceArn: 'arn:aws:sqs:us-east-1:000000000000:orders',
          functionArn: 'arn:aws:lambda:us-east-1:000000000000:function:orders',
          state: 'Enabled',
          batchSize: 10,
          lastModified: '2026-01-02T03:04:05.0000000Z',
        },
      ],
      s3Triggers: [{ bucketArn: 'arn:aws:s3:::orders-bucket' }],
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(result),
    });
    vi.stubGlobal('fetch', fetchMock);

    await expect(getLambdaEventSourceMappings('process orders')).resolves.toEqual(result);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/event-source-mappings',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getLambdaEventSourceMappings('orders')).rejects.toThrow(
      'Lambda event source mappings request failed with status 500',
    );
  });
});

describe('setLambdaEventSourceMappingState', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a PUT with the enabled flag to the encoded endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await setLambdaEventSourceMappingState('process orders', 'my uuid', false);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/event-source-mappings/my%20uuid/state',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ enabled: false }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(setLambdaEventSourceMappingState('orders', 'uuid', true)).rejects.toThrow(
      'Lambda event source mapping state request failed with status 400',
    );
  });
});

describe('getLambdaLogEvents', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded logs endpoint with the limit query and returns the payload', async () => {
    const result = {
      logGroupName: '/aws/lambda/orders',
      events: [
        {
          timestamp: '2026-01-02T03:04:05.0000000+00:00',
          message: 'START RequestId: abc',
          logStreamName: '2026/01/02/[$LATEST]abcdef',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(result),
    });
    vi.stubGlobal('fetch', fetchMock);

    await expect(getLambdaLogEvents('process orders', 50)).resolves.toEqual(result);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/logs?limit=50',
      { signal: undefined },
    );
  });

  it('omits the limit query when no limit is supplied', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ logGroupName: '/aws/lambda/orders', events: [] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await getLambdaLogEvents('orders');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/lambda/functions/orders/logs', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getLambdaLogEvents('orders')).rejects.toThrow(
      'Lambda log events request failed with status 500',
    );
  });
});

describe('getLambdaInvocationInsights', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded insights endpoint with the limit query and returns the payload', async () => {
    const result = {
      logGroupName: '/aws/lambda/orders',
      metrics: { invocationCount: 2, errorCount: 1, averageDurationMs: 15, maxDurationMs: 30 },
      recentInvocations: [
        {
          requestId: 'abc',
          timestamp: '2026-01-02T03:04:05.0000000+00:00',
          durationMs: 30,
          hasError: true,
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(result),
    });
    vi.stubGlobal('fetch', fetchMock);

    await expect(getLambdaInvocationInsights('process orders', 50)).resolves.toEqual(result);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/invocation-insights?limit=50',
      { signal: undefined },
    );
  });

  it('omits the limit query when no limit is supplied', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () =>
        Promise.resolve({
          logGroupName: '/aws/lambda/orders',
          metrics: { invocationCount: 0, errorCount: 0, averageDurationMs: 0, maxDurationMs: 0 },
          recentInvocations: [],
        }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await getLambdaInvocationInsights('orders');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/orders/invocation-insights',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getLambdaInvocationInsights('orders')).rejects.toThrow(
      'Lambda invocation insights request failed with status 500',
    );
  });
});

describe('getLambdaLayers', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded layers endpoint and returns the payload', async () => {
    const result = {
      layers: [
        { arn: 'arn:aws:lambda:eu-west-1:123456789012:layer:shared-utils:7', name: 'shared-utils', version: '7' },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(result),
    });
    vi.stubGlobal('fetch', fetchMock);

    await expect(getLambdaLayers('process orders')).resolves.toEqual(result);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/lambda/functions/process%20orders/layers',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getLambdaLayers('orders')).rejects.toThrow(
      'Lambda layers request failed with status 500',
    );
  });
});

describe('getS3Buckets', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed buckets when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        buckets: [{ name: 'orders', creationDate: '2026-01-02T03:04:05.0000000Z' }],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getS3Buckets();

    expect(result.buckets).toHaveLength(1);
    expect(result.buckets[0].name).toBe('orders');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/s3/buckets', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getS3Buckets()).rejects.toThrow('S3 buckets request failed with status 503');
  });
});

describe('getS3BucketStorageSummary', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed summary when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ objectCount: 9, totalSizeBytes: 8192 }),
    });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getS3BucketStorageSummary('my bucket', controller.signal);

    expect(result).toEqual({ objectCount: 9, totalSizeBytes: 8192 });
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/storage-summary',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getS3BucketStorageSummary('orders')).rejects.toThrow(
      'S3 bucket storage summary request failed with status 500',
    );
  });
});

describe('getS3BucketConfiguration', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed configuration when the request succeeds', async () => {
    const configuration = {
      versioningStatus: 'Enabled',
      encryptionAlgorithm: 'aws:kms',
      encryptionKeyId: 'key-1',
      lifecycleRules: [{ id: 'rule-1', status: 'Enabled', prefix: 'logs/' }],
      notifications: [{ type: 'Queue', targetArn: 'arn:aws:sqs:eu-west-1:000000000000:orders', events: ['s3:ObjectCreated:*'] }],
      policy: '{"Version":"2012-10-17"}',
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => configuration,
    });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getS3BucketConfiguration('my bucket', controller.signal);

    expect(result).toEqual(configuration);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/configuration',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getS3BucketConfiguration('orders')).rejects.toThrow(
      'S3 bucket configuration request failed with status 500',
    );
  });
});

describe('createS3Bucket', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the bucket name to the buckets endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createS3Bucket('orders');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/s3/buckets', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ bucketName: 'orders' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createS3Bucket('orders')).rejects.toThrow(
      'S3 bucket create request failed with status 400',
    );
  });
});

describe('deleteS3Bucket', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a DELETE to the encoded endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteS3Bucket('my bucket');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/s3/buckets/my%20bucket', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteS3Bucket('orders')).rejects.toThrow(
      'S3 bucket delete request failed with status 404',
    );
  });
});

describe('getSqsQueues', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed queue list when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        queues: [
          {
            name: 'orders',
            url: 'http://localstack:4566/000000000000/orders',
            approximateMessageCount: 3,
            approximateInFlightCount: 1,
            approximateDelayedCount: 0,
          },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    const result = await getSqsQueues(controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/sqs/queues', {
      signal: controller.signal,
    });
    expect(result.queues).toHaveLength(1);
    expect(result.queues[0]?.name).toBe('orders');
    expect(result.queues[0]?.approximateMessageCount).toBe(3);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getSqsQueues()).rejects.toThrow('SQS queues request failed with status 503');
  });
});

describe('createSqsQueue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the queue name and fifo flag to the queues endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createSqsQueue('orders', false);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/sqs/queues', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ queueName: 'orders', fifoQueue: false }),
      signal: undefined,
    });
  });

  it('posts the fifo flag as true when requested', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createSqsQueue('orders.fifo', true);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/sqs/queues', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ queueName: 'orders.fifo', fifoQueue: true }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createSqsQueue('orders', false)).rejects.toThrow(
      'SQS queue create request failed with status 400',
    );
  });
});

describe('deleteSqsQueue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a DELETE to the encoded endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteSqsQueue('my queue');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/sqs/queues/my%20queue', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteSqsQueue('orders')).rejects.toThrow(
      'SQS queue delete request failed with status 404',
    );
  });
});

describe('pollSqsMessages', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed message list and builds the peek query', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        messages: [
          {
            messageId: 'id-1',
            receiptHandle: 'receipt-1',
            body: 'hello',
            attributes: { SentTimestamp: '1' },
            messageAttributes: { trace: 'abc' },
          },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    const result = await pollSqsMessages('my queue', 'peek', 5, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/messages?mode=peek&maxMessages=5',
      { signal: controller.signal },
    );
    expect(result.messages).toHaveLength(1);
    expect(result.messages[0]?.messageId).toBe('id-1');
  });

  it('omits maxMessages when it is not provided', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ messages: [] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await pollSqsMessages('orders', 'consume');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/orders/messages?mode=consume',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(pollSqsMessages('orders', 'peek')).rejects.toThrow(
      'SQS poll request failed with status 500',
    );
  });
});

describe('deleteSqsMessage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('issues a delete with the encoded queue name and receipt handle', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteSqsMessage('my queue', 'receipt/handle');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/messages?receiptHandle=receipt%2Fhandle',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteSqsMessage('orders', 'receipt-1')).rejects.toThrow(
      'SQS delete request failed with status 404',
    );
  });
});

describe('purgeSqsQueue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('issues a post to the purge endpoint with the encoded queue name', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    await purgeSqsQueue('my queue', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/purge',
      { method: 'POST', signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(purgeSqsQueue('orders')).rejects.toThrow(
      'SQS purge request failed with status 500',
    );
  });
});

describe('sendSqsMessage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the message body and ids to the encoded send endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    await sendSqsMessage(
      'orders.fifo',
      {
        body: 'hello world',
        messageAttributes: { source: 'web' },
        messageGroupId: 'group-1',
        messageDeduplicationId: 'dedup-1',
      },
      controller.signal,
    );

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/orders.fifo/messages',
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          body: 'hello world',
          messageAttributes: { source: 'web' },
          messageGroupId: 'group-1',
          messageDeduplicationId: 'dedup-1',
        }),
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(sendSqsMessage('orders', { body: 'hi' })).rejects.toThrow(
      'SQS send request failed with status 400',
    );
  });
});

describe('getSqsQueueSubscriptions', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed subscriptions when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        subscriptions: [
          { topicArn: 'arn:aws:sns:eu-west-1:000000000000:order-events', topicName: 'order-events' },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    const result = await getSqsQueueSubscriptions('my queue', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/subscriptions',
      { signal: controller.signal },
    );
    expect(result.subscriptions).toHaveLength(1);
    expect(result.subscriptions[0]?.topicName).toBe('order-events');
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getSqsQueueSubscriptions('orders')).rejects.toThrow(
      'SQS subscriptions request failed with status 500',
    );
  });
});

describe('getSqsQueueConsumerLambdas', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed consumer Lambdas when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        lambdas: [
          {
            functionName: 'order-processor',
            functionArn: 'arn:aws:lambda:eu-west-1:000000000000:function:order-processor',
            state: 'Enabled',
          },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    const result = await getSqsQueueConsumerLambdas('my queue', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/lambda-triggers',
      { signal: controller.signal },
    );
    expect(result.lambdas).toHaveLength(1);
    expect(result.lambdas[0]?.functionName).toBe('order-processor');
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getSqsQueueConsumerLambdas('orders')).rejects.toThrow(
      'SQS consumer Lambdas request failed with status 500',
    );
  });
});

describe('getSqsQueueAttributes', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed attributes when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        visibilityTimeoutSeconds: 45,
        messageRetentionPeriodSeconds: 86400,
        delaySeconds: 10,
        receiveMessageWaitTimeSeconds: 5,
        maximumMessageSizeBytes: 262144,
        queueArn: 'arn:aws:sqs:eu-west-1:000000000000:orders',
        fifoQueue: false,
        approximateMessageCount: 7,
        approximateInFlightCount: 3,
        approximateDelayedCount: 2,
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    const result = await getSqsQueueAttributes('my queue', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/attributes',
      { signal: controller.signal },
    );
    expect(result.visibilityTimeoutSeconds).toBe(45);
    expect(result.queueArn).toBe('arn:aws:sqs:eu-west-1:000000000000:orders');
    expect(result.fifoQueue).toBe(false);
    expect(result.approximateMessageCount).toBe(7);
    expect(result.approximateInFlightCount).toBe(3);
    expect(result.approximateDelayedCount).toBe(2);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getSqsQueueAttributes('orders')).rejects.toThrow(
      'SQS attributes request failed with status 500',
    );
  });
});

describe('updateSqsQueueAttributes', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the editable attributes to the encoded attributes endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    await updateSqsQueueAttributes(
      'my queue',
      {
        visibilityTimeoutSeconds: 45,
        messageRetentionPeriodSeconds: 86400,
        delaySeconds: 10,
        receiveMessageWaitTimeSeconds: 5,
      },
      controller.signal,
    );

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/attributes',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          visibilityTimeoutSeconds: 45,
          messageRetentionPeriodSeconds: 86400,
          delaySeconds: 10,
          receiveMessageWaitTimeSeconds: 5,
        }),
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(
      updateSqsQueueAttributes('orders', {
        visibilityTimeoutSeconds: 0,
        messageRetentionPeriodSeconds: 60,
        delaySeconds: 0,
        receiveMessageWaitTimeSeconds: 0,
      }),
    ).rejects.toThrow('SQS attributes update request failed with status 400');
  });
});

describe('getSqsQueueRedrive', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed redrive relationships when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        deadLetterTarget: {
          queueArn: 'arn:aws:sqs:eu-west-1:000000000000:orders-dlq',
          queueName: 'orders-dlq',
          maxReceiveCount: 5,
        },
        sources: [
          { queueArn: 'arn:aws:sqs:eu-west-1:000000000000:orders', queueName: 'orders' },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    const result = await getSqsQueueRedrive('my queue', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/redrive',
      { signal: controller.signal },
    );
    expect(result.deadLetterTarget?.queueName).toBe('orders-dlq');
    expect(result.sources).toHaveLength(1);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getSqsQueueRedrive('orders')).rejects.toThrow(
      'SQS redrive request failed with status 500',
    );
  });
});

describe('redriveSqsQueue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts to the encoded redrive endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    await redriveSqsQueue('my queue', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sqs/queues/my%20queue/redrive',
      { method: 'POST', signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(redriveSqsQueue('orders')).rejects.toThrow(
      'SQS redrive start request failed with status 400',
    );
  });
});

describe('getS3Objects', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed listing when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        prefixes: ['orders/2026/'],
        objects: [{ key: 'orders/readme.txt', size: 12, lastModified: '2026-01-02T03:04:05.0000000Z' }],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getS3Objects('my bucket', 'orders/');

    expect(result.prefixes).toEqual(['orders/2026/']);
    expect(result.objects[0].key).toBe('orders/readme.txt');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/objects?prefix=orders%2F',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getS3Objects('orders', '')).rejects.toThrow(
      'S3 objects request failed with status 503',
    );
  });
});

describe('createS3Folder', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the folder key to the folders endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createS3Folder('my bucket', 'orders/2026/');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/s3/buckets/my%20bucket/folders', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ folderKey: 'orders/2026/' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createS3Folder('orders', 'data/')).rejects.toThrow(
      'S3 folder create request failed with status 400',
    );
  });
});

describe('uploadS3Object', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the file and prefix as multipart form data', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const file = new File(['hello'], 'note.txt', { type: 'text/plain' });

    await uploadS3Object('my bucket', 'orders/', file);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('/api/services/s3/buckets/my%20bucket/objects');
    expect(init.method).toBe('POST');
    expect(init.body).toBeInstanceOf(FormData);
    const form = init.body as FormData;
    expect(form.get('file')).toBe(file);
    expect(form.get('prefix')).toBe('orders/');
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));
    const file = new File(['hello'], 'note.txt', { type: 'text/plain' });

    await expect(uploadS3Object('orders', '', file)).rejects.toThrow(
      'S3 object upload request failed with status 500',
    );
  });
});

describe('deleteS3Object', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a DELETE request with the encoded key', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteS3Object('my bucket', 'orders/readme.txt');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/objects?key=orders%2Freadme.txt',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteS3Object('orders', 'data.txt')).rejects.toThrow(
      'S3 object delete request failed with status 404',
    );
  });
});

describe('s3ObjectDownloadUrl', () => {
  it('builds an encoded download URL', () => {
    expect(s3ObjectDownloadUrl('my bucket', 'orders/readme.txt')).toBe(
      '/api/services/s3/buckets/my%20bucket/objects/content?key=orders%2Freadme.txt',
    );
  });
});

describe('getS3ObjectPreview', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded preview endpoint and returns the result', async () => {
    const payload = {
      kind: 'Text',
      contentType: 'text/plain',
      truncated: false,
      totalSize: 11,
      text: 'hello world',
      dataUrl: null,
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(payload),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getS3ObjectPreview('my bucket', 'orders/readme.txt');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/objects/preview?key=orders%2Freadme.txt',
      { signal: undefined },
    );
    expect(result).toEqual(payload);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getS3ObjectPreview('orders', 'data.txt')).rejects.toThrow(
      'S3 object preview request failed with status 404',
    );
  });
});

describe('getS3PresignedUrl', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded presign endpoint and returns the result', async () => {
    const payload = { url: 'https://example.test/presigned', expirySeconds: 900 };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(payload),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getS3PresignedUrl('my bucket', 'orders/readme.txt', 900);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/objects/presign?key=orders%2Freadme.txt&expirySeconds=900',
      { signal: undefined },
    );
    expect(result).toEqual(payload);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 403 }));

    await expect(getS3PresignedUrl('orders', 'data.txt', 60)).rejects.toThrow(
      'S3 presigned URL request failed with status 403',
    );
  });
});

describe('getS3ObjectMetadata', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests the encoded metadata endpoint and returns the result', async () => {
    const payload = {
      contentType: 'text/plain',
      contentLength: 42,
      lastModified: '2026-01-02T03:04:05.0000000Z',
      eTag: '"abc123"',
      metadata: [{ key: 'owner', value: 'alice' }],
      tags: [{ key: 'stage', value: 'prod' }],
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(payload),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getS3ObjectMetadata('my bucket', 'orders/readme.txt');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/objects/metadata?key=orders%2Freadme.txt',
      { signal: undefined },
    );
    expect(result).toEqual(payload);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getS3ObjectMetadata('orders', 'data.txt')).rejects.toThrow(
      'S3 object metadata request failed with status 404',
    );
  });
});

describe('updateS3ObjectTags', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a PUT request with the encoded endpoint and tag body', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await updateS3ObjectTags('my bucket', 'orders/readme.txt', { stage: 'prod' });

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/objects/tags?key=orders%2Freadme.txt',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tags: { stage: 'prod' } }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(updateS3ObjectTags('orders', 'data.txt', {})).rejects.toThrow(
      'S3 object tags update request failed with status 400',
    );
  });
});

describe('copyS3Object', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a POST request with the encoded endpoint and destination body', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await copyS3Object('my bucket', 'orders/readme.txt', 'archive', 'orders/2026/readme.txt');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/objects/copy?key=orders%2Freadme.txt',
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          destinationBucketName: 'archive',
          destinationKey: 'orders/2026/readme.txt',
        }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(
      copyS3Object('orders', 'data.txt', 'orders', 'copies/data.txt'),
    ).rejects.toThrow('S3 object copy request failed with status 400');
  });
});

describe('moveS3Object', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a POST request with the encoded endpoint and destination body', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await moveS3Object('my bucket', 'orders/readme.txt', 'archive', 'moved/readme.txt');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/s3/buckets/my%20bucket/objects/move?key=orders%2Freadme.txt',
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          destinationBucketName: 'archive',
          destinationKey: 'moved/readme.txt',
        }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(
      moveS3Object('orders', 'data.txt', 'orders', 'moved/data.txt'),
    ).rejects.toThrow('S3 object move request failed with status 400');
  });
});

describe('getLogGroups', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed log groups when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        logGroups: [
          {
            name: '/aws/lambda/orders',
            arn: 'arn:aws:logs:eu-west-1:000000000000:log-group:/aws/lambda/orders',
            storedBytes: 2048,
            retentionInDays: 7,
            createdAt: '2026-01-02T03:04:05Z',
          },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getLogGroups();

    expect(result.logGroups).toHaveLength(1);
    expect(result.logGroups[0].name).toBe('/aws/lambda/orders');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/cloudwatch-logs/groups', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getLogGroups()).rejects.toThrow('Log groups request failed with status 503');
  });
});

describe('createLogGroup', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the log group name to the groups endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createLogGroup('/aws/lambda/orders');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/cloudwatch-logs/groups', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ logGroupName: '/aws/lambda/orders' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createLogGroup('/aws/lambda/orders')).rejects.toThrow(
      'Log group create request failed with status 400',
    );
  });
});

describe('deleteLogGroup', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a DELETE with the log group name query parameter', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteLogGroup('/aws/lambda/orders');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudwatch-logs/groups?logGroupName=%2Faws%2Flambda%2Forders',
      {
        method: 'DELETE',
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteLogGroup('/aws/lambda/orders')).rejects.toThrow(
      'Log group delete request failed with status 404',
    );
  });
});

describe('getLogStreams', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed log streams when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        logStreams: [{ name: '2026/01/02/[$LATEST]abc', lastEventTimestamp: '2026-01-02T03:04:05Z' }],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getLogStreams('/aws/lambda/orders');

    expect(result.logStreams).toHaveLength(1);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudwatch-logs/streams?logGroupName=%2Faws%2Flambda%2Forders',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getLogStreams('/aws/lambda/orders')).rejects.toThrow(
      'Log streams request failed with status 404',
    );
  });
});

describe('getLogEvents', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed log events when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        events: [{ timestamp: '2026-01-02T03:04:05Z', message: 'hello' }],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getLogEvents('/aws/lambda/orders', '2026/01/02/[$LATEST]abc');

    expect(result.events).toHaveLength(1);
    expect(result.events[0].message).toBe('hello');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudwatch-logs/events?logGroupName=%2Faws%2Flambda%2Forders&logStreamName=2026%2F01%2F02%2F%5B%24LATEST%5Dabc',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(
      getLogEvents('/aws/lambda/orders', '2026/01/02/[$LATEST]abc'),
    ).rejects.toThrow('Log events request failed with status 500');
  });
});

describe('filterLogEvents', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed events with filter pattern and start time', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        events: [{ timestamp: '2026-01-02T03:04:05Z', message: 'boom' }],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await filterLogEvents('/aws/lambda/orders', 'ERROR', 1700000000000);

    expect(result.events).toHaveLength(1);
    expect(result.events[0].message).toBe('boom');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudwatch-logs/filter?logGroupName=%2Faws%2Flambda%2Forders&filterPattern=ERROR&startTime=1700000000000',
      { signal: undefined },
    );
  });

  it('omits optional parameters when not supplied', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ events: [] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await filterLogEvents('/aws/lambda/orders');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudwatch-logs/filter?logGroupName=%2Faws%2Flambda%2Forders',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(filterLogEvents('/aws/lambda/orders', 'ERROR')).rejects.toThrow(
      'Log filter request failed with status 500',
    );
  });
});

describe('getDynamoDbTables', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed tables when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ tables: [{ name: 'orders' }] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getDynamoDbTables();

    expect(result.tables).toHaveLength(1);
    expect(result.tables[0].name).toBe('orders');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/dynamodb/tables', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getDynamoDbTables()).rejects.toThrow(
      'DynamoDB tables request failed with status 503',
    );
  });
});

describe('getDynamoDbTable', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed table detail when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ name: 'orders' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getDynamoDbTable('orders');

    expect(result.name).toBe('orders');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/dynamodb/tables/orders', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getDynamoDbTable('orders')).rejects.toThrow(
      'DynamoDB table request failed with status 404',
    );
  });
});

describe('createDynamoDbTable', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request = {
    tableName: 'orders',
    partitionKeyName: 'pk',
    partitionKeyType: 'S',
    sortKeyName: null,
    sortKeyType: null,
    billingMode: 'PAY_PER_REQUEST',
    readCapacityUnits: null,
    writeCapacityUnits: null,
  };

  it('posts the table specification when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await createDynamoDbTable(request, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/dynamodb/tables', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createDynamoDbTable(request)).rejects.toThrow(
      'DynamoDB table create request failed with status 400',
    );
  });
});

describe('deleteDynamoDbTable', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a delete request when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await deleteDynamoDbTable('orders', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/dynamodb/tables/orders', {
      method: 'DELETE',
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 409 }));

    await expect(deleteDynamoDbTable('orders')).rejects.toThrow(
      'DynamoDB table delete request failed with status 409',
    );
  });
});

describe('scanDynamoDbItems', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('requests a bounded page of items when the request succeeds', async () => {
    const payload = { items: [{ json: '{"pk":"a"}' }], truncated: true };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await scanDynamoDbItems('orders', 10, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/dynamodb/tables/orders/items?limit=10',
      { signal: controller.signal },
    );
    expect(result).toEqual(payload);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(scanDynamoDbItems('orders', 25)).rejects.toThrow(
      'DynamoDB item scan request failed with status 503',
    );
  });
});

describe('getDynamoDbItem', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const key = '{"pk":{"S":"a"}}';

  it('requests a single item by key when the request succeeds', async () => {
    const payload = { json: '{"pk":"a"}' };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getDynamoDbItem('orders', key, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      `/api/services/dynamodb/tables/orders/item?key=${encodeURIComponent(key)}`,
      { signal: controller.signal },
    );
    expect(result).toEqual(payload);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getDynamoDbItem('orders', key)).rejects.toThrow(
      'DynamoDB item request failed with status 404',
    );
  });
});

describe('putDynamoDbItem', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const itemJson = '{"pk":{"S":"a"}}';

  it('posts the item json when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await putDynamoDbItem('orders', itemJson, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/dynamodb/tables/orders/items', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ item: itemJson }),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(putDynamoDbItem('orders', itemJson)).rejects.toThrow(
      'DynamoDB item put request failed with status 400',
    );
  });
});

describe('deleteDynamoDbItem', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const key = '{"pk":{"S":"a"}}';

  it('sends a delete request by key when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await deleteDynamoDbItem('orders', key, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      `/api/services/dynamodb/tables/orders/item?key=${encodeURIComponent(key)}`,
      {
        method: 'DELETE',
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 409 }));

    await expect(deleteDynamoDbItem('orders', key)).rejects.toThrow(
      'DynamoDB item delete request failed with status 409',
    );
  });
});

describe('queryDynamoDbTable', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request: DynamoDbQueryRequest = {
    indexName: null,
    scan: false,
    partitionKey: { attributeName: 'pk', operator: '=', valueType: 'S', value: 'a', secondValue: null },
    sortKey: null,
    filters: [],
    limit: 25,
    startToken: null,
  };

  it('posts the query request and returns the page when the request succeeds', async () => {
    const payload = { items: [{ json: '{"pk":"a"}' }], nextToken: 'next' };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await queryDynamoDbTable('orders', request, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/dynamodb/tables/orders/query', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: controller.signal,
    });
    expect(result).toEqual(payload);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(queryDynamoDbTable('orders', request)).rejects.toThrow(
      'DynamoDB query request failed with status 400',
    );
  });
});

describe('executeDynamoDbStatement', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request: DynamoDbStatementRequest = {
    statement: 'SELECT * FROM "orders"',
    limit: 25,
    nextToken: null,
  };

  it('posts the statement request and returns the page when the request succeeds', async () => {
    const payload = { items: [{ json: '{"pk":"a"}' }], nextToken: 'next' };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await executeDynamoDbStatement(request, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/dynamodb/statement', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: controller.signal,
    });
    expect(result).toEqual(payload);
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(executeDynamoDbStatement(request)).rejects.toThrow(
      'DynamoDB statement request failed with status 400',
    );
  });
});

describe('getSecrets', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed secrets when the request succeeds', async () => {
    const payload = {
      secrets: [
        {
          name: 'db-password',
          arn: 'arn:db-password',
          description: 'primary db',
          createdDate: null,
          lastChangedDate: null,
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getSecrets();

    expect(result.secrets).toHaveLength(1);
    expect(result.secrets[0].name).toBe('db-password');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/secrets-manager/secrets', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getSecrets()).rejects.toThrow(
      'Secrets Manager secrets request failed with status 503',
    );
  });
});

describe('createSecret', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request: SecretCreateRequest = {
    name: 'db-password',
    description: 'primary db',
    secretString: 's3cr3t',
  };

  it('posts the secret specification when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await createSecret(request, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/secrets-manager/secrets', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createSecret(request)).rejects.toThrow(
      'Secrets Manager secret create request failed with status 400',
    );
  });
});

describe('deleteSecret', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a delete request when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await deleteSecret('db-password', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/secrets-manager/secrets/db-password', {
      method: 'DELETE',
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 409 }));

    await expect(deleteSecret('db-password')).rejects.toThrow(
      'Secrets Manager secret delete request failed with status 409',
    );
  });
});

describe('getSecretValue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed secret value when the request succeeds', async () => {
    const payload = {
      name: 'db-password',
      arn: 'arn:db-password',
      versionId: 'v1',
      value: '********',
      revealAllowed: true,
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getSecretValue('db-password', true, controller.signal);

    expect(result.name).toBe('db-password');
    expect(result.value).toBe('********');
    expect(result.revealAllowed).toBe(true);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/secrets-manager/secrets/db-password/value?reveal=true',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getSecretValue('db-password', false)).rejects.toThrow(
      'Secrets Manager secret value request failed with status 404',
    );
  });
});

describe('putSecretValue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request: SecretValueUpdateRequest = {
    secretString: 'new-value',
  };

  it('puts the secret value when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await putSecretValue('db-password', request, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/secrets-manager/secrets/db-password/value',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request),
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(putSecretValue('db-password', request)).rejects.toThrow(
      'Secrets Manager secret value update request failed with status 400',
    );
  });
});

describe('getSecretVersions', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed version list when the request succeeds', async () => {
    const payload = {
      name: 'db-password',
      arn: 'arn:db-password',
      versions: [
        { versionId: 'v2', stages: ['AWSCURRENT'], createdDate: null, lastAccessedDate: null },
        { versionId: 'v1', stages: ['AWSPREVIOUS'], createdDate: null, lastAccessedDate: null },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getSecretVersions('db-password', controller.signal);

    expect(result.name).toBe('db-password');
    expect(result.versions).toHaveLength(2);
    expect(result.versions[0].stages).toEqual(['AWSCURRENT']);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/secrets-manager/secrets/db-password/versions',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getSecretVersions('db-password')).rejects.toThrow(
      'Secrets Manager secret versions request failed with status 404',
    );
  });
});

describe('getParameters', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed parameters when the request succeeds', async () => {
    const payload = {
      path: '/',
      parameters: [
        {
          name: '/app/config/db-host',
          type: 'String',
          version: 3,
          lastModifiedDate: null,
          arn: 'arn:db-host',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getParameters('/app/config', true, controller.signal);

    expect(result.parameters).toHaveLength(1);
    expect(result.parameters[0].name).toBe('/app/config/db-host');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/ssm-parameter-store/parameters?path=%2Fapp%2Fconfig&recursive=true',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getParameters('/', false)).rejects.toThrow(
      'SSM parameters request failed with status 503',
    );
  });
});

describe('createParameter', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request: ParameterCreateRequest = {
    name: '/app/config/db-host',
    type: 'String',
    value: 'localhost',
    description: 'primary config',
  };

  it('posts the parameter specification when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await createParameter(request, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/ssm-parameter-store/parameters', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createParameter(request)).rejects.toThrow(
      'SSM parameter create request failed with status 400',
    );
  });
});

describe('deleteParameter', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a delete request when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await deleteParameter('/app/config/db-host', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/ssm-parameter-store/parameters?name=%2Fapp%2Fconfig%2Fdb-host',
      {
        method: 'DELETE',
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 409 }));

    await expect(deleteParameter('/app/config/db-host')).rejects.toThrow(
      'SSM parameter delete request failed with status 409',
    );
  });
});

describe('getParameterValue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const payload = {
    name: '/app/db/password',
    type: 'SecureString',
    version: 3,
    value: '********',
    isSensitive: true,
    revealAllowed: false,
  };

  it('requests the masked value without a reveal flag by default', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getParameterValue('/app/db/password', false, controller.signal);

    expect(result.name).toBe('/app/db/password');
    expect(result.value).toBe('********');
    expect(result.isSensitive).toBe(true);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/ssm-parameter-store/parameters/value?name=%2Fapp%2Fdb%2Fpassword',
      { signal: controller.signal },
    );
  });

  it('appends the reveal flag when reveal is requested', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);

    await getParameterValue('/app/db/password', true);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/ssm-parameter-store/parameters/value?name=%2Fapp%2Fdb%2Fpassword&reveal=true',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getParameterValue('/app/db/password')).rejects.toThrow(
      'SSM parameter value request failed with status 404',
    );
  });
});

describe('updateParameterValue', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the parameter value when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await updateParameterValue('/app/db/password', 'new-value', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/ssm-parameter-store/parameters/value?name=%2Fapp%2Fdb%2Fpassword',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ value: 'new-value' }),
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(updateParameterValue('/app/db/password', 'new-value')).rejects.toThrow(
      'SSM parameter value update request failed with status 400',
    );
  });
});

describe('getParameterHistory', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const payload = {
    name: '/app/db/password',
    revealAllowed: false,
    entries: [
      {
        type: 'SecureString',
        version: 3,
        value: '********',
        lastModifiedDate: '2024-05-06T07:08:09Z',
        lastModifiedUser: 'arn:user/admin',
        isSensitive: true,
      },
    ],
  };

  it('requests the masked history without a reveal flag by default', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getParameterHistory('/app/db/password', false, controller.signal);

    expect(result.name).toBe('/app/db/password');
    expect(result.entries).toHaveLength(1);
    expect(result.entries[0].version).toBe(3);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/ssm-parameter-store/parameters/history?name=%2Fapp%2Fdb%2Fpassword',
      { signal: controller.signal },
    );
  });

  it('appends the reveal flag when reveal is requested', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);

    await getParameterHistory('/app/db/password', true);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/ssm-parameter-store/parameters/history?name=%2Fapp%2Fdb%2Fpassword&reveal=true',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getParameterHistory('/app/db/password')).rejects.toThrow(
      'SSM parameter history request failed with status 404',
    );
  });
});

describe('getSnsTopics', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed topics when the request succeeds', async () => {
    const payload = {
      topics: [
        {
          name: 'orders-topic',
          topicArn: 'arn:aws:sns:eu-west-1:000000000000:orders-topic',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getSnsTopics(controller.signal);

    expect(result.topics).toHaveLength(1);
    expect(result.topics[0].name).toBe('orders-topic');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/sns/topics', {
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getSnsTopics()).rejects.toThrow('SNS topics request failed with status 503');
  });
});

describe('createSnsTopic', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request: SnsTopicCreateRequest = {
    name: 'orders-topic',
  };

  it('posts the topic specification when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await createSnsTopic(request, controller.signal);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/sns/topics', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createSnsTopic(request)).rejects.toThrow(
      'SNS topic create request failed with status 400',
    );
  });
});

describe('deleteSnsTopic', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends a delete request when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await deleteSnsTopic('arn:aws:sns:eu-west-1:000000000000:orders-topic', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sns/topics?arn=arn%3Aaws%3Asns%3Aeu-west-1%3A000000000000%3Aorders-topic',
      {
        method: 'DELETE',
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 409 }));

    await expect(
      deleteSnsTopic('arn:aws:sns:eu-west-1:000000000000:orders-topic'),
    ).rejects.toThrow('SNS topic delete request failed with status 409');
  });
});

describe('getSnsSubscriptions', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed subscriptions when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        subscriptions: [
          {
            subscriptionArn: 'arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f',
            protocol: 'sqs',
            endpoint: 'arn:aws:sqs:eu-west-1:000000000000:orders',
            owner: '000000000000',
          },
        ],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    const result = await getSnsSubscriptions(
      'arn:aws:sns:eu-west-1:000000000000:orders-topic',
      controller.signal,
    );

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sns/subscriptions?arn=arn%3Aaws%3Asns%3Aeu-west-1%3A000000000000%3Aorders-topic',
      { signal: controller.signal },
    );
    expect(result.subscriptions).toHaveLength(1);
    expect(result.subscriptions[0]?.protocol).toBe('sqs');
    expect(result.subscriptions[0]?.endpoint).toBe('arn:aws:sqs:eu-west-1:000000000000:orders');
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      getSnsSubscriptions('arn:aws:sns:eu-west-1:000000000000:orders-topic'),
    ).rejects.toThrow('SNS subscriptions request failed with status 503');
  });
});
