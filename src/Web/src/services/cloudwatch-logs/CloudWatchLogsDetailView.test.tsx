import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { CloudWatchLogsDetailView } from './CloudWatchLogsDetailView';
import {
  createLogStream,
  deleteLogStream,
  getLogEvents,
  getLogStreams,
  runLogInsightsQuery,
} from '../../api/client';
import type {
  LogEventListResult,
  LogInsightsQueryResult,
  LogStreamListResult,
} from '../../api/client';
import { streamLogGroup } from '../../api/notifications';
import type { LiveLogEvent, LiveLogEventHandler } from '../../api/notifications';

vi.mock('../../api/client');
vi.mock('../../api/notifications');
vi.mock('../../components/ResourceLink', () => ({
  ResourceLink: ({
    reference,
    service,
    label,
  }: {
    reference: string;
    service?: string;
    label?: string;
  }) => (
    <a data-testid="resource-link" data-reference={reference} data-service={service}>
      {label ?? reference}
    </a>
  ),
}));

const getLogStreamsMock = vi.mocked(getLogStreams);
const getLogEventsMock = vi.mocked(getLogEvents);
const streamLogGroupMock = vi.mocked(streamLogGroup);
const createLogStreamMock = vi.mocked(createLogStream);
const deleteLogStreamMock = vi.mocked(deleteLogStream);
const runLogInsightsQueryMock = vi.mocked(runLogInsightsQuery);

const streamsResult: LogStreamListResult = {
  logStreams: [
    { name: '2026/01/02/[$LATEST]abc', lastEventTimestamp: '2026-01-02T03:04:05Z' },
    { name: '2026/01/01/[$LATEST]def', lastEventTimestamp: null },
  ],
};

const eventsResult: LogEventListResult = {
  events: [
    { timestamp: '2026-01-02T03:04:05Z', message: 'order received' },
    { timestamp: '2026-01-02T03:04:06Z', message: 'order processed' },
  ],
};

