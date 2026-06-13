import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import {
  executeDynamoDbBatchGet,
  executeDynamoDbBatchWrite,
} from '../../api/client';
import type {
  DynamoDbBatchGetResult,
  DynamoDbBatchWriteResult,
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

const headingStyle: CSSProperties = { fontSize: 14 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const messageStyle: CSSProperties = { fontSize: 13 };

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
};

const fieldRowStyle: CSSProperties = { display: 'flex', gap: 8 };
const actionsRowStyle: CSSProperties = { display: 'flex', gap: 8, flexWrap: 'wrap' };

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
  minHeight: 60,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
  color: 'inherit',
  resize: 'vertical',
};

const preStyle: CSSProperties = {
  margin: 0,
  fontFamily: 'monospace',
  fontSize: 13,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

type Mode = 'write' | 'get';
type SubmitState = 'idle' | 'saving' | 'error';

interface WriteRow {
  operation: string;
  tableName: string;
  json: string;
}

interface GetRow {
  tableName: string;
  json: string;
}

export interface DynamoDbBatchPanelProps {
  tableName: string;
}

function newWriteRow(tableName: string): WriteRow {
  return { operation: 'Put', tableName, json: '{\n  \n}' };
}

function newGetRow(tableName: string): GetRow {
  return { tableName, json: '{\n  \n}' };
}

export function DynamoDbBatchPanel({ tableName }: DynamoDbBatchPanelProps) {
  const [mode, setMode] = useState<Mode>('write');
  const [writeRows, setWriteRows] = useState<WriteRow[]>(() => [newWriteRow(tableName)]);
  const [getRows, setGetRows] = useState<GetRow[]>(() => [newGetRow(tableName)]);
  const [submitState, setSubmitState] = useState<SubmitState>('idle');
  const [writeResult, setWriteResult] = useState<DynamoDbBatchWriteResult | null>(null);
  const [getResult, setGetResult] = useState<DynamoDbBatchGetResult | null>(null);

  const reset = () => {
    setSubmitState('idle');
    setWriteResult(null);
    setGetResult(null);
  };

  const submitWrite = () => {
    if (writeRows.some((row) => row.tableName.trim().length === 0 || row.json.trim().length === 0)) {
      setSubmitState('error');
      return;
    }
    setSubmitState('saving');
    setWriteResult(null);
    executeDynamoDbBatchWrite(
      writeRows.map((row) => ({
        operation: row.operation,
        tableName: row.tableName.trim(),
        json: row.json,
      })),
    )
      .then((result) => {
        setWriteResult(result);
        setSubmitState('idle');
      })
      .catch(() => setSubmitState('error'));
  };

  const submitGet = () => {
    if (getRows.some((row) => row.tableName.trim().length === 0 || row.json.trim().length === 0)) {
      setSubmitState('error');
      return;
    }
    setSubmitState('saving');
    setGetResult(null);
    executeDynamoDbBatchGet(
      getRows.map((row) => ({ tableName: row.tableName.trim(), json: row.json })),
    )
      .then((result) => {
        setGetResult(result);
        setSubmitState('idle');
      })
      .catch(() => setSubmitState('error'));
  };

  return (
    <div data-testid="dynamodb-batch-panel" style={panelStyle}>
      <Heading as="h4" style={headingStyle}>
        Batch operations
      </Heading>
      <div style={actionsRowStyle}>
        <button
          type="button"
          data-testid="dynamodb-batch-mode-write"
          style={{ ...buttonStyle, fontWeight: mode === 'write' ? 600 : 400 }}
          onClick={() => {
            setMode('write');
            reset();
          }}
        >
          Batch write
        </button>
        <button
          type="button"
          data-testid="dynamodb-batch-mode-get"
          style={{ ...buttonStyle, fontWeight: mode === 'get' ? 600 : 400 }}
          onClick={() => {
            setMode('get');
            reset();
          }}
        >
          Batch get
        </button>
      </div>

      {mode === 'write' ? (
        <div data-testid="dynamodb-batch-write" style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          {writeRows.map((row, index) => (
            <div key={index} data-testid={`dynamodb-batch-write-row-${index}`} style={rowStyle}>
              <div style={fieldRowStyle}>
                <select
                  data-testid={`dynamodb-batch-write-operation-${index}`}
                  style={inputStyle}
                  value={row.operation}
                  disabled={submitState === 'saving'}
                  onChange={(event) =>
                    setWriteRows((current) =>
                      current.map((r, i) => (i === index ? { ...r, operation: event.target.value } : r)),
                    )
                  }
                >
                  <option value="Put">Put</option>
                  <option value="Delete">Delete</option>
                </select>
                <input
                  data-testid={`dynamodb-batch-write-table-${index}`}
                  style={inputStyle}
                  value={row.tableName}
                  disabled={submitState === 'saving'}
                  onChange={(event) =>
                    setWriteRows((current) =>
                      current.map((r, i) => (i === index ? { ...r, tableName: event.target.value } : r)),
                    )
                  }
                />
              </div>
              <textarea
                data-testid={`dynamodb-batch-write-json-${index}`}
                style={textareaStyle}
                value={row.json}
                disabled={submitState === 'saving'}
                onChange={(event) =>
                  setWriteRows((current) =>
                    current.map((r, i) => (i === index ? { ...r, json: event.target.value } : r)),
                  )
                }
              />
              <button
                type="button"
                data-testid={`dynamodb-batch-write-remove-${index}`}
                style={buttonStyle}
                disabled={submitState === 'saving' || writeRows.length === 1}
                onClick={() => setWriteRows((current) => current.filter((_, i) => i !== index))}
              >
                Remove
              </button>
            </div>
          ))}
          <div style={actionsRowStyle}>
            <button
              type="button"
              data-testid="dynamodb-batch-write-add"
              style={buttonStyle}
              disabled={submitState === 'saving'}
              onClick={() => setWriteRows((current) => [...current, newWriteRow(tableName)])}
            >
              Add request
            </button>
            <button
              type="button"
              data-testid="dynamodb-batch-write-submit"
              style={buttonStyle}
              disabled={submitState === 'saving'}
              onClick={submitWrite}
            >
              {submitState === 'saving' ? 'Submitting\u2026' : 'Run batch write'}
            </button>
          </div>
          {writeResult !== null ? (
            <div data-testid="dynamodb-batch-write-result" style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
              <Text style={messageStyle}>
                Submitted {writeResult.requested} request(s);{' '}
                {writeResult.unprocessedItems.length} unprocessed.
              </Text>
              {writeResult.unprocessedItems.map((item, index) => (
                <pre key={index} data-testid={`dynamodb-batch-write-unprocessed-${index}`} style={preStyle}>
                  {item}
                </pre>
              ))}
            </div>
          ) : null}
        </div>
      ) : (
        <div data-testid="dynamodb-batch-get" style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          {getRows.map((row, index) => (
            <div key={index} data-testid={`dynamodb-batch-get-row-${index}`} style={rowStyle}>
              <input
                data-testid={`dynamodb-batch-get-table-${index}`}
                style={inputStyle}
                value={row.tableName}
                disabled={submitState === 'saving'}
                onChange={(event) =>
                  setGetRows((current) =>
                    current.map((r, i) => (i === index ? { ...r, tableName: event.target.value } : r)),
                  )
                }
              />
              <textarea
                data-testid={`dynamodb-batch-get-json-${index}`}
                style={textareaStyle}
                value={row.json}
                disabled={submitState === 'saving'}
                onChange={(event) =>
                  setGetRows((current) =>
                    current.map((r, i) => (i === index ? { ...r, json: event.target.value } : r)),
                  )
                }
              />
              <button
                type="button"
                data-testid={`dynamodb-batch-get-remove-${index}`}
                style={buttonStyle}
                disabled={submitState === 'saving' || getRows.length === 1}
                onClick={() => setGetRows((current) => current.filter((_, i) => i !== index))}
              >
                Remove
              </button>
            </div>
          ))}
          <div style={actionsRowStyle}>
            <button
              type="button"
              data-testid="dynamodb-batch-get-add"
              style={buttonStyle}
              disabled={submitState === 'saving'}
              onClick={() => setGetRows((current) => [...current, newGetRow(tableName)])}
            >
              Add key
            </button>
            <button
              type="button"
              data-testid="dynamodb-batch-get-submit"
              style={buttonStyle}
              disabled={submitState === 'saving'}
              onClick={submitGet}
            >
              {submitState === 'saving' ? 'Submitting\u2026' : 'Run batch get'}
            </button>
          </div>
          {getResult !== null ? (
            <div data-testid="dynamodb-batch-get-result" style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
              <Text style={messageStyle}>
                Requested {getResult.requested} key(s); found {getResult.items.length} item(s).
              </Text>
              {getResult.items.length === 0 ? (
                <Text data-testid="dynamodb-batch-get-empty" style={labelStyle}>
                  No matching items were found.
                </Text>
              ) : (
                getResult.items.map((item, index) => (
                  <pre key={index} data-testid={`dynamodb-batch-get-item-${index}`} style={preStyle}>
                    {item.json}
                  </pre>
                ))
              )}
            </div>
          ) : null}
        </div>
      )}

      {submitState === 'error' ? (
        <Text data-testid="dynamodb-batch-error" style={messageStyle}>
          Batch operation failed. Check each row and try again.
        </Text>
      ) : null}
    </div>
  );
}

export default DynamoDbBatchPanel;
