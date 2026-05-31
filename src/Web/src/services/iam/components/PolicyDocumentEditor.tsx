import { useMemo, useState } from 'react';
import type { ChangeEvent, CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { formatPolicyDocument, validatePolicyDocument } from './policyDocument';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
};

const buttonGroupStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
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

const disabledButtonStyle: CSSProperties = {
  ...buttonStyle,
  opacity: 0.5,
  cursor: 'not-allowed',
};

const preStyle: CSSProperties = {
  margin: 0,
  padding: 12,
  borderRadius: 6,
  background: '#161b22',
  fontFamily: 'monospace',
  fontSize: 12,
  lineHeight: 1.5,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
  overflowX: 'auto',
};

const textareaStyle: CSSProperties = {
  width: '100%',
  minHeight: 200,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
  fontFamily: 'monospace',
  fontSize: 12,
  lineHeight: 1.5,
  boxSizing: 'border-box',
  resize: 'vertical',
};

const errorListStyle: CSSProperties = {
  margin: 0,
  paddingLeft: 18,
  color: '#f85149',
  fontSize: 12,
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

/**
 * Narrow an unknown value to a plain JSON object (excluding arrays and null).
 */
interface PolicyDocumentViewerProps {
  value: unknown;
  title?: string;
  testId?: string;
}

/**
 * Read-only, pretty-printed view of a policy document. Used for trust policies and
 * attached-policy previews.
 */
export function PolicyDocumentViewer({
  value,
  title = 'Policy document',
  testId = 'policy-document-viewer',
}: PolicyDocumentViewerProps) {
  const json = useMemo(() => formatPolicyDocument(value), [value]);

  return (
    <div data-testid={testId} style={containerStyle}>
      <Heading as="h3" data-testid={`${testId}-title`} style={{ fontSize: 14 }}>
        {title}
      </Heading>
      <pre data-testid={`${testId}-content`} style={preStyle}>
        {json}
      </pre>
    </div>
  );
}

interface PolicyDocumentEditorProps {
  value: unknown;
  title?: string;
  readOnly?: boolean;
  onSave?: (document: unknown) => void;
  testId?: string;
}

/**
 * Reusable IAM policy-document editor/viewer shared by users, groups, roles and policies.
 * Supports a read-only view and an editable mode with inline client-side validation.
 */
export function PolicyDocumentEditor({
  value,
  title = 'Policy document',
  readOnly = false,
  onSave,
  testId = 'policy-document-editor',
}: PolicyDocumentEditorProps) {
  const initialText = useMemo(() => formatPolicyDocument(value), [value]);
  const [editing, setEditing] = useState(false);
  const [text, setText] = useState(initialText);

  const errors = useMemo(() => validatePolicyDocument(text), [text]);
  const isValid = errors.length === 0;

  if (readOnly) {
    return <PolicyDocumentViewer value={value} title={title} testId={`${testId}-readonly`} />;
  }

  const handleEdit = () => {
    setText(initialText);
    setEditing(true);
  };

  const handleCancel = () => {
    setText(initialText);
    setEditing(false);
  };

  const handleSave = () => {
    onSave?.(JSON.parse(text));
    setEditing(false);
  };

  return (
    <div data-testid={testId} style={containerStyle}>
      <div style={headerStyle}>
        <Heading as="h3" data-testid={`${testId}-title`} style={{ fontSize: 14 }}>
          {title}
        </Heading>
        {editing ? (
          <div style={buttonGroupStyle}>
            <button
              type="button"
              data-testid={`${testId}-save`}
              style={isValid ? buttonStyle : disabledButtonStyle}
              disabled={!isValid}
              onClick={handleSave}
            >
              Save
            </button>
            <button
              type="button"
              data-testid={`${testId}-cancel`}
              style={buttonStyle}
              onClick={handleCancel}
            >
              Cancel
            </button>
          </div>
        ) : (
          <button type="button" data-testid={`${testId}-edit`} style={buttonStyle} onClick={handleEdit}>
            Edit
          </button>
        )}
      </div>
      {editing ? (
        <>
          <textarea
            data-testid={`${testId}-input`}
            style={textareaStyle}
            spellCheck={false}
            value={text}
            onChange={(event: ChangeEvent<HTMLTextAreaElement>) => setText(event.target.value)}
          />
          {isValid ? null : (
            <ul data-testid={`${testId}-errors`} style={errorListStyle}>
              {errors.map((error) => (
                <li key={error}>{error}</li>
              ))}
            </ul>
          )}
        </>
      ) : (
        <pre data-testid={`${testId}-content`} style={preStyle}>
          {initialText}
        </pre>
      )}
    </div>
  );
}

export default PolicyDocumentEditor;
