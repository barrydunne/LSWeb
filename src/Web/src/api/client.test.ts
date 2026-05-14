import { afterEach, describe, expect, it, vi } from 'vitest';
import {
  getActivity,
  getCatalogue,
  getConnectivity,
  getHealth,
  getLiveness,
  getSearch,
  getSearchState,
  refreshCatalogue,
  refreshSearch,
  resolveReference,
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
