import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import {
  getApiGatewayRestApis,
  createApiGatewayRestApi,
  deleteApiGatewayRestApi,
} from '../../api/client';
import type { ApiGatewayRestApiItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const monoCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const textCellStyle: CSSProperties = { fontSize: 13 };

const mutedStyle: CSSProperties = { color: '#8b949e' };

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

const endpointTypes = ['EDGE', 'REGIONAL', 'PRIVATE'];

const columns: DataListColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'id', label: 'ID' },
  { key: 'description', label: 'Description' },
  { key: 'createdDate', label: 'Created' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; restApis: ApiGatewayRestApiItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

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
  const [showCreate, setShowCreate] = useState(false);
  const [apiName, setApiName] = useState('');
  const [description, setDescription] = useState('');
  const [version, setVersion] = useState('');
  const [endpointType, setEndpointType] = useState('REGIONAL');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getApiGatewayRestApis(controller.signal)
      .then((result) => setState({ kind: 'ready', restApis: result.restApis }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createApiGatewayRestApi({
      name: apiName,
      description: emptyToNull(description),
      version: emptyToNull(version),
      apiKeySource: null,
      endpointConfigurationTypes: [endpointType],
    })
      .then(() => {
        setCreateState('created');
        setApiName('');
        setDescription('');
        setVersion('');
        setEndpointType('REGIONAL');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (restApiId: string) => {
      deleteApiGatewayRestApi(restApiId)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

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
          <Link
            data-testid="apigateway-list-name"
            to={`/services/${serviceKey}/${encodeURIComponent(restApi.id)}`}
          >
            {restApi.name}
          </Link>
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
        actions: (
          <ConfirmationHost
            actionLabel="Delete"
            prompt={`Delete ${restApi.name}?`}
            confirmLabel="Confirm"
            onConfirm={() => handleDelete(restApi.id)}
          />
        ),
      },
    };
  });

  return (
    <div data-testid="apigateway-list-view">
      <button
        type="button"
        data-testid="apigateway-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create REST API'}
      </button>
      {showCreate ? (
        <div data-testid="apigateway-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigateway-create-name">
              Name
            </label>
            <input
              id="apigateway-create-name"
              type="text"
              data-testid="apigateway-create-name"
              style={inputStyle}
              value={apiName}
              onChange={(event) => setApiName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigateway-create-description">
              Description
            </label>
            <input
              id="apigateway-create-description"
              type="text"
              data-testid="apigateway-create-description"
              style={inputStyle}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigateway-create-version">
              Version
            </label>
            <input
              id="apigateway-create-version"
              type="text"
              data-testid="apigateway-create-version"
              style={inputStyle}
              value={version}
              onChange={(event) => setVersion(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigateway-create-endpoint-type">
              Endpoint type
            </label>
            <select
              id="apigateway-create-endpoint-type"
              data-testid="apigateway-create-endpoint-type"
              style={inputStyle}
              value={endpointType}
              onChange={(event) => setEndpointType(event.target.value)}
            >
              {endpointTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <button
            type="button"
            data-testid="apigateway-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="apigateway-create-status" style={messageStyle}>
          REST API created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="apigateway-create-error" style={messageStyle}>
          Unable to create the REST API.
        </p>
      ) : null}
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
