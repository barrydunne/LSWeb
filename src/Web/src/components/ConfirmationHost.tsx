import type { CSSProperties } from 'react';
import { Button, Text } from '@primer/react';
import { useConfirm } from '../hooks/useConfirm';

const hostStyle = { display: 'flex', alignItems: 'center', gap: 8 };

const overlayStyle: CSSProperties = {
  position: 'fixed',
  inset: 0,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  background: 'rgba(1, 4, 9, 0.6)',
  zIndex: 1100,
};

const dialogStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 16,
  padding: 20,
  borderRadius: 8,
  border: '1px solid #30363d',
  background: '#161b22',
  minWidth: 320,
  maxWidth: 420,
};

const actionsStyle: CSSProperties = { display: 'flex', justifyContent: 'flex-end', gap: 8 };

export interface ConfirmationHostProps {
  actionLabel: string;
  prompt: string;
  confirmLabel: string;
  onConfirm: () => void;
}

export function ConfirmationHost({ actionLabel, prompt, confirmLabel, onConfirm }: ConfirmationHostProps) {
  const { isArmed, arm, cancel, confirm } = useConfirm(onConfirm);

  return (
    <div data-testid="confirmation-host" style={hostStyle}>
      <Button variant="danger" size="small" data-testid="confirm-trigger" onClick={arm}>
        {actionLabel}
      </Button>
      {isArmed ? (
        <div data-testid="confirm-overlay" style={overlayStyle} onClick={cancel}>
          <div
            role="alertdialog"
            aria-modal="true"
            data-testid="confirm-dialog"
            style={dialogStyle}
            onClick={(event) => event.stopPropagation()}
          >
            <Text data-testid="confirm-prompt" style={{ fontSize: 14 }}>{prompt}</Text>
            <div style={actionsStyle}>
              <Button size="small" data-testid="confirm-cancel" onClick={cancel}>
                Cancel
              </Button>
              <Button variant="danger" size="small" data-testid="confirm-accept" onClick={confirm}>
                {confirmLabel}
              </Button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
