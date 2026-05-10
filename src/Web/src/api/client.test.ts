import { afterEach, describe, expect, it, vi } from 'vitest';
import { getCatalogue, getConnectivity, getHealth, getLiveness } from './client';

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
