import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { getStateMachines } from '../../api/client';
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

const columns: DataListColumn[] = [
  { key: 'name', label: 'State machine' },
  { key: 'type', label: 'Type' },
  { key: 'arn', label: 'ARN' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; stateMachines: StateMachineItem[] }
  | { kind: 'error' };

export function StepFunctionsListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getStateMachines(controller.signal)
      .then((result) => setState({ kind: 'ready', stateMachines: result.stateMachines }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
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
    },
  }));

  return (
    <div data-testid="step-functions-list-view">
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
