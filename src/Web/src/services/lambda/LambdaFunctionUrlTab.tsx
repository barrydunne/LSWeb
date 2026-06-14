import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import {
  createLambdaFunctionUrl,
  deleteLambdaFunctionUrl,
  getLambdaFunctionUrl,
  testLambdaFunctionUrl,
  updateLambdaFunctionUrl,
} from '../../api/client';
import type { LambdaFunctionUrlResult, LambdaFunctionUrlTestResult } from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const messageStyle: CSSProperties = { fontSize: 14 };
const rowStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 2 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace', wordBreak: 'break-word' };

const formRowStyle: CSSProperties = { display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const preStyle: CSSProperties = {
  margin: 0,
  padding: 12,
  borderRadius: 6,
  background: '#161b22',
  fontFamily: 'monospace',
  fontSize: 12,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
};

const authTypes = ['NONE', 'AWS_IAM'];

type LoadState = 'loading' | 'ready' | 'error';
type TestState = 'idle' | 'running' | 'done' | 'error';

/**
 * Manage a Lambda function's HTTP function URL: create it with an auth mode, view and update it,
 * delete it, and issue a test request against it.
 */
export function LambdaFunctionUrlTab({ functionName }: { functionName: string }) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [config, setConfig] = useState<LambdaFunctionUrlResult | null>(null);
  const [authType, setAuthType] = useState('NONE');
  const [mutationError, setMutationError] = useState(false);
  const [testState, setTestState] = useState<TestState>('idle');
  const [testResult, setTestResult] = useState<LambdaFunctionUrlTestResult | null>(null);

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getLambdaFunctionUrl(functionName, signal)
        .then((data) => {
          setConfig(data);
          if (data.configured) {
            setAuthType(data.authType);
          }
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [functionName],
  );

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const reload = useCallback(() => load(), [load]);

  const handleCreate = () => {
    setMutationError(false);
    createLambdaFunctionUrl(functionName, authType)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleUpdate = () => {
    setMutationError(false);
    updateLambdaFunctionUrl(functionName, authType)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleDelete = () => {
    setMutationError(false);
    setTestState('idle');
    deleteLambdaFunctionUrl(functionName)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleTest = () => {
    setTestState('running');
    testLambdaFunctionUrl(functionName)
      .then((result) => {
        setTestResult(result);
        setTestState('done');
      })
      .catch(() => setTestState('error'));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="lambda-url-loading" style={messageStyle}>
        Loading function URL&hellip;
      </p>
    );
  }

  if (loadState === 'error' || config === null) {
    return (
      <p data-testid="lambda-url-error" style={messageStyle}>
        Unable to load the function URL.
      </p>
    );
  }

  return (
    <div data-testid="lambda-url-tab" style={containerStyle}>
      <div style={formRowStyle}>
        <label style={labelStyle} htmlFor="lambda-url-auth-type">
          Auth mode
        </label>
        <select
          id="lambda-url-auth-type"
          data-testid="lambda-url-auth-type"
          style={inputStyle}
          value={authType}
          onChange={(event) => setAuthType(event.target.value)}
        >
          {authTypes.map((value) => (
            <option key={value} value={value}>
              {value}
            </option>
          ))}
        </select>
      </div>

      {config.configured ? (
        <>
          <div data-testid="lambda-url-value" style={rowStyle}>
            <Text style={labelStyle}>Function URL</Text>
            <a
              data-testid="lambda-url-link"
              style={valueStyle}
              href={config.functionUrl}
              target="_blank"
              rel="noreferrer"
            >
              {config.functionUrl}
            </a>
          </div>
          <div data-testid="lambda-url-current-auth" style={rowStyle}>
            <Text style={labelStyle}>Current auth mode</Text>
            <Text style={valueStyle}>{config.authType}</Text>
          </div>
          <button type="button" data-testid="lambda-url-update" style={buttonStyle} onClick={handleUpdate}>
            Update auth mode
          </button>
          <button type="button" data-testid="lambda-url-test" style={buttonStyle} onClick={handleTest}>
            Test URL
          </button>
          <ConfirmationHost
            actionLabel="Delete function URL"
            prompt={`Delete the function URL for ${functionName}?`}
            confirmLabel="Confirm delete"
            onConfirm={handleDelete}
          />
          {testState === 'running' ? (
            <Text data-testid="lambda-url-test-running" style={messageStyle}>
              Testing&hellip;
            </Text>
          ) : null}
          {testState === 'done' && testResult !== null ? (
            <div data-testid="lambda-url-test-result" style={rowStyle}>
              <Text style={labelStyle}>Status {testResult.statusCode}</Text>
              <pre style={preStyle}>{testResult.body}</pre>
            </div>
          ) : null}
          {testState === 'error' ? (
            <Text data-testid="lambda-url-test-error" style={messageStyle}>
              The test request could not be completed.
            </Text>
          ) : null}
        </>
      ) : (
        <>
          <Text data-testid="lambda-url-empty" style={messageStyle}>
            No function URL is configured for this function.
          </Text>
          <button type="button" data-testid="lambda-url-create" style={buttonStyle} onClick={handleCreate}>
            Create function URL
          </button>
        </>
      )}

      {mutationError ? (
        <Text data-testid="lambda-url-mutation-error" style={messageStyle}>
          The last action could not be completed.
        </Text>
      ) : null}
    </div>
  );
}

export default LambdaFunctionUrlTab;