function renderView() {
  return render(
    <CloudWatchLogsDetailView serviceKey="cloudwatch-logs" resourceId="/aws/lambda/orders" />,
  );
}
function renderViewWithResourceId(resourceId: string) {
  return render(
    <CloudWatchLogsDetailView serviceKey="cloudwatch-logs" resourceId={resourceId} />,
  );
}describe('CloudWatchLogsDetailView', () => {
  beforeEach(() => {
    getLogStreamsMock.mockResolvedValue(streamsResult);
    getLogEventsMock.mockResolvedValue(eventsResult);
    streamLogGroupMock.mockResolvedValue({ stop: vi.fn().mockResolvedValue(undefined) });
    createLogStreamMock.mockResolvedValue(undefined);
    deleteLogStreamMock.mockResolvedValue(undefined);
    runLogInsightsQueryMock.mockResolvedValue({
      status: 'Complete',
      rows: [{ fields: [{ field: '@message', value: 'hello' }] }],
      recordsMatched: 1,
      recordsScanned: 2,
    });
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows the log group name', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-detail-name')).toHaveTextContent('/aws/lambda/orders'));
  });

  it('shows a loading state before streams arrive', () => {
    getLogStreamsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('logs-streams-loading')).toBeInTheDocument();
  });

  it('shows an error state when the streams request fails', async () => {
    getLogStreamsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-error')).toBeInTheDocument());
  });

  it('shows an empty state when there are no streams', async () => {
    getLogStreamsMock.mockResolvedValue({ logStreams: [] });

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-empty')).toBeInTheDocument());
  });

  it('lists the log streams', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());

    const buttons = screen.getAllByTestId('logs-stream-button');
    expect(buttons).toHaveLength(2);
    expect(buttons[0]).toHaveTextContent('2026/01/02/[$LATEST]abc');
  });

  it('loads and shows events when a stream is selected', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('logs-stream-button')[0]);

    await waitFor(() => expect(screen.getByTestId('logs-events-list')).toBeInTheDocument());

    const messages = screen.getAllByTestId('logs-event-message');
    expect(messages[0]).toHaveTextContent('order received');
    expect(getLogEventsMock).toHaveBeenCalledWith(
      '/aws/lambda/orders',
      '2026/01/02/[$LATEST]abc',
      expect.anything(),
    );
  });

  it('shows an empty state when a stream has no events', async () => {
    getLogEventsMock.mockResolvedValue({ events: [] });

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.click(screen.getAllByTestId('logs-stream-button')[0]);

    await waitFor(() => expect(screen.getByTestId('logs-events-empty')).toBeInTheDocument());
  });

  it('shows an error state when the events request fails', async () => {
    getLogEventsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.click(screen.getAllByTestId('logs-stream-button')[0]);

    await waitFor(() => expect(screen.getByTestId('logs-events-error')).toBeInTheDocument());
  });

  it('shows a loading state while events are loading', async () => {
    getLogEventsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.click(screen.getAllByTestId('logs-stream-button')[0]);

    await waitFor(() => expect(screen.getByTestId('logs-events-loading')).toBeInTheDocument());
  });

  it('reloads streams and clears the selection on refresh', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.click(screen.getAllByTestId('logs-stream-button')[0]);
    await waitFor(() => expect(screen.getByTestId('logs-events-list')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('logs-detail-refresh'));

    await waitFor(() => expect(getLogStreamsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('logs-events-list')).not.toBeInTheDocument();
  });

  it('starts a live tail stream when follow is enabled', async () => {
    let capturedHandler: LiveLogEventHandler | null = null;
    streamLogGroupMock.mockImplementation((_group, _filter, handler) => {
      capturedHandler = handler;
      return Promise.resolve({ stop: vi.fn().mockResolvedValue(undefined) });
    });

    renderView();

    fireEvent.click(screen.getByTestId('logs-follow-toggle'));

    await waitFor(() =>
      expect(streamLogGroupMock).toHaveBeenCalledWith(
        '/aws/lambda/orders',
        '',
        expect.any(Function),
      ),
    );

    const event: LiveLogEvent = { timestamp: '2026-01-02T03:04:07Z', message: 'live order' };
    capturedHandler!(event);

    await waitFor(() =>
      expect(screen.getByTestId('logs-live-list')).toHaveTextContent('live order'),
    );
  });

  it('stops following and disposes the subscription', async () => {
    const stop = vi.fn().mockResolvedValue(undefined);
    streamLogGroupMock.mockResolvedValue({ stop });

    renderView();

    fireEvent.click(screen.getByTestId('logs-follow-toggle'));
    await waitFor(() => expect(streamLogGroupMock).toHaveBeenCalledTimes(1));

    fireEvent.click(screen.getByTestId('logs-follow-toggle'));

    await waitFor(() => expect(stop).toHaveBeenCalledTimes(1));
    expect(screen.queryByTestId('logs-live-list')).not.toBeInTheDocument();
  });

  it('passes the filter pattern to the live tail stream', async () => {
    renderView();

    fireEvent.change(screen.getByTestId('logs-filter-pattern'), {
      target: { value: 'ERROR' },
    });
    fireEvent.click(screen.getByTestId('logs-follow-toggle'));

    await waitFor(() =>
      expect(streamLogGroupMock).toHaveBeenCalledWith(
        '/aws/lambda/orders',
        'ERROR',
        expect.any(Function),
      ),
    );
  });

  it('stops following when the stream fails to start', async () => {
    streamLogGroupMock.mockRejectedValue(new Error('boom'));

    renderView();

    fireEvent.click(screen.getByTestId('logs-follow-toggle'));

    await waitFor(() =>
      expect(screen.getByTestId('logs-follow-toggle')).toHaveTextContent('Follow'),
    );
  });

  it('filters live events using the search box', async () => {
    let capturedHandler: LiveLogEventHandler | null = null;
    streamLogGroupMock.mockImplementation((_group, _filter, handler) => {
      capturedHandler = handler;
      return Promise.resolve({ stop: vi.fn().mockResolvedValue(undefined) });
    });

    renderView();

    fireEvent.click(screen.getByTestId('logs-follow-toggle'));
    await waitFor(() => expect(streamLogGroupMock).toHaveBeenCalledTimes(1));

    capturedHandler!({ timestamp: '2026-01-02T03:04:07Z', message: 'keep me' });
    capturedHandler!({ timestamp: '2026-01-02T03:04:08Z', message: 'drop that' });

    await waitFor(() =>
      expect(screen.getByTestId('logs-live-list')).toHaveTextContent('keep me'),
    );

    fireEvent.change(screen.getByTestId('logs-search'), { target: { value: 'keep' } });

    await waitFor(() =>
      expect(screen.getByTestId('logs-live-list')).not.toHaveTextContent('drop that'),
    );
    expect(screen.getByTestId('logs-live-list')).toHaveTextContent('keep me');
  });

  it('filters static events using the search box', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.click(screen.getAllByTestId('logs-stream-button')[0]);
    await waitFor(() => expect(screen.getByTestId('logs-events-list')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('logs-search'), { target: { value: 'processed' } });

    await waitFor(() =>
      expect(screen.getByTestId('logs-events-list')).not.toHaveTextContent('order received'),
    );
    expect(screen.getByTestId('logs-events-list')).toHaveTextContent('order processed');
  });

  it('renders a cross-link to the owning Lambda function', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-lambda-owner')).toBeInTheDocument());
    const link = screen.getByTestId('resource-link');
    expect(link).toHaveTextContent('orders');
    expect(link).toHaveAttribute('data-reference', 'orders');
    expect(link).toHaveAttribute('data-service', 'lambda');
  });

  it('does not render a Lambda cross-link for non-Lambda log groups', async () => {
    renderViewWithResourceId('/aws/apigateway/access');

    await waitFor(() => expect(screen.getByTestId('logs-detail-name')).toBeInTheDocument());
    expect(screen.queryByTestId('logs-lambda-owner')).not.toBeInTheDocument();
  });

  it('does not render a Lambda cross-link when the function name is empty', async () => {
    renderViewWithResourceId('/aws/lambda/');

    await waitFor(() => expect(screen.getByTestId('logs-detail-name')).toBeInTheDocument());
    expect(screen.queryByTestId('logs-lambda-owner')).not.toBeInTheDocument();
  });

  it('exports the selected stream events to a downloadable file', async () => {
    const originalCreate = URL.createObjectURL;
    const originalRevoke = URL.revokeObjectURL;
    const createObjectURL = vi.fn(() => 'blob:mock-url');
    const revokeObjectURL = vi.fn();
    URL.createObjectURL = createObjectURL;
    URL.revokeObjectURL = revokeObjectURL;
    let downloadName = '';
    let downloadHref = '';
    const clickSpy = vi
      .spyOn(HTMLAnchorElement.prototype, 'click')
      .mockImplementation(function (this: HTMLAnchorElement) {
        downloadName = this.download;
        downloadHref = this.href;
      });

    try {
      renderView();

      await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
      fireEvent.click(screen.getAllByTestId('logs-stream-button')[0]);
      await waitFor(() => expect(screen.getByTestId('logs-export-button')).toBeInTheDocument());

      fireEvent.click(screen.getByTestId('logs-export-button'));

      expect(createObjectURL).toHaveBeenCalledTimes(1);
      expect(clickSpy).toHaveBeenCalledTimes(1);
      expect(revokeObjectURL).toHaveBeenCalledWith('blob:mock-url');
      expect(downloadHref).toContain('blob:mock-url');
      expect(downloadName).toBe('_aws_lambda_orders_2026_01_02_LATEST_abc.log');
    } finally {
      URL.createObjectURL = originalCreate;
      URL.revokeObjectURL = originalRevoke;
      clickSpy.mockRestore();
    }
  });

  it('creates a new log stream and reloads the stream list', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('logs-new-stream-name'), {
      target: { value: 'new-stream' },
    });
    fireEvent.click(screen.getByTestId('logs-create-stream'));

    await waitFor(() =>
      expect(createLogStreamMock).toHaveBeenCalledWith('/aws/lambda/orders', 'new-stream'),
    );
    await waitFor(() =>
      expect(screen.getByTestId('logs-stream-action-success')).toHaveTextContent('new-stream'),
    );
  });

  it('does not call create when the new stream name is blank', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('logs-new-stream-name'), { target: { value: '   ' } });
    fireEvent.click(screen.getByTestId('logs-create-stream'));

    expect(createLogStreamMock).not.toHaveBeenCalled();
  });

  it('shows an error when creating a log stream fails', async () => {
    createLogStreamMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('logs-new-stream-name'), {
      target: { value: 'new-stream' },
    });
    fireEvent.click(screen.getByTestId('logs-create-stream'));

    await waitFor(() =>
      expect(screen.getByTestId('logs-stream-action-error')).toBeInTheDocument(),
    );
  });

  it('deletes a selected log stream and clears the events panel', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.click(screen.getAllByTestId('logs-stream-button')[0]);
    await waitFor(() => expect(screen.getByTestId('logs-events-list')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteLogStreamMock).toHaveBeenCalledWith(
        '/aws/lambda/orders',
        '2026/01/02/[$LATEST]abc',
      ),
    );
    await waitFor(() =>
      expect(screen.getByTestId('logs-stream-action-success')).toHaveTextContent('Deleted'),
    );
    expect(screen.queryByTestId('logs-events-list')).not.toBeInTheDocument();
  });

  it('shows an error when deleting a log stream fails', async () => {
    deleteLogStreamMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-streams-list')).toBeInTheDocument());
    fireEvent.click(screen.getAllByTestId('confirm-trigger')[1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('logs-stream-action-error')).toBeInTheDocument(),
    );
  });

  it('runs an Insights query with explicit times and limit and renders results', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-insights-query')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('logs-insights-query'), {
      target: { value: 'fields @message' },
    });
    fireEvent.change(screen.getByTestId('logs-insights-start'), {
      target: { value: '2026-01-01T00:00' },
    });
    fireEvent.change(screen.getByTestId('logs-insights-end'), {
      target: { value: '2026-01-02T00:00' },
    });
    fireEvent.change(screen.getByTestId('logs-insights-limit'), { target: { value: '50' } });
    fireEvent.click(screen.getByTestId('logs-insights-run'));

    await waitFor(() =>
      expect(runLogInsightsQueryMock).toHaveBeenCalledWith(
        '/aws/lambda/orders',
        'fields @message',
        new Date('2026-01-01T00:00').toISOString(),
        new Date('2026-01-02T00:00').toISOString(),
        50,
      ),
    );
    await waitFor(() =>
      expect(screen.getByTestId('logs-insights-table')).toHaveTextContent('hello'),
    );
    expect(screen.getByTestId('logs-insights-stats')).toHaveTextContent('1 matched');
  });

  it('runs an Insights query with default times and limit when fields are blank', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-insights-limit')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('logs-insights-limit'), { target: { value: '' } });
    fireEvent.click(screen.getByTestId('logs-insights-run'));

    await waitFor(() =>
      expect(runLogInsightsQueryMock).toHaveBeenCalledWith(
        '/aws/lambda/orders',
        expect.any(String),
        new Date(0).toISOString(),
        expect.any(String),
        1000,
      ),
    );
  });

  it('does not run an Insights query when the query is blank', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-insights-query')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('logs-insights-query'), { target: { value: '   ' } });
    fireEvent.click(screen.getByTestId('logs-insights-run'));

    expect(runLogInsightsQueryMock).not.toHaveBeenCalled();
  });

  it('shows an empty Insights result message when no rows are returned', async () => {
    const emptyResult: LogInsightsQueryResult = {
      status: 'Complete',
      rows: [],
      recordsMatched: 0,
      recordsScanned: 0,
    };
    runLogInsightsQueryMock.mockResolvedValue(emptyResult);

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-insights-run')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('logs-insights-run'));

    await waitFor(() =>
      expect(screen.getByTestId('logs-insights-empty')).toBeInTheDocument(),
    );
  });

  it('shows an error when the Insights query fails', async () => {
    runLogInsightsQueryMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('logs-insights-run')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('logs-insights-run'));

    await waitFor(() =>
      expect(screen.getByTestId('logs-insights-error')).toBeInTheDocument(),
    );
  });
});
