import { useEffect, useState, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createSecret, deleteSecret, getSecrets } from '../../api/client';
import type { SecretItem } from '../../api/client';
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
  { key: 'name', label: 'Secret' },
  { key: 'description', label: 'Description' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; secrets: SecretItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

export function SecretsManagerListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [secretString, setSecretString] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getSecrets(controller.signal)
      .then((result) => setState({ kind: 'ready', secrets: result.secrets }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    const trimmedDescription = description.trim();
    createSecret({
      name,
      description: trimmedDescription === '' ? null : trimmedDescription,
      secretString,
    })
      .then(() => {
        setCreateState('created');
        setName('');
        setDescription('');
        setSecretString('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (secretName: string) => {
      deleteSecret(secretName)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="secrets-manager-list-loading" style={messageStyle}>
        Loading secrets&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="secrets-manager-list-error" style={messageStyle}>
        Unable to load Secrets Manager secrets.
      </p>
    );
  }

  const rows: DataListRow[] = state.secrets.map((secret) => ({
    id: secret.name,
    filterText: secret.name,
    cells: {
      name: (
        <Link
          data-testid="secrets-manager-list-link"
          to={`/services/${serviceKey}/${encodeURIComponent(secret.name)}`}
        >
          {secret.name}
        </Link>
      ),
      description: secret.description ?? '\u2014',
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${secret.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(secret.name)}
        />
      ),
    },
  }));

  return (
    <div data-testid="secrets-manager-list-view">
      <button
        type="button"
        data-testid="secrets-manager-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create secret'}
      </button>
      {showCreate ? (
        <div data-testid="secrets-manager-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="secrets-manager-create-name">
              Secret name
            </label>
            <input
              id="secrets-manager-create-name"
              type="text"
              data-testid="secrets-manager-create-name"
              style={inputStyle}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="secrets-manager-create-description">
              Description (optional)
            </label>
            <input
              id="secrets-manager-create-description"
              type="text"
              data-testid="secrets-manager-create-description"
              style={inputStyle}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="secrets-manager-create-value">
              Secret value
            </label>
            <input
              id="secrets-manager-create-value"
              type="text"
              data-testid="secrets-manager-create-value"
              style={inputStyle}
              value={secretString}
              onChange={(event) => setSecretString(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="secrets-manager-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="secrets-manager-create-status" style={messageStyle}>
          Secret created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="secrets-manager-create-error" style={messageStyle}>
          Unable to create the secret.
        </p>
      ) : null}
      <DataListShell
        title="Secrets"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter secrets"
        columnPrefsKey="secrets-manager-secrets"
        emptyState={{ message: 'No secrets found on this backend.' }}
      />
    </div>
  );
}

export default SecretsManagerListView;
