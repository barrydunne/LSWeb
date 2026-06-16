import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createStateMachine, deleteStateMachine, getStateMachines } from '../../api/client';
import type { StateMachineItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const arnCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const typeBadgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
  fontFamily: 'monospace',
};

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  marginBottom: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const fieldRowStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 2 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const textareaStyle: CSSProperties = {
  ...inputStyle,
  fontFamily: 'monospace',
  minHeight: 120,
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

const defaultDefinition = JSON.stringify(
  {
    Comment: 'A minimal state machine',
    StartAt: 'Done',
    States: { Done: { Type: 'Pass', End: true } },
  },
  null,
  2,
);

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

const columns: DataListColumn[] = [
  { key: 'name', label: 'State machine' },
  { key: 'type', label: 'Type' },
  { key: 'arn', label: 'ARN' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; stateMachines: StateMachineItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'invalid' | 'error';

export function StepFunctionsListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState('');
  const [definition, setDefinition] = useState(defaultDefinition);
  const [roleArn, setRoleArn] = useState('');
  const [type, setType] = useState('STANDARD');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getStateMachines(controller.signal)
      .then((result) => setState({ kind: 'ready', stateMachines: result.stateMachines }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleDelete = useCallback(
    (arn: string) => {
      deleteStateMachine(arn)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  const handleCreate = () => {
    if (name.trim() === '' || !roleArn.trim().startsWith('arn:') || !isValidAslDefinition(definition)) {
      setCreateState('invalid');
      return;
    }
    setCreateState('saving');
    createStateMachine({ name: name.trim(), definition, roleArn: roleArn.trim(), type })
      .then(() => {
        setCreateState('created');
        setName('');
        setDefinition(defaultDefinition);
        setRoleArn('');
        setType('STANDARD');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="step-functions-list-loading" style={messageStyle}>
        Loading state machines&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="step-functions-list-error" style={messageStyle}>
        Unable to load Step Functions state machines.
      </p>
    );
  }

  const rows: DataListRow[] = state.stateMachines.map((stateMachine) => ({
    id: stateMachine.stateMachineArn,
    filterText: `${stateMachine.name} ${stateMachine.stateMachineArn} ${stateMachine.type}`,
    cells: {
      name: (
        <Link
          data-testid="step-functions-list-name"
          to={`/services/${serviceKey}/${encodeURIComponent(stateMachine.stateMachineArn)}`}
        >
          {stateMachine.name}
        </Link>
      ),
      type: (
        <span data-testid="step-functions-list-type" style={typeBadgeStyle}>
          {stateMachine.type}
        </span>
      ),
      arn: (
        <span data-testid="step-functions-list-arn" style={arnCellStyle}>
          {stateMachine.stateMachineArn}
        </span>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${stateMachine.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(stateMachine.stateMachineArn)}
        />
      ),
    },
  }));

  return (
    <div data-testid="step-functions-list-view">
      <button
        type="button"
        data-testid="step-functions-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create state machine'}
      </button>
      {showCreate ? (
        <div data-testid="step-functions-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="step-functions-create-name">
              Name
            </label>
            <input
              id="step-functions-create-name"
              type="text"
              data-testid="step-functions-create-name"
              style={inputStyle}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="step-functions-create-role">
              IAM role ARN
            </label>
            <input
              id="step-functions-create-role"
              type="text"
              data-testid="step-functions-create-role"
              style={inputStyle}
              placeholder="arn:aws:iam::000000000000:role/..."
              value={roleArn}
              onChange={(event) => setRoleArn(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="step-functions-create-type">
              Type
            </label>
            <select
              id="step-functions-create-type"
              data-testid="step-functions-create-type"
              style={inputStyle}
              value={type}
              onChange={(event) => setType(event.target.value)}
            >
              <option value="STANDARD">Standard</option>
              <option value="EXPRESS">Express</option>
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="step-functions-create-definition">
              Definition (Amazon States Language)
            </label>
            <textarea
              id="step-functions-create-definition"
              data-testid="step-functions-create-definition"
              style={textareaStyle}
              value={definition}
              onChange={(event) => setDefinition(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="step-functions-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
          {createState === 'invalid' ? (
            <p data-testid="step-functions-create-invalid" style={messageStyle}>
              Enter a name, an IAM role ARN, and a valid ASL definition (with StartAt and States).
            </p>
          ) : null}
          {createState === 'error' ? (
            <p data-testid="step-functions-create-error" style={messageStyle}>
              Unable to create the state machine.
            </p>
          ) : null}
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="step-functions-create-status" style={messageStyle}>
          State machine created.
        </p>
      ) : null}
      <DataListShell
        title="State machines"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter state machines"
        columnPrefsKey={`${serviceKey}-state-machines`}
        emptyState={{ message: 'No Step Functions state machines found on this backend.' }}
      />
    </div>
  );
}

export default StepFunctionsListView;
