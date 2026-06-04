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
  publishSnsMessage,
  getSnsSubscriptionFilterPolicy,
  setSnsSubscriptionFilterPolicy,
  getStateMachines,
  getStateMachine,
  getExecutions,
  startExecution,
  getExecutionHistory,
  getStacks,
  getStack,
  getStackTemplate,
  getStackResources,
  getStackEvents,
  createStack,
  updateStack,
  deleteStack,
  getChangeSets,
  getChangeSet,
  createChangeSet,
  executeChangeSet,
  deleteChangeSet,
  getEventBridgeRules,
  getEventBridgeTargets,
  putEventBridgeEvent,
  getAcmCertificates,
  getApiGatewayRestApis,
  getRoute53HostedZones,
  getSesIdentities,
  getIamUsers,
  getIamUser,
  createIamUser,
  deleteIamUser,
  addIamUserToGroup,
  removeIamUserFromGroup,
  attachIamUserPolicy,
  detachIamUserPolicy,
  putIamUserInlinePolicy,
  deleteIamUserInlinePolicy,
  createIamAccessKey,
  updateIamAccessKeyStatus,
  deleteIamAccessKey,
  tagIamUser,
  untagIamUser,
  putIamUserPermissionsBoundary,
  deleteIamUserPermissionsBoundary,
  getIamGroups,
  getIamGroup,
  createIamGroup,
  deleteIamGroup,
  addIamGroupMember,
  removeIamGroupMember,
  attachIamGroupPolicy,
  detachIamGroupPolicy,
  putIamGroupInlinePolicy,
  deleteIamGroupInlinePolicy,
  getIamRoles,
  getIamRole,
  getIamRoleUsedBy,
  createIamRole,
  updateIamRole,
  deleteIamRole,
  attachIamRolePolicy,
  detachIamRolePolicy,
  putIamRoleInlinePolicy,
  deleteIamRoleInlinePolicy,
  tagIamRole,
  untagIamRole,
  putIamRolePermissionsBoundary,
  deleteIamRolePermissionsBoundary,
  getIamPolicies,
  getIamPolicy,
  createIamPolicy,
  createIamPolicyVersion,
  setIamPolicyDefaultVersion,
  deleteIamPolicyVersion,
  deleteIamPolicy,
  tagIamPolicy,
  untagIamPolicy,
  getIamAccountSummary,
  getIamAccountPasswordPolicy,
  updateIamAccountPasswordPolicy,
  deleteIamAccountPasswordPolicy,
  getIamAccountAliases,
  createIamAccountAlias,
  deleteIamAccountAlias,
  IamNotSupportedError,
  detectStackDrift,
  getDriftStatus,
  getResourceDrifts,
  getExports,
  getImports,
} from './client';
import type {
  DynamoDbQueryRequest,
  DynamoDbStatementRequest,
  SecretCreateRequest,
  SecretValueUpdateRequest,
  ParameterCreateRequest,
  SnsTopicCreateRequest,
  StartExecutionRequest,
  PutEventBridgeEventRequest,
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

describe('publishSnsMessage', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the topic arn, subject, message, and attributes to the messages endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    await publishSnsMessage(
      'arn:aws:sns:eu-west-1:000000000000:orders-topic',
      {
        subject: 'Heads up',
        message: 'hello world',
        messageAttributes: { source: 'web' },
      },
      controller.signal,
    );

    expect(fetchMock).toHaveBeenCalledWith('/api/services/sns/topics/messages', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        topicArn: 'arn:aws:sns:eu-west-1:000000000000:orders-topic',
        subject: 'Heads up',
        message: 'hello world',
        messageAttributes: { source: 'web' },
      }),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(
      publishSnsMessage('arn:aws:sns:eu-west-1:000000000000:orders-topic', { message: 'hi' }),
    ).rejects.toThrow('SNS publish request failed with status 400');
  });
});

describe('getSnsSubscriptionFilterPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed filter policy when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ filterPolicy: '{"store":["example_corp"]}' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    const result = await getSnsSubscriptionFilterPolicy(
      'arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f',
      controller.signal,
    );

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/sns/subscriptions/filter-policy?arn=arn%3Aaws%3Asns%3Aeu-west-1%3A000000000000%3Aorders-topic%3A8c1f',
      { signal: controller.signal },
    );
    expect(result.filterPolicy).toBe('{"store":["example_corp"]}');
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      getSnsSubscriptionFilterPolicy('arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f'),
    ).rejects.toThrow('SNS filter policy request failed with status 503');
  });
});

describe('setSnsSubscriptionFilterPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the subscription arn and filter policy to the filter-policy endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const controller = new AbortController();
    await setSnsSubscriptionFilterPolicy(
      'arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f',
      '{"store":["example_corp"]}',
      controller.signal,
    );

    expect(fetchMock).toHaveBeenCalledWith('/api/services/sns/subscriptions/filter-policy', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        subscriptionArn: 'arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f',
        filterPolicy: '{"store":["example_corp"]}',
      }),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(
      setSnsSubscriptionFilterPolicy(
        'arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f',
        '{}',
      ),
    ).rejects.toThrow('SNS filter policy update request failed with status 400');
  });
});

describe('getStateMachines', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed state machines when the request succeeds', async () => {
    const payload = {
      stateMachines: [
        {
          name: 'orders-workflow',
          stateMachineArn:
            'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
          type: 'STANDARD',
          creationDate: '2024-01-01T00:00:00+00:00',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getStateMachines(controller.signal);

    expect(result.stateMachines).toHaveLength(1);
    expect(result.stateMachines[0].name).toBe('orders-workflow');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/step-functions/state-machines', {
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getStateMachines()).rejects.toThrow(
      'Step Functions state machines request failed with status 503',
    );
  });
});

