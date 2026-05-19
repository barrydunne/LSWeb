import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { createLambdaFunction, getLambdaFunctions } from '../../api/client';
import type { LambdaFunctionSummaryItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  marginBottom: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
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

const columns: DataListColumn[] = [
  { key: 'name', label: 'Function' },
  { key: 'runtime', label: 'Runtime' },
  { key: 'memory', label: 'Memory (MB)' },
  { key: 'timeout', label: 'Timeout (s)' },
  { key: 'lastModified', label: 'Last modified' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; functions: LambdaFunctionSummaryItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

interface CreateForm {
  functionName: string;
  runtime: string;
  handler: string;
  role: string;
  description: string;
  memorySize: string;
  timeout: string;
  zipFileBase64: string;
}

const emptyForm: CreateForm = {
  functionName: '',
  runtime: '',
  handler: '',
  role: '',
  description: '',
  memorySize: '128',
  timeout: '3',
  zipFileBase64: '',
};

const formFields: { key: keyof CreateForm; label: string }[] = [
  { key: 'functionName', label: 'Function name' },
  { key: 'runtime', label: 'Runtime' },
  { key: 'handler', label: 'Handler' },
  { key: 'role', label: 'Role ARN' },
  { key: 'description', label: 'Description' },
  { key: 'memorySize', label: 'Memory (MB)' },
  { key: 'timeout', label: 'Timeout (s)' },
  { key: 'zipFileBase64', label: 'Deployment package (base64 zip)' },
];

export function LambdaListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState<CreateForm>(emptyForm);
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getLambdaFunctions(controller.signal)
      .then((result) => setState({ kind: 'ready', functions: result.functions }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleFieldChange = (key: keyof CreateForm, value: string) =>
    setForm((current) => ({ ...current, [key]: value }));

  const handleCreate = () => {
    setCreateState('saving');
    createLambdaFunction({
      functionName: form.functionName,
      runtime: form.runtime,
      handler: form.handler,
      role: form.role,
      description: form.description,
      memorySize: Number(form.memorySize),
      timeout: Number(form.timeout),
      zipFileBase64: form.zipFileBase64,
    })
      .then(() => {
        setCreateState('created');
        setForm(emptyForm);
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="lambda-list-loading" style={messageStyle}>
        Loading functions&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="lambda-list-error" style={messageStyle}>
        Unable to load Lambda functions.
      </p>
    );
  }

  const rows: DataListRow[] = state.functions.map((fn) => ({
    id: fn.functionName,
    filterText: `${fn.functionName} ${fn.runtime}`,
    cells: {
      name: (
        <Link data-testid="lambda-list-link" to={`/services/${serviceKey}/${encodeURIComponent(fn.functionName)}`}>
          {fn.functionName}
        </Link>
      ),
      runtime: fn.runtime,
      memory: fn.memorySize,
      timeout: fn.timeout,
      lastModified: fn.lastModified,
    },
  }));

  return (
    <div data-testid="lambda-list-view">
      <button
        type="button"
        data-testid="lambda-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create function'}
      </button>
      {showCreate ? (
        <div data-testid="lambda-create-form" style={formStyle}>
          {formFields.map((field) => (
            <div key={field.key} style={fieldRowStyle}>
              <label style={labelStyle} htmlFor={`lambda-create-${field.key}`}>
                {field.label}
              </label>
              <input
                id={`lambda-create-${field.key}`}
                type="text"
                data-testid={`lambda-create-${field.key}`}
                style={inputStyle}
                value={form[field.key]}
                onChange={(event) => handleFieldChange(field.key, event.target.value)}
              />
            </div>
          ))}
          <button
            type="button"
            data-testid="lambda-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="lambda-create-status" style={messageStyle}>
          Function created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="lambda-create-error" style={messageStyle}>
          Unable to create the function.
        </p>
      ) : null}
      <DataListShell
        title="Functions"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter functions"
        columnPrefsKey="lambda-functions"
        emptyState={{ message: 'No Lambda functions found on this backend.' }}
      />
    </div>
  );
}

export default LambdaListView;
