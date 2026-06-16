import { useEffect, useRef, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { getExecutionHistory } from '../../api/client';
import type { ExecutionHistoryEvent } from '../../api/client';
import { RawJsonViewer } from '../../components/RawJsonViewer';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const sectionHeadingStyle: CSSProperties = { fontSize: 13 };
const messageStyle: CSSProperties = { fontSize: 13 };

const eventStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const eventHeaderStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'baseline',
  gap: 8,
  fontSize: 13,
};

const eventTypeStyle: CSSProperties = { fontWeight: 600 };
const eventMetaStyle: CSSProperties = { fontSize: 12, opacity: 0.7, fontFamily: 'monospace' };
const errorTextStyle: CSSProperties = { fontSize: 12, color: '#f85149', fontFamily: 'monospace' };

type HistoryState =
  | { kind: 'loading' }
  | { kind: 'ready'; events: ExecutionHistoryEvent[] }
  | { kind: 'error' };

const terminalEventStatuses: Record<string, string> = {
  ExecutionSucceeded: 'SUCCEEDED',
  ExecutionFailed: 'FAILED',
  ExecutionAborted: 'ABORTED',
  ExecutionTimedOut: 'TIMED_OUT',
};

function resolveTerminalStatus(events: ExecutionHistoryEvent[]): string | null {
  for (const event of events) {
    const status = terminalEventStatuses[event.type];
    if (status !== undefined) {
      return status;
    }
  }
  return null;
}

function parseJson(value: string): unknown {
  try {
    return JSON.parse(value) as unknown;
  } catch {
    return value;
  }
}

export function StepFunctionsExecutionHistoryPanel({
  executionArn,
  onResolvedStatus,
}: {
  executionArn: string;
  onResolvedStatus?: (status: string) => void;
}) {
  const [state, setState] = useState<HistoryState>({ kind: 'loading' });
  const onResolvedStatusRef = useRef(onResolvedStatus);
  onResolvedStatusRef.current = onResolvedStatus;

  useEffect(() => {
    const controller = new AbortController();
    getExecutionHistory(executionArn, controller.signal)
      .then((result) => {
        setState({ kind: 'ready', events: result.events });
        const terminalStatus = resolveTerminalStatus(result.events);
        if (terminalStatus !== null) {
          onResolvedStatusRef.current?.(terminalStatus);
        }
      })
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [executionArn]);

  return (
    <div data-testid="step-functions-history-panel" style={sectionStyle}>
      <Heading as="h4" data-testid="step-functions-history-heading" style={sectionHeadingStyle}>
        Execution history
      </Heading>
      {state.kind === 'loading' && (
        <p data-testid="step-functions-history-loading" style={messageStyle}>
          Loading history&hellip;
        </p>
      )}
      {state.kind === 'error' && (
        <p data-testid="step-functions-history-error" style={messageStyle}>
          Unable to load execution history.
        </p>
      )}
      {state.kind === 'ready' && state.events.length === 0 && (
        <p data-testid="step-functions-history-empty" style={messageStyle}>
          No history events found.
        </p>
      )}
      {state.kind === 'ready' &&
        state.events.map((event) => (
          <div key={event.id} data-testid="step-functions-history-event" style={eventStyle}>
            <div style={eventHeaderStyle}>
              <span style={eventTypeStyle} data-testid="step-functions-history-event-type">
                {event.type}
              </span>
              {event.name !== null && (
                <span style={eventMetaStyle} data-testid="step-functions-history-event-name">
                  {event.name}
                </span>
              )}
              <span style={eventMetaStyle}>{event.timestamp}</span>
            </div>
            {event.error !== null && (
              <span style={errorTextStyle} data-testid="step-functions-history-event-error">
                {event.error}
                {event.cause !== null ? `: ${event.cause}` : ''}
              </span>
            )}
            {event.input !== null && (
              <RawJsonViewer value={parseJson(event.input)} title="Input" />
            )}
            {event.output !== null && (
              <RawJsonViewer value={parseJson(event.output)} title="Output" />
            )}
          </div>
        ))}
    </div>
  );
}

export default StepFunctionsExecutionHistoryPanel;
