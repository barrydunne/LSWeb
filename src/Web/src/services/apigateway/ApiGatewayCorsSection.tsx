import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import {
  configureApiGatewayRestCors,
  getApiGatewayRestCors,
  getApiGatewayRestResources,
} from '../../api/client';
import type { ApiGatewayRestResourceItem } from '../../api/client';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const sectionHeadingStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
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
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const buttonRowStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
};

const presetOrigins = '*';
const presetMethods = 'GET, POST, PUT, DELETE, OPTIONS';
const presetHeaders = 'Content-Type, Authorization, X-Amz-Date, X-Api-Key, X-Amz-Security-Token';

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; resources: ApiGatewayRestResourceItem[] }
  | { kind: 'error' };

function splitList(input: string): string[] {
  return input
    .split(',')
    .map((value) => value.trim())
    .filter((value) => value !== '');
}

export function ApiGatewayCorsSection({ restApiId }: { restApiId: string }) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [resourceId, setResourceId] = useState('');
  const [allowOrigins, setAllowOrigins] = useState('');
  const [allowMethods, setAllowMethods] = useState('');
  const [allowHeaders, setAllowHeaders] = useState('');
  const [enabled, setEnabled] = useState(false);
  const [corsLoading, setCorsLoading] = useState(false);
  const [corsError, setCorsError] = useState(false);
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getApiGatewayRestResources(restApiId, controller.signal)
      .then((response) => {
        const resources = response.resources;
        setState({ kind: 'ready', resources });
        if (resources.length > 0) {
          setResourceId(resources[0].id);
        }
      })
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [restApiId]);

  useEffect(() => {
    if (resourceId === '') {
      return;
    }

    const controller = new AbortController();
    setCorsLoading(true);
    setCorsError(false);
    setSaveError(false);
    setSaveSuccess(false);
    getApiGatewayRestCors(restApiId, resourceId, controller.signal)
      .then((result) => {
        setEnabled(result.enabled);
        setAllowOrigins(result.allowOrigins.join(', '));
        setAllowMethods(result.allowMethods.join(', '));
        setAllowHeaders(result.allowHeaders.join(', '));
      })
      .catch(() => setCorsError(true))
      .finally(() => setCorsLoading(false));
    return () => controller.abort();
  }, [restApiId, resourceId]);

  const applyPreset = () => {
    setAllowOrigins(presetOrigins);
    setAllowMethods(presetMethods);
    setAllowHeaders(presetHeaders);
  };

  const handleSave = () => {
    setSaving(true);
    setSaveError(false);
    setSaveSuccess(false);
    configureApiGatewayRestCors(restApiId, resourceId, {
      allowOrigins: splitList(allowOrigins),
      allowMethods: splitList(allowMethods),
      allowHeaders: splitList(allowHeaders),
    })
      .then(() => {
        setSaveSuccess(true);
        setEnabled(true);
      })
      .catch(() => setSaveError(true))
      .finally(() => setSaving(false));
  };

  return (
    <div data-testid="apigateway-cors-section" style={sectionStyle}>
      <Heading as="h3" data-testid="apigateway-cors-heading" style={sectionHeadingStyle}>
        CORS configuration
      </Heading>

      {state.kind === 'loading' && (
        <p data-testid="apigateway-cors-loading" style={messageStyle}>
          Loading resources for CORS configuration&hellip;
        </p>
      )}

      {state.kind === 'error' && (
        <p data-testid="apigateway-cors-load-error" style={messageStyle}>
          Unable to load resources for CORS configuration.
        </p>
      )}

      {state.kind === 'ready' && state.resources.length === 0 && (
        <p data-testid="apigateway-cors-empty" style={messageStyle}>
          No resources found.
        </p>
      )}

      {state.kind === 'ready' && state.resources.length > 0 && (
        <div style={formStyle}>
          <label style={labelStyle} htmlFor="apigateway-cors-resource">
            Resource
          </label>
          <select
            id="apigateway-cors-resource"
            data-testid="apigateway-cors-resource"
            style={inputStyle}
            value={resourceId}
            onChange={(event) => setResourceId(event.target.value)}
          >
            {state.resources.map((resource) => (
              <option key={resource.id} value={resource.id}>
                {resource.path} ({resource.id})
              </option>
            ))}
          </select>

          <span data-testid="apigateway-cors-status" style={labelStyle}>
            {enabled ? 'CORS is enabled for this resource.' : 'CORS is not configured for this resource.'}
          </span>

          {corsLoading && (
            <p data-testid="apigateway-cors-config-loading" style={messageStyle}>
              Loading CORS configuration&hellip;
            </p>
          )}

          {corsError && (
            <p data-testid="apigateway-cors-config-error" style={messageStyle}>
              Unable to load CORS configuration.
            </p>
          )}

          <label style={labelStyle} htmlFor="apigateway-cors-allow-origins">
            Allowed origins (comma separated)
          </label>
          <input
            id="apigateway-cors-allow-origins"
            data-testid="apigateway-cors-allow-origins"
            style={inputStyle}
            value={allowOrigins}
            onChange={(event) => setAllowOrigins(event.target.value)}
          />

          <label style={labelStyle} htmlFor="apigateway-cors-allow-methods">
            Allowed methods (comma separated)
          </label>
          <input
            id="apigateway-cors-allow-methods"
            data-testid="apigateway-cors-allow-methods"
            style={inputStyle}
            value={allowMethods}
            onChange={(event) => setAllowMethods(event.target.value)}
          />

          <label style={labelStyle} htmlFor="apigateway-cors-allow-headers">
            Allowed headers (comma separated)
          </label>
          <input
            id="apigateway-cors-allow-headers"
            data-testid="apigateway-cors-allow-headers"
            style={inputStyle}
            value={allowHeaders}
            onChange={(event) => setAllowHeaders(event.target.value)}
          />

          <div style={buttonRowStyle}>
            <button
              type="button"
              data-testid="apigateway-cors-preset"
              style={buttonStyle}
              onClick={applyPreset}
            >
              Apply permissive preset
            </button>
            <button
              type="button"
              data-testid="apigateway-cors-save"
              style={buttonStyle}
              disabled={saving}
              onClick={handleSave}
            >
              {saving ? 'Saving…' : 'Save CORS configuration'}
            </button>
          </div>
        </div>
      )}

      {saveError && (
        <p data-testid="apigateway-cors-save-error" style={messageStyle}>
          Unable to save the CORS configuration.
        </p>
      )}

      {saveSuccess && (
        <p data-testid="apigateway-cors-save-success" style={messageStyle}>
          CORS configuration saved.
        </p>
      )}
    </div>
  );
}

export default ApiGatewayCorsSection;
