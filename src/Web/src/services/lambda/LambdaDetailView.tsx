import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { deleteLambdaFunction, getLambdaFunction } from '../../api/client';
import type { LambdaFunctionResult } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { ResourceLink } from '../../components/ResourceLink';
import { LambdaEnvironmentTab } from './LambdaEnvironmentTab';
import { LambdaTestTab } from './LambdaTestTab';
import { LambdaEventSourcesTab } from './LambdaEventSourcesTab';
import { LambdaLayersTab } from './LambdaLayersTab';
import { LambdaLogsTab } from './LambdaLogsTab';
import { LambdaInsightsTab } from './LambdaInsightsTab';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };

const tabBarStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  borderBottom: '1px solid #30363d',
  paddingBottom: 8,
};

const tabButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid transparent',
  background: 'transparent',
  color: 'inherit',
  cursor: 'pointer',
};

const activeTabButtonStyle: CSSProperties = {
  ...tabButtonStyle,
  border: '1px solid #30363d',
  background: '#21262d',
};

type DetailState =
  | { kind: 'loading' }
  | { kind: 'ready'; fn: LambdaFunctionResult }
  | { kind: 'error' };

type TabKey =
  | 'overview'
  | 'environment'
  | 'test'
  | 'eventsources'
  | 'layers'
  | 'logs'
  | 'insights';

type DeleteState = 'idle' | 'deleted' | 'error';

interface DetailField {
  key: string;
  label: string;
  value: string | number;
}

export function LambdaDetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<DetailState>({ kind: 'loading' });
  const [tab, setTab] = useState<TabKey>('overview');
  const [deleteState, setDeleteState] = useState<DeleteState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getLambdaFunction(resourceId, controller.signal)
      .then((fn) => setState({ kind: 'ready', fn }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId]);

  if (state.kind === 'loading') {
    return (
      <p data-testid="lambda-detail-loading" style={messageStyle}>
        Loading function&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="lambda-detail-error" style={messageStyle}>
        Unable to load this Lambda function.
      </p>
    );
  }

  const { fn } = state;
  const fields: DetailField[] = [
    { key: 'arn', label: 'ARN', value: fn.functionArn },
    { key: 'runtime', label: 'Runtime', value: fn.runtime },
    { key: 'handler', label: 'Handler', value: fn.handler },
    { key: 'description', label: 'Description', value: fn.description },
    { key: 'memory', label: 'Memory (MB)', value: fn.memorySize },
    { key: 'timeout', label: 'Timeout (s)', value: fn.timeout },
    { key: 'role', label: 'Role', value: fn.role },
    { key: 'lastModified', label: 'Last modified', value: fn.lastModified },
  ];

  const handleDelete = () => {
    setDeleteState('idle');
    deleteLambdaFunction(fn.functionName)
      .then(() => setDeleteState('deleted'))
      .catch(() => setDeleteState('error'));
  };

  return (
    <div data-testid="lambda-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="lambda-detail-name" style={{ fontSize: 16 }}>
        {fn.functionName}
      </Heading>
      <div style={tabBarStyle}>
        <button
          type="button"
          data-testid="lambda-detail-tab-overview"
          style={tab === 'overview' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('overview')}
        >
          Overview
        </button>
        <button
          type="button"
          data-testid="lambda-detail-tab-environment"
          style={tab === 'environment' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('environment')}
        >
          Environment
        </button>
        <button
          type="button"
          data-testid="lambda-detail-tab-test"
          style={tab === 'test' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('test')}
        >
          Test
        </button>
        <button
          type="button"
          data-testid="lambda-detail-tab-eventsources"
          style={tab === 'eventsources' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('eventsources')}
        >
          Event sources
        </button>
        <button
          type="button"
          data-testid="lambda-detail-tab-layers"
          style={tab === 'layers' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('layers')}
        >
          Layers
        </button>
        <button
          type="button"
          data-testid="lambda-detail-tab-logs"
          style={tab === 'logs' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('logs')}
        >
          Logs
        </button>
        <button
          type="button"
          data-testid="lambda-detail-tab-insights"
          style={tab === 'insights' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('insights')}
        >
          Insights
        </button>
      </div>
      {tab === 'overview' &&
        fields.map((field) => (
          <div key={field.key} data-testid={`lambda-detail-${field.key}`} style={rowStyle}>
            <Text style={labelStyle}>{field.label}</Text>
            {field.key === 'role' ? (
              <ResourceLink reference={fn.role} service="iam" />
            ) : (
              <Text style={valueStyle}>{field.value}</Text>
            )}
          </div>
        ))}
      {tab === 'overview' && (
        <div data-testid="lambda-detail-delete">
          <ConfirmationHost
            actionLabel="Delete function"
            prompt={`Delete ${fn.functionName}? This cannot be undone.`}
            confirmLabel="Confirm delete"
            onConfirm={handleDelete}
          />
          {deleteState === 'deleted' ? (
            <Text data-testid="lambda-detail-delete-status" style={messageStyle}>
              Function deleted.
            </Text>
          ) : null}
          {deleteState === 'error' ? (
            <Text data-testid="lambda-detail-delete-error" style={messageStyle}>
              Unable to delete this function.
            </Text>
          ) : null}
        </div>
      )}
      {tab === 'environment' && <LambdaEnvironmentTab functionName={fn.functionName} />}
      {tab === 'test' && <LambdaTestTab functionName={fn.functionName} />}
      {tab === 'eventsources' && <LambdaEventSourcesTab functionName={fn.functionName} />}
      {tab === 'layers' && <LambdaLayersTab functionName={fn.functionName} />}
      {tab === 'logs' && <LambdaLogsTab functionName={fn.functionName} />}
      {tab === 'insights' && <LambdaInsightsTab functionName={fn.functionName} />}
    </div>
  );
}

export default LambdaDetailView;
