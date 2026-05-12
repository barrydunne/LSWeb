import { useEffect, useState } from 'react';
import { subscribeToNotifications, type Notification } from '../api/notifications';
import { Toast } from './Toast';

function mergeNotification(current: Notification[], notification: Notification): Notification[] {
  const others = current.filter((entry) => entry.operationId !== notification.operationId);
  return [...others, notification];
}

const centerStyle = {
  position: 'fixed',
  top: 16,
  right: 16,
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  zIndex: 1000,
} as const;

export function NotificationCenter() {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    const subscription = subscribeToNotifications((notification) =>
      setNotifications((current) => mergeNotification(current, notification)),
    );
    return () => {
      void subscription.then((active) => active.stop()).catch(() => {});
    };
  }, []);

  const dismiss = (operationId: string) =>
    setNotifications((current) => current.filter((entry) => entry.operationId !== operationId));

  return (
    <div data-testid="notification-center" style={centerStyle}>
      {notifications.map((notification) => (
        <Toast
          key={notification.operationId}
          notification={notification}
          onDismiss={() => dismiss(notification.operationId)}
        />
      ))}
    </div>
  );
}
