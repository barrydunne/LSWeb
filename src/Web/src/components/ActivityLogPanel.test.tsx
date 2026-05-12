import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@primer/react';
import { ActivityLogPanel } from './ActivityLogPanel';
import { getActivity, type ActivityEntryItem } from '../api/client';
import { subscribeToNotifications, type Notification } from '../api/notifications';

vi.mock('../api/client');
vi.mock('../api/notifications');

const getActivityMock = vi.mocked(getActivity);
const subscribeMock = vi.mocked(subscribeToNotifications);

let capturedHandler: (notification: Notification) => void;
const stop = vi.fn().mockResolvedValue(undefined);

function entry(overrides: Partial<ActivityEntryItem>): ActivityEntryItem {
  return {
    operationId: 'op-1',
    operation: 'catalogue-refresh',
    state: 'Succeeded',
    message: 'Service catalogue refreshed.',
    occurredAt: '2026-01-02T03:04:05Z',
    ...overrides,
  };
}

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

function renderPanel() {
  return render(
    <ThemeProvider colorMode="night">
      <ActivityLogPanel />
    </ThemeProvider>,
  );
}

describe('ActivityLogPanel', () => {
  beforeEach(() => {
    getActivityMock.mockResolvedValue({ entries: [] });
    subscribeMock.mockImplementation((handler) => {
      capturedHandler = handler;
      return Promise.resolve({ stop });
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows the empty state when no activity has been recorded', async () => {
    renderPanel();

    expect(screen.getByTestId('activity-log-panel')).toBeInTheDocument();
    await waitFor(() => expect(screen.getByTestId('activity-log-empty')).toBeInTheDocument());
    expect(screen.queryByTestId('activity-log-entry')).not.toBeInTheDocument();
  });

  it('populates entries returned by the activity endpoint', async () => {
    getActivityMock.mockResolvedValue({
      entries: [entry({ operationId: 'op-1', operation: 'catalogue-refresh', state: 'Succeeded' })],
    });

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('activity-log-entry')).toBeInTheDocument());
    expect(screen.getByTestId('activity-log-entry-operation')).toHaveTextContent('catalogue-refresh');
    expect(screen.getByTestId('activity-log-entry-state')).toHaveTextContent('Succeeded');
  });

  it('renders an unknown state verbatim with a secondary label', async () => {
    getActivityMock.mockResolvedValue({
      entries: [entry({ operationId: 'op-x', state: 'Pending', message: 'Queued.' })],
    });

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('activity-log-entry-state')).toHaveTextContent('Pending'));
  });

  it('appends a live notification as a new log entry alongside the toast surface', async () => {
    renderPanel();
    await waitFor(() => expect(subscribeMock).toHaveBeenCalled());

    act(() => capturedHandler(notification({ operationId: 'op-2', operation: 'delete', message: 'Removing\u2026' })));

    await waitFor(() => expect(screen.getByTestId('activity-log-entry')).toBeInTheDocument());
    expect(screen.getByTestId('activity-log-entry-operation')).toHaveTextContent('delete');
    expect(screen.getByTestId('activity-log-entry-state')).toHaveTextContent('In progress');
  });

  it('updates an existing entry in place as its operation transitions', async () => {
    renderPanel();
    await waitFor(() => expect(subscribeMock).toHaveBeenCalled());

    act(() => capturedHandler(notification({ operationId: 'op-1', state: 'InProgress' })));
    await waitFor(() => expect(screen.getByTestId('activity-log-entry-state')).toHaveTextContent('In progress'));

    act(() => capturedHandler(notification({ operationId: 'op-1', state: 'Succeeded', message: 'Done.' })));

    await waitFor(() => expect(screen.getByTestId('activity-log-entry-state')).toHaveTextContent('Succeeded'));
    expect(screen.getAllByTestId('activity-log-entry')).toHaveLength(1);
    expect(screen.getByTestId('activity-log-entry-message')).toHaveTextContent('Done.');
  });

  it('collapses and expands the entry list', async () => {
    getActivityMock.mockResolvedValue({ entries: [entry({})] });

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('activity-log-entries')).toBeInTheDocument());

    await userEvent.click(screen.getByTestId('activity-log-toggle'));
    expect(screen.queryByTestId('activity-log-entries')).not.toBeInTheDocument();

    await userEvent.click(screen.getByTestId('activity-log-toggle'));
    expect(screen.getByTestId('activity-log-entries')).toBeInTheDocument();
  });

  it('ignores a failed activity request', async () => {
    getActivityMock.mockRejectedValueOnce(new Error('no backend'));

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('activity-log-empty')).toBeInTheDocument());
  });

  it('stops the subscription on unmount', async () => {
    const { unmount } = renderPanel();
    await waitFor(() => expect(subscribeMock).toHaveBeenCalled());

    unmount();

    await waitFor(() => expect(stop).toHaveBeenCalledTimes(1));
  });

  it('ignores a failed subscription on unmount', async () => {
    subscribeMock.mockRejectedValueOnce(new Error('no hub'));

    const { unmount } = renderPanel();
    unmount();

    await waitFor(() => expect(subscribeMock).toHaveBeenCalled());
    expect(stop).not.toHaveBeenCalled();
  });
});
