import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { CloudWatchLogsListView } from './CloudWatchLogsListView';
import { createLogGroup, deleteLogGroup, getLogGroups } from '../../api/client';
import type { LogGroupListResult } from '../../api/client';

vi.mock('../../api/client');

const getLogGroupsMock = vi.mocked(getLogGroups);
const createLogGroupMock = vi.mocked(createLogGroup);
const deleteLogGroupMock = vi.mocked(deleteLogGroup);

const listResult: LogGroupListResult = {
  logGroups: [
    {
      name: '/aws/lambda/orders',
      arn: 'arn:aws:logs:eu-west-1:000000000000:log-group:/aws/lambda/orders',
      storedBytes: 2048,
      retentionInDays: 7,
      createdAt: '2026-01-02T03:04:05Z',
    },
    {
      name: '/aws/lambda/invoices',
      arn: 'arn:aws:logs:eu-west-1:000000000000:log-group:/aws/lambda/invoices',
      storedBytes: 0,
      retentionInDays: null,
      createdAt: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <CloudWatchLogsListView serviceKey="cloudwatch-logs" />
    </MemoryRouter>,
  );
}

describe('CloudWatchLogsListView', () => {
  beforeEach(() => {
    getLogGroupsMock.mockResolvedValue(listResult);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before groups arrive', () => {
    getLogGroupsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('logs-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getLogGroupsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-list-error')).toBeInTheDocument());
  });

  it('renders a row per log group', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-list-view')).toBeInTheDocument());

    const links = screen.getAllByTestId('logs-list-link');
    expect(links[0]).toHaveTextContent('/aws/lambda/orders');
    expect(links[0]).toHaveAttribute(
      'href',
      '/services/cloudwatch-logs/%2Faws%2Flambda%2Forders',
    );
  });

  it('shows the stored bytes and retention for each group', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-list-view')).toBeInTheDocument());

    const stored = screen.getAllByTestId('logs-list-stored');
    const retention = screen.getAllByTestId('logs-list-retention');
    expect(stored[0]).toHaveTextContent('2048');
    expect(retention[0]).toHaveTextContent('7');
    expect(retention[1]).toHaveTextContent('Never');
  });

  it('reloads the groups when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() => expect(screen.getByTestId('logs-list-view')).toBeInTheDocument());
      expect(getLogGroupsMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getLogGroupsMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('creates a log group from the form and refreshes the list', async () => {
    createLogGroupMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('logs-create-toggle'));

    fireEvent.change(screen.getByTestId('logs-create-name'), {
      target: { value: '/aws/lambda/new' },
    });

    fireEvent.click(screen.getByTestId('logs-create-submit'));

    await waitFor(() => expect(screen.getByTestId('logs-create-status')).toBeInTheDocument());

    expect(createLogGroupMock).toHaveBeenCalledWith('/aws/lambda/new');
    await waitFor(() => expect(getLogGroupsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('logs-create-form')).not.toBeInTheDocument();
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('logs-create-toggle'));
    expect(screen.getByTestId('logs-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('logs-create-toggle'));
    expect(screen.queryByTestId('logs-create-form')).not.toBeInTheDocument();
  });

  it('shows an error when log group creation fails', async () => {
    createLogGroupMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('logs-create-toggle'));
    fireEvent.click(screen.getByTestId('logs-create-submit'));

    await waitFor(() => expect(screen.getByTestId('logs-create-error')).toBeInTheDocument());
    expect(screen.getByTestId('logs-create-form')).toBeInTheDocument();
  });

  it('deletes a log group after confirmation and refreshes the list', async () => {
    deleteLogGroupMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteLogGroupMock).toHaveBeenCalledWith('/aws/lambda/orders'));
    await waitFor(() => expect(getLogGroupsMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when log group deletion fails', async () => {
    deleteLogGroupMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('logs-list-error')).toBeInTheDocument());
  });
});