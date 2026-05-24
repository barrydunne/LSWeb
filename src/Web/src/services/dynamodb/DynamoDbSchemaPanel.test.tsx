import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { DynamoDbSchemaPanel } from './DynamoDbSchemaPanel';
import type { DynamoDbTableDetail } from '../../api/client';

vi.mock('../../components/ResourceLink', () => ({
  ResourceLink: ({ reference, service }: { reference: string; service?: string }) => (
    <a data-testid="resource-link" data-reference={reference} data-service={service}>
      {reference}
    </a>
  ),
}));

const fullTable: DynamoDbTableDetail = {
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

const enabledWithoutArn: DynamoDbTableDetail = {
  ...fullTable,
  attributes: [],
  globalSecondaryIndexes: [],
  localSecondaryIndexes: [],
  streamEnabled: true,
  streamViewType: 'KEYS_ONLY',
  latestStreamArn: null,
};

const disabledStream: DynamoDbTableDetail = {
  ...fullTable,
  attributes: [],
  globalSecondaryIndexes: [],
  localSecondaryIndexes: [],
  streamEnabled: false,
  streamViewType: null,
  latestStreamArn: null,
};

const streamWithoutViewType: DynamoDbTableDetail = {
  ...fullTable,
  attributes: [],
  globalSecondaryIndexes: [],
  localSecondaryIndexes: [],
  streamEnabled: true,
  streamViewType: null,
  latestStreamArn: 'arn:orders/stream/2025',
};

describe('DynamoDbSchemaPanel', () => {
  it('renders the primary key and attribute definitions', () => {
    render(<DynamoDbSchemaPanel table={fullTable} />);

    expect(screen.getByTestId('dynamodb-schema-key')).toHaveTextContent('id (HASH)');
    expect(screen.getByTestId('dynamodb-schema-attributes')).toHaveTextContent('id (S)');
  });

  it('renders global and local secondary indexes', () => {
    render(<DynamoDbSchemaPanel table={fullTable} />);

    expect(screen.getByTestId('dynamodb-schema-gsi-gsi-1')).toHaveTextContent('gsi-1 (ACTIVE)');
    expect(screen.getByTestId('dynamodb-schema-gsi-gsi-1')).toHaveTextContent('gid (HASH)');
    expect(screen.getByTestId('dynamodb-schema-lsi-lsi-1')).toHaveTextContent('lsi-1');
    expect(screen.getByTestId('dynamodb-schema-lsi-lsi-1')).toHaveTextContent('lid (RANGE)');
  });

  it('renders a link to the latest stream with its view type', () => {
    render(<DynamoDbSchemaPanel table={fullTable} />);

    const link = screen.getByTestId('resource-link');
    expect(link).toHaveAttribute('data-reference', 'arn:orders/stream/2024');
    expect(link).toHaveAttribute('data-service', 'dynamodb');
    expect(screen.getByTestId('dynamodb-schema-stream')).toHaveTextContent('(NEW_AND_OLD_IMAGES)');
  });

  it('renders the stream link without a view type suffix when none is reported', () => {
    render(<DynamoDbSchemaPanel table={streamWithoutViewType} />);

    const link = screen.getByTestId('resource-link');
    expect(link).toHaveAttribute('data-reference', 'arn:orders/stream/2025');
    expect(screen.getByTestId('dynamodb-schema-stream')).not.toHaveTextContent('(');
  });

  it('shows the enabled state with view type when no stream arn is reported', () => {
    render(<DynamoDbSchemaPanel table={enabledWithoutArn} />);

    expect(screen.queryByTestId('resource-link')).not.toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-schema-stream-status')).toHaveTextContent(
      'Enabled (KEYS_ONLY)',
    );
  });

  it('shows a not-enabled state when streaming is disabled', () => {
    render(<DynamoDbSchemaPanel table={disabledStream} />);

    expect(screen.queryByTestId('resource-link')).not.toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-schema-stream-status')).toHaveTextContent('Not enabled');
  });

  it('omits optional sections when there are no attributes or indexes', () => {
    render(<DynamoDbSchemaPanel table={disabledStream} />);

    expect(screen.queryByTestId('dynamodb-schema-attributes')).not.toBeInTheDocument();
    expect(screen.queryByTestId('dynamodb-schema-gsi')).not.toBeInTheDocument();
    expect(screen.queryByTestId('dynamodb-schema-lsi')).not.toBeInTheDocument();
  });
});
