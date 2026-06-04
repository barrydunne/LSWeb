import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { getAcmCertificates } from '../../api/client';
import type { AcmCertificateItem } from '../../api/client';
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

const mutedStyle: CSSProperties = { color: '#8b949e' };

const columns: DataListColumn[] = [
  { key: 'domainName', label: 'Domain' },
  { key: 'status', label: 'Status' },
  { key: 'type', label: 'Type' },
  { key: 'arn', label: 'ARN' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; certificates: AcmCertificateItem[] }
  | { kind: 'error' };

export function AcmListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getAcmCertificates(controller.signal)
      .then((result) => setState({ kind: 'ready', certificates: result.certificates }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="acm-list-loading" style={messageStyle}>
        Loading certificates&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="acm-list-error" style={messageStyle}>
        Unable to load ACM certificates.
      </p>
    );
  }

  const rows: DataListRow[] = state.certificates.map((certificate) => ({
    id: certificate.arn,
    filterText: `${certificate.domainName} ${certificate.status} ${certificate.type ?? ''} ${certificate.arn}`,
    cells: {
      domainName: (
        <span data-testid="acm-list-domain" style={monoCellStyle}>
          {certificate.domainName}
        </span>
      ),
      status: (
        <span data-testid="acm-list-status" style={badgeStyle}>
          {certificate.status}
        </span>
      ),
      type: certificate.type ? (
        <span data-testid="acm-list-type" style={badgeStyle}>
          {certificate.type}
        </span>
      ) : (
        <span data-testid="acm-list-type-empty" style={mutedStyle}>
          &mdash;
        </span>
      ),
      arn: (
        <span data-testid="acm-list-arn" style={monoCellStyle}>
          {certificate.arn}
        </span>
      ),
    },
  }));

  return (
    <div data-testid="acm-list-view">
      <DataListShell
        title="Certificates"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter certificates"
        columnPrefsKey={`${serviceKey}-certificates`}
        emptyState={{ message: 'No ACM certificates found on this backend.' }}
      />
    </div>
  );
}

export default AcmListView;
