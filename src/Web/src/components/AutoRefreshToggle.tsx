import { useEffect, useState, type CSSProperties } from 'react';
import { Button } from '@primer/react';

export interface AutoRefreshToggleProps {
  onRefresh: () => void;
  intervals?: number[];
  defaultIntervalSeconds?: number;
}

const defaultIntervals = [5, 15, 30];

const containerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
};

const selectStyle: CSSProperties = {
  background: '#0d1117',
  color: '#c9d1d9',
  border: '1px solid #30363d',
  borderRadius: 6,
  padding: '4px 8px',
};

export function AutoRefreshToggle({
  onRefresh,
  intervals = defaultIntervals,
  defaultIntervalSeconds,
}: AutoRefreshToggleProps) {
  const [enabled, setEnabled] = useState(false);
  const [intervalSeconds, setIntervalSeconds] = useState(defaultIntervalSeconds ?? intervals[0]);
  const [hidden, setHidden] = useState(false);

  useEffect(() => {
    const handleVisibility = () => setHidden(document.hidden);
    document.addEventListener('visibilitychange', handleVisibility);
    return () => document.removeEventListener('visibilitychange', handleVisibility);
  }, []);

  useEffect(() => {
    if (!enabled || hidden) {
      return undefined;
    }
    const timer = setInterval(onRefresh, intervalSeconds * 1000);
    return () => clearInterval(timer);
  }, [enabled, hidden, intervalSeconds, onRefresh]);

  return (
    <div data-testid="auto-refresh-toggle" style={containerStyle}>
      <Button
        size="small"
        variant={enabled ? 'primary' : 'default'}
        aria-pressed={enabled}
        data-testid="auto-refresh-switch"
        onClick={() => setEnabled((value) => !value)}
      >
        {enabled ? 'Auto-refresh on' : 'Auto-refresh off'}
      </Button>
      <select
        aria-label="Auto-refresh interval"
        data-testid="auto-refresh-interval"
        style={selectStyle}
        value={intervalSeconds}
        onChange={(event) => setIntervalSeconds(Number(event.target.value))}
      >
        {intervals.map((seconds) => (
          <option key={seconds} value={seconds}>
            {seconds}s
          </option>
        ))}
      </select>
    </div>
  );
}
