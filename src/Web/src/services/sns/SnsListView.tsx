import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createSnsTopic, deleteSnsTopic, getSnsTopics } from '../../api/client';
import type { SnsTopicItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const arnCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

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
  { key: 'name', label: 'Topic' },
  { key: 'arn', label: 'ARN' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; topics: SnsTopicItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

export function SnsListView({ serviceKey }: ServiceListViewProps) {

  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [topicName, setTopicName] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getSnsTopics(controller.signal)
      .then((result) => setState({ kind: 'ready', topics: result.topics }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createSnsTopic({ name: topicName })
      .then(() => {
        setCreateState('created');
        setTopicName('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (arn: string) => {
      deleteSnsTopic(arn)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="sns-list-loading" style={messageStyle}>
        Loading topics&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="sns-list-error" style={messageStyle}>
        Unable to load SNS topics.
      </p>
    );
  }

  const rows: DataListRow[] = state.topics.map((topic) => ({
    id: topic.topicArn,
    filterText: `${topic.name} ${topic.topicArn}`,
    cells: {
      name: (
        <Link
          data-testid="sns-list-name"
          to={`/services/${serviceKey}/${encodeURIComponent(topic.topicArn)}`}
        >
          {topic.name}
        </Link>
      ),
      arn: (
        <span data-testid="sns-list-arn" style={arnCellStyle}>
          {topic.topicArn}
        </span>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${topic.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(topic.topicArn)}
        />
      ),
    },
  }));

  return (
    <div data-testid="sns-list-view">
      <button
        type="button"
        data-testid="sns-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create topic'}
      </button>
      {showCreate ? (
        <div data-testid="sns-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="sns-create-topicName">
              Topic name
            </label>
            <input
              id="sns-create-topicName"
              type="text"
              data-testid="sns-create-topicName"
              style={inputStyle}
              value={topicName}
              onChange={(event) => setTopicName(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="sns-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="sns-create-status" style={messageStyle}>
          Topic created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="sns-create-error" style={messageStyle}>
          Unable to create the topic.
        </p>
      ) : null}
      <DataListShell
        title="Topics"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter topics"
        columnPrefsKey={`${serviceKey}-topics`}
        emptyState={{ message: 'No SNS topics found on this backend.' }}
      />
    </div>
  );
}

export default SnsListView;
