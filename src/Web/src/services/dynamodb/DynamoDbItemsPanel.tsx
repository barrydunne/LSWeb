import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { deleteDynamoDbItem, putDynamoDbItem, scanDynamoDbItems } from '../../api/client';
import type { DynamoDbItem, DynamoDbKeyAttribute } from '../../api/client';

const SCAN_LIMIT = 25;

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 8,
};

const sectionHeadingStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const editorStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const textareaStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 13,
  minHeight: 120,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
  color: 'inherit',
  resize: 'vertical',
};

const editorActionsStyle: CSSProperties = { display: 'flex', gap: 8 };

const itemStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const preStyle: CSSProperties = {
  margin: 0,
  fontFamily: 'monospace',
  fontSize: 13,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
};

const itemActionsStyle: CSSProperties = { display: 'flex', alignItems: 'center', gap: 8 };

export interface DynamoDbItemsPanelProps {
  tableName: string;
  keySchema: DynamoDbKeyAttribute[];
}

type PanelState =
  | { kind: 'loading' }
  | { kind: 'ready'; items: DynamoDbItem[]; truncated: boolean }
  | { kind: 'error' };

interface Editor {
  mode: 'create' | 'edit';
  value: string;
}

type EditorState = 'idle' | 'saving' | 'error';

function extractKey(itemJson: string, keySchema: DynamoDbKeyAttribute[]): string {
  const parsed = JSON.parse(itemJson) as Record<string, unknown>;
  const key: Record<string, unknown> = {};
  for (const attribute of keySchema) {
    key[attribute.attributeName] = parsed[attribute.attributeName];
  }
  return JSON.stringify(key);
}

export function DynamoDbItemsPanel({ tableName, keySchema }: DynamoDbItemsPanelProps) {
  const [state, setState] = useState<PanelState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [editor, setEditor] = useState<Editor | null>(null);
  const [editorState, setEditorState] = useState<EditorState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    scanDynamoDbItems(tableName, SCAN_LIMIT, controller.signal)
      .then((result) => setState({ kind: 'ready', items: result.items, truncated: result.truncated }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [tableName, reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const openCreate = () => {
    setEditor({ mode: 'create', value: '{\n  \n}' });
    setEditorState('idle');
  };

  const openEdit = (item: DynamoDbItem) => {
    setEditor({ mode: 'edit', value: item.json });
    setEditorState('idle');
  };

  const closeEditor = () => {
    setEditor(null);
    setEditorState('idle');
  };

  const handleSave = (value: string) => {
    setEditorState('saving');
    putDynamoDbItem(tableName, value)
      .then(() => {
        setEditor(null);
        setEditorState('idle');
        refresh();
      })
      .catch(() => setEditorState('error'));
  };

  const handleDelete = useCallback(
    (item: DynamoDbItem) => {
      let keyJson: string;
      try {
        keyJson = extractKey(item.json, keySchema);
      } catch {
        setState({ kind: 'error' });
        return;
      }
      deleteDynamoDbItem(tableName, keyJson)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [keySchema, tableName, refresh],
  );

  const isCreating = editor !== null && editor.mode === 'create';

  return (
    <div data-testid="dynamodb-items-panel" style={sectionStyle}>
      <div style={headerStyle}>
        <Heading as="h4" style={sectionHeadingStyle}>
          Items
        </Heading>
        <button
          type="button"
          data-testid="dynamodb-item-add-toggle"
          style={buttonStyle}
          onClick={isCreating ? closeEditor : openCreate}
        >
          {isCreating ? 'Cancel' : 'Add item'}
        </button>
      </div>

      {editor !== null ? (
        <div data-testid="dynamodb-item-editor" style={editorStyle}>
          <textarea
            data-testid="dynamodb-item-editor-input"
            style={textareaStyle}
            value={editor.value}
            onChange={(event) => setEditor({ mode: editor.mode, value: event.target.value })}
          />
          <div style={editorActionsStyle}>
            <button
              type="button"
              data-testid="dynamodb-item-editor-save"
              style={buttonStyle}
              disabled={editorState === 'saving'}
              onClick={() => handleSave(editor.value)}
            >
              {editorState === 'saving' ? 'Saving\u2026' : 'Save'}
            </button>
            <button
              type="button"
              data-testid="dynamodb-item-editor-cancel"
              style={buttonStyle}
              onClick={closeEditor}
            >
              Cancel
            </button>
          </div>
          {editorState === 'error' ? (
            <p data-testid="dynamodb-item-editor-error" style={messageStyle}>
              Unable to save this item.
            </p>
          ) : null}
        </div>
      ) : null}

      {state.kind === 'loading' ? (
        <p data-testid="dynamodb-items-loading" style={messageStyle}>
          Loading items&hellip;
        </p>
      ) : null}
      {state.kind === 'error' ? (
        <p data-testid="dynamodb-items-error" style={messageStyle}>
          Unable to load items.
        </p>
      ) : null}
      {state.kind === 'ready' && state.items.length === 0 ? (
        <p data-testid="dynamodb-items-empty" style={messageStyle}>
          This table has no items.
        </p>
      ) : null}
      {state.kind === 'ready' && state.items.length > 0 ? (
        <>
          {state.truncated ? (
            <p data-testid="dynamodb-items-truncated" style={messageStyle}>
              Showing the first {SCAN_LIMIT} items. More items exist.
            </p>
          ) : null}
          {state.items.map((item, index) => (
            <div key={item.json} data-testid={`dynamodb-item-${index}`} style={itemStyle}>
              <pre data-testid={`dynamodb-item-json-${index}`} style={preStyle}>
                {item.json}
              </pre>
              <div style={itemActionsStyle}>
                <button
                  type="button"
                  data-testid={`dynamodb-item-edit-${index}`}
                  style={buttonStyle}
                  onClick={() => openEdit(item)}
                >
                  Edit
                </button>
                <ConfirmationHost
                  actionLabel="Delete"
                  prompt="Delete this item?"
                  confirmLabel="Confirm"
                  onConfirm={() => handleDelete(item)}
                />
              </div>
            </div>
          ))}
        </>
      ) : null}
    </div>
  );
}

export default DynamoDbItemsPanel;
