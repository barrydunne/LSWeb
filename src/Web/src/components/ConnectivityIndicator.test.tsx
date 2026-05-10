import { afterEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { ConnectivityIndicator } from './ConnectivityIndicator';
import { getConnectivity } from '../api/client';

vi.mock('../api/client');

const getConnectivityMock = vi.mocked(getConnectivity);

function renderIndicator() {
  return render(
    <ThemeProvider colorMode="night">
      <ConnectivityIndicator />
    </ThemeProvider>,
  );
}

describe('ConnectivityIndicator', () => {
  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a checking state before the result resolves', () => {
    getConnectivityMock.mockReturnValue(new Promise(() => {}));

    renderIndicator();

    expect(screen.getByTestId('connectivity-indicator')).toHaveTextContent('checking');
  });

  it('shows a connected status with the resolved target and no error', async () => {
    getConnectivityMock.mockResolvedValue({
      status: 'Connected',
      endpoint: 'http://localhost:4566',
      region: 'eu-west-1',
      error: null,
    });

    renderIndicator();

    await waitFor(() => {
      expect(screen.getByTestId('connectivity-status')).toHaveTextContent('Connected');
    });
    expect(screen.getByTestId('connectivity-target')).toHaveTextContent('http://localhost:4566');
    expect(screen.getByTestId('connectivity-target')).toHaveTextContent('eu-west-1');
    expect(screen.queryByTestId('connectivity-error')).not.toBeInTheDocument();
  });

  it('shows a disconnected status with the backend error', async () => {
    getConnectivityMock.mockResolvedValue({
      status: 'Disconnected',
      endpoint: 'http://localhost:4566',
      region: 'eu-west-1',
      error: 'connection refused',
    });

    renderIndicator();

    await waitFor(() => {
      expect(screen.getByTestId('connectivity-status')).toHaveTextContent('Disconnected');
    });
    expect(screen.getByTestId('connectivity-error')).toHaveTextContent('connection refused');
  });

  it('shows an unavailable state when the request fails', async () => {
    getConnectivityMock.mockRejectedValue(new Error('boom'));

    renderIndicator();

    await waitFor(() => {
      expect(screen.getByTestId('connectivity-status')).toHaveTextContent('Unavailable');
    });
    expect(screen.getByTestId('connectivity-error')).toHaveTextContent('Unable to reach the backend.');
  });
});
