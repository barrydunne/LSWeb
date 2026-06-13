import { useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { createEventBridgeRule } from '../../api/client';

const panelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 10,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  marginBottom: 12,
};

const headingStyle: CSSProperties = { fontSize: 14, margin: 0 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const messageStyle: CSSProperties = { fontSize: 13 };

const rowStyle: CSSProperties = { display: 'flex', gap: 8, alignItems: 'flex-start' };
const fieldColumnStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 8 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
  flex: 1,
};

const previewStyle: CSSProperties = {
  margin: 0,
  fontFamily: 'monospace',
  fontSize: 12,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
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

interface PatternField {
  name: string;
  values: string;
}

type SaveState = 'idle' | 'saving' | 'done' | 'error';

export interface EventBridgePatternBuilderProps {
  onCreated: () => void;
}

function buildPattern(fields: PatternField[]): Record<string, string[]> {
  const pattern: Record<string, string[]> = {};
  for (const field of fields) {
    const name = field.name.trim();
    const values = field.values
      .split(',')
      .map((value) => value.trim())
      .filter((value) => value.length > 0);
    if (name.length > 0 && values.length > 0) {
      pattern[name] = values;
    }
  }
  return pattern;
}

export function EventBridgePatternBuilder({ onCreated }: EventBridgePatternBuilderProps) {
  const [ruleName, setRuleName] = useState('');
  const [state, setState] = useState('ENABLED');
  const [eventBusName, setEventBusName] = useState('');
  const [fields, setFields] = useState<PatternField[]>(() => [
    { name: 'source', values: '' },
    { name: 'detail-type', values: '' },
  ]);
  const [saveState, setSaveState] = useState<SaveState>('idle');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const pattern = useMemo(() => buildPattern(fields), [fields]);
  const preview = useMemo(() => JSON.stringify(pattern, null, 2), [pattern]);
  const hasPattern = Object.keys(pattern).length > 0;

  const updateField = (index: number, patch: Partial<PatternField>) => {
    setFields((current) => current.map((field, i) => (i === index ? { ...field, ...patch } : field)));
    setSaveState('idle');
  };

  const addField = () => {
    setFields((current) => [...current, { name: '', values: '' }]);
    setSaveState('idle');
  };

  const removeField = (index: number) => {
    setFields((current) => current.filter((_, i) => i !== index));
    setSaveState('idle');
  };

  const save = () => {
    if (ruleName.trim().length === 0) {
      setErrorMessage('Enter a rule name before saving.');
      setSaveState('error');
      return;
    }
    if (!hasPattern) {
      setErrorMessage('Add at least one field with at least one value.');
      setSaveState('error');
      return;
    }
    setErrorMessage(null);
    setSaveState('saving');
    createEventBridgeRule({
      name: ruleName.trim(),
      eventPattern: JSON.stringify(pattern),
      state,
      description: null,
      eventBusName: eventBusName.trim().length > 0 ? eventBusName.trim() : null,
    })
      .then(() => {
        setSaveState('done');
        onCreated();
      })
      .catch(() => {
        setErrorMessage('The rule could not be saved. Check the pattern and try again.');
        setSaveState('error');
      });
  };

  return (
    <div data-testid="eventbridge-pattern-builder" style={panelStyle}>
      <Heading as="h3" style={headingStyle}>
        Create event-pattern rule
      </Heading>

      <div style={rowStyle}>
        <input
          data-testid="eventbridge-pattern-name"
          style={inputStyle}
          placeholder="Rule name"
          value={ruleName}
          disabled={saveState === 'saving'}
          onChange={(event) => {
            setRuleName(event.target.value);
            setSaveState('idle');
          }}
        />
        <select
          data-testid="eventbridge-pattern-state"
          style={inputStyle}
          value={state}
          disabled={saveState === 'saving'}
          onChange={(event) => {
            setState(event.target.value);
            setSaveState('idle');
          }}
        >
          <option value="ENABLED">ENABLED</option>
          <option value="DISABLED">DISABLED</option>
        </select>
        <input
          data-testid="eventbridge-pattern-bus"
          style={inputStyle}
          placeholder="Event bus (optional)"
          value={eventBusName}
          disabled={saveState === 'saving'}
          onChange={(event) => {
            setEventBusName(event.target.value);
            setSaveState('idle');
          }}
        />
      </div>

      <div style={fieldColumnStyle}>
        <Text style={labelStyle}>Match fields (comma-separated values)</Text>
        {fields.map((field, index) => (
          <div key={index} data-testid={`eventbridge-pattern-field-${index}`} style={rowStyle}>
            <input
              data-testid={`eventbridge-pattern-field-name-${index}`}
              style={inputStyle}
              placeholder="Field (e.g. source)"
              value={field.name}
              disabled={saveState === 'saving'}
              onChange={(event) => updateField(index, { name: event.target.value })}
            />
            <input
              data-testid={`eventbridge-pattern-field-values-${index}`}
              style={inputStyle}
              placeholder="value1, value2"
              value={field.values}
              disabled={saveState === 'saving'}
              onChange={(event) => updateField(index, { values: event.target.value })}
            />
            <button
              type="button"
              data-testid={`eventbridge-pattern-field-remove-${index}`}
              style={buttonStyle}
              disabled={saveState === 'saving' || fields.length === 1}
              onClick={() => removeField(index)}
            >
              Remove
            </button>
          </div>
        ))}
        <button
          type="button"
          data-testid="eventbridge-pattern-add-field"
          style={buttonStyle}
          disabled={saveState === 'saving'}
          onClick={addField}
        >
          Add field
        </button>
      </div>

      <div style={fieldColumnStyle}>
        <Text style={labelStyle}>Pattern preview</Text>
        <pre data-testid="eventbridge-pattern-preview" style={previewStyle}>
          {preview}
        </pre>
        {hasPattern ? (
          <Text data-testid="eventbridge-pattern-valid" style={messageStyle}>
            Pattern is valid and ready to save.
          </Text>
        ) : (
          <Text data-testid="eventbridge-pattern-empty" style={messageStyle}>
            Add at least one field with a value to build a pattern.
          </Text>
        )}
      </div>

      <button
        type="button"
        data-testid="eventbridge-pattern-save"
        style={buttonStyle}
        disabled={saveState === 'saving'}
        onClick={save}
      >
        {saveState === 'saving' ? 'Saving\u2026' : 'Save rule'}
      </button>

      {saveState === 'done' ? (
        <Text data-testid="eventbridge-pattern-done" style={messageStyle}>
          Rule saved.
        </Text>
      ) : null}
      {saveState === 'error' && errorMessage !== null ? (
        <Text data-testid="eventbridge-pattern-error" style={messageStyle}>
          {errorMessage}
        </Text>
      ) : null}
    </div>
  );
}

export default EventBridgePatternBuilder;
