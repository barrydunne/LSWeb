import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { getLogEvents, getLogStreams } from '../../api/client';
import type { LogEventItem, LogStreamItem } from '../../api/client';
import { streamLogGroup } from '../../api/notifications';
import type { LiveLogEvent } from '../../api/notifications';
import { ResourceLink } from '../../components/ResourceLink';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';

const lambdaLogGroupPrefix = '/aws/lambda/';

function deriveLambdaFunctionName(logGroupName: string): string | null {
  if (!logGroupName.startsWith(lambdaLogGroupPrefix)) {
    return null;
  }
  const functionName = logGroupName.slice(lambdaLogGroupPrefix.length);
  return functionName.length > 0 ? functionName : null;
}

function exportEventsToFile(logGroupName: string, streamName: string, events: LogEventItem[]) {
  const content = events.map((event) => `${event.timestamp}\t${event.message}`).join('\n');
  const blob = new Blob([content], { type: 'text/plain' });
  const url = URL.createObjectURL(blob);
  const safeName = `${logGroupName}_${streamName}`.replace(/[^a-zA-Z0-9._-]+/g, '_');
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = `${safeName}.log`;
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);
  URL.revokeObjectURL(url);
}

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const messageStyle: CSSProperties = { fontSize: 14 };

const headingStyle: CSSProperties = { fontSize: 16, fontWeight: 600 };

const subHeadingStyle: CSSProperties = { fontSize: 14, fontWeight: 600 };

const resourceIdStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 13,
  wordBreak: 'break-all',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const followingButtonStyle: CSSProperties = {
  ...buttonStyle,
  border: '1px solid #1f6feb',
  background: '#0d1117',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  width: '100%',
};

const controlRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const streamListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  margin: 0,
  padding: 0,
  listStyle: 'none',
};

const streamButtonStyle: CSSProperties = {
  ...buttonStyle,
  textAlign: 'left',
  width: '100%',
};

const selectedStreamButtonStyle: CSSProperties = {
  ...streamButtonStyle,
  border: '1px solid #1f6feb',
  background: '#0d1117',
};

const eventCardStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const eventTimestampStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const eventMessageStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 12,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-all',
  margin: 0,
};

type StreamsState =
  | { kind: 'loading' }
  | { kind: 'ready'; streams: LogStreamItem[] }
  | { kind: 'error' };

type EventsState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'ready'; events: LogEventItem[] }
  | { kind: 'error' };

