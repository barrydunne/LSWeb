import { useCallback, useEffect, useRef, useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import { getLambdaEnvironment, updateLambdaEnvironment } from '../../api/client';
import type { LambdaEnvironmentResult } from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { RawJsonViewer } from '../../components/RawJsonViewer';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
};

const rowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const badgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #9e6a03',
  color: '#e3b341',
};

const sensitiveButtonStyle: CSSProperties = {
  ...badgeStyle,
  background: 'transparent',
  cursor: 'pointer',
};

const messageStyle: CSSProperties = { fontSize: 14 };

interface EditableRow {
  id: number;
  name: string;
  value: string;
  isSensitive: boolean;
}

type LoadState = 'loading' | 'ready' | 'error';
type SaveState = 'idle' | 'saved' | 'error';

export function LambdaEnvironmentTab({ functionName }: { functionName: string }) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [result, setResult] = useState<LambdaEnvironmentResult | null>(null);
  const [rows, setRows] = useState<EditableRow[]>([]);
  const [reveal, setReveal] = useState(false);
  const [saveState, setSaveState] = useState<SaveState>('idle');
  const nextId = useRef(0);

  const load = useCallback(
    (revealValue: boolean, signal?: AbortSignal) => {
      setLoadState('loading');
      return getLambdaEnvironment(functionName, revealValue, signal)
        .then((data) => {
          setResult(data);
          setRows(
            data.variables.map((variable) => ({
              id: nextId.current++,
              name: variable.name,
              value: variable.value,
              isSensitive: variable.isSensitive,
            })),
          );
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [functionName],
  );

  useEffect(() => {
    const controller = new AbortController();
    setReveal(false);
    setSaveState('idle');
    void load(false, controller.signal);
    return () => controller.abort();
  }, [load]);

  const handleToggleReveal = () => {
    const next = !reveal;
    setReveal(next);
    void load(next);
  };

  const handleNameChange = (id: number, name: string) =>
    setRows((current) => current.map((row) => (row.id === id ? { ...row, name } : row)));

  const handleValueChange = (id: number, value: string) =>
    setRows((current) => current.map((row) => (row.id === id ? { ...row, value } : row)));

  const handleAddRow = () =>
    setRows((current) => [
      ...current,
      { id: nextId.current++, name: '', value: '', isSensitive: false },
    ]);

  const handleRemoveRow = (id: number) =>
    setRows((current) => current.filter((row) => row.id !== id));

  const handleSave = () => {
    setSaveState('idle');
    updateLambdaEnvironment(
      functionName,
      rows.map((row) => ({ name: row.name, value: row.value })),
    )
      .then(() => {
        setSaveState('saved');
        return load(reveal);
      })
      .catch(() => setSaveState('error'));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="lambda-environment-loading" style={messageStyle}>
        Loading environment&hellip;
      </p>
    );
  }

  if (loadState === 'error') {
    return (
      <p data-testid="lambda-environment-error" style={messageStyle}>
        Unable to load the environment configuration.
      </p>
    );
  }

  const sensitiveKeys = rows.filter((row) => row.isSensitive).map((row) => row.name);
  const rawValue = Object.fromEntries(rows.map((row) => [row.name, row.value]));

  return (
    <div data-testid="lambda-environment-tab" style={containerStyle}>
      <div style={headerStyle}>
        <Text style={{ fontSize: 13, opacity: 0.7 }}>Environment variables</Text>
        {result?.revealAllowed ? (
          <button
            type="button"
            data-testid="lambda-environment-reveal"
            style={buttonStyle}
            onClick={handleToggleReveal}
          >
            {reveal ? 'Hide values' : 'Reveal values'}
          </button>
        ) : null}
      </div>

      {rows.map((row) => (
        <div key={row.id} data-testid={`lambda-environment-row-${row.id}`} style={rowStyle}>
          <input
            type="text"
            aria-label="Variable name"
            data-testid={`lambda-environment-name-${row.id}`}
            style={inputStyle}
            value={row.name}
            onChange={(event) => handleNameChange(row.id, event.target.value)}
          />
          <input
            type="text"
            aria-label="Variable value"
            data-testid={`lambda-environment-value-${row.id}`}
            style={inputStyle}
            value={row.value}
            onChange={(event) => handleValueChange(row.id, event.target.value)}
          />
          {row.isSensitive ? (
            result?.revealAllowed ? (
              <button
                type="button"
                data-testid={`lambda-environment-sensitive-${row.id}`}
                style={sensitiveButtonStyle}
                onClick={handleToggleReveal}
                title={reveal ? 'Hide value' : 'Reveal value'}
                aria-pressed={reveal}
              >
                {reveal ? 'Sensitive \u00b7 hide' : 'Sensitive \u00b7 reveal'}
              </button>
            ) : (
              <span data-testid={`lambda-environment-sensitive-${row.id}`} style={badgeStyle}>
                Sensitive
              </span>
            )
          ) : null}
          <button
            type="button"
            data-testid={`lambda-environment-remove-${row.id}`}
            style={buttonStyle}
            onClick={() => handleRemoveRow(row.id)}
          >
            Remove
          </button>
        </div>
      ))}

      <button
        type="button"
        data-testid="lambda-environment-add"
        style={buttonStyle}
        onClick={handleAddRow}
      >
        Add variable
      </button>

      <ConfirmationHost
        actionLabel="Save changes"
        prompt="Apply these environment changes?"
        confirmLabel="Confirm save"
        onConfirm={handleSave}
      />

      {saveState === 'saved' ? (
        <Text data-testid="lambda-environment-save-status" style={messageStyle}>
          Environment updated.
        </Text>
      ) : null}
      {saveState === 'error' ? (
        <Text data-testid="lambda-environment-save-error" style={messageStyle}>
          Unable to update the environment.
        </Text>
      ) : null}

      <RawJsonViewer value={rawValue} title="Environment JSON" sensitiveKeys={sensitiveKeys} />
    </div>
  );
}

export default LambdaEnvironmentTab;
