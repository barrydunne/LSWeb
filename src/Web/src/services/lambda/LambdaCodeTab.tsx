import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import { getLambdaFunctionCode } from '../../api/client';
import type { LambdaFunctionCodeResult } from '../../api/client';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const messageStyle: CSSProperties = { fontSize: 14 };

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace', wordBreak: 'break-word' };

const linkStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace', wordBreak: 'break-word' };

type LoadState = 'loading' | 'ready' | 'error';

interface CodeField {
  key: string;
  label: string;
  value: string;
}

/**
 * Format a byte count into a short human-readable string for the code package size.
 */
function formatCodeSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }
  if (bytes < 1024 * 1024) {
    return `${(bytes / 1024).toFixed(1)} KB`;
  }
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/**
 * Read-only viewer of a Lambda function's deployed package and entry point so a developer can
 * inspect what is deployed without leaving the UI.
 */
export function LambdaCodeTab({ functionName }: { functionName: string }) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [code, setCode] = useState<LambdaFunctionCodeResult | null>(null);

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getLambdaFunctionCode(functionName, signal)
        .then((data) => {
          setCode(data);
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

  if (loadState === 'loading') {
    return (
      <p data-testid="lambda-code-loading" style={messageStyle}>
        Loading code&hellip;
      </p>
    );
  }

  if (loadState === 'error' || code === null) {
    return (
      <p data-testid="lambda-code-error" style={messageStyle}>
        Unable to load the function code.
      </p>
    );
  }

  const fields: CodeField[] = [
    { key: 'handler', label: 'Handler (entry point)', value: code.handler || '\u2014' },
    { key: 'runtime', label: 'Runtime', value: code.runtime || '\u2014' },
    { key: 'packageType', label: 'Package type', value: code.packageType || '\u2014' },
    { key: 'codeSize', label: 'Code size', value: formatCodeSize(code.codeSize) },
    { key: 'codeSha256', label: 'Code SHA-256', value: code.codeSha256 || '\u2014' },
    { key: 'repositoryType', label: 'Repository type', value: code.repositoryType || '\u2014' },
  ];

  const isDownloadable = code.location.startsWith('http');

  return (
    <div data-testid="lambda-code-tab" style={containerStyle}>
      {fields.map((field) => (
        <div key={field.key} data-testid={`lambda-code-${field.key}`} style={rowStyle}>
          <Text style={labelStyle}>{field.label}</Text>
          <Text style={valueStyle}>{field.value}</Text>
        </div>
      ))}
      <div data-testid="lambda-code-location" style={rowStyle}>
        <Text style={labelStyle}>Package location</Text>
        {code.location === '' ? (
          <Text data-testid="lambda-code-location-empty" style={valueStyle}>
            {'\u2014'}
          </Text>
        ) : isDownloadable ? (
          <a
            data-testid="lambda-code-download"
            style={linkStyle}
            href={code.location}
            target="_blank"
            rel="noreferrer"
          >
            Download package
          </a>
        ) : (
          <Text data-testid="lambda-code-location-value" style={valueStyle}>
            {code.location}
          </Text>
        )}
      </div>
    </div>
  );
}

export default LambdaCodeTab;
