import { Button, Text } from '@primer/react';
import { useConfirm } from '../hooks/useConfirm';

const hostStyle = { display: 'flex', alignItems: 'center', gap: 8 };

export interface ConfirmationHostProps {
  actionLabel: string;
  prompt: string;
  confirmLabel: string;
  onConfirm: () => void;
}

export function ConfirmationHost({ actionLabel, prompt, confirmLabel, onConfirm }: ConfirmationHostProps) {
  const { isArmed, arm, cancel, confirm } = useConfirm(onConfirm);

  if (!isArmed) {
    return (
      <div data-testid="confirmation-host" style={hostStyle}>
        <Button variant="danger" size="small" data-testid="confirm-trigger" onClick={arm}>
          {actionLabel}
        </Button>
      </div>
    );
  }

  return (
    <div data-testid="confirmation-host" style={hostStyle}>
      <Text data-testid="confirm-prompt">{prompt}</Text>
      <Button variant="danger" size="small" data-testid="confirm-accept" onClick={confirm}>
        {confirmLabel}
      </Button>
      <Button size="small" data-testid="confirm-cancel" onClick={cancel}>
        Cancel
      </Button>
    </div>
  );
}
