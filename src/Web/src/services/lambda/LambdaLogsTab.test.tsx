import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import userEvent from '@testing-library/user-event';
import { LambdaLogsTab } from './LambdaLogsTab';
import { getLambdaLogEvents, resolveReference } from '../../api/client';
import type { LambdaLogEventItem } from '../../api/client';

vi.mock('../../api/client');

const getLogEventsMock = vi.mocked(getLambdaLogEvents);
const resolveReferenceMock = vi.mocked(resolveReference);

const firstEvent: LambdaLogEventItem = {
  timestamp: '2026-01-02T03:04:05.0000000+00:00',
  message: 'START RequestId: abc',
  logStreamName: '2026/01/02/[$LATEST]abcdef',
};

const secondEvent: LambdaLogEventItem = {
  timestamp: '2026-01-02T03:04:06.0000000+00:00',
  message: 'END RequestId: abc',
  logStreamName: '2026/01/02/[$LATEST]abcdef',
};

function renderTab() {
  return render(
    <MemoryRouter>
      <LambdaLogsTab functionName="process-orders" />
    </MemoryRouter>,
  );
}

describe('LambdaLogsTab', () => {
  beforeEach(() => {
    resolveReferenceMock.mockRejectedValue(new Error('unresolved'));
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before log events arrive', () => {
    getLogEventsMock.mockReturnValue(new Promise(() => {}));

    renderTab();

    expect(screen.getByTestId('lambda-logs-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getLogEventsMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-logs-error')).toBeInTheDocument());
  });

  it('shows an empty state when there are no log events', async () => {
    getLogEventsMock.mockResolvedValue({ logGroupName: '/aws/lambda/process-orders', events: [] });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-logs-empty')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-logs-group')).toHaveTextContent('/aws/lambda/process-orders');
    expect(getLogEventsMock).toHaveBeenCalledWith('process-orders', undefined, expect.anything());
  });

  it('links the log group to the CloudWatch Logs view', async () => {
    getLogEventsMock.mockResolvedValue({ logGroupName: '/aws/lambda/process-orders', events: [] });
    resolveReferenceMock.mockResolvedValue({
      serviceKey: 'cloudwatch-logs',
      resourceId: '/aws/lambda/process-orders',
      route: '/services/cloudwatch-logs/%2Faws%2Flambda%2Fprocess-orders',
    });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-logs-empty')).toBeInTheDocument());
    await waitFor(() =>
      expect(screen.getByTestId('resource-link')).toHaveAttribute(
        'href',
        '/services/cloudwatch-logs/%2Faws%2Flambda%2Fprocess-orders',
      ),
    );
    expect(screen.getByTestId('resource-link')).toHaveTextContent('/aws/lambda/process-orders');
    expect(resolveReferenceMock).toHaveBeenCalledWith(
      '/aws/lambda/process-orders',
      'cloudwatch-logs',
      expect.anything(),
    );
  });

  it('renders the log group and each log event when data is ready', async () => {
    getLogEventsMock.mockResolvedValue({
      logGroupName: '/aws/lambda/process-orders',
      events: [firstEvent, secondEvent],
    });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-logs-tab')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-logs-group')).toHaveTextContent('/aws/lambda/process-orders');
    expect(screen.getByTestId('lambda-log-event-0')).toHaveTextContent('START RequestId: abc');
    expect(screen.getByTestId('lambda-log-event-0')).toHaveTextContent(
      '2026-01-02T03:04:05.0000000+00:00',
    );
    expect(screen.getByTestId('lambda-log-event-1')).toHaveTextContent('END RequestId: abc');
  });

  it('reloads log events when the refresh button is clicked', async () => {
    getLogEventsMock.mockResolvedValue({
      logGroupName: '/aws/lambda/process-orders',
      events: [firstEvent],
    });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-logs-tab')).toBeInTheDocument());
    expect(getLogEventsMock).toHaveBeenCalledTimes(1);

    await userEvent.click(screen.getByTestId('lambda-logs-refresh'));

    await waitFor(() => expect(getLogEventsMock).toHaveBeenCalledTimes(2));
  });

  it('reloads log events when auto-refresh triggers a refresh', async () => {
    getLogEventsMock.mockResolvedValue({
      logGroupName: '/aws/lambda/process-orders',
      events: [firstEvent],
    });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-logs-tab')).toBeInTheDocument());
    expect(getLogEventsMock).toHaveBeenCalledTimes(1);

    vi.useFakeTimers();
    try {
      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      await act(async () => {
        await vi.advanceTimersByTimeAsync(5000);
      });
    } finally {
      vi.useRealTimers();
    }

    expect(getLogEventsMock.mock.calls.length).toBeGreaterThan(1);
  });
});
