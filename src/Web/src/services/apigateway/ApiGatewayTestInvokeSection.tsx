import { useEffect, useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import {
  getApiGatewayRestResources,
  testInvokeApiGatewayRestMethod,
} from '../../api/client';
import type {
  ApiGatewayRestResourceItem,
  ApiGatewayRestMethodTestInvokeResult,
} from '../../api/client';

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

const textareaStyle: CSSProperties = {
  ...inputStyle,
  minHeight: 72,
  fontFamily: 'monospace',
  resize: 'vertical',
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

const outputStyle: CSSProperties = {
  ...formStyle,
  background: '#11161d',
};

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; resources: ApiGatewayRestResourceItem[] }
  | { kind: 'error' };

function parseKeyValueLines(input: string): Record<string, string> {
  if (input.trim() === '') {
    return {};
  }

  const entries = input
    .split('\n')
    .map((line) => line.trim())
    .filter((line) => line !== '')
    .map((line) => {
      const separatorIndex = line.indexOf(':');
      if (separatorIndex < 1) {
        return null;
      }

      const key = line.slice(0, separatorIndex).trim();
      const value = line.slice(separatorIndex + 1).trim();
      return [key, value] as const;
    })
    .filter((entry): entry is readonly [string, string] => entry !== null);

  return Object.fromEntries(entries);
}

export function ApiGatewayTestInvokeSection({ restApiId }: { restApiId: string }) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [resourceId, setResourceId] = useState('');
  const [httpMethod, setHttpMethod] = useState('');
  const [pathWithQueryString, setPathWithQueryString] = useState('/');
  const [headersText, setHeadersText] = useState('');
  const [queryText, setQueryText] = useState('');
  const [stageVariablesText, setStageVariablesText] = useState('');
  const [body, setBody] = useState('');
  const [invoking, setInvoking] = useState(false);
  const [invokeError, setInvokeError] = useState(false);
  const [result, setResult] = useState<ApiGatewayRestMethodTestInvokeResult | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getApiGatewayRestResources(restApiId, controller.signal)
      .then((response) => {
        const resources = response.resources;
        setState({ kind: 'ready', resources });

        if (resources.length > 0) {
          const firstResource = resources[0];
          setResourceId(firstResource.id);
          setHttpMethod(firstResource.resourceMethods[0] ?? '');
          setPathWithQueryString(firstResource.path || '/');
        }
      })
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [restApiId]);

  const selectedResource = useMemo(() => {
    if (state.kind !== 'ready') {
      return null;
    }
    return state.resources.find((resource) => resource.id === resourceId) ?? null;
  }, [state, resourceId]);

  useEffect(() => {
    if (!selectedResource) {
      return;
    }

    setHttpMethod(selectedResource.resourceMethods[0] ?? '');
    setPathWithQueryString(selectedResource.path || '/');
  }, [selectedResource]);

  const canInvoke = resourceId !== '' && httpMethod !== '' && pathWithQueryString.trim() !== '';

  const handleInvoke = () => {
    setInvoking(true);
    setInvokeError(false);
    setResult(null);

    testInvokeApiGatewayRestMethod(restApiId, resourceId, httpMethod, {
      pathWithQueryString: pathWithQueryString.trim(),
      headers: parseKeyValueLines(headersText),
      queryStringParameters: parseKeyValueLines(queryText),
      body: body.trim() === '' ? null : body,
      stageVariables: parseKeyValueLines(stageVariablesText),
    })
      .then((response) => setResult(response))
      .catch(() => setInvokeError(true))
      .finally(() => setInvoking(false));
  };

  return (
    <div data-testid="apigateway-test-invoke-section" style={sectionStyle}>
      <Heading as="h3" data-testid="apigateway-test-invoke-heading" style={sectionHeadingStyle}>
        Test invoke method
      </Heading>

      {state.kind === 'loading' && (
        <p data-testid="apigateway-test-invoke-loading" style={messageStyle}>
          Loading resources for test invocation&hellip;
        </p>
      )}

      {state.kind === 'error' && (
        <p data-testid="apigateway-test-invoke-load-error" style={messageStyle}>
          Unable to load resources for test invocation.
        </p>
      )}

      {state.kind === 'ready' && state.resources.length === 0 && (
        <p data-testid="apigateway-test-invoke-empty" style={messageStyle}>
          No resources found.
        </p>
      )}

      {state.kind === 'ready' && state.resources.length > 0 && (
        <div style={formStyle}>
          <label style={labelStyle} htmlFor="apigateway-test-invoke-resource">
            Resource
          </label>
          <select
            id="apigateway-test-invoke-resource"
            data-testid="apigateway-test-invoke-resource"
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

          <label style={labelStyle} htmlFor="apigateway-test-invoke-method">
            Method
          </label>
          <select
            id="apigateway-test-invoke-method"
            data-testid="apigateway-test-invoke-method"
            style={inputStyle}
            value={httpMethod}
            onChange={(event) => setHttpMethod(event.target.value)}
          >
            {selectedResource!.resourceMethods.map((method) => (
              <option key={method} value={method}>
                {method}
              </option>
            ))}
          </select>

          <label style={labelStyle} htmlFor="apigateway-test-invoke-path">
            Path with query string
          </label>
          <input
            id="apigateway-test-invoke-path"
            data-testid="apigateway-test-invoke-path"
            style={inputStyle}
            value={pathWithQueryString}
            onChange={(event) => setPathWithQueryString(event.target.value)}
          />

          <label style={labelStyle} htmlFor="apigateway-test-invoke-headers">
            Headers (key:value per line)
          </label>
          <textarea
            id="apigateway-test-invoke-headers"
            data-testid="apigateway-test-invoke-headers"
            style={textareaStyle}
            value={headersText}
            onChange={(event) => setHeadersText(event.target.value)}
          />

          <label style={labelStyle} htmlFor="apigateway-test-invoke-query">
            Query parameters (key:value per line)
          </label>
          <textarea
            id="apigateway-test-invoke-query"
            data-testid="apigateway-test-invoke-query"
            style={textareaStyle}
            value={queryText}
            onChange={(event) => setQueryText(event.target.value)}
          />

          <label style={labelStyle} htmlFor="apigateway-test-invoke-stage-variables">
            Stage variables (key:value per line)
          </label>
          <textarea
            id="apigateway-test-invoke-stage-variables"
            data-testid="apigateway-test-invoke-stage-variables"
            style={textareaStyle}
            value={stageVariablesText}
            onChange={(event) => setStageVariablesText(event.target.value)}
          />

          <label style={labelStyle} htmlFor="apigateway-test-invoke-body">
            Body (optional)
          </label>
          <textarea
            id="apigateway-test-invoke-body"
            data-testid="apigateway-test-invoke-body"
            style={textareaStyle}
            value={body}
            onChange={(event) => setBody(event.target.value)}
          />

          <button
            type="button"
            data-testid="apigateway-test-invoke-submit"
            style={buttonStyle}
            disabled={!canInvoke || invoking}
            onClick={handleInvoke}
          >
            {invoking ? 'Invoking…' : 'Invoke'}
          </button>
        </div>
      )}

      {invokeError && (
        <p data-testid="apigateway-test-invoke-error" style={messageStyle}>
          Unable to test invoke the method.
        </p>
      )}

      {result && (
        <div data-testid="apigateway-test-invoke-result" style={outputStyle}>
          <div>
            <span style={labelStyle}>Status</span>
            <div data-testid="apigateway-test-invoke-status" style={messageStyle}>
              {result.statusCode}
            </div>
          </div>
          <div>
            <span style={labelStyle}>Latency (ms)</span>
            <div data-testid="apigateway-test-invoke-latency" style={messageStyle}>
              {result.latencyMilliseconds}
            </div>
          </div>
          <div>
            <span style={labelStyle}>Headers</span>
            <pre data-testid="apigateway-test-invoke-response-headers" style={textareaStyle}>
              {JSON.stringify(result.headers, null, 2)}
            </pre>
          </div>
          <div>
            <span style={labelStyle}>Body</span>
            <pre data-testid="apigateway-test-invoke-response-body" style={textareaStyle}>
              {result.body || ''}
            </pre>
          </div>
          <div>
            <span style={labelStyle}>Execution log</span>
            <pre data-testid="apigateway-test-invoke-response-log" style={textareaStyle}>
              {result.log || ''}
            </pre>
          </div>
        </div>
      )}
    </div>
  );
}

export default ApiGatewayTestInvokeSection;
