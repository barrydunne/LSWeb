import { useEffect, useState, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createParameter, deleteParameter, getParameters } from '../../api/client';
import type { ParameterItem } from '../../api/client';
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

const parameterTypes = ['String', 'StringList', 'SecureString'];

const breadcrumbStyle: CSSProperties = {
  display: 'flex',
  flexWrap: 'wrap',
  alignItems: 'center',
  gap: 4,
  marginBottom: 8,
  fontSize: 13,
  fontFamily: 'monospace',
};

const breadcrumbButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '1px 6px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  fontFamily: 'monospace',
};

const folderListStyle: CSSProperties = {
  display: 'flex',
  flexWrap: 'wrap',
  gap: 8,
  marginBottom: 12,
};

const folderButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
  cursor: 'pointer',
  fontFamily: 'monospace',
};

function childPrefix(path: string): string {
  return path === '/' ? '/' : `${path}/`;
}

function directParameters(parameters: ParameterItem[], path: string): ParameterItem[] {
  const prefix = childPrefix(path);
  return parameters.filter(
    (parameter) =>
      parameter.name.startsWith(prefix) && !parameter.name.slice(prefix.length).includes('/'),
  );
}

function childFolders(parameters: ParameterItem[], path: string): string[] {
  const prefix = childPrefix(path);
  const folders = new Set<string>();
  for (const parameter of parameters) {
    if (!parameter.name.startsWith(prefix)) {
      continue;
    }
    const remainder = parameter.name.slice(prefix.length);
    const separator = remainder.indexOf('/');
    if (separator > 0) {
      folders.add(remainder.slice(0, separator));
    }
  }
  return Array.from(folders).sort((left, right) => left.localeCompare(right));
}

const columns: DataListColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'type', label: 'Type' },
  { key: 'version', label: 'Version' },
  { key: 'lastModified', label: 'Last Modified' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; parameters: ParameterItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

export function SsmParameterStoreListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [currentPath, setCurrentPath] = useState('/');
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState('');
  const [type, setType] = useState('String');
  const [value, setValue] = useState('');
  const [description, setDescription] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getParameters('/', true, controller.signal)
      .then((result) => setState({ kind: 'ready', parameters: result.parameters }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    const trimmedDescription = description.trim();
    createParameter({
      name,
      type,
      value,
      description: trimmedDescription === '' ? null : trimmedDescription,
    })
      .then(() => {
        setCreateState('created');
        setName('');
        setType('String');
        setValue('');
        setDescription('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (parameterName: string) => {
      deleteParameter(parameterName)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="ssm-parameter-store-list-loading" style={messageStyle}>
        Loading parameters&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="ssm-parameter-store-list-error" style={messageStyle}>
        Unable to load SSM parameters.
      </p>
    );
  }

  const folders = childFolders(state.parameters, currentPath);
  const visibleParameters = directParameters(state.parameters, currentPath);
  const segments = currentPath === '/' ? [] : currentPath.slice(1).split('/');

  const rows: DataListRow[] = visibleParameters.map((parameter) => ({
    id: parameter.name,
    filterText: parameter.name,
    cells: {
      name: (
        <Link
          data-testid="ssm-parameter-store-list-link"
          to={`/services/${serviceKey}/${encodeURIComponent(parameter.name)}`}
        >
          {parameter.name}
        </Link>
      ),
      type: parameter.type,
      version: parameter.version,
      lastModified: parameter.lastModifiedDate ?? '\u2014',
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${parameter.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(parameter.name)}
        />
      ),
    },
  }));

  return (
    <div data-testid="ssm-parameter-store-list-view">
      <nav data-testid="ssm-path-breadcrumb" style={breadcrumbStyle}>
        <button
          type="button"
          data-testid="ssm-path-root"
          style={breadcrumbButtonStyle}
          onClick={() => setCurrentPath('/')}
        >
          /
        </button>
        {segments.map((segment, index) => {
          const targetPath = `/${segments.slice(0, index + 1).join('/')}`;
          return (
            <span key={targetPath} style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <span aria-hidden="true">/</span>
              <button
                type="button"
                data-testid="ssm-path-segment"
                style={breadcrumbButtonStyle}
                onClick={() => setCurrentPath(targetPath)}
              >
                {segment}
              </button>
            </span>
          );
        })}
      </nav>
      {folders.length > 0 ? (
        <div data-testid="ssm-folder-list" style={folderListStyle}>
          {folders.map((folder) => (
            <button
              key={folder}
              type="button"
              data-testid="ssm-folder"
              style={folderButtonStyle}
              onClick={() => setCurrentPath(`${childPrefix(currentPath)}${folder}`)}
            >
              {folder}/
            </button>
          ))}
        </div>
      ) : null}
      <button
        type="button"
        data-testid="ssm-parameter-store-create-toggle"
        style={buttonStyle}
        onClick={() => {
          setShowCreate((current) => {
            if (!current && name === '') {
              setName(childPrefix(currentPath));
            }
            return !current;
          });
        }}
      >
        {showCreate ? 'Cancel' : 'Create parameter'}
      </button>
      {showCreate ? (
        <div data-testid="ssm-parameter-store-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="ssm-parameter-store-create-name">
              Parameter name
            </label>
            <input
              id="ssm-parameter-store-create-name"
              type="text"
              data-testid="ssm-parameter-store-create-name"
              style={inputStyle}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="ssm-parameter-store-create-type">
              Type
            </label>
            <select
              id="ssm-parameter-store-create-type"
              data-testid="ssm-parameter-store-create-type"
              style={inputStyle}
              value={type}
              onChange={(event) => setType(event.target.value)}
            >
              {parameterTypes.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="ssm-parameter-store-create-value">
              Value
            </label>
            <input
              id="ssm-parameter-store-create-value"
              type="text"
              data-testid="ssm-parameter-store-create-value"
              style={inputStyle}
              value={value}
              onChange={(event) => setValue(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="ssm-parameter-store-create-description">
              Description (optional)
            </label>
            <input
              id="ssm-parameter-store-create-description"
              type="text"
              data-testid="ssm-parameter-store-create-description"
              style={inputStyle}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="ssm-parameter-store-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="ssm-parameter-store-create-status" style={messageStyle}>
          Parameter created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="ssm-parameter-store-create-error" style={messageStyle}>
          Unable to create the parameter.
        </p>
      ) : null}
      <DataListShell
        title="Parameters"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter parameters"
        columnPrefsKey="ssm-parameter-store-parameters"
        emptyState={{ message: 'No parameters found on this backend.' }}
      />
    </div>
  );
}

export default SsmParameterStoreListView;
