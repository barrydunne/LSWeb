import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { updateDynamoDbTtl } from '../../api/client';
import type { DynamoDbTableDetail } from '../../api/client';

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
const messageStyle: CSSProperties = { fontSize: 13 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
  color: 'inherit',
};

const actionsStyle: CSSProperties = { display: 'flex', gap: 8 };

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

type SaveState = 'idle' | 'saving' | 'error';

export interface DynamoDbTtlPanelProps {
  table: DynamoDbTableDetail;
  onUpdated: () => void;
}

function describeStatus(status: string | null): string {
  if (!status) {
    return 'Not configured';
  }
  return status;
}

export function DynamoDbTtlPanel({ table, onUpdated }: DynamoDbTtlPanelProps) {
  const [attributeName, setAttributeName] = useState(table.ttlAttributeName ?? '');
  const [saveState, setSaveState] = useState<SaveState>('idle');

  const apply = (enabled: boolean) => {
    const trimmed = attributeName.trim();
    if (trimmed.length === 0) {
      setSaveState('error');
      return;
    }
    setSaveState('saving');
    updateDynamoDbTtl(table.name, enabled, trimmed)
      .then(() => {
        setSaveState('idle');
        onUpdated();
      })
      .catch(() => setSaveState('error'));
  };

  return (
    <div data-testid="dynamodb-ttl-panel" style={panelStyle}>
      <Heading as="h4" style={headingStyle}>
        Time to live (TTL)
      </Heading>
      <div data-testid="dynamodb-ttl-status" style={rowStyle}>
        <Text style={labelStyle}>Status</Text>
        <Text style={valueStyle}>{describeStatus(table.ttlStatus)}</Text>
      </div>
      <div data-testid="dynamodb-ttl-attribute" style={rowStyle}>
        <Text style={labelStyle}>Attribute</Text>
        <Text style={valueStyle}>{table.ttlAttributeName ?? 'None'}</Text>
      </div>
      <div style={formStyle}>
        <label style={labelStyle} htmlFor="dynamodb-ttl-input">
          TTL attribute name
        </label>
        <input
          id="dynamodb-ttl-input"
          data-testid="dynamodb-ttl-input"
          style={inputStyle}
          value={attributeName}
          disabled={saveState === 'saving'}
          onChange={(event) => setAttributeName(event.target.value)}
        />
        <div style={actionsStyle}>
          <button
            type="button"
            data-testid="dynamodb-ttl-enable"
            style={buttonStyle}
            disabled={saveState === 'saving'}
            onClick={() => apply(true)}
          >
            Enable TTL
          </button>
          <button
            type="button"
            data-testid="dynamodb-ttl-disable"
            style={buttonStyle}
            disabled={saveState === 'saving'}
            onClick={() => apply(false)}
          >
            Disable TTL
          </button>
        </div>
        {saveState === 'error' ? (
          <Text data-testid="dynamodb-ttl-error" style={messageStyle}>
            Unable to update TTL. Provide an attribute name and try again.
          </Text>
        ) : null}
      </div>
    </div>
  );
}

export default DynamoDbTtlPanel;
