import { type CSSProperties, type ReactNode } from 'react';
import { Text } from '@primer/react';

export type EmptyStateVariant = 'no-resources' | 'no-matches';

export interface EmptyStateProps {
  variant: EmptyStateVariant;
  message?: string;
  cliHint?: string;
  action?: ReactNode;
}

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  gap: 12,
  padding: 32,
  textAlign: 'center',
  borderRadius: 6,
  border: '1px dashed #30363d',
  background: '#0d1117',
};

const iconStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  width: 40,
  height: 40,
  borderRadius: '50%',
  border: '1px solid #30363d',
  color: '#8b949e',
  fontSize: 20,
};

const cliHintStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 13,
  color: '#c9d1d9',
  background: '#161b22',
  border: '1px solid #30363d',
  borderRadius: 6,
  padding: '6px 10px',
};

const messageStyle: CSSProperties = {
  color: '#c9d1d9',
};

const defaultMessages: Record<EmptyStateVariant, string> = {
  'no-resources': 'No resources yet. Create one to get started.',
  'no-matches': 'No matches for the current filter. Try adjusting your search.',
};

export function EmptyState({ variant, message, cliHint, action }: EmptyStateProps) {
  return (
    <div data-testid="empty-state" data-variant={variant} style={containerStyle}>
      <span aria-hidden="true" style={iconStyle}>
        {variant === 'no-matches' ? '?' : '+'}
      </span>
      <Text data-testid="empty-state-message" style={messageStyle}>
        {message ?? defaultMessages[variant]}
      </Text>
      {cliHint ? (
        <code data-testid="empty-state-cli-hint" style={cliHintStyle}>
          {cliHint}
        </code>
      ) : null}
      {action ? <div data-testid="empty-state-action">{action}</div> : null}
    </div>
  );
}
