import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Button, Text } from '@primer/react';
import { getLambdaLogEvents } from '../../api/client';
import type { LambdaLogEventItem } from '../../api/client';
import { AutoRefreshToggle } from '../../components/AutoRefreshToggle';
import { ResourceLink } from '../../components/ResourceLink';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const messageStyle: CSSProperties = { fontSize: 14 };

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  flexWrap: 'wrap',
};

const controlsStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 13, fontFamily: 'monospace' };

const listStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const eventStyle: CSSProperties = {
  display: 'flex',
  gap: 12,
  fontFamily: 'monospace',
  fontSize: 12,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
};

const timestampStyle: CSSProperties = {
  flexShrink: 0,
  opacity: 0.7,
};

type LoadState = 'loading' | 'ready' | 'error';

export function LambdaLogsTab({ functionName }: { functionName: string }) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [logGroupName, setLogGroupName] = useState('');
  const [events, setEvents] = useState<LambdaLogEventItem[]>([]);

  const load = useCallback(
    (signal?: AbortSignal) => {
      return getLambdaLogEvents(functionName, undefined, signal)
        .then((data) => {
          setLogGroupName(data.logGroupName);
          setEvents(data.events);
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
      <p data-testid="lambda-logs-loading" style={messageStyle}>
        Loading log events&hellip;
      </p>
    );
  }

  if (loadState === 'error') {
    return (
      <p data-testid="lambda-logs-error" style={messageStyle}>
        Unable to load log events.
      </p>
    );
  }

  return (
    <div data-testid="lambda-logs-tab" style={containerStyle}>
      <div style={headerStyle}>
        <div>
          <Text style={labelStyle}>Log group</Text>{' '}
          <span data-testid="lambda-logs-group" style={valueStyle}>
            <ResourceLink reference={logGroupName} service="cloudwatch-logs" />
          </span>
        </div>
        <div style={controlsStyle}>
          <Button size="small" data-testid="lambda-logs-refresh" onClick={() => void load()}>
            Refresh
          </Button>
          <AutoRefreshToggle onRefresh={() => void load()} />
        </div>
      </div>
      {events.length === 0 ? (
        <p data-testid="lambda-logs-empty" style={messageStyle}>
          No recent log events for this function.
        </p>
      ) : (
        <div style={listStyle}>
          {events.map((event, index) => (
            <div
              key={`${event.logStreamName}-${event.timestamp}-${index}`}
              data-testid={`lambda-log-event-${index}`}
              style={eventStyle}
            >
              <span style={timestampStyle}>{event.timestamp}</span>
              <span>{event.message}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default LambdaLogsTab;
