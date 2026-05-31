import { useEffect, useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { getStateMachine } from '../../api/client';
import type { StateMachineDetailResult } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { ResourceLink } from '../../components/ResourceLink';
import { RawJsonViewer } from '../../components/RawJsonViewer';
import { StateMachineGraph } from './StateMachineGraph';
import { StepFunctionsExecutionsPanel } from './StepFunctionsExecutionsPanel';

function parseDefinition(definition: string): unknown {
  try {
    return JSON.parse(definition);
  } catch {
    return definition;
  }
}

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
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };
const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};
const sectionHeadingStyle: CSSProperties = { fontSize: 14 };

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; stateMachine: StateMachineDetailResult }
  | { kind: 'error' };

export function StepFunctionsDetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });

  const definition = state.kind === 'ready' ? state.stateMachine.definition : '';
  const parsedDefinition = useMemo(() => parseDefinition(definition), [definition]);

  useEffect(() => {
    const controller = new AbortController();
    getStateMachine(resourceId, controller.signal)
      .then((stateMachine) => setState({ kind: 'ready', stateMachine }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId]);

  if (state.kind === 'loading') {
    return (
      <p data-testid="step-functions-detail-loading" style={messageStyle}>
        Loading state machine&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="step-functions-detail-error" style={messageStyle}>
        Unable to load the state machine.
      </p>
    );
  }

  const stateMachine = state.stateMachine;

  return (
    <div data-testid="step-functions-detail-view" style={containerStyle}>
      <Heading as="h2" data-testid="step-functions-detail-name" style={{ fontSize: 18 }}>
        {stateMachine.name}
      </Heading>
      <div style={rowStyle}>
        <span style={labelStyle}>ARN</span>
        <span data-testid="step-functions-detail-arn" style={valueStyle}>
          {stateMachine.stateMachineArn}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Type</span>
        <span data-testid="step-functions-detail-type" style={valueStyle}>
          {stateMachine.type}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Status</span>
        <span data-testid="step-functions-detail-status" style={valueStyle}>
          {stateMachine.status}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Role</span>
        <span data-testid="step-functions-detail-role" style={valueStyle}>
          <ResourceLink reference={stateMachine.roleArn} service="iam" />
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Created</span>
        <span data-testid="step-functions-detail-created" style={valueStyle}>
          {stateMachine.creationDate}
        </span>
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="step-functions-detail-graph-heading"
          style={sectionHeadingStyle}
        >
          Workflow graph
        </Heading>
        <StateMachineGraph definition={stateMachine.definition} />
      </div>
      <div data-testid="step-functions-detail-definition" style={sectionStyle}>
        <RawJsonViewer value={parsedDefinition} title="ASL definition" />
      </div>
      <StepFunctionsExecutionsPanel stateMachineArn={stateMachine.stateMachineArn} />
    </div>
  );
}

export default StepFunctionsDetailView;
