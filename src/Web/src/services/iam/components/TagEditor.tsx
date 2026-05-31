import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import type { IamTag } from '../../../api/client';
import { ConfirmationHost } from '../../../components/ConfirmationHost';

const containerStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 8 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };

const listStyle: CSSProperties = {
  listStyle: 'none',
  margin: 0,
  padding: 0,
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const itemRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

export interface TagEditorProps {
  tags: IamTag[];
  onAdd: (key: string, value: string) => void;
  onRemove: (key: string) => void;
  testId: string;
}

/**
 * Reusable key/value tag editor used by the IAM user, role and policy detail views. Renders the
 * current tags with per-tag remove confirmation and a form for adding (or overwriting) a tag.
 */
export function TagEditor({ tags, onAdd, onRemove, testId }: TagEditorProps) {
  const [keyInput, setKeyInput] = useState('');
  const [valueInput, setValueInput] = useState('');
  const [keyError, setKeyError] = useState(false);

  const handleAdd = () => {
    const trimmedKey = keyInput.trim();
    if (trimmedKey === '') {
      setKeyError(true);
      return;
    }
    setKeyError(false);
    onAdd(trimmedKey, valueInput);
    setKeyInput('');
    setValueInput('');
  };

  return (
    <div data-testid={`${testId}`} style={containerStyle}>
      {tags.length === 0 ? (
        <Text data-testid={`${testId}-empty`} style={messageStyle}>
          No tags applied.
        </Text>
      ) : (
        <ul data-testid={`${testId}-list`} style={listStyle}>
          {tags.map((tag) => (
            <li key={tag.key} data-testid={`${testId}-item`} style={itemRowStyle}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Text style={labelStyle}>{tag.key}</Text>
                <Text style={valueStyle}>{tag.value === '' ? '\u2014' : tag.value}</Text>
              </div>
              <ConfirmationHost
                actionLabel="Remove"
                prompt={`Remove tag ${tag.key}?`}
                confirmLabel="Confirm"
                onConfirm={() => onRemove(tag.key)}
              />
            </li>
          ))}
        </ul>
      )}

      <div data-testid={`${testId}-form`} style={formStyle}>
        <label style={labelStyle} htmlFor={`${testId}-key`}>
          Key
        </label>
        <input
          id={`${testId}-key`}
          type="text"
          data-testid={`${testId}-key`}
          style={inputStyle}
          value={keyInput}
          onChange={(event) => setKeyInput(event.target.value)}
        />
        {keyError ? (
          <Text data-testid={`${testId}-key-error`} style={messageStyle}>
            Enter a key for the tag.
          </Text>
        ) : null}
        <label style={labelStyle} htmlFor={`${testId}-value`}>
          Value
        </label>
        <input
          id={`${testId}-value`}
          type="text"
          data-testid={`${testId}-value`}
          style={inputStyle}
          value={valueInput}
          onChange={(event) => setValueInput(event.target.value)}
        />
        <button type="button" data-testid={`${testId}-submit`} style={buttonStyle} onClick={handleAdd}>
          Add tag
        </button>
      </div>
    </div>
  );
}

export default TagEditor;