describe('getStateMachine', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed state machine when the request succeeds', async () => {
    const payload = {
      name: 'orders-workflow',
      stateMachineArn: 'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
      type: 'STANDARD',
      status: 'ACTIVE',
      roleArn: 'arn:aws:iam::000000000000:role/service-role/states',
      definition: '{"StartAt":"Done"}',
      creationDate: '2024-01-01T00:00:00+00:00',
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getStateMachine(
      'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
      controller.signal,
    );

    expect(result.name).toBe('orders-workflow');
    expect(result.status).toBe('ACTIVE');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/step-functions/state-machine?arn=arn%3Aaws%3Astates%3Aeu-west-1%3A000000000000%3AstateMachine%3Aorders-workflow',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(
      getStateMachine('arn:aws:states:eu-west-1:000000000000:stateMachine:missing'),
    ).rejects.toThrow('Step Functions state machine request failed with status 404');
  });
});

describe('getExecutions', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed executions when the request succeeds', async () => {
    const payload = {
      executions: [
        {
          executionArn:
            'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1',
          name: 'run-1',
          stateMachineArn:
            'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
          status: 'SUCCEEDED',
          startDate: '2024-01-01T00:00:00+00:00',
          stopDate: '2024-01-01T00:01:00+00:00',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getExecutions(
      'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
      controller.signal,
    );

    expect(result.executions).toHaveLength(1);
    expect(result.executions[0].name).toBe('run-1');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/step-functions/executions?arn=arn%3Aaws%3Astates%3Aeu-west-1%3A000000000000%3AstateMachine%3Aorders-workflow',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      getExecutions('arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow'),
    ).rejects.toThrow('Step Functions executions request failed with status 503');
  });
});

describe('startExecution', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request: StartExecutionRequest = {
    stateMachineArn: 'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
    name: 'run-1',
    input: '{"key":"value"}',
  };

  it('posts the request and returns the parsed result when it succeeds', async () => {
    const payload = {
      executionArn:
        'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1',
      startDate: '2024-01-01T00:00:00+00:00',
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await startExecution(request, controller.signal);

    expect(result.executionArn).toBe(payload.executionArn);
    expect(fetchMock).toHaveBeenCalledWith('/api/services/step-functions/executions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(startExecution(request)).rejects.toThrow(
      'Step Functions start execution request failed with status 400',
    );
  });
});

describe('getExecutionHistory', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed events when the request succeeds', async () => {
    const payload = {
      events: [
        {
          id: 1,
          previousEventId: null,
          type: 'ExecutionStarted',
          timestamp: '2024-01-01T00:00:00+00:00',
          name: null,
          input: '{"key":"value"}',
          output: null,
          error: null,
          cause: null,
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getExecutionHistory(
      'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1',
      controller.signal,
    );

    expect(result.events).toHaveLength(1);
    expect(result.events[0].type).toBe('ExecutionStarted');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/step-functions/execution-history?arn=arn%3Aaws%3Astates%3Aeu-west-1%3A000000000000%3Aexecution%3Aorders-workflow%3Arun-1',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      getExecutionHistory('arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1'),
    ).rejects.toThrow('Step Functions execution history request failed with status 503');
  });
});

describe('getStacks', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed stacks when the request succeeds', async () => {
    const payload = {
      stacks: [
        {
          stackName: 'orders-stack',
          stackId: 'arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc',
          stackStatus: 'CREATE_COMPLETE',
          description: 'Orders processing stack',
          creationTime: '2024-01-01T00:00:00+00:00',
          lastUpdatedTime: '2024-02-01T00:00:00+00:00',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getStacks(controller.signal);

    expect(result.stacks).toHaveLength(1);
    expect(result.stacks[0].stackName).toBe('orders-stack');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/cloudformation/stacks', {
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getStacks()).rejects.toThrow(
      'CloudFormation stacks request failed with status 503',
    );
  });
});

describe('getStack', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed stack when the request succeeds', async () => {
    const payload = {
      stackName: 'orders-stack',
      stackId: 'arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc',
      stackStatus: 'CREATE_COMPLETE',
      stackStatusReason: null,
      description: 'Orders processing stack',
      creationTime: '2024-01-01T00:00:00+00:00',
      lastUpdatedTime: null,
      parameters: [],
      outputs: [],
      tags: [],
      capabilities: [],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getStack('orders-stack', controller.signal);

    expect(result.stackName).toBe('orders-stack');
    expect(result.stackStatus).toBe('CREATE_COMPLETE');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack?name=orders-stack',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getStack('missing-stack')).rejects.toThrow(
      'CloudFormation stack request failed with status 404',
    );
  });
});

describe('getStackTemplate', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed template when the request succeeds', async () => {
    const payload = {
      templateBody: '{"Resources":{}}',
      format: 'json',
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getStackTemplate('orders-stack', controller.signal);

    expect(result.templateBody).toBe('{"Resources":{}}');
    expect(result.format).toBe('json');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack/template?name=orders-stack',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getStackTemplate('missing-stack')).rejects.toThrow(
      'CloudFormation stack template request failed with status 500',
    );
  });
});

describe('createStack', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the stack definition and returns the stack id', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue({ ok: true, json: async () => ({ stackId: 'arn:stack/new' }) });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await createStack(
      'new-stack',
      '{"Resources":{}}',
      [{ parameterKey: 'Env', parameterValue: 'dev' }],
      ['CAPABILITY_IAM'],
      controller.signal,
    );

    expect(result.stackId).toBe('arn:stack/new');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/cloudformation/stack', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        stackName: 'new-stack',
        templateBody: '{"Resources":{}}',
        parameters: [{ parameterKey: 'Env', parameterValue: 'dev' }],
        capabilities: ['CAPABILITY_IAM'],
      }),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(createStack('new-stack', '{}', [], [])).rejects.toThrow(
      'CloudFormation stack create request failed with status 400',
    );
  });
});

