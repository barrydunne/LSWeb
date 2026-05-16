import type { CSSProperties } from 'react';
import { Text } from '@primer/react';

const fieldStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  padding: '8px 12px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const labelStyle: CSSProperties = {
  fontSize: 13,
  opacity: 0.8,
};

const valueStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 13,
};

const toggleStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

export function MaskedValueField({
  name,
  value,
  source,
  isSensitive,
  revealed,
  revealAllowed,
  onToggleReveal,
}: {
  name: string;
  value: string;
  source: string;
  isSensitive: boolean;
  revealed: boolean;
  revealAllowed: boolean;
  onToggleReveal: () => void;
}) {
  const showToggle = isSensitive && revealAllowed;

  return (
    <div data-testid="masked-value-field" style={fieldStyle}>
      <Text data-testid="masked-value-name" style={labelStyle}>
        {name}
      </Text>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <Text data-testid="masked-value-source" style={labelStyle}>
          {source}
        </Text>
        <Text data-testid="masked-value-value" style={valueStyle}>
          {value}
        </Text>
        {showToggle ? (
          <button
            type="button"
            data-testid="masked-value-toggle"
            style={toggleStyle}
            onClick={onToggleReveal}
          >
            {revealed ? 'Hide' : 'Reveal'}
          </button>
        ) : null}
      </div>
    </div>
  );
}
