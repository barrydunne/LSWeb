import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { App } from './App';
import {
  getActivity,
  getCatalogue,
  getCircuitStatus,
  getConnectivity,
  getDiagnostics,
  getFavourites,
  getHealth,
  getLiveness,
  getRecentlyViewed,
  getSearchState,
  getSeedTemplates,
} from './api/client';
import { subscribeToNotifications } from './api/notifications';

vi.mock('./api/client');
vi.mock('./api/notifications');

const getLivenessMock = vi.mocked(getLiveness);
const getConnectivityMock = vi.mocked(getConnectivity);
const getCatalogueMock = vi.mocked(getCatalogue);
const getHealthMock = vi.mocked(getHealth);
const getCircuitStatusMock = vi.mocked(getCircuitStatus);
const getActivityMock = vi.mocked(getActivity);
const getSearchStateMock = vi.mocked(getSearchState);
const getRecentlyViewedMock = vi.mocked(getRecentlyViewed);
const getFavouritesMock = vi.mocked(getFavourites);
const getDiagnosticsMock = vi.mocked(getDiagnostics);
const subscribeToNotificationsMock = vi.mocked(subscribeToNotifications);
const getSeedTemplatesMock = vi.mocked(getSeedTemplates);
describe('App', () => {
  beforeEach(() => {
    getConnectivityMock.mockResolvedValue({
      status: 'Connected',
      endpoint: 'http://localhost:4566',
      region: 'eu-west-1',
      error: null,
    });
    getCatalogueMock.mockResolvedValue({ services: [] });
    getHealthMock.mockResolvedValue({ services: [] });
    getCircuitStatusMock.mockResolvedValue({ isOpen: false, affectedServices: [] });
    getActivityMock.mockResolvedValue({ entries: [] });
    getSearchStateMock.mockResolvedValue({
      builtAt: '2026-01-01T00:00:00Z',
      entryCount: 0,
      isBuilding: false,
    });
    getRecentlyViewedMock.mockResolvedValue({ references: [] });
    getFavouritesMock.mockResolvedValue({ references: [] });
    getDiagnosticsMock.mockResolvedValue({
      configuration: [],
      endpoint: 'http://localhost:4566',
      region: 'eu-west-1',
      connectivityStatus: 'Connected',
      connectivityError: null,
      revealAllowed: false,
    });
    subscribeToNotificationsMock.mockResolvedValue({ stop: vi.fn().mockResolvedValue(undefined) });
    getSeedTemplatesMock.mockResolvedValue({ templates: [] });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('renders the console shell and shows the reported status', async () => {
    getLivenessMock.mockResolvedValue({ status: 'Healthy' });

    render(<App />);

    expect(screen.getByTestId('app-root')).toBeInTheDocument();
    expect(screen.getByTestId('app-title')).toHaveTextContent('LocalStack Web');

    await waitFor(() =>
      expect(screen.getByTestId('health-status')).toHaveTextContent('Service status: Healthy'),
    );
  });

  it('shows an unavailable status when the health request fails', async () => {
    getLivenessMock.mockRejectedValue(new Error('boom'));

    render(<App />);

    await waitFor(() =>
      expect(screen.getByTestId('health-status')).toHaveTextContent('Service status: unavailable'),
    );
  });
});
