import { useEffect, useState, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createIamRole, deleteIamRole, getIamRoles } from '../../api/client';
import type { IamRoleSummary } from '../../api/client';
import { PolicyDocumentEditor } from './components/PolicyDocumentEditor';

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
  { key: 'name', label: 'Role' },
  { key: 'description', label: 'Description' },
  { key: 'path', label: 'Path' },
  { key: 'created', label: 'Created' },
  { key: 'actions', label: 'Actions' },
];

/**
 * Sensible default trust policy granting the AWS Lambda service permission to assume the role.
 */
const defaultTrustPolicy = {
  Version: '2012-10-17',
  Statement: [
    {
      Effect: 'Allow',
      Principal: { Service: 'lambda.amazonaws.com' },
      Action: 'sts:AssumeRole',
    },
  ],
};

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; roles: IamRoleSummary[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

interface IamRolesPanelProps {
  serviceKey: string;
}

/**
 * Lists IAM roles with create and delete actions. Each row links to the role detail view. The
 * create form requires a trust policy document, defaulting to a Lambda-service template.
 */
export function IamRolesPanel({ serviceKey }: IamRolesPanelProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [roleName, setRoleName] = useState('');
  const [path, setPath] = useState('');
  const [description, setDescription] = useState('');
  const [maxSession, setMaxSession] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getIamRoles(controller.signal)
      .then((result) => setState({ kind: 'ready', roles: result.roles }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = (document: unknown) => {
    setCreateState('saving');
    const trimmedPath = path.trim();
    const trimmedDescription = description.trim();
    const trimmedMaxSession = maxSession.trim();
    createIamRole({
      roleName,
      assumeRolePolicyDocument: JSON.stringify(document),
      path: trimmedPath === '' ? null : trimmedPath,
      description: trimmedDescription === '' ? null : trimmedDescription,
      maxSessionDuration: trimmedMaxSession === '' ? null : Number(trimmedMaxSession),
    })
      .then(() => {
        setCreateState('created');
        setRoleName('');
        setPath('');
        setDescription('');
        setMaxSession('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (name: string) => {
      deleteIamRole(name)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="iam-roles-loading" style={messageStyle}>
        Loading roles&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="iam-roles-error" style={messageStyle}>
        Unable to load IAM roles.
      </p>
    );
  }

  const rows: DataListRow[] = state.roles.map((role) => ({
    id: role.roleName,
    filterText: `${role.roleName} ${role.path} ${role.description ?? ''}`,
    cells: {
      name: (
        <Link
          data-testid="iam-roles-link"
          to={`/services/${serviceKey}/${encodeURIComponent(`role/${role.roleName}`)}`}
        >
          {role.roleName}
        </Link>
      ),
      description: role.description ?? '\u2014',
      path: role.path,
      created: role.createDate ?? '\u2014',
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${role.roleName}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(role.roleName)}
        />
      ),
    },
  }));

  return (
    <div data-testid="iam-roles-panel">
      <button
        type="button"
        data-testid="iam-roles-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create role'}
      </button>
      {showCreate ? (
        <div data-testid="iam-roles-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-roles-create-name">
              Role name
            </label>
            <input
              id="iam-roles-create-name"
              type="text"
              data-testid="iam-roles-create-name"
              style={inputStyle}
              value={roleName}
              onChange={(event) => setRoleName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-roles-create-path">
              Path (optional)
            </label>
            <input
              id="iam-roles-create-path"
              type="text"
              data-testid="iam-roles-create-path"
              style={inputStyle}
              value={path}
              onChange={(event) => setPath(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-roles-create-description">
              Description (optional)
            </label>
            <input
              id="iam-roles-create-description"
              type="text"
              data-testid="iam-roles-create-description"
              style={inputStyle}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-roles-create-max-session">
              Max session duration in seconds (optional)
            </label>
            <input
              id="iam-roles-create-max-session"
              type="number"
              data-testid="iam-roles-create-max-session"
              style={inputStyle}
              value={maxSession}
              onChange={(event) => setMaxSession(event.target.value)}
            />
          </div>
          <PolicyDocumentEditor
            value={defaultTrustPolicy}
            title="Trust relationship policy"
            onSave={handleCreate}
            testId="iam-roles-create-trust-policy"
          />
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="iam-roles-create-status" style={messageStyle}>
          Role created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="iam-roles-create-error" style={messageStyle}>
          Unable to create the role.
        </p>
      ) : null}
      <DataListShell
        title="Roles"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter roles"
        columnPrefsKey="iam-roles"
        emptyState={{ message: 'No IAM roles found on this backend.' }}
      />
    </div>
  );
}

export default IamRolesPanel;
