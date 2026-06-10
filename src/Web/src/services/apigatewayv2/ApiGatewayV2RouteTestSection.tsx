import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { testHttpApiRoute } from '../../api/client';
import type { HttpRouteInvocationResult } from '../../api/client';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

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

const messageStyle: CSSProperties = { fontSize: 14 };

const resultPanelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const authorizedBannerStyle: CSSProperties = {
  fontSize: 13,
  fontWeight: 600,
  alignSelf: 'flex-start',
  padding: '2px 10px',
  borderRadius: 999,
  border: '1px solid currentColor',
  color: '#3fb950',
};

const unauthorizedBannerStyle: CSSProperties = {
  fontSize: 13,
  fontWeight: 600,
  alignSelf: 'flex-start',
  padding: '2px 10px',
  borderRadius: 999,
  border: '1px solid currentColor',
  color: '#f85149',
};

const bodyStyle: CSSProperties = {
  fontSize: 12,
  fontFamily: 'monospace',
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
  margin: 0,
};

const httpMethods = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'HEAD', 'OPTIONS'];

type InvokeState =
  | { kind: 'idle' }
  | { kind: 'running' }
  | { kind: 'ready'; result: HttpRouteInvocationResult }
  | { kind: 'error' };

export interface ApiGatewayV2RouteTestSectionProps {
  apiId: string;
}

export function ApiGatewayV2RouteTestSection({ apiId }: ApiGatewayV2RouteTestSectionProps) {
  const [stage, setStage] = useState('$default');
  const [method, setMethod] = useState('GET');
  const [path, setPath] = useState('/');
  const [token, setToken] = useState('');
  const [body, setBody] = useState('');
  const [invokeState, setInvokeState] = useState<InvokeState>({ kind: 'idle' });

  const handleSend = () => {
    setInvokeState({ kind: 'running' });
    testHttpApiRoute(apiId, {
      stage,
      method,
      path,
      token: token.trim() === '' ? null : token,
      body: body === '' ? null : body,
    })
      .then((result) => setInvokeState({ kind: 'ready', result }))
      .catch(() => setInvokeState({ kind: 'error' }));
  };

  return (
    <div data-testid="apigatewayv2-route-test-section" style={sectionStyle}>
      <Heading as="h3" style={{ fontSize: 15 }}>
        Test invoke route
      </Heading>
      <p style={messageStyle}>
        Send a live request to this API to confirm whether a route is protected. Provide a bearer
        token to verify authorized access, or leave it blank to confirm the request is rejected.
      </p>
      <div style={formStyle}>
        <div style={fieldRowStyle}>
          <label style={labelStyle} htmlFor="apigatewayv2-route-test-stage">
            Stage
          </label>
          <input
            id="apigatewayv2-route-test-stage"
            data-testid="apigatewayv2-route-test-stage"
            style={inputStyle}
            value={stage}
            onChange={(event) => setStage(event.target.value)}
          />
        </div>
        <div style={fieldRowStyle}>
          <label style={labelStyle} htmlFor="apigatewayv2-route-test-method">
            Method
          </label>
          <select
            id="apigatewayv2-route-test-method"
            data-testid="apigatewayv2-route-test-method"
            style={inputStyle}
            value={method}
            onChange={(event) => setMethod(event.target.value)}
          >
            {httpMethods.map((httpMethod) => (
              <option key={httpMethod} value={httpMethod}>
                {httpMethod}
              </option>
            ))}
          </select>
        </div>
        <div style={fieldRowStyle}>
          <label style={labelStyle} htmlFor="apigatewayv2-route-test-path">
            Path
          </label>
          <input
            id="apigatewayv2-route-test-path"
            data-testid="apigatewayv2-route-test-path"
            style={inputStyle}
            value={path}
            onChange={(event) => setPath(event.target.value)}
          />
        </div>
        <div style={fieldRowStyle}>
          <label style={labelStyle} htmlFor="apigatewayv2-route-test-token">
            Bearer token
          </label>
          <input
            id="apigatewayv2-route-test-token"
            data-testid="apigatewayv2-route-test-token"
            style={inputStyle}
            value={token}
            onChange={(event) => setToken(event.target.value)}
          />
        </div>
        <div style={fieldRowStyle}>
          <label style={labelStyle} htmlFor="apigatewayv2-route-test-body">
            Request body
          </label>
          <textarea
            id="apigatewayv2-route-test-body"
            data-testid="apigatewayv2-route-test-body"
            style={{ ...inputStyle, minHeight: 60, fontFamily: 'monospace' }}
            value={body}
            onChange={(event) => setBody(event.target.value)}
          />
        </div>
        <button
          type="button"
          data-testid="apigatewayv2-route-test-send"
          style={buttonStyle}
          onClick={handleSend}
          disabled={invokeState.kind === 'running'}
        >
          {invokeState.kind === 'running' ? 'Sending\u2026' : 'Send request'}
        </button>
      </div>

      {invokeState.kind === 'error' ? (
        <p data-testid="apigatewayv2-route-test-error" style={messageStyle}>
          Unable to invoke the route.
        </p>
      ) : null}

      {invokeState.kind === 'ready' ? (
        <div data-testid="apigatewayv2-route-test-result" style={resultPanelStyle}>
          {invokeState.result.authorized ? (
            <span
              data-testid="apigatewayv2-route-test-authorized"
              style={authorizedBannerStyle}
            >
              Authorized
            </span>
          ) : (
            <span
              data-testid="apigatewayv2-route-test-unauthorized"
              style={unauthorizedBannerStyle}
            >
              Unauthorized
            </span>
          )}
          <span data-testid="apigatewayv2-route-test-status" style={messageStyle}>
            Status {invokeState.result.statusCode}
          </span>
          <span data-testid="apigatewayv2-route-test-latency" style={messageStyle}>
            {invokeState.result.latencyMilliseconds} ms
          </span>
          <pre data-testid="apigatewayv2-route-test-body-result" style={bodyStyle}>
            {invokeState.result.body}
          </pre>
        </div>
      ) : null}
    </div>
  );
}
