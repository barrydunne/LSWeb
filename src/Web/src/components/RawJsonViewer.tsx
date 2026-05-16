import { useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';

const MASK = '********';

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

const buttonGroupStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
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

const preStyle: CSSProperties = {
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

function maskSensitive(value: unknown, sensitiveKeys: ReadonlySet<string>): unknown {
  if (Array.isArray(value)) {
    return value.map((item) => maskSensitive(item, sensitiveKeys));
  }
  if (value !== null && typeof value === 'object') {
    const entries = Object.entries(value as Record<string, unknown>).map(([key, nested]) => {
      if (sensitiveKeys.has(key)) {
        return [key, MASK] as const;
      }
      return [key, maskSensitive(nested, sensitiveKeys)] as const;
    });
    return Object.fromEntries(entries);
  }
  return value;
}

export function RawJsonViewer({
  value,
  title = 'Raw JSON',
  sensitiveKeys = [],
  initiallyExpanded = false,
}: {
  value: unknown;
  title?: string;
  sensitiveKeys?: readonly string[];
  initiallyExpanded?: boolean;
}) {
  const [expanded, setExpanded] = useState(initiallyExpanded);
  const [copied, setCopied] = useState(false);

  const json = useMemo(() => {
    const masked = maskSensitive(value, new Set(sensitiveKeys));
    return JSON.stringify(masked, null, 2);
  }, [value, sensitiveKeys]);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(json);
      setCopied(true);
    } catch {
      setCopied(false);
    }
  };

  return (
    <div data-testid="raw-json-viewer" style={containerStyle}>
      <div style={headerStyle}>
        <Heading as="h3" data-testid="raw-json-title" style={{ fontSize: 14 }}>
          {title}
        </Heading>
        <div style={buttonGroupStyle}>
          {expanded ? (
            <button
              type="button"
              data-testid="raw-json-copy"
              style={buttonStyle}
              onClick={handleCopy}
            >
              {copied ? 'Copied' : 'Copy'}
            </button>
          ) : null}
          <button
            type="button"
            data-testid="raw-json-toggle"
            style={buttonStyle}
            onClick={() => {
              setExpanded((current) => !current);
              setCopied(false);
            }}
          >
            {expanded ? 'Hide' : 'Show'}
          </button>
        </div>
      </div>
      {expanded ? (
        <pre data-testid="raw-json-content" style={preStyle}>
          {json}
        </pre>
      ) : (
        <Text data-testid="raw-json-collapsed" style={{ fontSize: 12, opacity: 0.7 }}>
          Hidden &mdash; select Show to inspect the raw response.
        </Text>
      )}
    </div>
  );
}
