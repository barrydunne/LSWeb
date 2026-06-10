import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { getApiGatewayRestApi, updateApiGatewayRestApi } from '../../api/client';
import type { ApiGatewayRestApiDetailResult } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { RawJsonViewer } from '../../components/RawJsonViewer';
import { ApiGatewayResourcesSection } from './ApiGatewayResourcesSection';
import { ApiGatewayAuthorizersSection } from './ApiGatewayAuthorizersSection';
import { ApiGatewayStagesSection } from './ApiGatewayStagesSection';
import { ApiGatewayTestInvokeSection } from './ApiGatewayTestInvokeSection';
import { ApiGatewayCorsSection } from './ApiGatewayCorsSection';
import { ApiGatewayProtectedRouteVerificationSection } from './ApiGatewayProtectedRouteVerificationSection';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

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

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; api: ApiGatewayRestApiDetailResult }
  | { kind: 'error' };

type SaveState = 'idle' | 'saving' | 'saved' | 'error';

function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

function formatList(values: string[]): string {
  return values.length === 0 ? '\u2014' : values.join(', ');
}

export function ApiGatewayDetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  const [editing, setEditing] = useState(false);
  const [editName, setEditName] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [saveState, setSaveState] = useState<SaveState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getApiGatewayRestApi(resourceId, controller.signal)
      .then((api) => setState({ kind: 'ready', api }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleStartEdit = (api: ApiGatewayRestApiDetailResult) => {
    setEditName(api.name);
    setEditDescription(api.description ?? '');
    setSaveState('idle');
    setEditing(true);
  };

  const handleUpdate = () => {
    setSaveState('saving');
    updateApiGatewayRestApi(resourceId, {
      name: editName,
      description: emptyToNull(editDescription),
    })
      .then(() => {
        setSaveState('saved');
        setEditing(false);
        refresh();
      })
      .catch(() => setSaveState('error'));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="apigateway-detail-loading" style={messageStyle}>
        Loading REST API&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="apigateway-detail-error" style={messageStyle}>
        Unable to load the REST API.
      </p>
    );
  }

  const api = state.api;

  return (
    <div data-testid="apigateway-detail-view" style={containerStyle}>
      <Heading as="h2" data-testid="apigateway-detail-name" style={{ fontSize: 18 }}>
        {api.name}
      </Heading>
      <div style={rowStyle}>
        <span style={labelStyle}>REST API ID</span>
        <span data-testid="apigateway-detail-id" style={valueStyle}>
          {api.id}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Description</span>
        <span data-testid="apigateway-detail-description" style={valueStyle}>
          {api.description ?? '\u2014'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Version</span>
        <span data-testid="apigateway-detail-version" style={valueStyle}>
          {api.version ?? '\u2014'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>API key source</span>
        <span data-testid="apigateway-detail-api-key-source" style={valueStyle}>
          {api.apiKeySource ?? '\u2014'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Endpoint configuration types</span>
        <span data-testid="apigateway-detail-endpoint-types" style={valueStyle}>
          {formatList(api.endpointConfigurationTypes)}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Binary media types</span>
        <span data-testid="apigateway-detail-binary-media-types" style={valueStyle}>
          {formatList(api.binaryMediaTypes)}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Created</span>
        <span data-testid="apigateway-detail-created" style={valueStyle}>
          {api.createdDate ?? '\u2014'}
        </span>
      </div>

      <button
        type="button"
        data-testid="apigateway-edit-toggle"
        style={buttonStyle}
        onClick={() => (editing ? setEditing(false) : handleStartEdit(api))}
      >
        {editing ? 'Cancel' : 'Edit REST API'}
      </button>

      {editing ? (
        <div data-testid="apigateway-edit-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigateway-edit-name">
              Name
            </label>
            <input
              id="apigateway-edit-name"
              type="text"
              data-testid="apigateway-edit-name"
              style={inputStyle}
              value={editName}
              onChange={(event) => setEditName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigateway-edit-description">
              Description
            </label>
            <input
              id="apigateway-edit-description"
              type="text"
              data-testid="apigateway-edit-description"
              style={inputStyle}
              value={editDescription}
              onChange={(event) => setEditDescription(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="apigateway-edit-submit"
            style={buttonStyle}
            disabled={saveState === 'saving'}
            onClick={handleUpdate}
          >
            {saveState === 'saving' ? 'Saving\u2026' : 'Save'}
          </button>
        </div>
      ) : null}

      {saveState === 'saved' ? (
        <p data-testid="apigateway-edit-status" style={messageStyle}>
          REST API updated.
        </p>
      ) : null}
      {saveState === 'error' ? (
        <p data-testid="apigateway-edit-error" style={messageStyle}>
          Unable to update the REST API.
        </p>
      ) : null}

      <ApiGatewayResourcesSection restApiId={api.id} />

      <ApiGatewayAuthorizersSection restApiId={api.id} />

      <ApiGatewayStagesSection restApiId={api.id} />

        <ApiGatewayProtectedRouteVerificationSection restApiId={api.id} />

      <ApiGatewayTestInvokeSection restApiId={api.id} />

      <ApiGatewayCorsSection restApiId={api.id} />

      <RawJsonViewer value={api} title="Raw REST API" />
    </div>
  );
}

export default ApiGatewayDetailView;
