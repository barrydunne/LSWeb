import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { DynamoDbDetailView } from './DynamoDbDetailView';
import { getDynamoDbTable } from '../../api/client';
import type { DynamoDbTableDetail } from '../../api/client';

vi.mock('../../api/client');

vi.mock('./DynamoDbItemsPanel', () => ({
  DynamoDbItemsPanel: ({ tableName }: { tableName: string }) => (
    <div data-testid="dynamodb-items-panel-stub" data-table={tableName} />
  ),
}));

vi.mock('./DynamoDbQueryPanel', () => ({
  DynamoDbQueryPanel: ({ tableName, indexNames }: { tableName: string; indexNames: string[] }) => (
    <div
      data-testid="dynamodb-query-panel-stub"
      data-table={tableName}
      data-indexes={indexNames.join(',')}
    />
  ),
}));

vi.mock('./DynamoDbStatementPanel', () => ({
  DynamoDbStatementPanel: ({ tableName }: { tableName: string }) => (
    <div data-testid="dynamodb-statement-panel-stub" data-table={tableName} />
  ),
}));

vi.mock('./DynamoDbSchemaPanel', () => ({
  DynamoDbSchemaPanel: ({ table }: { table: { name: string } }) => (
    <div data-testid="dynamodb-schema-panel-stub" data-table={table.name} />
  ),
}));

const getDynamoDbTableMock = vi.mocked(getDynamoDbTable);

const fullDetail: DynamoDbTableDetail = {
  name: 'orders',
  arn: 'arn:orders',
  status: 'ACTIVE',
  itemCount: 5,
  tableSizeBytes: 1024,
  billingMode: 'PAY_PER_REQUEST',
  readCapacityUnits: 10,
  writeCapacityUnits: 20,
  createdAt: '2026-01-02T03:04:05Z',
  keySchema: [{ attributeName: 'id', keyType: 'HASH' }],
  attributes: [{ attributeName: 'id', attributeType: 'S' }],
  globalSecondaryIndexes: [
    { name: 'gsi-1', status: 'ACTIVE', keySchema: [{ attributeName: 'gid', keyType: 'HASH' }] },
  ],
  localSecondaryIndexes: [
    { name: 'lsi-1', status: null, keySchema: [{ attributeName: 'lid', keyType: 'RANGE' }] },
  ],
  streamEnabled: true,
  streamViewType: 'NEW_AND_OLD_IMAGES',
  latestStreamArn: 'arn:orders/stream/2024',
};

const minimalDetail: DynamoDbTableDetail = {
  name: 'minimal',
  arn: 'arn:minimal',
  status: 'CREATING',
  itemCount: 0,
  tableSizeBytes: 0,
  billingMode: null,
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
};

function renderView() {
  return render(<DynamoDbDetailView serviceKey="dynamodb" resourceId="orders" />);
}

describe('DynamoDbDetailView', () => {
  beforeEach(() => {
    getDynamoDbTableMock.mockResolvedValue(fullDetail);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the table arrives', () => {
    getDynamoDbTableMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('dynamodb-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getDynamoDbTableMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('dynamodb-detail-error')).toBeInTheDocument());
  });

  it('renders the table metadata and delegates schema rendering to the schema panel', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('dynamodb-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('dynamodb-detail-name')).toHaveTextContent('orders');
    expect(screen.getByTestId('dynamodb-detail-arn')).toHaveTextContent('arn:orders');
    expect(screen.getByTestId('dynamodb-detail-status')).toHaveTextContent('ACTIVE');
    expect(screen.getByTestId('dynamodb-detail-itemCount')).toHaveTextContent('5');
    expect(screen.getByTestId('dynamodb-detail-sizeBytes')).toHaveTextContent('1024');
    expect(screen.getByTestId('dynamodb-detail-billingMode')).toHaveTextContent('PAY_PER_REQUEST');
    expect(screen.getByTestId('dynamodb-detail-readCapacity')).toHaveTextContent('10');
    expect(screen.getByTestId('dynamodb-detail-writeCapacity')).toHaveTextContent('20');
    expect(screen.getByTestId('dynamodb-schema-panel-stub')).toHaveAttribute('data-table', 'orders');
  });

  it('renders the items panel for the table', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('dynamodb-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('dynamodb-items-panel-stub')).toHaveAttribute('data-table', 'orders');
  });

  it('renders the query panel with the table index names', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('dynamodb-detail-view')).toBeInTheDocument());

    const panel = screen.getByTestId('dynamodb-query-panel-stub');
    expect(panel).toHaveAttribute('data-table', 'orders');
    expect(panel).toHaveAttribute('data-indexes', 'gsi-1,lsi-1');
  });

  it('renders the PartiQL statement panel for the table', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('dynamodb-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('dynamodb-statement-panel-stub')).toHaveAttribute(
      'data-table',
      'orders',
    );
  });

  it('renders fallbacks when optional values are absent', async () => {
    getDynamoDbTableMock.mockResolvedValue(minimalDetail);

    renderView();

    await waitFor(() => expect(screen.getByTestId('dynamodb-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('dynamodb-detail-billingMode')).toHaveTextContent('Unknown');
    expect(screen.getByTestId('dynamodb-detail-readCapacity')).toHaveTextContent('On-demand');
    expect(screen.getByTestId('dynamodb-detail-writeCapacity')).toHaveTextContent('On-demand');
    expect(screen.getByTestId('dynamodb-detail-createdAt')).toHaveTextContent('Unknown');
    expect(screen.getByTestId('dynamodb-schema-panel-stub')).toHaveAttribute('data-table', 'minimal');
  });
});