describe('updateStack', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the stack definition with the name in the query and returns the stack id', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue({ ok: true, json: async () => ({ stackId: 'arn:stack/updated' }) });
    vi.stubGlobal('fetch', fetchMock);

    const result = await updateStack(
      'orders stack',
      '{"Resources":{}}',
      [{ parameterKey: 'Env', parameterValue: 'prod' }],
      ['CAPABILITY_NAMED_IAM'],
    );

    expect(result.stackId).toBe('arn:stack/updated');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack?name=orders%20stack',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          templateBody: '{"Resources":{}}',
          parameters: [{ parameterKey: 'Env', parameterValue: 'prod' }],
          capabilities: ['CAPABILITY_NAMED_IAM'],
        }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 409 }));

    await expect(updateStack('orders-stack', '{}', [], [])).rejects.toThrow(
      'CloudFormation stack update request failed with status 409',
    );
  });
});

describe('deleteStack', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('deletes the stack by name', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await deleteStack('orders stack', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack?name=orders%20stack',
      {
        method: 'DELETE',
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteStack('missing-stack')).rejects.toThrow(
      'CloudFormation stack delete request failed with status 404',
    );
  });
});

describe('getStackResources', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed resources when the request succeeds', async () => {
    const payload = {
      resources: [
        {
          logicalResourceId: 'OrdersQueue',
          physicalResourceId: 'orders-queue',
          resourceType: 'AWS::SQS::Queue',
          resourceStatus: 'CREATE_COMPLETE',
          resourceStatusReason: null,
          lastUpdatedTime: '2024-01-01T00:00:00Z',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getStackResources('orders-stack', controller.signal);

    expect(result.resources).toHaveLength(1);
    expect(result.resources[0].logicalResourceId).toBe('OrdersQueue');
    expect(result.resources[0].resourceType).toBe('AWS::SQS::Queue');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack/resources?name=orders-stack',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getStackResources('missing-stack')).rejects.toThrow(
      'CloudFormation stack resources request failed with status 500',
    );
  });
});

describe('getStackEvents', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed events when the request succeeds', async () => {
    const payload = {
      events: [
        {
          eventId: 'event-1',
          timestamp: '2024-01-01T00:00:00Z',
          logicalResourceId: 'OrdersQueue',
          physicalResourceId: 'orders-queue',
          resourceType: 'AWS::SQS::Queue',
          resourceStatus: 'CREATE_COMPLETE',
          resourceStatusReason: 'Resource creation initiated',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getStackEvents('orders-stack', controller.signal);

    expect(result.events).toHaveLength(1);
    expect(result.events[0].eventId).toBe('event-1');
    expect(result.events[0].resourceStatus).toBe('CREATE_COMPLETE');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack/events?name=orders-stack',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getStackEvents('missing-stack')).rejects.toThrow(
      'CloudFormation stack events request failed with status 500',
    );
  });
});

describe('getChangeSets', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed change sets when the request succeeds', async () => {
    const payload = {
      changeSets: [
        {
          changeSetId: 'arn:changeset/add-queue',
          changeSetName: 'add-queue',
          stackName: 'orders-stack',
          status: 'CREATE_COMPLETE',
          statusReason: null,
          executionStatus: 'AVAILABLE',
          description: 'Adds a queue',
          creationTime: '2024-01-01T00:00:00Z',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getChangeSets('orders stack', controller.signal);

    expect(result.changeSets).toHaveLength(1);
    expect(result.changeSets[0].changeSetName).toBe('add-queue');
    expect(result.changeSets[0].executionStatus).toBe('AVAILABLE');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/change-sets?name=orders%20stack',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getChangeSets('missing-stack')).rejects.toThrow(
      'CloudFormation change sets request failed with status 500',
    );
  });
});

describe('getChangeSet', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed change set detail when the request succeeds', async () => {
    const payload = {
      changeSetName: 'add-queue',
      changeSetId: 'arn:changeset/add-queue',
      stackName: 'orders-stack',
      stackId: 'arn:stack/orders-stack',
      status: 'CREATE_COMPLETE',
      statusReason: null,
      executionStatus: 'AVAILABLE',
      description: 'Adds a queue',
      creationTime: '2024-01-01T00:00:00Z',
      parameters: [{ parameterKey: 'Env', parameterValue: 'dev' }],
      capabilities: ['CAPABILITY_IAM'],
      changes: [
        {
          action: 'Add',
          logicalResourceId: 'OrdersQueue',
          physicalResourceId: null,
          resourceType: 'AWS::SQS::Queue',
          replacement: null,
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getChangeSet('orders stack', 'add queue', controller.signal);

    expect(result.changeSetName).toBe('add-queue');
    expect(result.changes).toHaveLength(1);
    expect(result.changes[0].action).toBe('Add');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/change-set?name=orders%20stack&changeSet=add%20queue',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getChangeSet('missing-stack', 'missing')).rejects.toThrow(
      'CloudFormation change set request failed with status 404',
    );
  });
});

describe('createChangeSet', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the change set definition and returns the change set id', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue({ ok: true, json: async () => ({ changeSetId: 'arn:changeset/new' }) });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await createChangeSet(
      'orders-stack',
      'add-queue',
      'UPDATE',
      '{"Resources":{}}',
      [{ parameterKey: 'Env', parameterValue: 'dev' }],
      ['CAPABILITY_IAM'],
      controller.signal,
    );

    expect(result.changeSetId).toBe('arn:changeset/new');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/cloudformation/change-set', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        stackName: 'orders-stack',
        changeSetName: 'add-queue',
        changeSetType: 'UPDATE',
        templateBody: '{"Resources":{}}',
        parameters: [{ parameterKey: 'Env', parameterValue: 'dev' }],
        capabilities: ['CAPABILITY_IAM'],
      }),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(
      createChangeSet('orders-stack', 'add-queue', 'UPDATE', '{}', [], []),
    ).rejects.toThrow('CloudFormation change set create request failed with status 400');
  });
});

