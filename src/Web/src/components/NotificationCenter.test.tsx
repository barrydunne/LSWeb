import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@primer/react';
import { NotificationCenter } from './NotificationCenter';
import { subscribeToNotifications, type Notification } from '../api/notifications';

vi.mock('../api/notifications');

const subscribeMock = vi.mocked(subscribeToNotifications);

let capturedHandler: (notification: Notification) => void;
const stop = vi.fn().mockResolvedValue(undefined);

function notification(overrides: Partial<Notification>): Notification {
  return {
    operationId: 'op-1',
    operation: 'catalogue-refresh',
    state: 'InProgress',
    message: 'Working\u2026',
    occurredAt: '2026-01-02T03:04:05Z',
    ...overrides,
  };
}

function renderCenter() {
  return render(
    <ThemeProvider colorMode="night">
      <NotificationCenter />
    </ThemeProvider>,
  );
}

describe('NotificationCenter', () => {
  beforeEach(() => {
    subscribeMock.mockImplementation((handler) => {
      capturedHandler = handler;
      return Promise.resolve({ stop });
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders an empty center before any notification arrives', () => {
    renderCenter();

    expect(screen.getByTestId('notification-center')).toBeInTheDocument();
    expect(screen.queryByTestId('toast')).not.toBeInTheDocument();
  });

  it('shows a toast and replaces it as the operation transitions to a terminal state', async () => {
    renderCenter();

    act(() => capturedHandler(notification({ state: 'InProgress' })));
    await waitFor(() => expect(screen.getByTestId('toast-status')).toHaveTextContent('In progress'));

    act(() => capturedHandler(notification({ state: 'Succeeded', message: 'Catalogue refreshed.' })));

    await waitFor(() => expect(screen.getByTestId('toast-status')).toHaveTextContent('Succeeded'));
    expect(screen.getAllByTestId('toast')).toHaveLength(1);
    expect(screen.getByTestId('toast-message')).toHaveTextContent('Catalogue refreshed.');
  });

  it('shows a separate toast for each operation', async () => {
    renderCenter();

    act(() => {
      capturedHandler(notification({ operationId: 'op-1', operation: 'refresh' }));
      capturedHandler(notification({ operationId: 'op-2', operation: 'delete', state: 'Failed', message: 'Boom.' }));
    });

    await waitFor(() => expect(screen.getAllByTestId('toast')).toHaveLength(2));
  });

  it('dismisses a single toast while leaving the others', async () => {
    renderCenter();

    act(() => {
      capturedHandler(notification({ operationId: 'op-1' }));
      capturedHandler(notification({ operationId: 'op-2', state: 'Failed', message: 'Boom.' }));
    });
    await waitFor(() => expect(screen.getAllByTestId('toast')).toHaveLength(2));

    await userEvent.click(screen.getAllByTestId('toast-dismiss')[0]);

    expect(screen.getAllByTestId('toast')).toHaveLength(1);
  });

  it('stops the subscription on unmount', async () => {
    const { unmount } = renderCenter();
    await waitFor(() => expect(subscribeMock).toHaveBeenCalled());

    unmount();

    await waitFor(() => expect(stop).toHaveBeenCalledTimes(1));
  });

  it('ignores a failed subscription on unmount', async () => {
    subscribeMock.mockRejectedValueOnce(new Error('no hub'));

    const { unmount } = renderCenter();
    unmount();

    await waitFor(() => expect(subscribeMock).toHaveBeenCalled());
    expect(stop).not.toHaveBeenCalled();
  });
});
