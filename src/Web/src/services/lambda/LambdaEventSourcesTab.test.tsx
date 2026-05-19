import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { LambdaEventSourcesTab } from './LambdaEventSourcesTab';
import {
  getLambdaEventSourceMappings,
  setLambdaEventSourceMappingState,
  resolveReference,
} from '../../api/client';
import type { LambdaEventSourceMappingItem } from '../../api/client';

vi.mock('../../api/client');

const getMappingsMock = vi.mocked(getLambdaEventSourceMappings);
const setStateMock = vi.mocked(setLambdaEventSourceMappingState);
const resolveReferenceMock = vi.mocked(resolveReference);

const enabledMapping: LambdaEventSourceMappingItem = {
  uuid: 'uuid-1',
  eventSourceArn: 'arn:aws:sqs:us-east-1:000000000000:orders',
  functionArn: 'arn:aws:lambda:us-east-1:000000000000:function:orders',
  state: 'Enabled',
  batchSize: 10,
  lastModified: '2026-01-02T03:04:05.0000000Z',
};

const disabledMapping: LambdaEventSourceMappingItem = {
  uuid: 'uuid-2',
  eventSourceArn: 'arn:aws:dynamodb:us-east-1:000000000000:table/orders/stream/1',
  functionArn: 'arn:aws:lambda:us-east-1:000000000000:function:orders',
  state: 'Disabled',
  batchSize: 5,
  lastModified: '2026-01-03T03:04:05.0000000Z',
};

function renderTab() {
  return render(
    <MemoryRouter>
      <LambdaEventSourcesTab functionName="process-orders" />
    </MemoryRouter>,
  );
}

describe('LambdaEventSourcesTab', () => {
  beforeEach(() => {
    resolveReferenceMock.mockRejectedValue(new Error('unresolved'));
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before mappings arrive', () => {
    getMappingsMock.mockReturnValue(new Promise(() => {}));

    renderTab();

    expect(screen.getByTestId('lambda-event-sources-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getMappingsMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-event-sources-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty state when there are no mappings', async () => {
    getMappingsMock.mockResolvedValue({ mappings: [] });

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-event-sources-empty')).toBeInTheDocument(),
    );
    expect(getMappingsMock).toHaveBeenCalledWith('process-orders', expect.anything());
  });

  it('renders enabled and disabled mappings with their details', async () => {
    getMappingsMock.mockResolvedValue({ mappings: [enabledMapping, disabledMapping] });

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-event-sources-tab')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('lambda-event-source-state-uuid-1')).toHaveTextContent('Enabled');
    expect(screen.getByTestId('lambda-event-source-toggle-uuid-1')).toHaveTextContent('Disable');
    expect(screen.getByTestId('lambda-event-source-state-uuid-2')).toHaveTextContent('Disabled');
    expect(screen.getByTestId('lambda-event-source-toggle-uuid-2')).toHaveTextContent('Enable');
    expect(screen.getByTestId('lambda-event-source-uuid-1')).toHaveTextContent('10');
  });

  it('disables an enabled mapping and reloads', async () => {
    getMappingsMock.mockResolvedValue({ mappings: [enabledMapping] });
    setStateMock.mockResolvedValue();
    const user = userEvent.setup();

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-event-sources-tab')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('lambda-event-source-toggle-uuid-1'));

    await waitFor(() =>
      expect(setStateMock).toHaveBeenCalledWith('process-orders', 'uuid-1', false),
    );
    expect(getMappingsMock).toHaveBeenCalledTimes(2);
  });

  it('enables a disabled mapping and reloads', async () => {
    getMappingsMock.mockResolvedValue({ mappings: [disabledMapping] });
    setStateMock.mockResolvedValue();
    const user = userEvent.setup();

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-event-sources-tab')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('lambda-event-source-toggle-uuid-2'));

    await waitFor(() =>
      expect(setStateMock).toHaveBeenCalledWith('process-orders', 'uuid-2', true),
    );
  });

  it('shows an action error when the toggle fails', async () => {
    getMappingsMock.mockResolvedValue({ mappings: [enabledMapping] });
    setStateMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-event-sources-tab')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('lambda-event-source-toggle-uuid-1'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-event-sources-action-error')).toBeInTheDocument(),
    );
  });
});