describe('executeChangeSet', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the execute request with the name and change set in the query', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await executeChangeSet('orders stack', 'add queue', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/change-set/execute?name=orders%20stack&changeSet=add%20queue',
      {
        method: 'POST',
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 409 }));

    await expect(executeChangeSet('orders-stack', 'add-queue')).rejects.toThrow(
      'CloudFormation change set execute request failed with status 409',
    );
  });
});

describe('deleteChangeSet', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('deletes the change set by name and change set', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    await deleteChangeSet('orders stack', 'add queue', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/change-set?name=orders%20stack&changeSet=add%20queue',
      {
        method: 'DELETE',
        signal: controller.signal,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(deleteChangeSet('missing-stack', 'missing')).rejects.toThrow(
      'CloudFormation change set delete request failed with status 404',
    );
  });
});

describe('getEventBridgeRules', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed rules when the request succeeds', async () => {
    const payload = {
      rules: [
        {
          name: 'orders-rule',
          arn: 'arn:aws:events:eu-west-1:000000000000:rule/orders-rule',
          eventBusName: 'default',
          state: 'ENABLED',
          description: 'Routes order events',
          scheduleExpression: 'rate(5 minutes)',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getEventBridgeRules(controller.signal);

    expect(result.rules).toHaveLength(1);
    expect(result.rules[0].name).toBe('orders-rule');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/eventbridge/rules', {
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getEventBridgeRules()).rejects.toThrow(
      'EventBridge rules request failed with status 503',
    );
  });
});

describe('getEventBridgeTargets', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed targets and forwards the rule name', async () => {
    const payload = {
      targets: [
        {
          id: 'target-1',
          arn: 'arn:aws:lambda:eu-west-1:000000000000:function:orders-handler',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getEventBridgeTargets('orders rule', controller.signal);

    expect(result.targets).toHaveLength(1);
    expect(result.targets[0].id).toBe('target-1');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/eventbridge/targets?rule=orders%20rule',
      { signal: controller.signal },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getEventBridgeTargets('missing')).rejects.toThrow(
      'EventBridge targets request failed with status 404',
    );
  });
});

describe('putEventBridgeEvent', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const request: PutEventBridgeEventRequest = {
    source: 'orders.service',
    detailType: 'OrderPlaced',
    detail: '{"orderId":"123"}',
    eventBusName: 'default',
  };

  it('posts the request and returns the parsed result when it succeeds', async () => {
    const payload = {
      accepted: true,
      eventId: 'event-1',
      errorCode: null,
      errorMessage: null,
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await putEventBridgeEvent(request, controller.signal);

    expect(result.accepted).toBe(true);
    expect(result.eventId).toBe('event-1');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/eventbridge/events', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 400 }));

    await expect(putEventBridgeEvent(request)).rejects.toThrow(
      'EventBridge put event request failed with status 400',
    );
  });
});

describe('getAcmCertificates', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed certificates when the request succeeds', async () => {
    const payload = {
      certificates: [
        {
          arn: 'arn:aws:acm:eu-west-1:000000000000:certificate/abc',
          domainName: 'example.com',
          status: 'ISSUED',
          type: 'AMAZON_ISSUED',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getAcmCertificates(controller.signal);

    expect(result.certificates).toHaveLength(1);
    expect(result.certificates[0].domainName).toBe('example.com');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/acm/certificates', {
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getAcmCertificates()).rejects.toThrow(
      'ACM certificates request failed with status 503',
    );
  });
});

describe('getApiGatewayRestApis', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed REST APIs when the request succeeds', async () => {
    const payload = {
      restApis: [
        {
          id: 'api-1',
          name: 'orders-api',
          description: 'Orders service',
          createdDate: '2024-01-01T00:00:00+00:00',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getApiGatewayRestApis(controller.signal);

    expect(result.restApis).toHaveLength(1);
    expect(result.restApis[0].name).toBe('orders-api');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/apigateway/restapis', {
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }));

    await expect(getApiGatewayRestApis()).rejects.toThrow(
      'API Gateway REST APIs request failed with status 500',
    );
  });
});

describe('getRoute53HostedZones', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed hosted zones when the request succeeds', async () => {
    const payload = {
      hostedZones: [
        {
          id: '/hostedzone/Z123',
          name: 'example.com.',
          recordCount: 4,
          privateZone: false,
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getRoute53HostedZones(controller.signal);

    expect(result.hostedZones).toHaveLength(1);
    expect(result.hostedZones[0].name).toBe('example.com.');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/route53/hostedzones', {
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 502 }));

    await expect(getRoute53HostedZones()).rejects.toThrow(
      'Route 53 hosted zones request failed with status 502',
    );
  });
});

describe('getSesIdentities', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed identities when the request succeeds', async () => {
    const payload = {
      identities: [
        {
          identity: 'sender@example.com',
          identityType: 'EmailAddress',
          verificationStatus: 'Success',
        },
      ],
    };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => payload });
    vi.stubGlobal('fetch', fetchMock);
    const controller = new AbortController();

    const result = await getSesIdentities(controller.signal);

    expect(result.identities).toHaveLength(1);
    expect(result.identities[0].identity).toBe('sender@example.com');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/ses/identities', {
      signal: controller.signal,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(getSesIdentities()).rejects.toThrow(
      'SES identities request failed with status 404',
    );
  });
});

describe('getIamUsers', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed users when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ users: [{ userName: 'alice', arn: 'arn:user/alice' }] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamUsers();

    expect(result.users).toHaveLength(1);
    expect(result.users[0].userName).toBe('alice');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamUsers()).rejects.toThrow('IAM users request failed with status 503');
  });
});

describe('getIamUser', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the user detail when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ userName: 'alice', arn: 'arn:user/alice' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamUser('alice');

    expect(result.userName).toBe('alice');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamUser('alice')).rejects.toThrow('IAM user request failed with status 503');
  });
});

describe('createIamUser', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the create request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createIamUser({ userName: 'alice', path: null });

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ userName: 'alice', path: null }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(createIamUser({ userName: 'alice', path: null })).rejects.toThrow(
      'IAM user create request failed with status 503',
    );
  });
});

