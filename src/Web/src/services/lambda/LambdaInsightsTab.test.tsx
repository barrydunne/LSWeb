import { afterEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LambdaInsightsTab } from './LambdaInsightsTab';
import { getLambdaInvocationInsights } from '../../api/client';
import type { LambdaInvocationInsightsResult } from '../../api/client';

vi.mock('../../api/client');

const getInsightsMock = vi.mocked(getLambdaInvocationInsights);

const insightsResult: LambdaInvocationInsightsResult = {
  logGroupName: '/aws/lambda/process-orders',
  metrics: {
    invocationCount: 3,
    errorCount: 1,
    averageDurationMs: 20.5,
    maxDurationMs: 40,
  },
  recentInvocations: [
    {
      requestId: 'req-1',
      timestamp: '2026-01-02T03:04:07.0000000+00:00',
      durationMs: 40,
      hasError: true,
    },
    {
      requestId: 'req-2',
      timestamp: '2026-01-02T03:04:06.0000000+00:00',
      durationMs: 10,
      hasError: false,
    },
  ],
};

function renderTab() {
  return render(<LambdaInsightsTab functionName="process-orders" />);
}

describe('LambdaInsightsTab', () => {
  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before insights arrive', () => {
    getInsightsMock.mockReturnValue(new Promise(() => {}));

    renderTab();

    expect(screen.getByTestId('lambda-insights-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getInsightsMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-insights-error')).toBeInTheDocument());
  });

  it('shows an empty state when there are no invocations', async () => {
    getInsightsMock.mockResolvedValue({
      logGroupName: '/aws/lambda/process-orders',
      metrics: { invocationCount: 0, errorCount: 0, averageDurationMs: 0, maxDurationMs: 0 },
      recentInvocations: [],
    });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-insights-empty')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-insights-group')).toHaveTextContent(
      '/aws/lambda/process-orders',
    );
    expect(screen.getByTestId('lambda-insights-metric-count')).toHaveTextContent('0');
    expect(getInsightsMock).toHaveBeenCalledWith('process-orders', undefined, expect.anything());
  });

  it('renders metrics, sparkline and recent invocations when data is ready', async () => {
    getInsightsMock.mockResolvedValue(insightsResult);

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-insights-tab')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-insights-group')).toHaveTextContent(
      '/aws/lambda/process-orders',
    );
    expect(screen.getByTestId('lambda-insights-metric-count')).toHaveTextContent('3');
    expect(screen.getByTestId('lambda-insights-metric-errors')).toHaveTextContent('1');
    expect(screen.getByTestId('lambda-insights-metric-avg')).toHaveTextContent('20.5 ms');
    expect(screen.getByTestId('lambda-insights-metric-max')).toHaveTextContent('40.0 ms');
    expect(screen.getByTestId('lambda-insights-bar-0')).toBeInTheDocument();
    expect(screen.getByTestId('lambda-insights-bar-1')).toBeInTheDocument();
    expect(screen.getByTestId('lambda-insights-invocation-0')).toHaveTextContent('req-1');
    expect(screen.getByTestId('lambda-insights-invocation-0')).toHaveTextContent('Error');
    expect(screen.getByTestId('lambda-insights-invocation-1')).toHaveTextContent('req-2');
    expect(screen.getByTestId('lambda-insights-invocation-1')).toHaveTextContent('OK');
  });

  it('renders flat bars when all durations are zero', async () => {
    getInsightsMock.mockResolvedValue({
      logGroupName: '/aws/lambda/process-orders',
      metrics: { invocationCount: 1, errorCount: 0, averageDurationMs: 0, maxDurationMs: 0 },
      recentInvocations: [
        {
          requestId: 'req-zero',
          timestamp: '2026-01-02T03:04:05.0000000+00:00',
          durationMs: 0,
          hasError: false,
        },
      ],
    });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-insights-tab')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-insights-bar-0')).toBeInTheDocument();
  });

  it('reloads insights when the refresh button is clicked', async () => {
    getInsightsMock.mockResolvedValue(insightsResult);

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-insights-tab')).toBeInTheDocument());
    expect(getInsightsMock).toHaveBeenCalledTimes(1);

    await userEvent.click(screen.getByTestId('lambda-insights-refresh'));

    await waitFor(() => expect(getInsightsMock).toHaveBeenCalledTimes(2));
  });

  it('reloads insights when auto-refresh triggers a refresh', async () => {
    getInsightsMock.mockResolvedValue(insightsResult);

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-insights-tab')).toBeInTheDocument());
    expect(getInsightsMock).toHaveBeenCalledTimes(1);

    vi.useFakeTimers();
    try {
      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      await act(async () => {
        await vi.advanceTimersByTimeAsync(5000);
      });
    } finally {
      vi.useRealTimers();
    }

    expect(getInsightsMock.mock.calls.length).toBeGreaterThan(1);
  });
});
