import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { App } from './App';
import { getConnectivity, getLiveness } from './api/client';

vi.mock('./api/client');

const getLivenessMock = vi.mocked(getLiveness);
const getConnectivityMock = vi.mocked(getConnectivity);

describe('App', () => {
  beforeEach(() => {
    getConnectivityMock.mockResolvedValue({
      status: 'Connected',
      endpoint: 'http://localhost:4566',
      region: 'eu-west-1',
      error: null,
    });
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
