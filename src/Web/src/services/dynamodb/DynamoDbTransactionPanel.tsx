import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { executeDynamoDbTransaction } from '../../api/client';

const panelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const headingStyle: CSSProperties = { fontSize: 14 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const messageStyle: CSSProperties = { fontSize: 13 };

const actionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
};

const fieldRowStyle: CSSProperties = { display: 'flex', gap: 8 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
  color: 'inherit',
  flex: 1,
};

const textareaStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 13,
  minHeight: 70,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
  color: 'inherit',
  resize: 'vertical',
};

const actionsRowStyle: CSSProperties = { display: 'flex', gap: 8, flexWrap: 'wrap' };

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

interface ActionRow {
  operation: string;
  tableName: string;
  json: string;
}

type SubmitState = 'idle' | 'saving' | 'success' | 'error';

export interface DynamoDbTransactionPanelProps {
  tableName: string;
}

function newRow(tableName: string): ActionRow {
  return { operation: 'Put', tableName, json: '{\n  \n}' };
}

export function DynamoDbTransactionPanel({ tableName }: DynamoDbTransactionPanelProps) {
  const [rows, setRows] = useState<ActionRow[]>(() => [newRow(tableName)]);
  const [submitState, setSubmitState] = useState<SubmitState>('idle');

  const updateRow = (index: number, patch: Partial<ActionRow>) => {
    setRows((current) => current.map((row, i) => (i === index ? { ...row, ...patch } : row)));
    setSubmitState('idle');
  };

  const addRow = () => {
    setRows((current) => [...current, newRow(tableName)]);
    setSubmitState('idle');
  };

  const removeRow = (index: number) => {
    setRows((current) => current.filter((_, i) => i !== index));
    setSubmitState('idle');
  };

  const submit = () => {
    if (rows.some((row) => row.tableName.trim().length === 0 || row.json.trim().length === 0)) {
      setSubmitState('error');
      return;
    }
    setSubmitState('saving');
    executeDynamoDbTransaction(
      rows.map((row) => ({
        operation: row.operation,
        tableName: row.tableName.trim(),
        json: row.json,
      })),
    )
      .then(() => setSubmitState('success'))
      .catch(() => setSubmitState('error'));
  };

  return (
    <div data-testid="dynamodb-transaction-panel" style={panelStyle}>
      <Heading as="h4" style={headingStyle}>
        Transactional write
      </Heading>
      <Text style={labelStyle}>
        All actions are applied atomically: either every action succeeds, or none are applied.
      </Text>

      {rows.map((row, index) => (
        <div key={index} data-testid={`dynamodb-transaction-action-${index}`} style={actionStyle}>
          <div style={fieldRowStyle}>
            <select
              data-testid={`dynamodb-transaction-operation-${index}`}
              style={inputStyle}
              value={row.operation}
              disabled={submitState === 'saving'}
              onChange={(event) => updateRow(index, { operation: event.target.value })}
            >
              <option value="Put">Put</option>
              <option value="Delete">Delete</option>
            </select>
            <input
              data-testid={`dynamodb-transaction-table-${index}`}
              style={inputStyle}
              value={row.tableName}
              disabled={submitState === 'saving'}
              onChange={(event) => updateRow(index, { tableName: event.target.value })}
            />
          </div>
          <textarea
            data-testid={`dynamodb-transaction-json-${index}`}
            style={textareaStyle}
            value={row.json}
            disabled={submitState === 'saving'}
            onChange={(event) => updateRow(index, { json: event.target.value })}
          />
          <button
            type="button"
            data-testid={`dynamodb-transaction-remove-${index}`}
            style={buttonStyle}
            disabled={submitState === 'saving' || rows.length === 1}
            onClick={() => removeRow(index)}
          >
            Remove
          </button>
        </div>
      ))}

      <div style={actionsRowStyle}>
        <button
          type="button"
          data-testid="dynamodb-transaction-add"
          style={buttonStyle}
          disabled={submitState === 'saving'}
          onClick={addRow}
        >
          Add action
        </button>
        <button
          type="button"
          data-testid="dynamodb-transaction-submit"
          style={buttonStyle}
          disabled={submitState === 'saving'}
          onClick={submit}
        >
          {submitState === 'saving' ? 'Submitting\u2026' : 'Submit transaction'}
        </button>
      </div>

      {submitState === 'success' ? (
        <Text data-testid="dynamodb-transaction-success" style={messageStyle}>
          Transaction committed successfully.
        </Text>
      ) : null}
      {submitState === 'error' ? (
        <Text data-testid="dynamodb-transaction-error" style={messageStyle}>
          Transaction failed. No actions were applied. Check each action and try again.
        </Text>
      ) : null}
    </div>
  );
}

export default DynamoDbTransactionPanel;
