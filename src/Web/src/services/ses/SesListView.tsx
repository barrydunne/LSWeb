import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { getSesIdentities } from '../../api/client';
import type { SesIdentityItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const monoCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const badgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
  fontFamily: 'monospace',
};

const columns: DataListColumn[] = [
  { key: 'identity', label: 'Identity' },
  { key: 'identityType', label: 'Type' },
  { key: 'verificationStatus', label: 'Verification' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; identities: SesIdentityItem[] }
  | { kind: 'error' };

export function SesListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getSesIdentities(controller.signal)
      .then((result) => setState({ kind: 'ready', identities: result.identities }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="ses-list-loading" style={messageStyle}>
        Loading identities&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="ses-list-error" style={messageStyle}>
        Unable to load SES identities.
      </p>
    );
  }

  const rows: DataListRow[] = state.identities.map((identity) => ({
    id: identity.identity,
    filterText: `${identity.identity} ${identity.identityType} ${identity.verificationStatus}`,
    cells: {
      identity: (
        <span data-testid="ses-list-identity" style={monoCellStyle}>
          {identity.identity}
        </span>
      ),
      identityType: (
        <span data-testid="ses-list-type" style={badgeStyle}>
          {identity.identityType}
        </span>
      ),
      verificationStatus: (
        <span data-testid="ses-list-verification" style={badgeStyle}>
          {identity.verificationStatus}
        </span>
      ),
    },
  }));

  return (
    <div data-testid="ses-list-view">
      <DataListShell
        title="Identities"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter identities"
        columnPrefsKey={`${serviceKey}-identities`}
        emptyState={{ message: 'No SES identities found on this backend.' }}
      />
    </div>
  );
}

export default SesListView;
