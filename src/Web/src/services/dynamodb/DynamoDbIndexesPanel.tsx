import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createDynamoDbIndex, deleteDynamoDbIndex } from '../../api/client';
import type { DynamoDbSecondaryIndex } from '../../api/client';

const panelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const sectionStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 8 };
const rowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 8,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
};

const headingStyle: CSSProperties = { fontSize: 14 };
const sectionHeadingStyle: CSSProperties = { fontSize: 13 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 13 };
const messageStyle: CSSProperties = { fontSize: 13 };

const formStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 8 };
const fieldStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 4 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

type SaveState = 'idle' | 'saving' | 'error';

export interface DynamoDbIndexesPanelProps {
  tableName: string;
  globalSecondaryIndexes: DynamoDbSecondaryIndex[];
  localSecondaryIndexes: DynamoDbSecondaryIndex[];
  onChanged: () => void;
}

function describeKeySchema(index: DynamoDbSecondaryIndex): string {
  return index.keySchema.map((key) => `${key.attributeName} (${key.keyType})`).join(', ');
}

export function DynamoDbIndexesPanel({
  tableName,
  globalSecondaryIndexes,
  localSecondaryIndexes,
  onChanged,
}: DynamoDbIndexesPanelProps) {
  const [indexName, setIndexName] = useState('');
  const [partitionKeyName, setPartitionKeyName] = useState('');
  const [partitionKeyType, setPartitionKeyType] = useState('S');
  const [sortKeyName, setSortKeyName] = useState('');
  const [sortKeyType, setSortKeyType] = useState('S');
  const [projectionType, setProjectionType] = useState('ALL');
  const [saveState, setSaveState] = useState<SaveState>('idle');

  const create = () => {
    if (indexName.trim().length === 0 || partitionKeyName.trim().length === 0) {
      setSaveState('error');
      return;
    }
    setSaveState('saving');
    const sortName = sortKeyName.trim();
    createDynamoDbIndex(tableName, {
      indexName: indexName.trim(),
      partitionKeyName: partitionKeyName.trim(),
      partitionKeyType,
      sortKeyName: sortName.length > 0 ? sortName : null,
      sortKeyType: sortName.length > 0 ? sortKeyType : null,
      projectionType,
    })
      .then(() => {
        setIndexName('');
        setPartitionKeyName('');
        setSortKeyName('');
        setSaveState('idle');
        onChanged();
      })
      .catch(() => setSaveState('error'));
  };

  const remove = (name: string) => {
    deleteDynamoDbIndex(tableName, name)
      .then(() => onChanged())
      .catch(() => setSaveState('error'));
  };

  return (
    <div data-testid="dynamodb-indexes-panel" style={panelStyle}>
      <Heading as="h4" style={headingStyle}>
        Secondary indexes
      </Heading>

      <div data-testid="dynamodb-indexes-gsi" style={sectionStyle}>
        <Heading as="h5" style={sectionHeadingStyle}>
          Global secondary indexes
        </Heading>
        {globalSecondaryIndexes.length === 0 ? (
          <Text data-testid="dynamodb-indexes-gsi-empty" style={messageStyle}>
            No global secondary indexes.
          </Text>
        ) : (
          globalSecondaryIndexes.map((index) => (
            <div key={index.name} data-testid={`dynamodb-indexes-gsi-${index.name}`} style={rowStyle}>
              <div>
                <Text style={valueStyle}>
                  {index.name}
                  {index.status ? ` (${index.status})` : ''}
                </Text>
                <Text style={labelStyle}> {describeKeySchema(index)}</Text>
              </div>
              <ConfirmationHost
                actionLabel="Delete"
                prompt={`Delete index ${index.name}?`}
                confirmLabel="Confirm"
                onConfirm={() => remove(index.name)}
              />
            </div>
          ))
        )}
      </div>

      <div data-testid="dynamodb-indexes-lsi" style={sectionStyle}>
        <Heading as="h5" style={sectionHeadingStyle}>
          Local secondary indexes
        </Heading>
        {localSecondaryIndexes.length === 0 ? (
          <Text data-testid="dynamodb-indexes-lsi-empty" style={messageStyle}>
            No local secondary indexes.
          </Text>
        ) : (
          localSecondaryIndexes.map((index) => (
            <div key={index.name} data-testid={`dynamodb-indexes-lsi-${index.name}`} style={rowStyle}>
              <div>
                <Text style={valueStyle}>{index.name}</Text>
                <Text style={labelStyle}> {describeKeySchema(index)}</Text>
              </div>
            </div>
          ))
        )}
        <Text data-testid="dynamodb-indexes-lsi-note" style={labelStyle}>
          Local secondary indexes can only be defined when the table is created.
        </Text>
      </div>

      <div data-testid="dynamodb-indexes-create" style={formStyle}>
        <Heading as="h5" style={sectionHeadingStyle}>
          Add global secondary index
        </Heading>
        <div style={fieldStyle}>
          <label style={labelStyle} htmlFor="dynamodb-index-name">
            Index name
          </label>
          <input
            id="dynamodb-index-name"
            data-testid="dynamodb-index-name"
            style={inputStyle}
            value={indexName}
            disabled={saveState === 'saving'}
            onChange={(event) => setIndexName(event.target.value)}
          />
        </div>
        <div style={fieldStyle}>
          <label style={labelStyle} htmlFor="dynamodb-index-pk-name">
            Partition key name
          </label>
          <input
            id="dynamodb-index-pk-name"
            data-testid="dynamodb-index-pk-name"
            style={inputStyle}
            value={partitionKeyName}
            disabled={saveState === 'saving'}
            onChange={(event) => setPartitionKeyName(event.target.value)}
          />
        </div>
        <div style={fieldStyle}>
          <label style={labelStyle} htmlFor="dynamodb-index-pk-type">
            Partition key type
          </label>
          <select
            id="dynamodb-index-pk-type"
            data-testid="dynamodb-index-pk-type"
            style={inputStyle}
            value={partitionKeyType}
            disabled={saveState === 'saving'}
            onChange={(event) => setPartitionKeyType(event.target.value)}
          >
            <option value="S">String (S)</option>
            <option value="N">Number (N)</option>
            <option value="B">Binary (B)</option>
          </select>
        </div>
        <div style={fieldStyle}>
          <label style={labelStyle} htmlFor="dynamodb-index-sk-name">
            Sort key name (optional)
          </label>
          <input
            id="dynamodb-index-sk-name"
            data-testid="dynamodb-index-sk-name"
            style={inputStyle}
            value={sortKeyName}
            disabled={saveState === 'saving'}
            onChange={(event) => setSortKeyName(event.target.value)}
          />
        </div>
        <div style={fieldStyle}>
          <label style={labelStyle} htmlFor="dynamodb-index-sk-type">
            Sort key type
          </label>
          <select
            id="dynamodb-index-sk-type"
            data-testid="dynamodb-index-sk-type"
            style={inputStyle}
            value={sortKeyType}
            disabled={saveState === 'saving'}
            onChange={(event) => setSortKeyType(event.target.value)}
          >
            <option value="S">String (S)</option>
            <option value="N">Number (N)</option>
            <option value="B">Binary (B)</option>
          </select>
        </div>
        <div style={fieldStyle}>
          <label style={labelStyle} htmlFor="dynamodb-index-projection">
            Projection
          </label>
          <select
            id="dynamodb-index-projection"
            data-testid="dynamodb-index-projection"
            style={inputStyle}
            value={projectionType}
            disabled={saveState === 'saving'}
            onChange={(event) => setProjectionType(event.target.value)}
          >
            <option value="ALL">All attributes (ALL)</option>
            <option value="KEYS_ONLY">Keys only (KEYS_ONLY)</option>
          </select>
        </div>
        <button
          type="button"
          data-testid="dynamodb-index-create-submit"
          style={buttonStyle}
          disabled={saveState === 'saving'}
          onClick={create}
        >
          {saveState === 'saving' ? 'Creating\u2026' : 'Create index'}
        </button>
        {saveState === 'error' ? (
          <Text data-testid="dynamodb-indexes-error" style={messageStyle}>
            Unable to update indexes. Check the index details and try again.
          </Text>
        ) : null}
      </div>
    </div>
  );
}

export default DynamoDbIndexesPanel;
