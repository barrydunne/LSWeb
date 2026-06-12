import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createUserPool, deleteUserPool, getUserPools } from '../../api/client';
import type { UserPoolSummaryItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const idCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

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

const checkboxRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 6,
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
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const mfaModes = ['OFF', 'ON', 'OPTIONAL'];

const columns: DataListColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'id', label: 'Pool ID' },
  { key: 'created', label: 'Created' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; userPools: UserPoolSummaryItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

function parseAttributes(value: string): string[] {
  return value
    .split(',')
    .map((entry) => entry.trim())
    .filter((entry) => entry !== '');
}

export function CognitoListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [poolName, setPoolName] = useState('');
  const [mfaConfiguration, setMfaConfiguration] = useState('OFF');
  const [usernameAttributes, setUsernameAttributes] = useState('');
  const [autoVerifiedAttributes, setAutoVerifiedAttributes] = useState('');
  const [passwordMinLength, setPasswordMinLength] = useState('8');
  const [requireUppercase, setRequireUppercase] = useState(true);
  const [requireLowercase, setRequireLowercase] = useState(true);
  const [requireNumbers, setRequireNumbers] = useState(true);
  const [requireSymbols, setRequireSymbols] = useState(false);
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getUserPools(controller.signal)
      .then((result) => setState({ kind: 'ready', userPools: result.userPools }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createUserPool({
      name: poolName,
      mfaConfiguration,
      usernameAttributes: parseAttributes(usernameAttributes),
      autoVerifiedAttributes: parseAttributes(autoVerifiedAttributes),
      passwordPolicy: {
        minimumLength: Number(passwordMinLength) || 8,
        requireUppercase,
        requireLowercase,
        requireNumbers,
        requireSymbols,
      },
    })
      .then(() => {
        setCreateState('created');
        setPoolName('');
        setUsernameAttributes('');
        setAutoVerifiedAttributes('');
        setPasswordMinLength('8');
        setRequireUppercase(true);
        setRequireLowercase(true);
        setRequireNumbers(true);
        setRequireSymbols(false);
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (id: string) => {
      deleteUserPool(id)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="cognito-list-loading" style={messageStyle}>
        Loading user pools&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="cognito-list-error" style={messageStyle}>
        Unable to load Cognito user pools.
      </p>
    );
  }

  const rows: DataListRow[] = state.userPools.map((userPool) => ({
    id: userPool.id,
    filterText: `${userPool.name} ${userPool.id}`,
    cells: {
      name: (
        <Link
          data-testid="cognito-list-name"
          to={`/services/${serviceKey}/${encodeURIComponent(userPool.id)}`}
        >
          {userPool.name}
        </Link>
      ),
      id: (
        <span data-testid="cognito-list-id" style={idCellStyle}>
          {userPool.id}
        </span>
      ),
      created: (
        <span data-testid="cognito-list-created" style={messageStyle}>
          {userPool.creationDate ?? '—'}
        </span>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${userPool.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(userPool.id)}
        />
      ),
    },
  }));

  return (
    <div data-testid="cognito-list-view">
      <button
        type="button"
        data-testid="cognito-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create user pool'}
      </button>
      {showCreate ? (
        <div data-testid="cognito-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-create-name">
              Pool name
            </label>
            <input
              id="cognito-create-name"
              type="text"
              data-testid="cognito-create-name"
              style={inputStyle}
              value={poolName}
              onChange={(event) => setPoolName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-create-mfa">
              MFA configuration
            </label>
            <select
              id="cognito-create-mfa"
              data-testid="cognito-create-mfa"
              style={inputStyle}
              value={mfaConfiguration}
              onChange={(event) => setMfaConfiguration(event.target.value)}
            >
              {mfaModes.map((mode) => (
                <option key={mode} value={mode}>
                  {mode}
                </option>
              ))}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-create-username-attributes">
              Username attributes (comma separated)
            </label>
            <input
              id="cognito-create-username-attributes"
              type="text"
              data-testid="cognito-create-username-attributes"
              style={inputStyle}
              value={usernameAttributes}
              onChange={(event) => setUsernameAttributes(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-create-auto-verified-attributes">
              Auto-verified attributes (comma separated)
            </label>
            <input
              id="cognito-create-auto-verified-attributes"
              type="text"
              data-testid="cognito-create-auto-verified-attributes"
              style={inputStyle}
              value={autoVerifiedAttributes}
              onChange={(event) => setAutoVerifiedAttributes(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-create-password-length">
              Password minimum length
            </label>
            <input
              id="cognito-create-password-length"
              type="number"
              data-testid="cognito-create-password-length"
              style={inputStyle}
              value={passwordMinLength}
              onChange={(event) => setPasswordMinLength(event.target.value)}
            />
          </div>
          <label style={checkboxRowStyle}>
            <input
              type="checkbox"
              data-testid="cognito-create-require-uppercase"
              checked={requireUppercase}
              onChange={(event) => setRequireUppercase(event.target.checked)}
            />
            <span style={labelStyle}>Require uppercase</span>
          </label>
          <label style={checkboxRowStyle}>
            <input
              type="checkbox"
              data-testid="cognito-create-require-lowercase"
              checked={requireLowercase}
              onChange={(event) => setRequireLowercase(event.target.checked)}
            />
            <span style={labelStyle}>Require lowercase</span>
          </label>
          <label style={checkboxRowStyle}>
            <input
              type="checkbox"
              data-testid="cognito-create-require-numbers"
              checked={requireNumbers}
              onChange={(event) => setRequireNumbers(event.target.checked)}
            />
            <span style={labelStyle}>Require numbers</span>
          </label>
          <label style={checkboxRowStyle}>
            <input
              type="checkbox"
              data-testid="cognito-create-require-symbols"
              checked={requireSymbols}
              onChange={(event) => setRequireSymbols(event.target.checked)}
            />
            <span style={labelStyle}>Require symbols</span>
          </label>
          <button
            type="button"
            data-testid="cognito-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="cognito-create-status" style={messageStyle}>
          User pool created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="cognito-create-error" style={messageStyle}>
          Unable to create the user pool.
        </p>
      ) : null}
      <DataListShell
        title="User pools"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter user pools"
        columnPrefsKey={`${serviceKey}-user-pools`}
        emptyState={{ message: 'No Cognito user pools found on this backend.' }}
      />
    </div>
  );
}

export default CognitoListView;
