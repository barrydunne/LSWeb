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