describe('deleteIamUser', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamUser('alice');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamUser('alice')).rejects.toThrow(
      'IAM user delete request failed with status 503',
    );
  });
});

describe('addIamUserToGroup', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the membership request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await addIamUserToGroup('alice', 'admins');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/groups', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ groupName: 'admins' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(addIamUserToGroup('alice', 'admins')).rejects.toThrow(
      'IAM add-user-to-group request failed with status 503',
    );
  });
});

describe('removeIamUserFromGroup', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await removeIamUserFromGroup('alice', 'admins');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/groups/admins', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(removeIamUserFromGroup('alice', 'admins')).rejects.toThrow(
      'IAM remove-user-from-group request failed with status 503',
    );
  });
});

describe('attachIamUserPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the attach request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await attachIamUserPolicy('alice', 'arn:policy/ReadOnly');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/attached-policies', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyArn: 'arn:policy/ReadOnly' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(attachIamUserPolicy('alice', 'arn:policy/ReadOnly')).rejects.toThrow(
      'IAM attach-user-policy request failed with status 503',
    );
  });
});

describe('detachIamUserPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the detach request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await detachIamUserPolicy('alice', 'arn:policy/ReadOnly');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/users/alice/attached-policies?policyArn=arn%3Apolicy%2FReadOnly',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(detachIamUserPolicy('alice', 'arn:policy/ReadOnly')).rejects.toThrow(
      'IAM detach-user-policy request failed with status 503',
    );
  });
});

describe('putIamUserInlinePolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the inline policy when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await putIamUserInlinePolicy('alice', 'deny-all', '{"Statement":[]}');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/users/alice/inline-policies/deny-all',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ policyDocument: '{"Statement":[]}' }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(putIamUserInlinePolicy('alice', 'deny-all', '{}')).rejects.toThrow(
      'IAM put-user-inline-policy request failed with status 503',
    );
  });
});

describe('deleteIamUserInlinePolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamUserInlinePolicy('alice', 'deny-all');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/users/alice/inline-policies/deny-all',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamUserInlinePolicy('alice', 'deny-all')).rejects.toThrow(
      'IAM delete-user-inline-policy request failed with status 503',
    );
  });
});

describe('createIamAccessKey', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the created access key when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        accessKeyId: 'AKIA1',
        secretAccessKey: 'secret',
        status: 'Active',
        createDate: null,
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await createIamAccessKey('alice');

    expect(result.accessKeyId).toBe('AKIA1');
    expect(result.secretAccessKey).toBe('secret');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/access-keys', {
      method: 'POST',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(createIamAccessKey('alice')).rejects.toThrow(
      'IAM create-access-key request failed with status 503',
    );
  });
});

describe('updateIamAccessKeyStatus', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the status update when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await updateIamAccessKeyStatus('alice', 'AKIA1', 'Inactive');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/users/alice/access-keys/AKIA1/status',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: 'Inactive' }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(updateIamAccessKeyStatus('alice', 'AKIA1', 'Inactive')).rejects.toThrow(
      'IAM update-access-key-status request failed with status 503',
    );
  });
});

describe('deleteIamAccessKey', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamAccessKey('alice', 'AKIA1');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/access-keys/AKIA1', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamAccessKey('alice', 'AKIA1')).rejects.toThrow(
      'IAM delete-access-key request failed with status 503',
    );
  });
});

describe('tagIamUser', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the tags when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await tagIamUser('alice', [{ key: 'team', value: 'platform' }]);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/tags', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ tags: [{ key: 'team', value: 'platform' }] }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(tagIamUser('alice', [{ key: 'team', value: 'platform' }])).rejects.toThrow(
      'IAM user tag request failed with status 503',
    );
  });
});

describe('untagIamUser', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the untag request with repeated key params when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await untagIamUser('alice', ['team', 'env']);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/tags?key=team&key=env', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(untagIamUser('alice', ['team'])).rejects.toThrow(
      'IAM user untag request failed with status 503',
    );
  });
});

describe('putIamUserPermissionsBoundary', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the permissions boundary when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await putIamUserPermissionsBoundary('alice', 'arn:policy/Boundary');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/permissions-boundary', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ permissionsBoundaryArn: 'arn:policy/Boundary' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(putIamUserPermissionsBoundary('alice', 'arn:policy/Boundary')).rejects.toThrow(
      'IAM user permissions-boundary request failed with status 503',
    );
  });
});

describe('deleteIamUserPermissionsBoundary', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamUserPermissionsBoundary('alice');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/users/alice/permissions-boundary', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamUserPermissionsBoundary('alice')).rejects.toThrow(
      'IAM user permissions-boundary delete request failed with status 503',
    );
  });
});

describe('getIamGroups', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed groups when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ groups: [{ groupName: 'admins', arn: 'arn:group/admins' }] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamGroups();

    expect(result.groups).toHaveLength(1);
    expect(result.groups[0].groupName).toBe('admins');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/groups', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamGroups()).rejects.toThrow('IAM groups request failed with status 503');
  });
});

describe('getIamGroup', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the group detail when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ groupName: 'admins', arn: 'arn:group/admins' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamGroup('admins');

    expect(result.groupName).toBe('admins');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/groups/admins', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamGroup('admins')).rejects.toThrow('IAM group request failed with status 503');
  });
});

describe('createIamGroup', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the create request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createIamGroup({ groupName: 'admins', path: null });

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/groups', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ groupName: 'admins', path: null }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(createIamGroup({ groupName: 'admins', path: null })).rejects.toThrow(
      'IAM group create request failed with status 503',
    );
  });
});

describe('deleteIamGroup', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamGroup('admins');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/groups/admins', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamGroup('admins')).rejects.toThrow(
      'IAM group delete request failed with status 503',
    );
  });
});

