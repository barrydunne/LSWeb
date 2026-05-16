import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import { getCliSnippet } from '../api/client';
import type { CliSnippetParameter } from '../api/client';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const commandStyle: CSSProperties = {
  margin: 0,
  padding: 12,
  borderRadius: 6,
  background: '#161b22',
  fontFamily: 'monospace',
  fontSize: 12,
  lineHeight: 1.5,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
  overflowX: 'auto',
};

const statusStyle: CSSProperties = {
  fontSize: 12,
  opacity: 0.7,
};

const errorStyle: CSSProperties = {
  fontSize: 12,
  color: '#f85149',
};

export function CopyAsCliButton({
  service,
  operation,
  parameters = [],
  label = 'Copy as CLI',
}: {
  service: string;
  operation: string;
  parameters?: CliSnippetParameter[];
  label?: string;
}) {
  const [command, setCommand] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState(false);

  const handleClick = async () => {
    setBusy(true);
    setError(false);
    setCopied(false);
    try {
      const result = await getCliSnippet({ service, operation, parameters });
      setCommand(result.command);
      await navigator.clipboard.writeText(result.command);
      setCopied(true);
    } catch {
      setError(true);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div data-testid="copy-as-cli" style={containerStyle}>
      <div style={headerStyle}>
        <Text data-testid="copy-as-cli-title" style={{ fontSize: 14, fontWeight: 600 }}>
          AWS CLI equivalent
        </Text>
        <button
          type="button"
          data-testid="copy-as-cli-button"
          style={buttonStyle}
          disabled={busy}
          onClick={handleClick}
        >
          {busy ? 'Generating…' : copied ? 'Copied' : label}
        </button>
      </div>
      {command !== null ? (
        <pre data-testid="copy-as-cli-command" style={commandStyle}>
          {command}
        </pre>
      ) : (
        <Text data-testid="copy-as-cli-hint" style={statusStyle}>
          Select {label} to generate a runnable command with secrets masked.
        </Text>
      )}
      {error ? (
        <Text data-testid="copy-as-cli-error" style={errorStyle}>
          Unable to generate the CLI snippet.
        </Text>
      ) : null}
    </div>
  );
}
