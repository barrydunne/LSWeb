import { useEffect, useState, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createIamUser, deleteIamUser, getIamUsers } from '../../api/client';
import type { IamUserSummary } from '../../api/client';

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
  { key: 'name', label: 'User' },
  { key: 'path', label: 'Path' },
  { key: 'created', label: 'Created' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; users: IamUserSummary[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

interface IamUsersPanelProps {
  serviceKey: string;
}

/**
 * Lists IAM users with create and delete actions. Each row links to the user detail view.
 */
export function IamUsersPanel({ serviceKey }: IamUsersPanelProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [userName, setUserName] = useState('');
  const [path, setPath] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getIamUsers(controller.signal)
      .then((result) => setState({ kind: 'ready', users: result.users }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    const trimmedPath = path.trim();
    createIamUser({
      userName,
      path: trimmedPath === '' ? null : trimmedPath,
    })
      .then(() => {
        setCreateState('created');
        setUserName('');
        setPath('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (name: string) => {
      deleteIamUser(name)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="iam-users-loading" style={messageStyle}>
        Loading users&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="iam-users-error" style={messageStyle}>
        Unable to load IAM users.
      </p>
    );
  }

  const rows: DataListRow[] = state.users.map((user) => ({
    id: user.userName,
    filterText: `${user.userName} ${user.path}`,
    cells: {
      name: (
        <Link
          data-testid="iam-users-link"
          to={`/services/${serviceKey}/${encodeURIComponent(`user/${user.userName}`)}`}
        >
          {user.userName}
        </Link>
      ),
      path: user.path,
      created: user.createDate ?? '\u2014',
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${user.userName}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(user.userName)}
        />
      ),
    },
  }));

  return (
    <div data-testid="iam-users-panel">
      <button
        type="button"
        data-testid="iam-users-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create user'}
      </button>
      {showCreate ? (
        <div data-testid="iam-users-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-users-create-name">
              User name
            </label>
            <input
              id="iam-users-create-name"
              type="text"
              data-testid="iam-users-create-name"
              style={inputStyle}
              value={userName}
              onChange={(event) => setUserName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-users-create-path">
              Path (optional)
            </label>
            <input
              id="iam-users-create-path"
              type="text"
              data-testid="iam-users-create-path"
              style={inputStyle}
              value={path}
              onChange={(event) => setPath(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="iam-users-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="iam-users-create-status" style={messageStyle}>
          User created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="iam-users-create-error" style={messageStyle}>
          Unable to create the user.
        </p>
      ) : null}
      <DataListShell
        title="Users"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter users"
        columnPrefsKey="iam-users"
        emptyState={{ message: 'No IAM users found on this backend.' }}
      />
    </div>
  );
}

export default IamUsersPanel;
