import { useMemo, useRef, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { formatPolicyDocument, isPlainObject } from './policyDocument';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
  flexWrap: 'wrap',
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
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

const preStyle: CSSProperties = {
  margin: 0,
  padding: 12,
  borderRadius: 6,
  background: '#161b22',
  fontFamily: 'monospace',
  fontSize: 12,
  lineHeight: 1.5,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
  overflowX: 'auto',
};

const errorListStyle: CSSProperties = {
  margin: 0,
  paddingLeft: 18,
  color: '#f85149',
  fontSize: 12,
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const defaultAction = 'sts:AssumeRole';

type PrincipalType = 'Service' | 'AWS' | 'Federated';

const principalTypes: { value: PrincipalType; label: string }[] = [
  { value: 'Service', label: 'AWS service' },
  { value: 'AWS', label: 'AWS account or ARN' },
  { value: 'Federated', label: 'Federated provider' },
];

interface TrustRow {
  id: number;
  type: PrincipalType;
  value: string;
  action: string;
}

/**
 * Convert an existing trust policy document into editable builder rows, one row per
 * trusted principal value so that arrays and grouped principals can be edited individually.
 */
function documentToRows(value: unknown, startId: number): TrustRow[] {
  if (!isPlainObject(value)) {
    return [];
  }
  const statementsRaw = value.Statement;
  const statements = Array.isArray(statementsRaw)
    ? statementsRaw
    : statementsRaw === undefined
      ? []
      : [statementsRaw];
  const rows: TrustRow[] = [];
  let id = startId;
  statements.forEach((statement) => {
    if (!isPlainObject(statement)) {
      return;
    }
    const action = normalizeAction(statement.Action);
    const principal = statement.Principal;
    if (typeof principal === 'string') {
      rows.push({ id: id++, type: 'AWS', value: principal, action });
      return;
    }
    if (!isPlainObject(principal)) {
      return;
    }
    principalTypes.forEach(({ value: type }) => {
      const entry = principal[type];
      const values = Array.isArray(entry) ? entry : entry === undefined ? [] : [entry];
      values.forEach((candidate) => {
        if (typeof candidate === 'string') {
          rows.push({ id: id++, type, value: candidate, action });
        }
      });
    });
  });
  return rows;
}

/**
 * Reduce a statement Action (string or array) to a single editable action string.
 */
function normalizeAction(action: unknown): string {
  if (typeof action === 'string') {
    return action;
  }
  if (Array.isArray(action) && typeof action[0] === 'string') {
    return action[0];
  }
  return defaultAction;
}

/**
 * Build a valid trust policy document from the builder rows.
 */
function rowsToDocument(rows: TrustRow[]): unknown {
  return {
    Version: '2012-10-17',
    Statement: rows.map((row) => ({
      Effect: 'Allow',
      Principal: { [row.type]: row.value },
      Action: row.action.trim() === '' ? defaultAction : row.action.trim(),
    })),
  };
}

interface TrustPolicyBuilderProps {
  value: unknown;
  onSave: (document: unknown) => void;
  testId?: string;
}

/**
 * Guided builder for IAM role trust relationships. Lets the operator add, edit and remove
 * trusted principals through form controls and generates a valid trust policy JSON document,
 * validated before it is saved.
 */
export function TrustPolicyBuilder({
  value,
  onSave,
  testId = 'trust-policy-builder',
}: TrustPolicyBuilderProps) {
  const idRef = useRef(0);
  const [rows, setRows] = useState<TrustRow[]>(() => {
    const initial = documentToRows(value, idRef.current);
    idRef.current = initial.length;
    return initial;
  });
  const [errors, setErrors] = useState<string[]>([]);

  const preview = useMemo(() => formatPolicyDocument(rowsToDocument(rows)), [rows]);

  const handleAddRow = () => {
    const id = idRef.current;
    idRef.current = id + 1;
    setRows((current) => [...current, { id, type: 'Service', value: '', action: defaultAction }]);
  };

  const handleRemoveRow = (id: number) => {
    setRows((current) => current.filter((row) => row.id !== id));
  };

  const handleChangeType = (id: number, type: PrincipalType) => {
    setRows((current) => current.map((row) => (row.id === id ? { ...row, type } : row)));
  };

  const handleChangeValue = (id: number, newValue: string) => {
    setRows((current) => current.map((row) => (row.id === id ? { ...row, value: newValue } : row)));
  };

  const handleChangeAction = (id: number, action: string) => {
    setRows((current) => current.map((row) => (row.id === id ? { ...row, action } : row)));
  };

  const handleSave = () => {
    const trimmed = rows.map((row) => ({ ...row, value: row.value.trim() }));
    if (trimmed.length === 0 || trimmed.some((row) => row.value === '')) {
      setErrors(['Add at least one trusted principal and give every principal a value.']);
      return;
    }
    setErrors([]);
    onSave(rowsToDocument(trimmed));
  };

  return (
    <div data-testid={testId} style={containerStyle}>
      <Heading as="h3" data-testid={`${testId}-title`} style={{ fontSize: 14 }}>
        Guided trust relationship builder
      </Heading>

      {rows.length === 0 ? (
        <p data-testid={`${testId}-empty`} style={labelStyle}>
          No trusted principals yet. Add one to allow an entity to assume this role.
        </p>
      ) : (
        rows.map((row) => (
          <div key={row.id} data-testid={`${testId}-row`} style={rowStyle}>
            <select
              data-testid={`${testId}-row-type`}
              style={inputStyle}
              value={row.type}
              onChange={(event) => handleChangeType(row.id, event.target.value as PrincipalType)}
            >
              {principalTypes.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
            <input
              type="text"
              data-testid={`${testId}-row-value`}
              style={inputStyle}
              placeholder="lambda.amazonaws.com"
              value={row.value}
              onChange={(event) => handleChangeValue(row.id, event.target.value)}
            />
            <input
              type="text"
              data-testid={`${testId}-row-action`}
              style={inputStyle}
              value={row.action}
              onChange={(event) => handleChangeAction(row.id, event.target.value)}
            />
            <button
              type="button"
              data-testid={`${testId}-row-remove`}
              style={buttonStyle}
              onClick={() => handleRemoveRow(row.id)}
            >
              Remove
            </button>
          </div>
        ))
      )}

      <button type="button" data-testid={`${testId}-add`} style={buttonStyle} onClick={handleAddRow}>
        Add trusted principal
      </button>

      {errors.length > 0 ? (
        <ul data-testid={`${testId}-errors`} style={errorListStyle}>
          {errors.map((error) => (
            <li key={error}>{error}</li>
          ))}
        </ul>
      ) : null}

      <span style={labelStyle}>Generated trust policy</span>
      <pre data-testid={`${testId}-preview`} style={preStyle}>
        {preview}
      </pre>

      <button type="button" data-testid={`${testId}-save`} style={buttonStyle} onClick={handleSave}>
        Save trust policy
      </button>
    </div>
  );
}

export default TrustPolicyBuilder;
