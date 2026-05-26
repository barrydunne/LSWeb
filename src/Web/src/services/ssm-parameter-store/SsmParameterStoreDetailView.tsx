import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { getParameterHistory, getParameterValue, updateParameterValue } from '../../api/client';
import type { ParameterHistoryResult, ParameterValueResult } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { ConfirmationHost } from '../../components/ConfirmationHost';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };

const textareaStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  fontFamily: 'monospace',
  minHeight: 64,
  resize: 'vertical',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const historyListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const historyEntryStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const historyMetaStyle: CSSProperties = {
  display: 'flex',
  gap: 12,
  flexWrap: 'wrap',
};

type LoadState = 'loading' | 'ready' | 'error';
type SaveState = 'idle' | 'saved' | 'error';
type HistoryState = 'idle' | 'loading' | 'ready' | 'error';

export function SsmParameterStoreDetailView({ resourceId }: ServiceDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [result, setResult] = useState<ParameterValueResult | null>(null);
  const [reveal, setReveal] = useState(false);
  const [editValue, setEditValue] = useState('');
  const [saveState, setSaveState] = useState<SaveState>('idle');
  const [showHistory, setShowHistory] = useState(false);
  const [historyState, setHistoryState] = useState<HistoryState>('idle');
  const [history, setHistory] = useState<ParameterHistoryResult | null>(null);

  const load = useCallback(
    (revealValue: boolean, signal?: AbortSignal) => {
      setLoadState('loading');
      return getParameterValue(resourceId, revealValue, signal)
        .then((data) => {
          setResult(data);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [resourceId],
  );

  const loadHistory = useCallback(
    (revealValue: boolean, signal?: AbortSignal) => {
      setHistoryState('loading');
      return getParameterHistory(resourceId, revealValue, signal)
        .then((data) => {
          setHistory(data);
          setHistoryState('ready');
        })
        .catch(() => setHistoryState('error'));
    },
    [resourceId],
  );

  useEffect(() => {
    const controller = new AbortController();
    setReveal(false);
    setSaveState('idle');
    setEditValue('');
    setShowHistory(false);
    setHistoryState('idle');
    setHistory(null);
    void load(false, controller.signal);
    return () => controller.abort();
  }, [load]);

  const handleToggleReveal = () => {
    const next = !reveal;
    setReveal(next);
    void load(next);
    if (showHistory) {
      void loadHistory(next);
    }
  };

  const handleToggleHistory = () => {
    const next = !showHistory;
    setShowHistory(next);
    if (next && historyState === 'idle') {
      void loadHistory(reveal);
    }
  };

  const handleSave = () => {
    setSaveState('idle');
    updateParameterValue(resourceId, editValue)
      .then(() => {
        setSaveState('saved');
        setEditValue('');
        if (showHistory) {
          void loadHistory(reveal);
        }
        return load(reveal);
      })
      .catch(() => setSaveState('error'));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="ssm-parameter-store-detail-loading" style={messageStyle}>
        Loading parameter&hellip;
      </p>
    );
  }

  if (loadState === 'error' || result === null) {
    return (
      <p data-testid="ssm-parameter-store-detail-error" style={messageStyle}>
        Unable to load this parameter.
      </p>
    );
  }

  return (
    <div data-testid="ssm-parameter-store-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="ssm-parameter-store-detail-name" style={{ fontSize: 16 }}>
        {result.name}
      </Heading>
      <div data-testid="ssm-parameter-store-detail-type" style={rowStyle}>
        <Text style={labelStyle}>Type</Text>
        <Text style={valueStyle}>{result.type}</Text>
      </div>
      <div data-testid="ssm-parameter-store-detail-version" style={rowStyle}>
        <Text style={labelStyle}>Version</Text>
        <Text style={valueStyle}>{result.version}</Text>
      </div>
      <div style={rowStyle}>
        <div style={headerStyle}>
          <Text style={labelStyle}>Value</Text>
          {result.revealAllowed ? (
            <button
              type="button"
              data-testid="ssm-parameter-store-detail-reveal"
              style={buttonStyle}
              onClick={handleToggleReveal}
            >
              {reveal ? 'Hide value' : 'Reveal value'}
            </button>
          ) : null}
        </div>
        <Text data-testid="ssm-parameter-store-detail-value" style={valueStyle}>
          {result.value}
        </Text>
      </div>

      <div style={rowStyle}>
        <label style={labelStyle} htmlFor="ssm-parameter-store-detail-edit">
          New value
        </label>
        <textarea
          id="ssm-parameter-store-detail-edit"
          data-testid="ssm-parameter-store-detail-edit"
          style={textareaStyle}
          value={editValue}
          onChange={(event) => setEditValue(event.target.value)}
        />
      </div>

      <ConfirmationHost
        actionLabel="Save value"
        prompt={`Update the value for ${result.name}?`}
        confirmLabel="Confirm save"
        onConfirm={handleSave}
      />

      {saveState === 'saved' ? (
        <Text data-testid="ssm-parameter-store-detail-save-status" style={messageStyle}>
          Parameter value updated.
        </Text>
      ) : null}
      {saveState === 'error' ? (
        <Text data-testid="ssm-parameter-store-detail-save-error" style={messageStyle}>
          Unable to update the parameter value.
        </Text>
      ) : null}

      <div style={rowStyle}>
        <div style={headerStyle}>
          <Text style={labelStyle}>History</Text>
          <button
            type="button"
            data-testid="ssm-parameter-store-detail-history-toggle"
            style={buttonStyle}
            onClick={handleToggleHistory}
          >
            {showHistory ? 'Hide history' : 'Show history'}
          </button>
        </div>
        {showHistory && historyState === 'loading' ? (
          <Text data-testid="ssm-parameter-store-detail-history-loading" style={messageStyle}>
            Loading history&hellip;
          </Text>
        ) : null}
        {showHistory && historyState === 'error' ? (
          <Text data-testid="ssm-parameter-store-detail-history-error" style={messageStyle}>
            Unable to load the parameter history.
          </Text>
        ) : null}
        {showHistory && historyState === 'ready' && history !== null ? (
          history.entries.length === 0 ? (
            <Text data-testid="ssm-parameter-store-detail-history-empty" style={messageStyle}>
              No history available for this parameter.
            </Text>
          ) : (
            <div data-testid="ssm-parameter-store-detail-history" style={historyListStyle}>
              {history.entries.map((entry) => (
                <div
                  key={entry.version}
                  data-testid="ssm-parameter-store-detail-history-entry"
                  style={historyEntryStyle}
                >
                  <div style={historyMetaStyle}>
                    <Text data-testid="ssm-parameter-store-detail-history-version" style={labelStyle}>
                      Version {entry.version}
                    </Text>
                    <Text data-testid="ssm-parameter-store-detail-history-user" style={labelStyle}>
                      {entry.lastModifiedUser === '' ? '\u2014' : entry.lastModifiedUser}
                    </Text>
                    <Text data-testid="ssm-parameter-store-detail-history-date" style={labelStyle}>
                      {entry.lastModifiedDate ?? '\u2014'}
                    </Text>
                  </div>
                  <Text data-testid="ssm-parameter-store-detail-history-value" style={valueStyle}>
                    {entry.value}
                  </Text>
                </div>
              ))}
            </div>
          )
        ) : null}
      </div>
    </div>
  );
}

export default SsmParameterStoreDetailView;
