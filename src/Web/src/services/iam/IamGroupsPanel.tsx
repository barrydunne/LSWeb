import { useEffect, useState, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createIamGroup, deleteIamGroup, getIamGroups } from '../../api/client';
import type { IamGroupSummary } from '../../api/client';

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
  { key: 'name', label: 'Group' },
  { key: 'path', label: 'Path' },
  { key: 'created', label: 'Created' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; groups: IamGroupSummary[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

interface IamGroupsPanelProps {
  serviceKey: string;
}

/**
 * Lists IAM groups with create and delete actions. Each row links to the group detail view.
 */
export function IamGroupsPanel({ serviceKey }: IamGroupsPanelProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [groupName, setGroupName] = useState('');
  const [path, setPath] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getIamGroups(controller.signal)
      .then((result) => setState({ kind: 'ready', groups: result.groups }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    const trimmedPath = path.trim();
    createIamGroup({
      groupName,
      path: trimmedPath === '' ? null : trimmedPath,
    })
      .then(() => {
        setCreateState('created');
        setGroupName('');
        setPath('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (name: string) => {
      deleteIamGroup(name)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="iam-groups-loading" style={messageStyle}>
        Loading groups&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="iam-groups-error" style={messageStyle}>
        Unable to load IAM groups.
      </p>
    );
  }

  const rows: DataListRow[] = state.groups.map((group) => ({
    id: group.groupName,
    filterText: `${group.groupName} ${group.path}`,
    cells: {
      name: (
        <Link
          data-testid="iam-groups-link"
          to={`/services/${serviceKey}/${encodeURIComponent(`group/${group.groupName}`)}`}
        >
          {group.groupName}
        </Link>
      ),
      path: group.path,
      created: group.createDate ?? '\u2014',
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${group.groupName}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(group.groupName)}
        />
      ),
    },
  }));

  return (
    <div data-testid="iam-groups-panel">
      <button
        type="button"
        data-testid="iam-groups-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create group'}
      </button>
      {showCreate ? (
        <div data-testid="iam-groups-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-groups-create-name">
              Group name
            </label>
            <input
              id="iam-groups-create-name"
              type="text"
              data-testid="iam-groups-create-name"
              style={inputStyle}
              value={groupName}
              onChange={(event) => setGroupName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-groups-create-path">
              Path (optional)
            </label>
            <input
              id="iam-groups-create-path"
              type="text"
              data-testid="iam-groups-create-path"
              style={inputStyle}
              value={path}
              onChange={(event) => setPath(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="iam-groups-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="iam-groups-create-status" style={messageStyle}>
          Group created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="iam-groups-create-error" style={messageStyle}>
          Unable to create the group.
        </p>
      ) : null}
      <DataListShell
        title="Groups"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter groups"
        columnPrefsKey="iam-groups"
        emptyState={{ message: 'No IAM groups found on this backend.' }}
      />
    </div>
  );
}

export default IamGroupsPanel;
