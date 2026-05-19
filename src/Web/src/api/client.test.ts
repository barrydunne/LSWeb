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
