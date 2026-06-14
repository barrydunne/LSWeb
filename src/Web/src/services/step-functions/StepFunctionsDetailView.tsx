import { useCallback, useEffect, useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { getStateMachine, updateStateMachineDefinition } from '../../api/client';
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

function isValidAslDefinition(definition: string): boolean {
  try {
    const parsed: unknown = JSON.parse(definition);
    if (typeof parsed !== 'object' || parsed === null || Array.isArray(parsed)) {
      return false;
    }
    const candidate = parsed as { StartAt?: unknown; States?: unknown };
    return (
      typeof candidate.StartAt === 'string'
      && typeof candidate.States === 'object'
      && candidate.States !== null
      && !Array.isArray(candidate.States)
    );
  } catch {
    return false;
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

const editorTextAreaStyle: CSSProperties = {
  fontSize: 13,
  fontFamily: 'monospace',
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  minHeight: 180,
  resize: 'vertical',
};

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

type EditState = 'idle' | 'saving' | 'saved' | 'invalid' | 'error';

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; stateMachine: StateMachineDetailResult }
  | { kind: 'error' };

export function StepFunctionsDetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [editing, setEditing] = useState(false);
  const [draftDefinition, setDraftDefinition] = useState('');
  const [editState, setEditState] = useState<EditState>('idle');

  const definition = state.kind === 'ready' ? state.stateMachine.definition : '';
  const parsedDefinition = useMemo(() => parseDefinition(definition), [definition]);

  useEffect(() => {
    const controller = new AbortController();
    getStateMachine(resourceId, controller.signal)
      .then((stateMachine) => setState({ kind: 'ready', stateMachine }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, reloadToken]);

  const startEditing = useCallback((currentDefinition: string) => {
    setDraftDefinition(currentDefinition);
    setEditState('idle');
    setEditing(true);
  }, []);

  const handleSaveDefinition = useCallback(
    (stateMachineArn: string) => {
      if (!isValidAslDefinition(draftDefinition)) {
        setEditState('invalid');
        return;
      }
      setEditState('saving');
      updateStateMachineDefinition(stateMachineArn, draftDefinition)
        .then(() => {
          setEditState('saved');
          setEditing(false);
          setReloadToken((token) => token + 1);
        })
        .catch(() => setEditState('error'));
    },
    [draftDefinition],
  );

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
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 8 }}>
          <Heading as="h3" style={sectionHeadingStyle}>
            ASL definition
          </Heading>
          {!editing ? (
            <button
              type="button"
              data-testid="step-functions-definition-edit"
              style={buttonStyle}
              onClick={() => startEditing(stateMachine.definition)}
            >
              Edit definition
            </button>
          ) : null}
        </div>
        {editing ? (
          <div style={sectionStyle}>
            <textarea
              data-testid="step-functions-definition-editor"
              style={editorTextAreaStyle}
              value={draftDefinition}
              onChange={(event) => setDraftDefinition(event.target.value)}
            />
            <div style={{ display: 'flex', gap: 8 }}>
              <button
                type="button"
                data-testid="step-functions-definition-save"
                style={buttonStyle}
                disabled={editState === 'saving'}
                onClick={() => handleSaveDefinition(stateMachine.stateMachineArn)}
              >
                {editState === 'saving' ? 'Saving\u2026' : 'Save definition'}
              </button>
              <button
                type="button"
                data-testid="step-functions-definition-cancel"
                style={buttonStyle}
                onClick={() => setEditing(false)}
              >
                Cancel
              </button>
            </div>
            {editState === 'invalid' ? (
              <p data-testid="step-functions-definition-invalid" style={messageStyle}>
                The definition must be valid ASL JSON (with StartAt and States).
              </p>
            ) : null}
            {editState === 'error' ? (
              <p data-testid="step-functions-definition-error" style={messageStyle}>
                Unable to save the definition.
              </p>
            ) : null}
          </div>
        ) : (
          <RawJsonViewer value={parsedDefinition} title="ASL definition" />
        )}
      </div>
      <StepFunctionsExecutionsPanel stateMachineArn={stateMachine.stateMachineArn} />
    </div>
  );
}

export default StepFunctionsDetailView;
