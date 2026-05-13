import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';

interface ShortcutHint {
  keys: string;
  description: string;
}

const shortcutHints: ShortcutHint[] = [
  { keys: '/', description: 'Focus the search box' },
  { keys: 'g h', description: 'Go to the home page' },
  { keys: '?', description: 'Toggle this shortcuts help' },
  { keys: 'Esc', description: 'Close this help' },
];

function isTypingTarget(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) {
    return false;
  }
  return target.tagName === 'INPUT' || target.tagName === 'TEXTAREA';
}

function focusSearch(): void {
  const input = document.querySelector<HTMLElement>('[data-testid="home-search-input"]');
  input?.focus();
}

const overlayStyle: CSSProperties = { position: 'fixed', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(1, 4, 9, 0.6)', zIndex: 1000 };
const panelStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 12, padding: 24, borderRadius: 8, border: '1px solid #30363d', background: '#161b22', minWidth: 320 };
const rowStyle: CSSProperties = { display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 16 };
const keyStyle: CSSProperties = { fontFamily: 'monospace', padding: '2px 6px', borderRadius: 4, border: '1px solid #30363d', background: '#0d1117' };
const closeStyle: CSSProperties = { alignSelf: 'flex-end', padding: '6px 12px', borderRadius: 6, border: '1px solid #30363d', background: '#21262d', color: 'inherit', cursor: 'pointer' };

export function KeyboardShortcuts({
  onNavigate = (path: string) => {
    window.location.assign(path);
  },
}: {
  onNavigate?: (path: string) => void;
}) {
  const [helpOpen, setHelpOpen] = useState(false);

  useEffect(() => {
    let pendingGo = false;
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setHelpOpen(false);
        pendingGo = false;
        return;
      }
      if (isTypingTarget(event.target)) {
        return;
      }
      if (pendingGo) {
        pendingGo = false;
        if (event.key === 'h') {
          event.preventDefault();
          onNavigate('/');
        }
        return;
      }
      if (event.key === '/') {
        event.preventDefault();
        focusSearch();
        return;
      }
      if (event.key === '?') {
        event.preventDefault();
        setHelpOpen((open) => !open);
        return;
      }
      if (event.key === 'g') {
        pendingGo = true;
      }
    }
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [onNavigate]);

  if (!helpOpen) {
    return null;
  }

  return (
    <div data-testid="shortcut-help" role="dialog" aria-label="Keyboard shortcuts" style={overlayStyle}>
      <div style={panelStyle}>
        <Heading as="h2" data-testid="shortcut-help-title" style={{ fontSize: 16 }}>Keyboard shortcuts</Heading>
        {shortcutHints.map((hint) => (
          <div key={hint.keys} data-testid="shortcut-help-item" style={rowStyle}>
            <Text style={{ fontSize: 14 }}>{hint.description}</Text>
            <span style={keyStyle}>{hint.keys}</span>
          </div>
        ))}
        <button type="button" data-testid="shortcut-help-close" onClick={() => setHelpOpen(false)} style={closeStyle}>Close</button>
      </div>
    </div>
  );
}
