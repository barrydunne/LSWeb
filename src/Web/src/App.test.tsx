import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { App } from './App';
import { getActivity, getCatalogue, getConnectivity, getHealth, getLiveness } from './api/client';
import { subscribeToNotifications } from './api/notifications';

vi.mock('./api/client');
vi.mock('./api/notifications');

const getLivenessMock = vi.mocked(getLiveness);
const getConnectivityMock = vi.mocked(getConnectivity);
const getCatalogueMock = vi.mocked(getCatalogue);
const getHealthMock = vi.mocked(getHealth);
const getActivityMock = vi.mocked(getActivity);
const subscribeToNotificationsMock = vi.mocked(subscribeToNotifications);

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
    getActivityMock.mockResolvedValue({ entries: [] });
    subscribeToNotificationsMock.mockResolvedValue({ stop: vi.fn().mockResolvedValue(undefined) });
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
