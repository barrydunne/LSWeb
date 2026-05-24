import { useEffect, useState, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createLogGroup, deleteLogGroup, getLogGroups } from '../../api/client';
import type { LogGroupItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const numberCellStyle: CSSProperties = { textAlign: 'right', fontVariantNumeric: 'tabular-nums' };

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
  { key: 'name', label: 'Log group' },
  { key: 'storedBytes', label: 'Stored bytes' },
  { key: 'retention', label: 'Retention (days)' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; groups: LogGroupItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

export function CloudWatchLogsListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [logGroupName, setLogGroupName] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getLogGroups(controller.signal)
      .then((result) => setState({ kind: 'ready', groups: result.logGroups }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createLogGroup(logGroupName)
      .then(() => {
        setCreateState('created');
        setLogGroupName('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (name: string) => {
      deleteLogGroup(name)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="logs-list-loading" style={messageStyle}>
        Loading log groups&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="logs-list-error" style={messageStyle}>
        Unable to load CloudWatch log groups.
      </p>
    );
  }

  const rows: DataListRow[] = state.groups.map((group) => ({
    id: group.name,
    filterText: group.name,
    cells: {
      name: (
        <Link
          data-testid="logs-list-link"
          to={`/services/${serviceKey}/${encodeURIComponent(group.name)}`}
        >
          {group.name}
        </Link>
      ),
      storedBytes: (
        <span data-testid="logs-list-stored" style={numberCellStyle}>
          {group.storedBytes}
        </span>
      ),
      retention: (
        <span data-testid="logs-list-retention" style={numberCellStyle}>
          {group.retentionInDays ?? 'Never'}
        </span>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${group.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(group.name)}
        />
      ),
    },
  }));

  return (
    <div data-testid="logs-list-view">
      <button
        type="button"
        data-testid="logs-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create log group'}
      </button>
      {showCreate ? (
        <div data-testid="logs-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="logs-create-name">
              Log group name
            </label>
            <input
              id="logs-create-name"
              type="text"
              data-testid="logs-create-name"
              style={inputStyle}
              value={logGroupName}
              onChange={(event) => setLogGroupName(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="logs-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="logs-create-status" style={messageStyle}>
          Log group created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="logs-create-error" style={messageStyle}>
          Unable to create the log group.
        </p>
      ) : null}
      <DataListShell
        title="Log groups"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter log groups"
        columnPrefsKey="cloudwatch-logs-groups"
        emptyState={{ message: 'No CloudWatch log groups found on this backend.' }}
      />
    </div>
  );
}

export default CloudWatchLogsListView;
