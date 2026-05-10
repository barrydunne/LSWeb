import { useEffect, useState } from 'react';
import { Label, Text } from '@primer/react';
import { getConnectivity, type ConnectivityResult } from '../api/client';

type IndicatorState =
  | { kind: 'checking' }
  | { kind: 'ready'; result: ConnectivityResult }
  | { kind: 'error' };

export function ConnectivityIndicator() {
  const [state, setState] = useState<IndicatorState>({ kind: 'checking' });

  useEffect(() => {
    const controller = new AbortController();
    getConnectivity(controller.signal)
      .then((result) => setState({ kind: 'ready', result }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, []);

  if (state.kind === 'checking') {
    return (
      <Text data-testid="connectivity-indicator" style={{ fontSize: 14 }}>
        Connectivity: checking&hellip;
      </Text>
    );
  }

  if (state.kind === 'error') {
    return (
      <span data-testid="connectivity-indicator" style={{ display: 'inline-flex', alignItems: 'center', gap: 8 }}>
        <Label variant="danger" data-testid="connectivity-status">
          Unavailable
        </Label>
        <Text data-testid="connectivity-error" style={{ fontSize: 12 }}>
          Unable to reach the backend.
        </Text>
      </span>
    );
  }

  const { result } = state;
  const connected = result.status === 'Connected';

  return (
    <span data-testid="connectivity-indicator" style={{ display: 'inline-flex', alignItems: 'center', gap: 8 }}>
      <Label variant={connected ? 'success' : 'danger'} data-testid="connectivity-status">
        {connected ? 'Connected' : 'Disconnected'}
      </Label>
      <Text data-testid="connectivity-target" style={{ fontSize: 12 }}>
        {result.endpoint} &middot; {result.region}
      </Text>
      {!connected && result.error ? (
        <Text data-testid="connectivity-error" style={{ fontSize: 12 }}>
          {result.error}
        </Text>
      ) : null}
    </span>
  );
}
