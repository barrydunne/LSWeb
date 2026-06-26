import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createStack, deleteStack, getStacks, validateTemplate } from '../../api/client';
import type {
  CloudFormationStackItem,
  CloudFormationTemplateValidationResult,
  StackParameter,
} from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';
import { CloudFormationStackForm } from './CloudFormationStackForm';
import type { StackFormValue, TemplateSource } from './CloudFormationStackForm';

const messageStyle: CSSProperties = { fontSize: 14 };

const statusBadgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
  fontFamily: 'monospace',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  marginBottom: 12,
};

const columns: DataListColumn[] = [
  { key: 'name', label: 'Stack' },
  { key: 'status', label: 'Status' },
  { key: 'created', label: 'Created' },
  { key: 'updated', label: 'Last updated' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; stacks: CloudFormationStackItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

type ValidateState =
  | { kind: 'idle' }
  | { kind: 'validating' }
  | { kind: 'valid'; result: CloudFormationTemplateValidationResult }
  | { kind: 'error' };

function toParameters(value: StackFormValue): StackParameter[] {
  return value.parameters
    .filter((parameter) => parameter.parameterKey.trim() !== '')
    .map((parameter) => ({
      parameterKey: parameter.parameterKey,
      parameterValue: parameter.parameterValue,
    }));
}

export function CloudFormationListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [createState, setCreateState] = useState<CreateState>('idle');
  const [validateState, setValidateState] = useState<ValidateState>({ kind: 'idle' });

  useEffect(() => {
    const controller = new AbortController();
    getStacks(controller.signal)
      .then((result) => setState({ kind: 'ready', stacks: result.stacks }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setReloadToken((token) => token + 1);
  };

  const handleCreate = (value: StackFormValue) => {
    setCreateState('saving');
    createStack(
      value.stackName,
      value.templateUrl.trim() !== '' ? null : value.templateBody,
      value.templateUrl.trim() !== '' ? value.templateUrl : null,
      toParameters(value),
      value.capabilities,
    )
      .then(() => {
        setCreateState('created');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleValidate = (source: TemplateSource) => {
    setValidateState({ kind: 'validating' });
    validateTemplate(source.templateBody, source.templateUrl)
      .then((result) => setValidateState({ kind: 'valid', result }))
      .catch(() => setValidateState({ kind: 'error' }));
  };

  const handleDelete = (name: string) => {
    deleteStack(name)
      .then(() => refresh())
      .catch(() => setState({ kind: 'error' }));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="cloudformation-list-loading" style={messageStyle}>
        Loading stacks&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="cloudformation-list-error" style={messageStyle}>
        Unable to load CloudFormation stacks.
      </p>
    );
  }

  const rows: DataListRow[] = state.stacks.map((stack) => ({
    id: stack.stackName,
    filterText: `${stack.stackName} ${stack.stackStatus}`,
    cells: {
      name: (
        <Link
          data-testid="cloudformation-list-name"
          to={`/services/${serviceKey}/${encodeURIComponent(stack.stackName)}`}
        >
          {stack.stackName}
        </Link>
      ),
      status: (
        <span data-testid="cloudformation-list-status" style={statusBadgeStyle}>
          {stack.stackStatus}
        </span>
      ),
      created: stack.creationTime,
      updated: stack.lastUpdatedTime ?? '\u2014',
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${stack.stackName}? This removes all resources it manages.`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(stack.stackName)}
        />
      ),
    },
  }));

  return (
    <div data-testid="cloudformation-list-view">
      <button
        type="button"
        data-testid="cloudformation-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create stack'}
      </button>
      {showCreate ? (
        <CloudFormationStackForm
          testIdPrefix="cloudformation-create"
          submitLabel="Create"
          saving={createState === 'saving'}
          requireName
          allowTemplateUrl
          validating={validateState.kind === 'validating'}
          onSubmit={handleCreate}
          onValidate={handleValidate}
        />
      ) : null}
      {showCreate && validateState.kind === 'valid' ? (
        <div data-testid="cloudformation-validate-result" style={messageStyle}>
          <p>Template is valid.</p>
          <p data-testid="cloudformation-validate-description">
            {validateState.result.description === ''
              ? 'No description provided.'
              : validateState.result.description}
          </p>
          <p data-testid="cloudformation-validate-capabilities">
            {validateState.result.capabilities.length === 0
              ? 'No additional capabilities required.'
              : validateState.result.capabilities.join(', ')}
          </p>
          <ul>
            {validateState.result.parameters.map((parameter) => (
              <li key={parameter.parameterKey} data-testid="cloudformation-validate-parameter">
                {parameter.parameterKey}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
      {showCreate && validateState.kind === 'error' ? (
        <p data-testid="cloudformation-validate-error" style={messageStyle}>
          Unable to validate the template.
        </p>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="cloudformation-create-status" style={messageStyle}>
          Stack creation requested.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="cloudformation-create-error" style={messageStyle}>
          Unable to create the stack.
        </p>
      ) : null}
      <DataListShell
        title="Stacks"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter stacks"
        columnPrefsKey={`${serviceKey}-stacks`}
        emptyState={{ message: 'No CloudFormation stacks found on this backend.' }}
      />
    </div>
  );
}

export default CloudFormationListView;
