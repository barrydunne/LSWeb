import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import { getLambdaLayers } from '../../api/client';
import type { LambdaLayerItem } from '../../api/client';
import { ResourceLink } from '../../components/ResourceLink';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const messageStyle: CSSProperties = { fontSize: 14 };

const layerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
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

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 13 };

type LoadState = 'loading' | 'ready' | 'error';

export function LambdaLayersTab({ functionName }: { functionName: string }) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [layers, setLayers] = useState<LambdaLayerItem[]>([]);

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getLambdaLayers(functionName, signal)
        .then((data) => {
          setLayers(data.layers);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [functionName],
  );

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  if (loadState === 'loading') {
    return (
      <p data-testid="lambda-layers-loading" style={messageStyle}>
        Loading layers&hellip;
      </p>
    );
  }

  if (loadState === 'error') {
    return (
      <p data-testid="lambda-layers-error" style={messageStyle}>
        Unable to load layers.
      </p>
    );
  }

  if (layers.length === 0) {
    return (
      <p data-testid="lambda-layers-empty" style={messageStyle}>
        No layers are attached to this function.
      </p>
    );
  }

  return (
    <div data-testid="lambda-layers-tab" style={containerStyle}>
      {layers.map((layer) => (
        <div key={layer.arn} data-testid={`lambda-layer-${layer.arn}`} style={layerStyle}>
          <ResourceLink reference={layer.arn} />
          <div style={metaRowStyle}>
            <div>
              <Text style={labelStyle}>Name</Text>
              <Text style={valueStyle}> {layer.name}</Text>
            </div>
            <div>
              <Text style={labelStyle}>Version</Text>
              <Text style={valueStyle}> {layer.version}</Text>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

export default LambdaLayersTab;
