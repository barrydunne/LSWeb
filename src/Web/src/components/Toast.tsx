import { useEffect, useRef } from 'react';
import { Button, Label, Text } from '@primer/react';
import type { Notification, OperationState } from '../api/notifications';

type LabelVariant = 'accent' | 'success' | 'danger';

const autoDismissMs = 5000;

const variantByState: Record<OperationState, LabelVariant> = {
  InProgress: 'accent',
  Succeeded: 'success',
  Failed: 'danger',
};

const statusByState: Record<OperationState, string> = {
  InProgress: 'In progress',
  Succeeded: 'Succeeded',
  Failed: 'Failed',
};

const toastStyle = {
  display: 'flex',
  alignItems: 'center',
  gap: 12,
  padding: '12px 16px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  minWidth: 320,
} as const;

export interface ToastProps {
  notification: Notification;
  onDismiss: () => void;
}

export function Toast({ notification, onDismiss }: ToastProps) {
  const onDismissRef = useRef(onDismiss);
  onDismissRef.current = onDismiss;

  useEffect(() => {
    if (notification.state === 'InProgress') {
      return;
    }
    const handle = setTimeout(() => onDismissRef.current(), autoDismissMs);
    return () => clearTimeout(handle);
  }, [notification.state]);

  return (
    <div data-testid="toast" style={toastStyle}>
      <Label variant={variantByState[notification.state]} data-testid="toast-status">
        {statusByState[notification.state]}
      </Label>
      <div style={{ display: 'flex', flexDirection: 'column', flex: 1, gap: 2 }}>
        <Text data-testid="toast-operation" style={{ fontWeight: 600, fontSize: 14 }}>
          {notification.operation}
        </Text>
        <Text data-testid="toast-message" style={{ fontSize: 13 }}>
          {notification.message}
        </Text>
      </div>
      <Button size="small" data-testid="toast-dismiss" onClick={onDismiss}>
        Dismiss
      </Button>
    </div>
  );
}
