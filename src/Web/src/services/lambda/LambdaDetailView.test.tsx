import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { LambdaDetailView } from './LambdaDetailView';
import {
  deleteLambdaFunction,
  getLambdaFunction,
  getLambdaEnvironment,
  getLambdaTestEvents,
  getLambdaEventSourceMappings,
  getLambdaLogEvents,
  getLambdaInvocationInsights,
  getLambdaLayers,
  resolveReference,
} from '../../api/client';
import type { LambdaFunctionResult } from '../../api/client';

vi.mock('../../api/client');

const getLambdaFunctionMock = vi.mocked(getLambdaFunction);
const getLambdaEnvironmentMock = vi.mocked(getLambdaEnvironment);
const deleteLambdaFunctionMock = vi.mocked(deleteLambdaFunction);
const getLambdaTestEventsMock = vi.mocked(getLambdaTestEvents);
const getLambdaEventSourceMappingsMock = vi.mocked(getLambdaEventSourceMappings);
const getLambdaLogEventsMock = vi.mocked(getLambdaLogEvents);
const getLambdaInvocationInsightsMock = vi.mocked(getLambdaInvocationInsights);
const getLambdaLayersMock = vi.mocked(getLambdaLayers);
const resolveReferenceMock = vi.mocked(resolveReference);

const functionResult: LambdaFunctionResult = {
  functionName: 'process-orders',
  functionArn: 'arn:aws:lambda:eu-west-1:000000000000:function:process-orders',
  runtime: 'dotnet8',
  handler: 'Orders::Handler',
  description: 'Order processor',
  lastModified: '2026-01-02T03:04:05Z',
  memorySize: 256,
  timeout: 30,
  role: 'arn:aws:iam::000000000000:role/lambda-orders',
};

function renderView() {
  return render(
    <MemoryRouter>
      <LambdaDetailView serviceKey="lambda" resourceId="process-orders" />
    </MemoryRouter>,
  );
}

describe('LambdaDetailView', () => {
  beforeEach(() => {
    getLambdaFunctionMock.mockResolvedValue(functionResult);
    getLambdaEnvironmentMock.mockResolvedValue({ variables: [], revealAllowed: false });
    getLambdaTestEventsMock.mockResolvedValue({ events: [], templates: [] });
    getLambdaEventSourceMappingsMock.mockResolvedValue({ mappings: [], s3Triggers: [] });
    getLambdaLogEventsMock.mockResolvedValue({ logGroupName: '/aws/lambda/process-orders', events: [] });
    getLambdaInvocationInsightsMock.mockResolvedValue({
      logGroupName: '/aws/lambda/process-orders',
      metrics: { invocationCount: 0, errorCount: 0, averageDurationMs: 0, maxDurationMs: 0 },
      recentInvocations: [],
    });
    resolveReferenceMock.mockRejectedValue(new Error('unresolved'));
    getLambdaLayersMock.mockResolvedValue({ layers: [] });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the function arrives', () => {
    getLambdaFunctionMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('lambda-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getLambdaFunctionMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-error')).toBeInTheDocument());
  });

  it('requests the function by id and renders its fields', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    expect(getLambdaFunctionMock).toHaveBeenCalledWith('process-orders', expect.anything());
    expect(screen.getByTestId('lambda-detail-name')).toHaveTextContent('process-orders');
    expect(screen.getByTestId('lambda-detail-arn')).toHaveTextContent(functionResult.functionArn);
    expect(screen.getByTestId('lambda-detail-runtime')).toHaveTextContent('dotnet8');
    expect(screen.getByTestId('lambda-detail-handler')).toHaveTextContent('Orders::Handler');
    expect(screen.getByTestId('lambda-detail-memory')).toHaveTextContent('256');
    expect(screen.getByTestId('lambda-detail-timeout')).toHaveTextContent('30');
    expect(screen.getByTestId('lambda-detail-role')).toHaveTextContent('lambda-orders');
  });

  it('switches between the overview and environment tabs', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    await user.click(screen.getByTestId('lambda-detail-tab-environment'));
    await waitFor(() => expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument());
    expect(screen.queryByTestId('lambda-detail-arn')).not.toBeInTheDocument();

    await user.click(screen.getByTestId('lambda-detail-tab-overview'));
    expect(screen.getByTestId('lambda-detail-arn')).toBeInTheDocument();
  });

  it('switches to the test tab', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    await user.click(screen.getByTestId('lambda-detail-tab-test'));
    await waitFor(() => expect(screen.getByTestId('lambda-test-tab')).toBeInTheDocument());
    expect(screen.queryByTestId('lambda-detail-arn')).not.toBeInTheDocument();
  });

  it('switches to the event sources tab', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    await user.click(screen.getByTestId('lambda-detail-tab-eventsources'));
    await waitFor(() =>
      expect(screen.getByTestId('lambda-event-sources-empty')).toBeInTheDocument(),
    );
    expect(getLambdaEventSourceMappingsMock).toHaveBeenCalledWith(
      'process-orders',
      expect.anything(),
    );
    expect(screen.queryByTestId('lambda-detail-arn')).not.toBeInTheDocument();
  });

  it('switches to the logs tab', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    await user.click(screen.getByTestId('lambda-detail-tab-logs'));
    await waitFor(() => expect(screen.getByTestId('lambda-logs-empty')).toBeInTheDocument());
    expect(getLambdaLogEventsMock).toHaveBeenCalledWith(
      'process-orders',
      undefined,
      expect.anything(),
    );
    expect(screen.queryByTestId('lambda-detail-arn')).not.toBeInTheDocument();
  });

  it('switches to the insights tab', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    await user.click(screen.getByTestId('lambda-detail-tab-insights'));
    await waitFor(() => expect(screen.getByTestId('lambda-insights-tab')).toBeInTheDocument());
    expect(getLambdaInvocationInsightsMock).toHaveBeenCalledWith(
      'process-orders',
      undefined,
      expect.anything(),
    );
    expect(screen.queryByTestId('lambda-detail-arn')).not.toBeInTheDocument();
  });

  it('switches to the layers tab', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    await user.click(screen.getByTestId('lambda-detail-tab-layers'));
    await waitFor(() => expect(screen.getByTestId('lambda-layers-empty')).toBeInTheDocument());
    expect(getLambdaLayersMock).toHaveBeenCalledWith('process-orders', expect.anything());
    expect(screen.queryByTestId('lambda-detail-arn')).not.toBeInTheDocument();
  });

  it('deletes the function after confirmation', async () => {
    deleteLambdaFunctionMock.mockResolvedValue();
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-detail-delete-status')).toBeInTheDocument(),
    );
    expect(deleteLambdaFunctionMock).toHaveBeenCalledWith('process-orders');
  });

  it('shows an error when deletion fails', async () => {
    deleteLambdaFunctionMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-detail-view')).toBeInTheDocument());

    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-detail-delete-error')).toBeInTheDocument(),
    );
  });
});