export function CloudWatchLogsDetailView({ resourceId }: ServiceDetailViewProps) {
  const [streamsState, setStreamsState] = useState<StreamsState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [selectedStream, setSelectedStream] = useState<string | null>(null);
  const [eventsState, setEventsState] = useState<EventsState>({ kind: 'idle' });
  const [filterPattern, setFilterPattern] = useState('');
  const [search, setSearch] = useState('');
  const [following, setFollowing] = useState(false);
  const [liveEvents, setLiveEvents] = useState<LiveLogEvent[]>([]);

  useEffect(() => {
    const controller = new AbortController();
    setStreamsState({ kind: 'loading' });
    getLogStreams(resourceId, controller.signal)
      .then((result) => setStreamsState({ kind: 'ready', streams: result.logStreams }))
      .catch(() => setStreamsState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, reloadToken]);

  useEffect(() => {
    if (selectedStream === null) {
      return;
    }
    const controller = new AbortController();
    setEventsState({ kind: 'loading' });
    getLogEvents(resourceId, selectedStream, controller.signal)
      .then((result) => setEventsState({ kind: 'ready', events: result.events }))
      .catch(() => setEventsState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, selectedStream]);

  const refresh = useCallback(() => {
    setSelectedStream(null);
    setEventsState({ kind: 'idle' });
    setReloadToken((token) => token + 1);
  }, []);

  useEffect(() => {
    if (!following) {
      return;
    }
    setLiveEvents([]);
    const subPromise = streamLogGroup(resourceId, filterPattern, (event) =>
      setLiveEvents((prev) => [...prev, event]),
    ).catch(() => {
      setFollowing(false);
      return null;
    });
    return () => {
      void subPromise.then((subscription) => subscription?.stop());
    };
  }, [following, resourceId, filterPattern]);

  const matchesSearch = (message: string) =>
    search === '' || message.toLowerCase().includes(search.toLowerCase());

  const lambdaFunctionName = deriveLambdaFunctionName(resourceId);

  return (
    <div data-testid="logs-detail-view" style={containerStyle}>
      <div>
        <p style={headingStyle}>Log group</p>
        <p data-testid="logs-detail-name" style={resourceIdStyle}>
          {resourceId}
        </p>
        {lambdaFunctionName !== null ? (
          <p data-testid="logs-lambda-owner" style={messageStyle}>
            Owning function:{' '}
            <ResourceLink
              reference={lambdaFunctionName}
              service="lambda"
              label={lambdaFunctionName}
            />
          </p>
        ) : null}
      </div>
      <button type="button" data-testid="logs-detail-refresh" style={buttonStyle} onClick={refresh}>
        Refresh
      </button>

      <div style={controlRowStyle}>
        <p style={subHeadingStyle}>Live tail</p>
        <input
          type="text"
          data-testid="logs-filter-pattern"
          style={inputStyle}
          placeholder="Filter pattern"
          value={filterPattern}
          onChange={(event) => setFilterPattern(event.target.value)}
        />
        <input
          type="text"
          data-testid="logs-search"
          style={inputStyle}
          placeholder="Search"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        <button
          type="button"
          data-testid="logs-follow-toggle"
          style={following ? followingButtonStyle : buttonStyle}
          onClick={() => setFollowing((value) => !value)}
        >
          {following ? 'Stop following' : 'Follow'}
        </button>
        {following ? (
          <div data-testid="logs-live-list">
            {liveEvents
              .filter((event) => matchesSearch(event.message))
              .map((event, index) => (
                <div key={`${event.timestamp}-${index}`} style={eventCardStyle}>
                  <span data-testid="logs-live-timestamp" style={eventTimestampStyle}>
                    {event.timestamp}
                  </span>
                  <pre data-testid="logs-live-message" style={eventMessageStyle}>
                    {event.message}
                  </pre>
                </div>
              ))}
          </div>
        ) : null}
      </div>

      <div>
        <p style={subHeadingStyle}>Log streams</p>
        {streamsState.kind === 'loading' ? (
          <p data-testid="logs-streams-loading" style={messageStyle}>
            Loading log streams&hellip;
          </p>
        ) : null}
        {streamsState.kind === 'error' ? (
          <p data-testid="logs-streams-error" style={messageStyle}>
            Unable to load log streams.
          </p>
        ) : null}
        {streamsState.kind === 'ready' && streamsState.streams.length === 0 ? (
          <p data-testid="logs-streams-empty" style={messageStyle}>
            No log streams found in this log group.
          </p>
        ) : null}
        {streamsState.kind === 'ready' && streamsState.streams.length > 0 ? (
          <ul data-testid="logs-streams-list" style={streamListStyle}>
            {streamsState.streams.map((stream) => (
              <li key={stream.name}>
                <button
                  type="button"
                  data-testid="logs-stream-button"
                  style={
                    selectedStream === stream.name ? selectedStreamButtonStyle : streamButtonStyle
                  }
                  onClick={() => setSelectedStream(stream.name)}
                >
                  {stream.name}
                </button>
              </li>
            ))}
          </ul>
        ) : null}
      </div>

      {selectedStream !== null ? (
        <div>
          <p style={subHeadingStyle}>Events</p>
          {eventsState.kind === 'loading' ? (
            <p data-testid="logs-events-loading" style={messageStyle}>
              Loading log events&hellip;
            </p>
          ) : null}
          {eventsState.kind === 'error' ? (
            <p data-testid="logs-events-error" style={messageStyle}>
              Unable to load log events.
            </p>
          ) : null}
          {eventsState.kind === 'ready' && eventsState.events.length === 0 ? (
            <p data-testid="logs-events-empty" style={messageStyle}>
              No log events found in this stream.
            </p>
          ) : null}
          {eventsState.kind === 'ready' && eventsState.events.length > 0 ? (
            <div data-testid="logs-events-list">
              <button
                type="button"
                data-testid="logs-export-button"
                style={buttonStyle}
                onClick={() =>
                  exportEventsToFile(resourceId, selectedStream, eventsState.events)
                }
              >
                Export events
              </button>
              {eventsState.events
                .filter((event) => matchesSearch(event.message))
                .map((event, index) => (
                  <div key={`${event.timestamp}-${index}`} style={eventCardStyle}>
                    <span data-testid="logs-event-timestamp" style={eventTimestampStyle}>
                      {event.timestamp}
                    </span>
                    <pre data-testid="logs-event-message" style={eventMessageStyle}>
                      {event.message}
                    </pre>
                  </div>
                ))}
            </div>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}

export default CloudWatchLogsDetailView;
