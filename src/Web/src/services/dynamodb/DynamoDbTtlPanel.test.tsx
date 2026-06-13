import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { DynamoDbTtlPanel } from './DynamoDbTtlPanel';
import { updateDynamoDbTtl } from '../../api/client';
import type { DynamoDbTableDetail } from '../../api/client';

vi.mock('../../api/client');

const updateDynamoDbTtlMock = vi.mocked(updateDynamoDbTtl);

function buildTable(overrides: Partial<DynamoDbTableDetail> = {}): DynamoDbTableDetail {
  return {
    name: 'orders',
    arn: 'arn:orders',
    status: 'ACTIVE',
    itemCount: 0,
    tableSizeBytes: 0,
    billingMode: 'PAY_PER_REQUEST',
    readCapacityUnits: null,
    writeCapacityUnits: null,
    createdAt: null,
    keySchema: [{ attributeName: 'pk', keyType: 'HASH' }],
    attributes: [],
    globalSecondaryIndexes: [],
    localSecondaryIndexes: [],
    streamEnabled: false,
    streamViewType: null,
    latestStreamArn: null,
    ttlStatus: null,
    ttlAttributeName: null,
    ...overrides,
  };
}

function renderPanel(table: DynamoDbTableDetail, onUpdated = vi.fn()) {
  render(
    <ThemeProvider colorMode="night">
      <DynamoDbTtlPanel table={table} onUpdated={onUpdated} />
    </ThemeProvider>,
  );
  return onUpdated;
}

describe('DynamoDbTtlPanel', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows "Not configured" and "None" when TTL is not set', () => {
    renderPanel(buildTable());

    expect(screen.getByTestId('dynamodb-ttl-status')).toHaveTextContent('Not configured');
    expect(screen.getByTestId('dynamodb-ttl-attribute')).toHaveTextContent('None');
  });

  it('shows the current status and attribute when TTL is configured', () => {
    renderPanel(buildTable({ ttlStatus: 'ENABLED', ttlAttributeName: 'expiresAt' }));

    expect(screen.getByTestId('dynamodb-ttl-status')).toHaveTextContent('ENABLED');
    expect(screen.getByTestId('dynamodb-ttl-attribute')).toHaveTextContent('expiresAt');
    expect(screen.getByTestId('dynamodb-ttl-input')).toHaveValue('expiresAt');
  });

  it('enables TTL and notifies the parent when the update succeeds', async () => {
    updateDynamoDbTtlMock.mockResolvedValue();
    const onUpdated = renderPanel(buildTable());

    fireEvent.change(screen.getByTestId('dynamodb-ttl-input'), { target: { value: 'expiresAt' } });
    fireEvent.click(screen.getByTestId('dynamodb-ttl-enable'));

    await waitFor(() => expect(onUpdated).toHaveBeenCalledTimes(1));
    expect(updateDynamoDbTtlMock).toHaveBeenCalledWith('orders', true, 'expiresAt');
  });

  it('disables TTL when the disable button is clicked', async () => {
    updateDynamoDbTtlMock.mockResolvedValue();
    const onUpdated = renderPanel(buildTable({ ttlAttributeName: 'expiresAt' }));

    fireEvent.click(screen.getByTestId('dynamodb-ttl-disable'));

    await waitFor(() => expect(onUpdated).toHaveBeenCalledTimes(1));
    expect(updateDynamoDbTtlMock).toHaveBeenCalledWith('orders', false, 'expiresAt');
  });

  it('shows an error and does not call the API when the attribute name is blank', () => {
    const onUpdated = renderPanel(buildTable());

    fireEvent.change(screen.getByTestId('dynamodb-ttl-input'), { target: { value: '   ' } });
    fireEvent.click(screen.getByTestId('dynamodb-ttl-enable'));

    expect(screen.getByTestId('dynamodb-ttl-error')).toBeInTheDocument();
    expect(updateDynamoDbTtlMock).not.toHaveBeenCalled();
    expect(onUpdated).not.toHaveBeenCalled();
  });

  it('shows an error when the update fails', async () => {
    updateDynamoDbTtlMock.mockRejectedValue(new Error('boom'));
    const onUpdated = renderPanel(buildTable({ ttlAttributeName: 'expiresAt' }));

    fireEvent.click(screen.getByTestId('dynamodb-ttl-enable'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-ttl-error')).toBeInTheDocument());
    expect(onUpdated).not.toHaveBeenCalled();
  });
});
