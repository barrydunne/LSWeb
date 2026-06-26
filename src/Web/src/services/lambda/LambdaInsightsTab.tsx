import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Button, Text } from '@primer/react';
import { getLambdaInvocationInsights } from '../../api/client';
import type {
  LambdaInvocationMetrics,
  LambdaRecentInvocationItem,
} from '../../api/client';
import { AutoRefreshToggle } from '../../components/AutoRefreshToggle';

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

const metricsStyle: CSSProperties = {
  display: 'flex',
  flexWrap: 'wrap',
  gap: 12,
};

const metricCardStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  minWidth: 120,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const metricLabelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const metricValueStyle: CSSProperties = { fontSize: 20, fontWeight: 600 };

const sparklineStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'flex-end',
  gap: 2,
  height: 48,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const listStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const invocationStyle: CSSProperties = {
  display: 'flex',
  gap: 12,
  alignItems: 'center',
  fontFamily: 'monospace',
  fontSize: 12,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
};

const timestampStyle: CSSProperties = {
  flexShrink: 0,
  opacity: 0.7,
};

const errorBadgeStyle: CSSProperties = {
  flexShrink: 0,
  padding: '0 6px',
  borderRadius: 4,
  background: '#f8514926',
  color: '#f85149',
};

const okBadgeStyle: CSSProperties = {
  flexShrink: 0,
  padding: '0 6px',
  borderRadius: 4,
  background: '#3fb95026',
  color: '#3fb950',
};

type LoadState = 'loading' | 'ready' | 'error';

function formatDuration(durationMs: number): string {
  return `${durationMs.toFixed(1)} ms`;
}

export function LambdaInsightsTab({ functionName }: { functionName: string }) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [logGroupName, setLogGroupName] = useState('');
  const [metrics, setMetrics] = useState<LambdaInvocationMetrics | null>(null);
  const [invocations, setInvocations] = useState<LambdaRecentInvocationItem[]>([]);

  const load = useCallback(
    (signal?: AbortSignal) => {
      return getLambdaInvocationInsights(functionName, undefined, signal)
        .then((data) => {
          setLogGroupName(data.logGroupName);
          setMetrics(data.metrics);
          setInvocations(data.recentInvocations);
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
      <p data-testid="lambda-insights-loading" style={messageStyle}>
        Loading invocation insights&hellip;
      </p>
    );
  }

  if (loadState === 'error') {
    return (
      <p data-testid="lambda-insights-error" style={messageStyle}>
        Unable to load invocation insights.
      </p>
    );
  }

  const maxDuration = invocations.reduce((max, item) => Math.max(max, item.durationMs), 0);

  return (
    <div data-testid="lambda-insights-tab" style={containerStyle}>
      <div style={headerStyle}>
        <div>
          <Text style={labelStyle}>Log group</Text>
          <Text data-testid="lambda-insights-group" style={valueStyle}>
            {' '}
            {logGroupName}
          </Text>
        </div>
        <div style={controlsStyle}>
          <Button size="small" data-testid="lambda-insights-refresh" onClick={() => void load()}>
            Refresh
          </Button>
          <AutoRefreshToggle onRefresh={() => void load()} />
        </div>
      </div>
      <div data-testid="lambda-insights-metrics" style={metricsStyle}>
        <div style={metricCardStyle}>
          <span style={metricLabelStyle}>Invocations</span>
          <span data-testid="lambda-insights-metric-count" style={metricValueStyle}>
            {metrics!.invocationCount}
          </span>
        </div>
        <div style={metricCardStyle}>
          <span style={metricLabelStyle}>Errors</span>
          <span data-testid="lambda-insights-metric-errors" style={metricValueStyle}>
            {metrics!.errorCount}
          </span>
        </div>
        <div style={metricCardStyle}>
          <span style={metricLabelStyle}>Avg duration</span>
          <span data-testid="lambda-insights-metric-avg" style={metricValueStyle}>
            {formatDuration(metrics!.averageDurationMs)}
          </span>
        </div>
        <div style={metricCardStyle}>
          <span style={metricLabelStyle}>Max duration</span>
          <span data-testid="lambda-insights-metric-max" style={metricValueStyle}>
            {formatDuration(metrics!.maxDurationMs)}
          </span>
        </div>
      </div>
      {invocations.length === 0 ? (
        <p data-testid="lambda-insights-empty" style={messageStyle}>
          No recent invocations for this function.
        </p>
      ) : (
        <>
          <div data-testid="lambda-insights-sparkline" style={sparklineStyle}>
            {invocations.map((item, index) => (
              <div
                key={`bar-${item.requestId}-${index}`}
                data-testid={`lambda-insights-bar-${index}`}
                style={{
                  flex: 1,
                  minWidth: 2,
                  height: `${maxDuration === 0 ? 0 : Math.round((item.durationMs / maxDuration) * 100)}%`,
                  background: item.hasError ? '#f85149' : '#3fb950',
                }}
              />
            ))}
          </div>
          <div style={listStyle}>
            {invocations.map((item, index) => (
              <div
                key={`${item.requestId}-${index}`}
                data-testid={`lambda-insights-invocation-${index}`}
                style={invocationStyle}
              >
                <span style={timestampStyle}>{item.timestamp}</span>
                <span>{item.requestId}</span>
                <span>{formatDuration(item.durationMs)}</span>
                {item.hasError ? (
                  <span style={errorBadgeStyle}>Error</span>
                ) : (
                  <span style={okBadgeStyle}>OK</span>
                )}
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}

export default LambdaInsightsTab;
