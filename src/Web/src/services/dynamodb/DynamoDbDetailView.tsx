import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { getDynamoDbTable } from '../../api/client';
import type { DynamoDbTableDetail } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { DynamoDbItemsPanel } from './DynamoDbItemsPanel';
import { DynamoDbBatchPanel } from './DynamoDbBatchPanel';
import { DynamoDbIndexesPanel } from './DynamoDbIndexesPanel';
import { DynamoDbQueryPanel } from './DynamoDbQueryPanel';
import { DynamoDbSchemaPanel } from './DynamoDbSchemaPanel';
import { DynamoDbStatementPanel } from './DynamoDbStatementPanel';
import { DynamoDbTransactionPanel } from './DynamoDbTransactionPanel';
import { DynamoDbTtlPanel } from './DynamoDbTtlPanel';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };

type DetailState =
  | { kind: 'loading' }
  | { kind: 'ready'; table: DynamoDbTableDetail }
  | { kind: 'error' };

interface DetailField {
  key: string;
  label: string;
  value: string | number;
}

export function DynamoDbDetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<DetailState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getDynamoDbTable(resourceId, controller.signal)
      .then((table) => setState({ kind: 'ready', table }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, reloadToken]);

  if (state.kind === 'loading') {
    return (
      <p data-testid="dynamodb-detail-loading" style={messageStyle}>
        Loading table&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="dynamodb-detail-error" style={messageStyle}>
        Unable to load this DynamoDB table.
      </p>
    );
  }

  const { table } = state;
  const fields: DetailField[] = [
    { key: 'arn', label: 'ARN', value: table.arn },
    { key: 'status', label: 'Status', value: table.status },
    { key: 'itemCount', label: 'Item count', value: table.itemCount },
    { key: 'sizeBytes', label: 'Size (bytes)', value: table.tableSizeBytes },
    { key: 'billingMode', label: 'Billing mode', value: table.billingMode ?? 'Unknown' },
    {
      key: 'readCapacity',
      label: 'Read capacity units',
      value: table.readCapacityUnits ?? 'On-demand',
    },
    {
      key: 'writeCapacity',
      label: 'Write capacity units',
      value: table.writeCapacityUnits ?? 'On-demand',
    },
    { key: 'createdAt', label: 'Created', value: table.createdAt ?? 'Unknown' },
  ];

  return (
    <div data-testid="dynamodb-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="dynamodb-detail-name" style={{ fontSize: 16 }}>
        {table.name}
      </Heading>
      {fields.map((field) => (
        <div key={field.key} data-testid={`dynamodb-detail-${field.key}`} style={rowStyle}>
          <Text style={labelStyle}>{field.label}</Text>
          <Text style={valueStyle}>{field.value}</Text>
        </div>
      ))}
      <DynamoDbSchemaPanel table={table} />
      <DynamoDbTtlPanel table={table} onUpdated={() => setReloadToken((token) => token + 1)} />
      <DynamoDbIndexesPanel
        tableName={table.name}
        globalSecondaryIndexes={table.globalSecondaryIndexes}
        localSecondaryIndexes={table.localSecondaryIndexes}
        onChanged={() => setReloadToken((token) => token + 1)}
      />
      <DynamoDbItemsPanel tableName={table.name} keySchema={table.keySchema} />
      <DynamoDbBatchPanel tableName={table.name} />
      <DynamoDbQueryPanel
        tableName={table.name}
        indexNames={[
          ...table.globalSecondaryIndexes.map((index) => index.name),
          ...table.localSecondaryIndexes.map((index) => index.name),
        ]}
      />
      <DynamoDbStatementPanel tableName={table.name} />
      <DynamoDbTransactionPanel tableName={table.name} />
    </div>
  );
}

export default DynamoDbDetailView;