describe('addIamGroupMember', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the member request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await addIamGroupMember('admins', 'alice');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/groups/admins/members', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ userName: 'alice' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(addIamGroupMember('admins', 'alice')).rejects.toThrow(
      'IAM add-group-member request failed with status 503',
    );
  });
});

describe('removeIamGroupMember', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await removeIamGroupMember('admins', 'alice');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/groups/admins/members/alice', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(removeIamGroupMember('admins', 'alice')).rejects.toThrow(
      'IAM remove-group-member request failed with status 503',
    );
  });
});

describe('attachIamGroupPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the attach request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await attachIamGroupPolicy('admins', 'arn:policy/ReadOnly');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/groups/admins/attached-policies', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyArn: 'arn:policy/ReadOnly' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(attachIamGroupPolicy('admins', 'arn:policy/ReadOnly')).rejects.toThrow(
      'IAM attach-group-policy request failed with status 503',
    );
  });
});

describe('detachIamGroupPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the detach request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await detachIamGroupPolicy('admins', 'arn:policy/ReadOnly');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/groups/admins/attached-policies?policyArn=arn%3Apolicy%2FReadOnly',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(detachIamGroupPolicy('admins', 'arn:policy/ReadOnly')).rejects.toThrow(
      'IAM detach-group-policy request failed with status 503',
    );
  });
});

describe('putIamGroupInlinePolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the inline policy when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await putIamGroupInlinePolicy('admins', 'deny-all', '{"Statement":[]}');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/groups/admins/inline-policies/deny-all',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ policyDocument: '{"Statement":[]}' }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(putIamGroupInlinePolicy('admins', 'deny-all', '{}')).rejects.toThrow(
      'IAM put-group-inline-policy request failed with status 503',
    );
  });
});

describe('deleteIamGroupInlinePolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamGroupInlinePolicy('admins', 'deny-all');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/groups/admins/inline-policies/deny-all',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamGroupInlinePolicy('admins', 'deny-all')).rejects.toThrow(
      'IAM delete-group-inline-policy request failed with status 503',
    );
  });
});

describe('getIamRoles', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed roles when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ roles: [{ roleName: 'lambda-exec', arn: 'arn:role/lambda-exec' }] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamRoles();

    expect(result.roles).toHaveLength(1);
    expect(result.roles[0].roleName).toBe('lambda-exec');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/roles', { signal: undefined });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamRoles()).rejects.toThrow('IAM roles request failed with status 503');
  });
});

describe('getIamRole', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the role detail when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ roleName: 'lambda-exec', arn: 'arn:role/lambda-exec' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamRole('lambda-exec');

    expect(result.roleName).toBe('lambda-exec');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/roles/lambda-exec', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamRole('lambda-exec')).rejects.toThrow('IAM role request failed with status 503');
  });
});

describe('getIamRoleUsedBy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the consumers when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ consumers: [{ serviceKey: 'lambda', resourceId: 'fn-1' }] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamRoleUsedBy('lambda-exec');

    expect(result.consumers).toHaveLength(1);
    expect(result.consumers[0].serviceKey).toBe('lambda');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/roles/lambda-exec/used-by', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamRoleUsedBy('lambda-exec')).rejects.toThrow(
      'IAM role used-by request failed with status 503',
    );
  });
});

describe('createIamRole', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the create request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const request = {
      roleName: 'lambda-exec',
      assumeRolePolicyDocument: '{"Statement":[]}',
      path: null,
      description: null,
      maxSessionDuration: null,
    };
    await createIamRole(request);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/roles', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      createIamRole({
        roleName: 'lambda-exec',
        assumeRolePolicyDocument: '{}',
        path: null,
        description: null,
        maxSessionDuration: null,
      }),
    ).rejects.toThrow('IAM role create request failed with status 503');
  });
});

describe('updateIamRole', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the update request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const request = {
      description: 'updated',
      maxSessionDuration: 3600,
      trustPolicyDocument: null,
    };
    await updateIamRole('lambda-exec', request);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/roles/lambda-exec', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      updateIamRole('lambda-exec', {
        description: null,
        maxSessionDuration: null,
        trustPolicyDocument: null,
      }),
    ).rejects.toThrow('IAM role update request failed with status 503');
  });
});

describe('deleteIamRole', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamRole('lambda-exec');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/roles/lambda-exec', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamRole('lambda-exec')).rejects.toThrow(
      'IAM role delete request failed with status 503',
    );
  });
});

describe('attachIamRolePolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the attach request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await attachIamRolePolicy('lambda-exec', 'arn:policy/ReadOnly');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/roles/lambda-exec/attached-policies', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyArn: 'arn:policy/ReadOnly' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(attachIamRolePolicy('lambda-exec', 'arn:policy/ReadOnly')).rejects.toThrow(
      'IAM attach-role-policy request failed with status 503',
    );
  });
});

describe('detachIamRolePolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the detach request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await detachIamRolePolicy('lambda-exec', 'arn:policy/ReadOnly');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/roles/lambda-exec/attached-policies?policyArn=arn%3Apolicy%2FReadOnly',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(detachIamRolePolicy('lambda-exec', 'arn:policy/ReadOnly')).rejects.toThrow(
      'IAM detach-role-policy request failed with status 503',
    );
  });
});

describe('putIamRoleInlinePolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the inline policy when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await putIamRoleInlinePolicy('lambda-exec', 'deny-all', '{"Statement":[]}');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/roles/lambda-exec/inline-policies/deny-all',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ policyDocument: '{"Statement":[]}' }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(putIamRoleInlinePolicy('lambda-exec', 'deny-all', '{}')).rejects.toThrow(
      'IAM put-role-inline-policy request failed with status 503',
    );
  });
});

describe('deleteIamRoleInlinePolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamRoleInlinePolicy('lambda-exec', 'deny-all');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/roles/lambda-exec/inline-policies/deny-all',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamRoleInlinePolicy('lambda-exec', 'deny-all')).rejects.toThrow(
      'IAM delete-role-inline-policy request failed with status 503',
    );
  });
});

