import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createSqsQueue, deleteSqsQueue, getSqsQueues } from '../../api/client';
import type { SqsQueueSummaryItem } from '../../api/client';
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

const checkboxRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'row',
  alignItems: 'center',
  gap: 6,
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
  { key: 'name', label: 'Queue' },
  { key: 'available', label: 'Available' },
  { key: 'inFlight', label: 'In flight' },
  { key: 'delayed', label: 'Delayed' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; queues: SqsQueueSummaryItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

export function SqsListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [queueName, setQueueName] = useState('');
  const [fifoQueue, setFifoQueue] = useState(false);
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getSqsQueues(controller.signal)
      .then((result) => setState({ kind: 'ready', queues: result.queues }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createSqsQueue(queueName, fifoQueue)
      .then(() => {
        setCreateState('created');
        setQueueName('');
        setFifoQueue(false);
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (name: string) => {
      deleteSqsQueue(name)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="sqs-list-loading" style={messageStyle}>
        Loading queues&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="sqs-list-error" style={messageStyle}>
        Unable to load SQS queues.
      </p>
    );
  }

  const rows: DataListRow[] = state.queues.map((queue) => ({
    id: queue.name,
    filterText: queue.name,
    cells: {
      name: (
        <Link data-testid="sqs-list-link" to={`/services/${serviceKey}/${encodeURIComponent(queue.name)}`}>
          {queue.name}
        </Link>
      ),
      available: (
        <span data-testid="sqs-list-available" style={numberCellStyle}>
          {queue.approximateMessageCount}
        </span>
      ),
      inFlight: (
        <span data-testid="sqs-list-inflight" style={numberCellStyle}>
          {queue.approximateInFlightCount}
        </span>
      ),
      delayed: (
        <span data-testid="sqs-list-delayed" style={numberCellStyle}>
          {queue.approximateDelayedCount}
        </span>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${queue.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(queue.name)}
        />
      ),
    },
  }));

  return (
    <div data-testid="sqs-list-view">
      <button
        type="button"
        data-testid="sqs-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create queue'}
      </button>
      {showCreate ? (
        <div data-testid="sqs-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="sqs-create-queueName">
              Queue name
            </label>
            <input
              id="sqs-create-queueName"
              type="text"
              data-testid="sqs-create-queueName"
              style={inputStyle}
              value={queueName}
              onChange={(event) => setQueueName(event.target.value)}
            />
          </div>
          <div style={checkboxRowStyle}>
            <input
              id="sqs-create-fifo"
              type="checkbox"
              data-testid="sqs-create-fifo"
              checked={fifoQueue}
              onChange={(event) => setFifoQueue(event.target.checked)}
            />
            <label style={labelStyle} htmlFor="sqs-create-fifo">
              FIFO queue (name must end with .fifo)
            </label>
          </div>
          <button
            type="button"
            data-testid="sqs-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="sqs-create-status" style={messageStyle}>
          Queue created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="sqs-create-error" style={messageStyle}>
          Unable to create the queue.
        </p>
      ) : null}
      <DataListShell
        title="Queues"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter queues"
        columnPrefsKey="sqs-queues"
        emptyState={{ message: 'No SQS queues found on this backend.' }}
      />
    </div>
  );
}

export default SqsListView;
