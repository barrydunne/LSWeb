import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import {
  createEventBridgeEventBus,
  deleteEventBridgeEventBus,
  getEventBridgeEventBuses,
} from '../../api/client';
import type { EventBridgeEventBusItem } from '../../api/client';

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
const messageStyle: CSSProperties = { fontSize: 13 };
const rowStyle: CSSProperties = { display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' };
const columnStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 8 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
  flex: 1,
  minWidth: 160,
};

const busRowStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  alignItems: 'center',
  justifyContent: 'space-between',
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
};

const arnStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12, wordBreak: 'break-all' };
const mutedStyle: CSSProperties = { color: '#8b949e', fontSize: 12 };

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; buses: EventBridgeEventBusItem[] }
  | { kind: 'error' };

type SaveState = 'idle' | 'saving' | 'error';

function isDefaultBus(name: string): boolean {
  return name === 'default';
}

export function EventBridgeBusesManager() {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [busName, setBusName] = useState('');
  const [saveState, setSaveState] = useState<SaveState>('idle');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    getEventBridgeEventBuses(controller.signal)
      .then((result) => setState({ kind: 'ready', buses: result.buses }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const createBus = () => {
    const name = busName.trim();
    if (name.length === 0) {
      setErrorMessage('Enter an event bus name.');
      setSaveState('error');
      return;
    }
    setErrorMessage(null);
    setSaveState('saving');
    createEventBridgeEventBus(name)
      .then(() => {
        setBusName('');
        setSaveState('idle');
        refresh();
      })
      .catch(() => {
        setErrorMessage('The event bus could not be created. Check the name and try again.');
        setSaveState('error');
      });
  };

  const deleteBus = (name: string) => {
    deleteEventBridgeEventBus(name)
      .then(() => refresh())
      .catch(() => {
        setErrorMessage(`The event bus ${name} could not be deleted.`);
        setSaveState('error');
      });
  };

  return (
    <div data-testid="eventbridge-buses-manager" style={panelStyle}>
      <div style={rowStyle}>
        <Heading as="h3" style={headingStyle}>
          Event buses
        </Heading>
        <button
          type="button"
          data-testid="eventbridge-buses-refresh"
          style={buttonStyle}
          onClick={refresh}
        >
          Refresh
        </button>
      </div>

      {state.kind === 'loading' ? (
        <Text data-testid="eventbridge-buses-loading" style={messageStyle}>
          Loading event buses&hellip;
        </Text>
      ) : null}
      {state.kind === 'error' ? (
        <Text data-testid="eventbridge-buses-error" style={messageStyle}>
          Unable to load event buses.
        </Text>
      ) : null}
      {state.kind === 'ready' && state.buses.length === 0 ? (
        <Text data-testid="eventbridge-buses-empty" style={messageStyle}>
          No event buses found.
        </Text>
      ) : null}
      {state.kind === 'ready' && state.buses.length > 0 ? (
        <div style={columnStyle}>
          {state.buses.map((bus) => (
            <div key={bus.name} data-testid={`eventbridge-bus-${bus.name}`} style={busRowStyle}>
              <span style={arnStyle}>
                {bus.name}: {bus.arn}
              </span>
              {isDefaultBus(bus.name) ? (
                <span data-testid={`eventbridge-bus-default-${bus.name}`} style={mutedStyle}>
                  default
                </span>
              ) : (
                <ConfirmationHost
                  actionLabel="Delete"
                  prompt={`Delete event bus ${bus.name}?`}
                  confirmLabel="Confirm"
                  onConfirm={() => deleteBus(bus.name)}
                />
              )}
            </div>
          ))}
        </div>
      ) : null}

      <div style={rowStyle}>
        <input
          data-testid="eventbridge-bus-name"
          style={inputStyle}
          placeholder="New event bus name"
          value={busName}
          disabled={saveState === 'saving'}
          onChange={(event) => setBusName(event.target.value)}
        />
        <button
          type="button"
          data-testid="eventbridge-bus-create"
          style={buttonStyle}
          disabled={saveState === 'saving'}
          onClick={createBus}
        >
          {saveState === 'saving' ? 'Creating\u2026' : 'Create bus'}
        </button>
      </div>

      {saveState === 'error' && errorMessage !== null ? (
        <Text data-testid="eventbridge-buses-form-error" style={messageStyle}>
          {errorMessage}
        </Text>
      ) : null}
    </div>
  );
}

export default EventBridgeBusesManager;