describe('tagIamRole', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the tags when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await tagIamRole('lambda-exec', [{ key: 'team', value: 'platform' }]);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/roles/lambda-exec/tags', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ tags: [{ key: 'team', value: 'platform' }] }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(tagIamRole('lambda-exec', [{ key: 'team', value: 'platform' }])).rejects.toThrow(
      'IAM role tag request failed with status 503',
    );
  });
});

describe('untagIamRole', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the untag request with repeated key params when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await untagIamRole('lambda-exec', ['team', 'env']);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/roles/lambda-exec/tags?key=team&key=env',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(untagIamRole('lambda-exec', ['team'])).rejects.toThrow(
      'IAM role untag request failed with status 503',
    );
  });
});

describe('putIamRolePermissionsBoundary', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the permissions boundary when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await putIamRolePermissionsBoundary('lambda-exec', 'arn:policy/Boundary');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/roles/lambda-exec/permissions-boundary',
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ permissionsBoundaryArn: 'arn:policy/Boundary' }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      putIamRolePermissionsBoundary('lambda-exec', 'arn:policy/Boundary'),
    ).rejects.toThrow('IAM role permissions-boundary request failed with status 503');
  });
});

describe('deleteIamRolePermissionsBoundary', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamRolePermissionsBoundary('lambda-exec');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/roles/lambda-exec/permissions-boundary',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamRolePermissionsBoundary('lambda-exec')).rejects.toThrow(
      'IAM role permissions-boundary delete request failed with status 503',
    );
  });
});

describe('getIamPolicies', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed policies when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ policies: [{ policyName: 'ReadOnly', arn: 'arn:policy/ReadOnly' }] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamPolicies('local');

    expect(result.policies).toHaveLength(1);
    expect(result.policies[0].policyName).toBe('ReadOnly');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/policies?scope=local', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamPolicies('aws')).rejects.toThrow('IAM policies request failed with status 503');
  });
});

describe('getIamPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the policy detail when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ policyName: 'ReadOnly', arn: 'arn:policy/ReadOnly' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamPolicy('arn:policy/ReadOnly');

    expect(result.policyName).toBe('ReadOnly');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/policies/detail?policyArn=arn%3Apolicy%2FReadOnly',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamPolicy('arn:policy/ReadOnly')).rejects.toThrow(
      'IAM policy request failed with status 503',
    );
  });
});

describe('createIamPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the create request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const request = {
      policyName: 'ReadOnly',
      policyDocument: '{"Statement":[]}',
      description: null,
      path: null,
    };
    await createIamPolicy(request);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/policies', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      createIamPolicy({
        policyName: 'ReadOnly',
        policyDocument: '{}',
        description: null,
        path: null,
      }),
    ).rejects.toThrow('IAM policy create request failed with status 503');
  });
});

describe('createIamPolicyVersion', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the version request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createIamPolicyVersion('arn:policy/ReadOnly', '{"Statement":[]}', true);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/policies/versions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        policyArn: 'arn:policy/ReadOnly',
        policyDocument: '{"Statement":[]}',
        setAsDefault: true,
      }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      createIamPolicyVersion('arn:policy/ReadOnly', '{}', false),
    ).rejects.toThrow('IAM policy version create request failed with status 503');
  });
});

describe('setIamPolicyDefaultVersion', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the default version request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await setIamPolicyDefaultVersion('arn:policy/ReadOnly', 'v2');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/policies/default-version', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyArn: 'arn:policy/ReadOnly', versionId: 'v2' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(setIamPolicyDefaultVersion('arn:policy/ReadOnly', 'v2')).rejects.toThrow(
      'IAM policy default-version request failed with status 503',
    );
  });
});

describe('deleteIamPolicyVersion', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the version delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamPolicyVersion('arn:policy/ReadOnly', 'v2');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/policies/versions?policyArn=arn%3Apolicy%2FReadOnly&versionId=v2',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamPolicyVersion('arn:policy/ReadOnly', 'v2')).rejects.toThrow(
      'IAM policy version delete request failed with status 503',
    );
  });
});

describe('deleteIamPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamPolicy('arn:policy/ReadOnly');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/policies?policyArn=arn%3Apolicy%2FReadOnly',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamPolicy('arn:policy/ReadOnly')).rejects.toThrow(
      'IAM policy delete request failed with status 503',
    );
  });
});

describe('tagIamPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the tags when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await tagIamPolicy('arn:policy/ReadOnly', [{ key: 'team', value: 'platform' }]);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/policies/tags?policyArn=arn%3Apolicy%2FReadOnly',
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tags: [{ key: 'team', value: 'platform' }] }),
        signal: undefined,
      },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      tagIamPolicy('arn:policy/ReadOnly', [{ key: 'team', value: 'platform' }]),
    ).rejects.toThrow('IAM policy tag request failed with status 503');
  });
});

describe('untagIamPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the untag request with repeated key params when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await untagIamPolicy('arn:policy/ReadOnly', ['team', 'env']);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/iam/policies/tags?policyArn=arn%3Apolicy%2FReadOnly&key=team&key=env',
      { method: 'DELETE', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(untagIamPolicy('arn:policy/ReadOnly', ['team'])).rejects.toThrow(
      'IAM policy untag request failed with status 503',
    );
  });
});

describe('getIamAccountSummary', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed summary when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ entries: { Users: 3, Groups: 1 } }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamAccountSummary();

    expect(result.entries.Users).toBe(3);
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/account/summary', {
      signal: undefined,
    });
  });

  it('throws IamNotSupportedError when the backend returns 501', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 501 }));

    await expect(getIamAccountSummary()).rejects.toThrow(IamNotSupportedError);
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 501 }));
    await expect(getIamAccountSummary()).rejects.toThrow(
      'IAM account summary is not supported by the current backend.',
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamAccountSummary()).rejects.toThrow(
      'IAM account summary request failed with status 503',
    );
  });
});

