import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { CloudWatchLogsDetailView } from './CloudWatchLogsDetailView';
import { getLogEvents, getLogStreams } from '../../api/client';
import type { LogEventListResult, LogStreamListResult } from '../../api/client';
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
  });

  afterEach(() => {
    vi.resetAllMocks();
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
});
