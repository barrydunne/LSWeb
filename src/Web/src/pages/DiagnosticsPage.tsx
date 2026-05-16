import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { getDiagnostics } from '../api/client';
import type { DiagnosticsResult } from '../api/client';
import { MaskedValueField } from '../components/MaskedValueField';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 16,
};

const summaryStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const listStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const hintStyle: CSSProperties = {
  fontSize: 12,
  opacity: 0.8,
};

type DiagnosticsState =
  | { kind: 'loading' }
  | { kind: 'ready'; diagnostics: DiagnosticsResult }
  | { kind: 'error' };

export function DiagnosticsPage() {
  const [state, setState] = useState<DiagnosticsState>({ kind: 'loading' });
  const [revealed, setRevealed] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    getDiagnostics(revealed, controller.signal)
      .then((diagnostics) => setState({ kind: 'ready', diagnostics }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [revealed]);

  const toggleReveal = useCallback(() => setRevealed((current) => !current), []);

  if (state.kind === 'loading') {
    return (
      <section data-testid="diagnostics-page" style={containerStyle}>
        <Heading as="h2" data-testid="diagnostics-heading" style={{ fontSize: 18 }}>
          Connection diagnostics
        </Heading>
        <Text data-testid="diagnostics-loading" style={{ fontSize: 14 }}>
          Loading diagnostics&hellip;
        </Text>
      </section>
    );
  }

  if (state.kind === 'error') {
    return (
      <section data-testid="diagnostics-page" style={containerStyle}>
        <Heading as="h2" data-testid="diagnostics-heading" style={{ fontSize: 18 }}>
          Connection diagnostics
        </Heading>
        <Text data-testid="diagnostics-error" style={{ fontSize: 14 }}>
          Unable to load diagnostics.
        </Text>
      </section>
    );
  }

  const { diagnostics } = state;

  return (
    <section data-testid="diagnostics-page" style={containerStyle}>
      <Heading as="h2" data-testid="diagnostics-heading" style={{ fontSize: 18 }}>
        Connection diagnostics
      </Heading>

      <div data-testid="diagnostics-summary" style={summaryStyle}>
        <Text data-testid="diagnostics-status" style={{ fontSize: 14 }}>
          Connectivity: {diagnostics.connectivityStatus}
        </Text>
        {diagnostics.connectivityError ? (
          <Text data-testid="diagnostics-status-error" style={hintStyle}>
            {diagnostics.connectivityError}
          </Text>
        ) : null}
        <Text data-testid="diagnostics-endpoint" style={hintStyle}>
          Endpoint: {diagnostics.endpoint}
        </Text>
        <Text data-testid="diagnostics-region" style={hintStyle}>
          Region: {diagnostics.region}
        </Text>
      </div>

      {diagnostics.revealAllowed ? null : (
        <Text data-testid="diagnostics-reveal-disabled" style={hintStyle}>
          Revealing sensitive values is disabled on this host.
        </Text>
      )}

      <div data-testid="diagnostics-config" style={listStyle}>
        {diagnostics.configuration.map((item) => (
          <MaskedValueField
            key={item.name}
            name={item.name}
            value={item.value}
            source={item.source}
            isSensitive={item.isSensitive}
            revealed={revealed}
            revealAllowed={diagnostics.revealAllowed}
            onToggleReveal={toggleReveal}
          />
        ))}
      </div>
    </section>
  );
}
