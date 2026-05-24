import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { ResourceLink } from '../../components/ResourceLink';
import type {
  DynamoDbKeyAttribute,
  DynamoDbSecondaryIndex,
  DynamoDbTableDetail,
} from '../../api/client';

const panelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14 };
const headingStyle: CSSProperties = { fontSize: 14 };
const sectionHeadingStyle: CSSProperties = { fontSize: 13 };

function describeKeySchema(keySchema: DynamoDbKeyAttribute[]): string {
  return keySchema.map((key) => `${key.attributeName} (${key.keyType})`).join(', ');
}

function describeStreamView(streamViewType: string | null): string {
  return streamViewType ? ` (${streamViewType})` : '';
}

export function DynamoDbSchemaPanel({ table }: { table: DynamoDbTableDetail }) {
  const renderIndexes = (testId: string, indexes: DynamoDbSecondaryIndex[]) =>
    indexes.map((index) => (
      <div key={index.name} data-testid={`${testId}-${index.name}`} style={rowStyle}>
        <Text style={labelStyle}>
          {index.name}
          {index.status ? ` (${index.status})` : ''}
        </Text>
        <Text style={valueStyle}>{describeKeySchema(index.keySchema)}</Text>
      </div>
    ));

  return (
    <div data-testid="dynamodb-schema-panel" style={panelStyle}>
      <Heading as="h4" style={headingStyle}>
        Schema
      </Heading>
      <div data-testid="dynamodb-schema-key" style={rowStyle}>
        <Text style={labelStyle}>Primary key</Text>
        <Text style={valueStyle}>{describeKeySchema(table.keySchema)}</Text>
      </div>
      {table.attributes.length > 0 ? (
        <div data-testid="dynamodb-schema-attributes" style={rowStyle}>
          <Text style={labelStyle}>Attributes</Text>
          <Text style={valueStyle}>
            {table.attributes
              .map((attribute) => `${attribute.attributeName} (${attribute.attributeType})`)
              .join(', ')}
          </Text>
        </div>
      ) : null}
      {table.globalSecondaryIndexes.length > 0 ? (
        <div data-testid="dynamodb-schema-gsi">
          <Heading as="h5" style={sectionHeadingStyle}>
            Global secondary indexes
          </Heading>
          {renderIndexes('dynamodb-schema-gsi', table.globalSecondaryIndexes)}
        </div>
      ) : null}
      {table.localSecondaryIndexes.length > 0 ? (
        <div data-testid="dynamodb-schema-lsi">
          <Heading as="h5" style={sectionHeadingStyle}>
            Local secondary indexes
          </Heading>
          {renderIndexes('dynamodb-schema-lsi', table.localSecondaryIndexes)}
        </div>
      ) : null}
      <div data-testid="dynamodb-schema-stream" style={rowStyle}>
        <Text style={labelStyle}>Stream</Text>
        {table.latestStreamArn ? (
          <Text style={valueStyle}>
            <ResourceLink reference={table.latestStreamArn} service="dynamodb" />
            {describeStreamView(table.streamViewType)}
          </Text>
        ) : (
          <Text data-testid="dynamodb-schema-stream-status" style={valueStyle}>
            {table.streamEnabled
              ? `Enabled${describeStreamView(table.streamViewType)}`
              : 'Not enabled'}
          </Text>
        )}
      </div>
    </div>
  );
}

export default DynamoDbSchemaPanel;
