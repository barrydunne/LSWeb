import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { createRoute53HostedZone, getRoute53HostedZones } from '../../api/client';
import type { Route53HostedZoneItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const monoCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const textCellStyle: CSSProperties = { fontSize: 13 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  marginBottom: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const badgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
  fontFamily: 'monospace',
};

const columns: DataListColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'id', label: 'ID' },
  { key: 'recordCount', label: 'Records' },
  { key: 'visibility', label: 'Visibility' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; hostedZones: Route53HostedZoneItem[] }
  | { kind: 'error' };

export function Route53ListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState('');
  const [comment, setComment] = useState('');
  const [createState, setCreateState] = useState<'idle' | 'saving' | 'created' | 'error'>('idle');
  const [createError, setCreateError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    getRoute53HostedZones(controller.signal)
      .then((result) => setState({ kind: 'ready', hostedZones: result.hostedZones }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setReloadToken((token) => token + 1);
  };

  const handleCreate = () => {
    const trimmedName = name.trim();
    if (!trimmedName.includes('.')) {
      setCreateError('Enter a fully qualified domain name, for example example.com.');
      setCreateState('error');
      return;
    }
    setCreateError(null);
    setCreateState('saving');
    const trimmedComment = comment.trim();
    createRoute53HostedZone(trimmedName, trimmedComment === '' ? null : trimmedComment)
      .then(() => {
        setCreateState('created');
        setName('');
        setComment('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="route53-list-loading" style={messageStyle}>
        Loading hosted zones&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="route53-list-error" style={messageStyle}>
        Unable to load Route 53 hosted zones.
      </p>
    );
  }

  const rows: DataListRow[] = state.hostedZones.map((hostedZone) => ({
    id: hostedZone.id,
    filterText: `${hostedZone.name} ${hostedZone.id}`,
    cells: {
      name: (
        <Link
          data-testid="route53-list-link"
          to={`/services/${serviceKey}/${encodeURIComponent(hostedZone.id)}`}
          style={textCellStyle}
        >
          {hostedZone.name}
        </Link>
      ),
      id: (
        <span data-testid="route53-list-id" style={monoCellStyle}>
          {hostedZone.id}
        </span>
      ),
      recordCount: (
        <span data-testid="route53-list-record-count" style={monoCellStyle}>
          {hostedZone.recordCount}
        </span>
      ),
      visibility: (
        <span data-testid="route53-list-visibility" style={badgeStyle}>
          {hostedZone.privateZone ? 'Private' : 'Public'}
        </span>
      ),
    },
  }));

  return (
    <div data-testid="route53-list-view">
      <button
        type="button"
        data-testid="route53-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create hosted zone'}
      </button>
      {showCreate ? (
        <div data-testid="route53-create-form" style={formStyle}>
          <label style={messageStyle} htmlFor="route53-create-name">
            Domain name
          </label>
          <input
            id="route53-create-name"
            type="text"
            data-testid="route53-create-name"
            style={inputStyle}
            placeholder="example.com"
            value={name}
            onChange={(event) => setName(event.target.value)}
          />
          <label style={messageStyle} htmlFor="route53-create-comment">
            Comment (optional)
          </label>
          <input
            id="route53-create-comment"
            type="text"
            data-testid="route53-create-comment"
            style={inputStyle}
            value={comment}
            onChange={(event) => setComment(event.target.value)}
          />
          <button
            type="button"
            data-testid="route53-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
          {createState === 'error' ? (
            <p data-testid="route53-create-error" style={messageStyle}>
              {createError ?? 'Unable to create the hosted zone.'}
            </p>
          ) : null}
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="route53-create-status" style={messageStyle}>
          Hosted zone created.
        </p>
      ) : null}
      <DataListShell
        title="Hosted zones"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter hosted zones"
        columnPrefsKey={`${serviceKey}-hostedzones`}
        emptyState={{ message: 'No Route 53 hosted zones found on this backend.' }}
      />
    </div>
  );
}

export default Route53ListView;
