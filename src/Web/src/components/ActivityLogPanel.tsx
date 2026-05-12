import { useEffect, useState } from 'react';
import { Button, Label, Text } from '@primer/react';
import { getActivity, type ActivityEntryItem } from '../api/client';
import { subscribeToNotifications, type Notification } from '../api/notifications';

type LabelVariant = 'accent' | 'success' | 'danger' | 'secondary';

const variantByState: Record<string, LabelVariant> = {
  InProgress: 'accent',
  Succeeded: 'success',
  Failed: 'danger',
};

const statusByState: Record<string, string> = {
  InProgress: 'In progress',
  Succeeded: 'Succeeded',
  Failed: 'Failed',
};

function mergeEntry(current: ActivityEntryItem[], entry: ActivityEntryItem): ActivityEntryItem[] {
  const others = current.filter((existing) => existing.operationId !== entry.operationId);
  return [entry, ...others];
}

function toEntry(notification: Notification): ActivityEntryItem {
  return {
    operationId: notification.operationId,
    operation: notification.operation,
    state: notification.state,
    message: notification.message,
    occurredAt: notification.occurredAt,
  };
}

const panelStyle = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
} as const;

const entryStyle = {
  display: 'flex',
  alignItems: 'center',
  gap: 12,
  padding: '8px 12px',
  borderRadius: 6,
  border: '1px solid #21262d',
  background: '#161b22',
} as const;

export function ActivityLogPanel() {
  const [entries, setEntries] = useState<ActivityEntryItem[]>([]);
  const [collapsed, setCollapsed] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    getActivity(controller.signal)
      .then((result) => setEntries(result.entries))
      .catch(() => {});
    return () => controller.abort();
  }, []);

  useEffect(() => {
    const subscription = subscribeToNotifications((notification) =>
      setEntries((current) => mergeEntry(current, toEntry(notification))),
    );
    return () => {
      void subscription.then((active) => active.stop()).catch(() => {});
    };
  }, []);

  return (
    <section data-testid="activity-log-panel" style={panelStyle}>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12 }}>
        <Text data-testid="activity-log-title" style={{ fontWeight: 600, fontSize: 14 }}>
          Activity log
        </Text>
        <Button
          size="small"
          data-testid="activity-log-toggle"
          aria-expanded={!collapsed}
          onClick={() => setCollapsed((current) => !current)}
        >
          {collapsed ? 'Expand' : 'Collapse'}
        </Button>
      </div>
      {!collapsed && (
        <div data-testid="activity-log-entries" style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          {entries.length === 0 ? (
            <Text data-testid="activity-log-empty" style={{ fontSize: 13, opacity: 0.7 }}>
              No activity recorded yet.
            </Text>
          ) : (
            entries.map((entry) => (
              <div key={entry.operationId} data-testid="activity-log-entry" style={entryStyle}>
                <Label variant={variantByState[entry.state] ?? 'secondary'} data-testid="activity-log-entry-state">
                  {statusByState[entry.state] ?? entry.state}
                </Label>
                <div style={{ display: 'flex', flexDirection: 'column', flex: 1, gap: 2 }}>
                  <Text data-testid="activity-log-entry-operation" style={{ fontWeight: 600, fontSize: 13 }}>
                    {entry.operation}
                  </Text>
                  <Text data-testid="activity-log-entry-message" style={{ fontSize: 13 }}>
                    {entry.message}
                  </Text>
                </div>
              </div>
            ))
          )}
        </div>
      )}
    </section>
  );
}
