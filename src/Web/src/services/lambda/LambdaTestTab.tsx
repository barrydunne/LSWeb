import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import {
  deleteLambdaTestEvent,
  getLambdaTestEvents,
  invokeLambdaFunction,
  saveLambdaTestEvent,
} from '../../api/client';
import type { LambdaInvocationResult, LambdaTestEventItem } from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { RawJsonViewer } from '../../components/RawJsonViewer';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const textareaStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 13,
  minHeight: 120,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  resize: 'vertical',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 12px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const panelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const metaRowStyle: CSSProperties = {
  display: 'flex',
  gap: 16,
  flexWrap: 'wrap',
};

const metaStyle: CSSProperties = { fontSize: 13 };

const errorBadgeStyle: CSSProperties = {
  fontSize: 12,
  padding: '1px 8px',
  borderRadius: 10,
  border: '1px solid #f85149',
  color: '#f85149',
};

const messageStyle: CSSProperties = { fontSize: 14 };

const logStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 12,
  whiteSpace: 'pre-wrap',
  margin: 0,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const historyStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const selectStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  alignSelf: 'flex-start',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  alignItems: 'center',
  flexWrap: 'wrap',
};

const savedListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
};

const savedRowStyle: CSSProperties = {
  display: 'flex',
  gap: 12,
  alignItems: 'center',
  justifyContent: 'space-between',
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

interface InvocationHistoryItem {
  id: number;
  request: string;
  result: LambdaInvocationResult;
}

export function LambdaTestTab({ functionName }: { functionName: string }) {
  const [payload, setPayload] = useState('{}');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState(false);
  const [history, setHistory] = useState<InvocationHistoryItem[]>([]);
  const [nextId, setNextId] = useState(0);
  const [savedEvents, setSavedEvents] = useState<LambdaTestEventItem[]>([]);
  const [templates, setTemplates] = useState<LambdaTestEventItem[]>([]);
  const [eventName, setEventName] = useState('');
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState(false);

  const reloadEvents = useCallback(() => {
    getLambdaTestEvents(functionName)
      .then((result) => {
        setSavedEvents(result.events);
        setTemplates(result.templates);
      })
      .catch(() => {
        setSavedEvents([]);
        setTemplates([]);
      });
  }, [functionName]);

  useEffect(() => {
    reloadEvents();
  }, [reloadEvents]);

  const handleInvoke = () => {
    setBusy(true);
    setError(false);
    invokeLambdaFunction(functionName, payload)
      .then((result) => {
        setHistory((current) => [{ id: nextId, request: payload, result }, ...current]);
        setNextId((value) => value + 1);
      })
      .catch(() => setError(true))
      .finally(() => setBusy(false));
  };

  const handleSelect = (value: string) => {
    if (value === '') {
      return;
    }
    const match =
      savedEvents.find((item) => `saved:${item.name}` === value) ??
      templates.find((item) => `template:${item.name}` === value);
    if (match) {
      setPayload(match.payload);
      setEventName(match.name);
    }
  };

  const handleSave = () => {
    setSaving(true);
    setSaveError(false);
    saveLambdaTestEvent(functionName, eventName, payload)
      .then(() => reloadEvents())
      .catch(() => setSaveError(true))
      .finally(() => setSaving(false));
  };

  const handleDelete = (name: string) => {
    deleteLambdaTestEvent(functionName, name)
      .then(() => reloadEvents())
      .catch(() => setSaveError(true));
  };

  return (
    <div data-testid="lambda-test-tab" style={containerStyle}>
      <Text style={labelStyle}>Load saved event or template</Text>
      <select
        data-testid="lambda-test-selector"
        style={selectStyle}
        value=""
        onChange={(event) => handleSelect(event.target.value)}
      >
        <option value="">Select an event…</option>
        {savedEvents.length > 0 && (
          <optgroup label="Saved events">
            {savedEvents.map((item) => (
              <option key={`saved:${item.name}`} value={`saved:${item.name}`}>
                {item.name}
              </option>
            ))}
          </optgroup>
        )}
        {templates.length > 0 && (
          <optgroup label="Templates">
            {templates.map((item) => (
              <option key={`template:${item.name}`} value={`template:${item.name}`}>
                {item.name}
              </option>
            ))}
          </optgroup>
        )}
      </select>
      <Text style={labelStyle}>Request payload (JSON)</Text>
      <textarea
        data-testid="lambda-test-payload"
        style={textareaStyle}
        value={payload}
        onChange={(event) => setPayload(event.target.value)}
      />
      <div style={rowStyle}>
        <input
          data-testid="lambda-test-name"
          style={inputStyle}
          placeholder="Event name"
          value={eventName}
          onChange={(event) => setEventName(event.target.value)}
        />
        <button
          type="button"
          data-testid="lambda-test-save"
          style={buttonStyle}
          disabled={saving || eventName.trim() === ''}
          onClick={handleSave}
        >
          {saving ? 'Saving…' : 'Save event'}
        </button>
      </div>
      {saveError && (
        <p data-testid="lambda-test-save-error" style={messageStyle}>
          Unable to save or delete the test event.
        </p>
      )}
      {savedEvents.length > 0 && (
        <div data-testid="lambda-test-saved-list" style={savedListStyle}>
          {savedEvents.map((item) => (
            <div
              key={item.name}
              data-testid={`lambda-test-saved-${item.name}`}
              style={savedRowStyle}
            >
              <Text style={metaStyle}>{item.name}</Text>
              <ConfirmationHost
                actionLabel="Delete"
                prompt={`Delete ${item.name}?`}
                confirmLabel="Delete event"
                onConfirm={() => handleDelete(item.name)}
              />
            </div>
          ))}
        </div>
      )}
      <button
        type="button"
        data-testid="lambda-test-invoke"
        style={buttonStyle}
        disabled={busy}
        onClick={handleInvoke}
      >
        {busy ? 'Invoking…' : 'Invoke'}
      </button>
      {error && (
        <p data-testid="lambda-test-error" style={messageStyle}>
          Unable to invoke this Lambda function.
        </p>
      )}
      <div data-testid="lambda-test-history" style={historyStyle}>
        {history.map((item) => (
          <div key={item.id} data-testid={`lambda-test-result-${item.id}`} style={panelStyle}>
            <div style={metaRowStyle}>
              <Text data-testid={`lambda-test-status-${item.id}`} style={metaStyle}>
                Status: {item.result.statusCode}
              </Text>
              <Text data-testid={`lambda-test-duration-${item.id}`} style={metaStyle}>
                Duration: {item.result.durationMs} ms
              </Text>
              {item.result.functionError && (
                <span data-testid={`lambda-test-function-error-${item.id}`} style={errorBadgeStyle}>
                  {item.result.functionError}
                </span>
              )}
            </div>
            <RawJsonViewer value={item.request} title="Request" />
            <RawJsonViewer value={item.result.payload} title="Response" initiallyExpanded />
            {item.result.logTail && (
              <pre data-testid={`lambda-test-logs-${item.id}`} style={logStyle}>
                {item.result.logTail}
              </pre>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

export default LambdaTestTab;
