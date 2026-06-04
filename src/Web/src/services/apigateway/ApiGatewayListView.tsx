import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { getApiGatewayRestApis } from '../../api/client';
import type { ApiGatewayRestApiItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const monoCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const textCellStyle: CSSProperties = { fontSize: 13 };

const mutedStyle: CSSProperties = { color: '#8b949e' };

const columns: DataListColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'id', label: 'ID' },
  { key: 'description', label: 'Description' },
  { key: 'createdDate', label: 'Created' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; restApis: ApiGatewayRestApiItem[] }
  | { kind: 'error' };

function formatCreatedDate(value: string | null): string | null {
  if (!value) {
    return null;
  }
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }
  return parsed.toISOString();
}

export function ApiGatewayListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getApiGatewayRestApis(controller.signal)
      .then((result) => setState({ kind: 'ready', restApis: result.restApis }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="apigateway-list-loading" style={messageStyle}>
        Loading REST APIs&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="apigateway-list-error" style={messageStyle}>
        Unable to load API Gateway REST APIs.
      </p>
    );
  }

  const rows: DataListRow[] = state.restApis.map((restApi) => {
    const created = formatCreatedDate(restApi.createdDate);
    return {
      id: restApi.id,
      filterText: `${restApi.name} ${restApi.id} ${restApi.description ?? ''}`,
      cells: {
        name: (
          <span data-testid="apigateway-list-name" style={textCellStyle}>
            {restApi.name}
          </span>
        ),
        id: (
          <span data-testid="apigateway-list-id" style={monoCellStyle}>
            {restApi.id}
          </span>
        ),
        description: restApi.description ? (
          <span data-testid="apigateway-list-description" style={textCellStyle}>
            {restApi.description}
          </span>
        ) : (
          <span data-testid="apigateway-list-description-empty" style={mutedStyle}>
            &mdash;
          </span>
        ),
        createdDate: created ? (
          <span data-testid="apigateway-list-created" style={monoCellStyle}>
            {created}
          </span>
        ) : (
          <span data-testid="apigateway-list-created-empty" style={mutedStyle}>
            &mdash;
          </span>
        ),
      },
    };
  });

  return (
    <div data-testid="apigateway-list-view">
      <DataListShell
        title="REST APIs"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter REST APIs"
        columnPrefsKey={`${serviceKey}-restapis`}
        emptyState={{ message: 'No API Gateway REST APIs found on this backend.' }}
      />
    </div>
  );
}

export default ApiGatewayListView;
