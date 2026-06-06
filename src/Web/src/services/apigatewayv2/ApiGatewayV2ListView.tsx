import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createHttpApi, deleteHttpApi, getHttpApis } from '../../api/client';
import type { HttpApiSummaryItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const idCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

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

const protocolTypes = ['HTTP', 'WEBSOCKET'];

const columns: DataListColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'id', label: 'API ID' },
  { key: 'protocol', label: 'Protocol' },
  { key: 'created', label: 'Created' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; apis: HttpApiSummaryItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

export function ApiGatewayV2ListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [apiName, setApiName] = useState('');
  const [protocolType, setProtocolType] = useState('HTTP');
  const [description, setDescription] = useState('');
  const [version, setVersion] = useState('');
  const [routeSelectionExpression, setRouteSelectionExpression] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getHttpApis(controller.signal)
      .then((result) => setState({ kind: 'ready', apis: result.apis }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createHttpApi({
      name: apiName,
      protocolType,
      description: emptyToNull(description),
      version: emptyToNull(version),
      routeSelectionExpression: emptyToNull(routeSelectionExpression),
    })
      .then(() => {
        setCreateState('created');
        setApiName('');
        setDescription('');
        setVersion('');
        setRouteSelectionExpression('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (apiId: string) => {
      deleteHttpApi(apiId)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="apigatewayv2-list-loading" style={messageStyle}>
        Loading HTTP APIs&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="apigatewayv2-list-error" style={messageStyle}>
        Unable to load API Gateway v2 APIs.
      </p>
    );
  }

  const rows: DataListRow[] = state.apis.map((api) => ({
    id: api.apiId,
    filterText: `${api.name} ${api.apiId} ${api.protocolType}`,
    cells: {
      name: (
        <Link
          data-testid="apigatewayv2-list-name"
          to={`/services/${serviceKey}/${encodeURIComponent(api.apiId)}`}
        >
          {api.name}
        </Link>
      ),
      id: (
        <span data-testid="apigatewayv2-list-id" style={idCellStyle}>
          {api.apiId}
        </span>
      ),
      protocol: (
        <span data-testid="apigatewayv2-list-protocol" style={messageStyle}>
          {api.protocolType}
        </span>
      ),
      created: (
        <span data-testid="apigatewayv2-list-created" style={messageStyle}>
          {api.createdDate ?? '\u2014'}
        </span>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${api.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(api.apiId)}
        />
      ),
    },
  }));

  return (
    <div data-testid="apigatewayv2-list-view">
      <button
        type="button"
        data-testid="apigatewayv2-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create API'}
      </button>
      {showCreate ? (
        <div data-testid="apigatewayv2-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-create-name">
              Name
            </label>
            <input
              id="apigatewayv2-create-name"
              type="text"
              data-testid="apigatewayv2-create-name"
              style={inputStyle}
              value={apiName}
              onChange={(event) => setApiName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-create-protocol">
              Protocol type
            </label>
            <select
              id="apigatewayv2-create-protocol"
              data-testid="apigatewayv2-create-protocol"
              style={inputStyle}
              value={protocolType}
              onChange={(event) => setProtocolType(event.target.value)}
            >
              {protocolTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-create-description">
              Description
            </label>
            <input
              id="apigatewayv2-create-description"
              type="text"
              data-testid="apigatewayv2-create-description"
              style={inputStyle}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-create-version">
              Version
            </label>
            <input
              id="apigatewayv2-create-version"
              type="text"
              data-testid="apigatewayv2-create-version"
              style={inputStyle}
              value={version}
              onChange={(event) => setVersion(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-create-route-selection">
              Route selection expression
            </label>
            <input
              id="apigatewayv2-create-route-selection"
              type="text"
              data-testid="apigatewayv2-create-route-selection"
              style={inputStyle}
              value={routeSelectionExpression}
              onChange={(event) => setRouteSelectionExpression(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="apigatewayv2-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="apigatewayv2-create-status" style={messageStyle}>
          API created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="apigatewayv2-create-error" style={messageStyle}>
          Unable to create the API.
        </p>
      ) : null}
      <DataListShell
        title="HTTP APIs"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter APIs"
        columnPrefsKey={`${serviceKey}-apis`}
        emptyState={{ message: 'No API Gateway v2 APIs found on this backend.' }}
      />
    </div>
  );
}

export default ApiGatewayV2ListView;
