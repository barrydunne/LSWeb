import { afterEach, describe, expect, it, vi } from 'vitest';
import { act, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { ConnectivityIndicator } from './ConnectivityIndicator';
import { getConnectivity, type ConnectivityResult } from '../api/client';

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

  it('polls and refreshes the status without a page reload', async () => {
    vi.useFakeTimers();
    try {
      getConnectivityMock.mockResolvedValue({
        status: 'Disconnected',
        endpoint: 'http://localhost:4566',
        region: 'eu-west-1',
        error: 'connection refused',
      });

      renderIndicator();

      await act(async () => {
        await Promise.resolve();
      });
      expect(screen.getByTestId('connectivity-status')).toHaveTextContent('Disconnected');

      getConnectivityMock.mockResolvedValue({
        status: 'Connected',
        endpoint: 'http://localhost:4566',
        region: 'eu-west-1',
        error: null,
      });

      await act(async () => {
        await vi.advanceTimersByTimeAsync(10_000);
      });

      expect(screen.getByTestId('connectivity-status')).toHaveTextContent('Connected');
    } finally {
      vi.useRealTimers();
    }
  });

  it('ignores a resolved result after unmount', async () => {
    let resolve: ((result: ConnectivityResult) => void) | undefined;
    getConnectivityMock.mockReturnValue(
      new Promise<ConnectivityResult>((res) => {
        resolve = res;
      }),
    );

    const { unmount } = renderIndicator();
    unmount();
    resolve?.({ status: 'Connected', endpoint: 'http://localhost:4566', region: 'eu-west-1', error: null });

    await Promise.resolve();
    expect(screen.queryByTestId('connectivity-status')).not.toBeInTheDocument();
  });

  it('ignores a rejected result after unmount', async () => {
    let reject: ((reason: unknown) => void) | undefined;
    getConnectivityMock.mockReturnValue(
      new Promise<ConnectivityResult>((_, rej) => {
        reject = rej;
      }),
    );

    const { unmount } = renderIndicator();
    unmount();
    reject?.(new Error('late failure'));

    await Promise.resolve();
    expect(screen.queryByTestId('connectivity-status')).not.toBeInTheDocument();
  });
});
