import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createS3Bucket, deleteS3Bucket, getS3Buckets } from '../../api/client';
import type { S3BucketSummaryItem } from '../../api/client';
import { formatTimestamp } from './formatTimestamp';
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
  { key: 'name', label: 'Bucket' },
  { key: 'creationDate', label: 'Created' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; buckets: S3BucketSummaryItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

export function S3ListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [bucketName, setBucketName] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getS3Buckets(controller.signal)
      .then((result) => setState({ kind: 'ready', buckets: result.buckets }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createS3Bucket(bucketName)
      .then(() => {
        setCreateState('created');
        setBucketName('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (name: string) => {
      deleteS3Bucket(name)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="s3-list-loading" style={messageStyle}>
        Loading buckets&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="s3-list-error" style={messageStyle}>
        Unable to load S3 buckets.
      </p>
    );
  }

  const rows: DataListRow[] = state.buckets.map((bucket) => ({
    id: bucket.name,
    filterText: bucket.name,
    cells: {
      name: (
        <Link data-testid="s3-list-link" to={`/services/${serviceKey}/${encodeURIComponent(bucket.name)}`}>
          {bucket.name}
        </Link>
      ),
      creationDate: <span title={bucket.creationDate}>{formatTimestamp(bucket.creationDate)}</span>,
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${bucket.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(bucket.name)}
        />
      ),
    },
  }));

  return (
    <div data-testid="s3-list-view">
      <button
        type="button"
        data-testid="s3-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create bucket'}
      </button>
      {showCreate ? (
        <div data-testid="s3-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="s3-create-bucketName">
              Bucket name
            </label>
            <input
              id="s3-create-bucketName"
              type="text"
              data-testid="s3-create-bucketName"
              style={inputStyle}
              value={bucketName}
              onChange={(event) => setBucketName(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="s3-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="s3-create-status" style={messageStyle}>
          Bucket created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="s3-create-error" style={messageStyle}>
          Unable to create the bucket.
        </p>
      ) : null}
      <DataListShell
        title="Buckets"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter buckets"
        columnPrefsKey="s3-buckets"
        emptyState={{ message: 'No S3 buckets found on this backend.' }}
      />
    </div>
  );
}

export default S3ListView;
