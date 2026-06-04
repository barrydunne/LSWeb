import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { getRoute53HostedZones } from '../../api/client';
import type { Route53HostedZoneItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const monoCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const textCellStyle: CSSProperties = { fontSize: 13 };

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

  useEffect(() => {
    const controller = new AbortController();
    getRoute53HostedZones(controller.signal)
      .then((result) => setState({ kind: 'ready', hostedZones: result.hostedZones }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
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
        <span data-testid="route53-list-name" style={textCellStyle}>
          {hostedZone.name}
        </span>
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
