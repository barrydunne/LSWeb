import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createIamPolicy, deleteIamPolicy, getIamPolicies } from '../../api/client';
import type { IamPolicyScope, IamPolicySummary } from '../../api/client';
import { PolicyDocumentEditor } from './components/PolicyDocumentEditor';

const messageStyle: CSSProperties = { fontSize: 14 };

const toggleBarStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  marginBottom: 12,
};

const scopeButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid transparent',
  background: 'transparent',
  color: 'inherit',
  cursor: 'pointer',
};

const activeScopeButtonStyle: CSSProperties = {
  ...scopeButtonStyle,
  border: '1px solid #30363d',
  background: '#21262d',
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

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
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

const columns: DataListColumn[] = [
  { key: 'name', label: 'Policy' },
  { key: 'type', label: 'Type' },
  { key: 'attachments', label: 'Attachments' },
  { key: 'actions', label: 'Actions' },
];

/**
 * Sensible default document for a new customer-managed policy.
 */
const defaultPolicyDocument = {
  Version: '2012-10-17',
  Statement: [{ Effect: 'Allow', Action: '*', Resource: '*' }],
};

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; policies: IamPolicySummary[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

interface IamPoliciesPanelProps {
  serviceKey: string;
}

/**
 * Lists IAM managed policies with a scope toggle between customer-managed (default) and
 * read-only AWS-managed policies. Customer-managed policies support create and delete; each row
 * links to the policy detail view.
 */
export function IamPoliciesPanel({ serviceKey }: IamPoliciesPanelProps) {
  const [scope, setScope] = useState<IamPolicyScope>('local');
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [policyName, setPolicyName] = useState('');
  const [path, setPath] = useState('');
  const [description, setDescription] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getIamPolicies(scope, controller.signal)
      .then((result) => setState({ kind: 'ready', policies: result.policies }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [scope, reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = (document: unknown) => {
    setCreateState('saving');
    const trimmedPath = path.trim();
    const trimmedDescription = description.trim();
    createIamPolicy({
      policyName,
      policyDocument: JSON.stringify(document),
      description: trimmedDescription === '' ? null : trimmedDescription,
      path: trimmedPath === '' ? null : trimmedPath,
    })
      .then(() => {
        setCreateState('created');
        setPolicyName('');
        setPath('');
        setDescription('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (arn: string) => {
      deleteIamPolicy(arn)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  const awsManaged = scope === 'aws';

  return (
    <div data-testid="iam-policies-panel">
      <div role="tablist" style={toggleBarStyle}>
        <button
          type="button"
          role="tab"
          aria-selected={!awsManaged}
          data-testid="iam-policies-scope-local"
          style={awsManaged ? scopeButtonStyle : activeScopeButtonStyle}
          onClick={() => setScope('local')}
        >
          Customer managed
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={awsManaged}
          data-testid="iam-policies-scope-aws"
          style={awsManaged ? activeScopeButtonStyle : scopeButtonStyle}
          onClick={() => setScope('aws')}
        >
          AWS managed
        </button>
      </div>

      {awsManaged ? null : (
        <button
          type="button"
          data-testid="iam-policies-create-toggle"
          style={buttonStyle}
          onClick={() => setShowCreate((current) => !current)}
        >
          {showCreate ? 'Cancel' : 'Create policy'}
        </button>
      )}
      {showCreate && !awsManaged ? (
        <div data-testid="iam-policies-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-policies-create-name">
              Policy name
            </label>
            <input
              id="iam-policies-create-name"
              type="text"
              data-testid="iam-policies-create-name"
              style={inputStyle}
              value={policyName}
              onChange={(event) => setPolicyName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-policies-create-path">
              Path (optional)
            </label>
            <input
              id="iam-policies-create-path"
              type="text"
              data-testid="iam-policies-create-path"
              style={inputStyle}
              value={path}
              onChange={(event) => setPath(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="iam-policies-create-description">
              Description (optional)
            </label>
            <input
              id="iam-policies-create-description"
              type="text"
              data-testid="iam-policies-create-description"
              style={inputStyle}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
            />
          </div>
          <PolicyDocumentEditor
            value={defaultPolicyDocument}
            title="Policy document"
            onSave={handleCreate}
            testId="iam-policies-create-document"
          />
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="iam-policies-create-status" style={messageStyle}>
          Policy created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="iam-policies-create-error" style={messageStyle}>
          Unable to create the policy.
        </p>
      ) : null}

      {state.kind === 'loading' ? (
        <p data-testid="iam-policies-loading" style={messageStyle}>
          Loading policies&hellip;
        </p>
      ) : null}
      {state.kind === 'error' ? (
        <p data-testid="iam-policies-error" style={messageStyle}>
          Unable to load IAM policies.
        </p>
      ) : null}
      {state.kind === 'ready' ? (
        <DataListShell
          title="Policies"
          onRefresh={refresh}
          columns={columns}
          rows={buildRows(state.policies, serviceKey, awsManaged, handleDelete)}
          itemCount={state.policies.length}
          filterPlaceholder="Filter policies"
          columnPrefsKey="iam-policies"
          emptyState={{ message: 'No IAM policies found on this backend.' }}
        />
      ) : null}
    </div>
  );
}

/**
 * Build the data-list rows for the current set of policies. AWS-managed policies render a
 * read-only action cell; customer-managed policies expose a delete confirmation.
 */
function buildRows(
  policies: IamPolicySummary[],
  serviceKey: string,
  awsManaged: boolean,
  onDelete: (arn: string) => void,
): DataListRow[] {
  return policies.map((policy) => ({
    id: policy.arn,
    filterText: `${policy.policyName} ${policy.arn} ${policy.path}`,
    cells: {
      name: (
        <Link
          data-testid="iam-policies-link"
          to={`/services/${serviceKey}/${encodeURIComponent(`policy/${policy.arn}`)}`}
        >
          {policy.policyName}
        </Link>
      ),
      type: awsManaged ? 'AWS managed' : 'Customer managed',
      attachments: String(policy.attachmentCount),
      actions: awsManaged ? (
        <span data-testid="iam-policies-readonly">Read-only</span>
      ) : (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${policy.policyName}?`}
          confirmLabel="Confirm"
          onConfirm={() => onDelete(policy.arn)}
        />
      ),
    },
  }));
}

export default IamPoliciesPanel;
