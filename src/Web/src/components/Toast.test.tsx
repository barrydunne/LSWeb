import { afterEach, describe, expect, it, vi } from 'vitest';
import { act, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@primer/react';
import { Toast } from './Toast';
import type { Notification } from '../api/notifications';

function renderToast(notification: Notification, onDismiss = vi.fn()) {
  render(
    <ThemeProvider colorMode="night">
      <Toast notification={notification} onDismiss={onDismiss} />
    </ThemeProvider>,
  );
  return onDismiss;
}

const base: Notification = {
  operationId: 'op-1',
  operation: 'catalogue-refresh',
  state: 'InProgress',
  message: 'Refreshing the catalogue\u2026',
  occurredAt: '2026-01-02T03:04:05Z',
};

describe('Toast', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('shows an in-progress status', () => {
    renderToast({ ...base, state: 'InProgress' });

    expect(screen.getByTestId('toast-status')).toHaveTextContent('In progress');
    expect(screen.getByTestId('toast-operation')).toHaveTextContent('catalogue-refresh');
    expect(screen.getByTestId('toast-message')).toHaveTextContent('Refreshing the catalogue');
  });

  it('shows a success status', () => {
    renderToast({ ...base, state: 'Succeeded', message: 'Catalogue refreshed.' });

    expect(screen.getByTestId('toast-status')).toHaveTextContent('Succeeded');
    expect(screen.getByTestId('toast-message')).toHaveTextContent('Catalogue refreshed.');
  });

  it('shows a failure status with the backend message', () => {
    renderToast({ ...base, state: 'Failed', message: 'AccessDenied: not authorised.' });

    expect(screen.getByTestId('toast-status')).toHaveTextContent('Failed');
    expect(screen.getByTestId('toast-message')).toHaveTextContent('AccessDenied: not authorised.');
  });

  it('invokes onDismiss when the dismiss button is clicked', async () => {
    const onDismiss = renderToast({ ...base, state: 'Succeeded' });

    await userEvent.click(screen.getByTestId('toast-dismiss'));

    expect(onDismiss).toHaveBeenCalledTimes(1);
  });

  it('auto-dismisses a terminal toast after the timeout', () => {
    vi.useFakeTimers();
    const onDismiss = renderToast({ ...base, state: 'Succeeded' });

    expect(onDismiss).not.toHaveBeenCalled();
    act(() => {
      vi.advanceTimersByTime(5000);
    });

    expect(onDismiss).toHaveBeenCalledTimes(1);
  });

  it('does not auto-dismiss an in-progress toast', () => {
    vi.useFakeTimers();
    const onDismiss = renderToast({ ...base, state: 'InProgress' });

    act(() => {
      vi.advanceTimersByTime(60000);
    });

    expect(onDismiss).not.toHaveBeenCalled();
  });
});
