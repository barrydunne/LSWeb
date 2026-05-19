import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import {
  getLambdaEventSourceMappings,
  setLambdaEventSourceMappingState,
} from '../../api/client';
import type { LambdaEventSourceMappingItem } from '../../api/client';
import { ResourceLink } from '../../components/ResourceLink';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const messageStyle: CSSProperties = { fontSize: 14 };

const mappingStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  flexWrap: 'wrap',
};

const metaRowStyle: CSSProperties = {
  display: 'flex',
  gap: 16,
  flexWrap: 'wrap',
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 13 };

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const enabledBadgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 8px',
  borderRadius: 10,
  border: '1px solid #2ea043',
  color: '#3fb950',
};

const disabledBadgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 8px',
  borderRadius: 10,
  border: '1px solid #6e7681',
  color: '#8b949e',
};

type LoadState = 'loading' | 'ready' | 'error';
type ActionState = 'idle' | 'error';

function isEnabled(state: string): boolean {
  return state.toLowerCase() === 'enabled';
}

export function LambdaEventSourcesTab({ functionName }: { functionName: string }) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [mappings, setMappings] = useState<LambdaEventSourceMappingItem[]>([]);
  const [actionState, setActionState] = useState<ActionState>('idle');

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getLambdaEventSourceMappings(functionName, signal)
        .then((data) => {
          setMappings(data.mappings);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [functionName],
  );

  useEffect(() => {
    const controller = new AbortController();
    setActionState('idle');
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const handleToggle = (mapping: LambdaEventSourceMappingItem) => {
    setActionState('idle');
    setLambdaEventSourceMappingState(functionName, mapping.uuid, !isEnabled(mapping.state))
      .then(() => load())
      .catch(() => setActionState('error'));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="lambda-event-sources-loading" style={messageStyle}>
        Loading event source mappings&hellip;
      </p>
    );
  }

  if (loadState === 'error') {
    return (
      <p data-testid="lambda-event-sources-error" style={messageStyle}>
        Unable to load event source mappings.
      </p>
    );
  }

  if (mappings.length === 0) {
    return (
      <p data-testid="lambda-event-sources-empty" style={messageStyle}>
        No event source mappings are configured for this function.
      </p>
    );
  }

  return (
    <div data-testid="lambda-event-sources-tab" style={containerStyle}>
      {actionState === 'error' ? (
        <Text data-testid="lambda-event-sources-action-error" style={messageStyle}>
          Unable to update the event source mapping.
        </Text>
      ) : null}
      {mappings.map((mapping) => {
        const enabled = isEnabled(mapping.state);
        return (
          <div
            key={mapping.uuid}
            data-testid={`lambda-event-source-${mapping.uuid}`}
            style={mappingStyle}
          >
            <div style={rowStyle}>
              <ResourceLink reference={mapping.eventSourceArn} />
              <div style={rowStyle}>
                <span
                  data-testid={`lambda-event-source-state-${mapping.uuid}`}
                  style={enabled ? enabledBadgeStyle : disabledBadgeStyle}
                >
                  {enabled ? 'Enabled' : 'Disabled'}
                </span>
                <button
                  type="button"
                  data-testid={`lambda-event-source-toggle-${mapping.uuid}`}
                  style={buttonStyle}
                  onClick={() => handleToggle(mapping)}
                >
                  {enabled ? 'Disable' : 'Enable'}
                </button>
              </div>
            </div>
            <div style={metaRowStyle}>
              <div>
                <Text style={labelStyle}>State</Text>
                <Text style={valueStyle}> {mapping.state}</Text>
              </div>
              <div>
                <Text style={labelStyle}>Batch size</Text>
                <Text style={valueStyle}> {mapping.batchSize}</Text>
              </div>
              <div>
                <Text style={labelStyle}>Last modified</Text>
                <Text style={valueStyle}> {mapping.lastModified}</Text>
              </div>
            </div>
            <div>
              <Text style={labelStyle}>UUID</Text>
              <Text style={valueStyle}> {mapping.uuid}</Text>
            </div>
          </div>
        );
      })}
    </div>
  );
}

export default LambdaEventSourcesTab;