describe('getIamAccountPasswordPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed policy when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ minimumPasswordLength: 8, requireSymbols: true }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamAccountPasswordPolicy();

    expect(result?.minimumPasswordLength).toBe(8);
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/account/password-policy', {
      signal: undefined,
    });
  });

  it('returns null when the backend returns 404', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    const result = await getIamAccountPasswordPolicy();

    expect(result).toBeNull();
  });

  it('throws IamNotSupportedError when the backend returns 501', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 501 }));

    await expect(getIamAccountPasswordPolicy()).rejects.toThrow(IamNotSupportedError);
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 501 }));
    await expect(getIamAccountPasswordPolicy()).rejects.toThrow(
      'IAM account password policy is not supported by the current backend.',
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamAccountPasswordPolicy()).rejects.toThrow(
      'IAM account password policy request failed with status 503',
    );
  });
});

describe('updateIamAccountPasswordPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('puts the update request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    const request = {
      minimumPasswordLength: 12,
      requireSymbols: true,
      requireNumbers: true,
      requireUppercaseCharacters: true,
      requireLowercaseCharacters: true,
      allowUsersToChangePassword: true,
      maxPasswordAge: 90,
      passwordReusePrevention: 5,
      hardExpiry: false,
    };
    await updateIamAccountPasswordPolicy(request);

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/account/password-policy', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(
      updateIamAccountPasswordPolicy({
        minimumPasswordLength: 12,
        requireSymbols: true,
        requireNumbers: true,
        requireUppercaseCharacters: true,
        requireLowercaseCharacters: true,
        allowUsersToChangePassword: true,
        maxPasswordAge: 90,
        passwordReusePrevention: 5,
        hardExpiry: false,
      }),
    ).rejects.toThrow('IAM account password policy update request failed with status 503');
  });
});

describe('deleteIamAccountPasswordPolicy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamAccountPasswordPolicy();

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/account/password-policy', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamAccountPasswordPolicy()).rejects.toThrow(
      'IAM account password policy delete request failed with status 503',
    );
  });
});

describe('getIamAccountAliases', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the parsed aliases when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ aliases: ['my-account'] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getIamAccountAliases();

    expect(result.aliases).toEqual(['my-account']);
    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/account/aliases', {
      signal: undefined,
    });
  });

  it('throws IamNotSupportedError when the backend returns 501', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 501 }));

    await expect(getIamAccountAliases()).rejects.toThrow(IamNotSupportedError);
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 501 }));
    await expect(getIamAccountAliases()).rejects.toThrow(
      'IAM account aliases are not supported by the current backend.',
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getIamAccountAliases()).rejects.toThrow(
      'IAM account aliases request failed with status 503',
    );
  });
});

describe('createIamAccountAlias', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('posts the create request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await createIamAccountAlias('my-account');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/account/aliases', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ accountAlias: 'my-account' }),
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(createIamAccountAlias('my-account')).rejects.toThrow(
      'IAM account alias create request failed with status 503',
    );
  });
});

describe('deleteIamAccountAlias', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('sends the delete request when invoked', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true });
    vi.stubGlobal('fetch', fetchMock);

    await deleteIamAccountAlias('my-account');

    expect(fetchMock).toHaveBeenCalledWith('/api/services/iam/account/aliases/my-account', {
      method: 'DELETE',
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(deleteIamAccountAlias('my-account')).rejects.toThrow(
      'IAM account alias delete request failed with status 503',
    );
  });
});

describe('detectStackDrift', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the drift detection id when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ stackDriftDetectionId: 'drift-1' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await detectStackDrift('my-stack');

    expect(result.stackDriftDetectionId).toBe('drift-1');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack/drift?name=my-stack',
      { method: 'POST', signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(detectStackDrift('my-stack')).rejects.toThrow(
      'CloudFormation drift detection request failed with status 503',
    );
  });
});

describe('getDriftStatus', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the drift status when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        stackDriftDetectionId: 'drift-1',
        stackId: 'arn:stack/my-stack',
        detectionStatus: 'DETECTION_COMPLETE',
        detectionStatusReason: null,
        stackDriftStatus: 'IN_SYNC',
        driftedStackResourceCount: 0,
        timestamp: '2026-01-02T03:04:05Z',
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getDriftStatus('drift-1');

    expect(result.detectionStatus).toBe('DETECTION_COMPLETE');
    expect(result.stackDriftStatus).toBe('IN_SYNC');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack/drift?driftDetectionId=drift-1',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getDriftStatus('drift-1')).rejects.toThrow(
      'CloudFormation drift status request failed with status 503',
    );
  });
});

describe('getResourceDrifts', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the resource drifts when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        drifts: [{ logicalResourceId: 'Bucket', stackResourceDriftStatus: 'MODIFIED' }],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getResourceDrifts('my-stack');

    expect(result.drifts).toHaveLength(1);
    expect(result.drifts[0].logicalResourceId).toBe('Bucket');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/stack/drift/resources?name=my-stack',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getResourceDrifts('my-stack')).rejects.toThrow(
      'CloudFormation resource drifts request failed with status 503',
    );
  });
});

describe('getExports', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the exports when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        exports: [{ name: 'VpcId', value: 'vpc-1', exportingStackId: 'arn:stack/net' }],
      }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getExports();

    expect(result.exports).toHaveLength(1);
    expect(result.exports[0].name).toBe('VpcId');
    expect(fetchMock).toHaveBeenCalledWith('/api/services/cloudformation/exports', {
      signal: undefined,
    });
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getExports()).rejects.toThrow(
      'CloudFormation exports request failed with status 503',
    );
  });
});

describe('getImports', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('returns the importing stack names when the request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ importingStackNames: ['app-stack'] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await getImports('VpcId');

    expect(result.importingStackNames).toEqual(['app-stack']);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/services/cloudformation/exports/VpcId/imports',
      { signal: undefined },
    );
  });

  it('throws when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(getImports('VpcId')).rejects.toThrow(
      'CloudFormation imports request failed with status 503',
    );
  });
});
