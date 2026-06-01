import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import {
  createChangeSet,
  deleteChangeSet,
  executeChangeSet,
  getChangeSet,
  getChangeSets,
} from '../../api/client';
import type {
  CloudFormationChangeSetDetailResult,
  CloudFormationChangeSetListResult,
  StackParameter,
} from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { CloudFormationStackForm } from './CloudFormationStackForm';
import type { StackFormValue } from './CloudFormationStackForm';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};
const sectionHeadingStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };
const tableStyle: CSSProperties = {
  borderCollapse: 'collapse',
  fontSize: 13,
  width: '100%',
};
const cellStyle: CSSProperties = {
  textAlign: 'left',
  padding: '4px 8px',
  border: '1px solid #30363d',
  fontFamily: 'monospace',
};
const headerCellStyle: CSSProperties = {
  ...cellStyle,
  fontFamily: 'inherit',
  opacity: 0.7,
};
const actionsCellStyle: CSSProperties = {
  ...cellStyle,
  display: 'flex',
  gap: 6,
  alignItems: 'center',
};
const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
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
const previewStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const CHANGE_SET_TYPES = ['CREATE', 'UPDATE'] as const;

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; result: CloudFormationChangeSetListResult }
  | { kind: 'error' };

type PreviewState =
  | { kind: 'loading' }
  | { kind: 'ready'; detail: CloudFormationChangeSetDetailResult }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';
type ActionState = 'idle' | 'working' | 'done' | 'error';

function toParameters(value: StackFormValue): StackParameter[] {
  return value.parameters
    .filter((parameter) => parameter.parameterKey.trim() !== '')
    .map((parameter) => ({
      parameterKey: parameter.parameterKey,
      parameterValue: parameter.parameterValue,
    }));
}

interface CloudFormationChangeSetPanelProps {
  stackName: string;
  initialTemplateBody: string;
  initialParameters: StackParameter[];
  initialCapabilities: string[];
}

export function CloudFormationChangeSetPanel({
  stackName,
  initialTemplateBody,
  initialParameters,
  initialCapabilities,
}: CloudFormationChangeSetPanelProps) {
  const [listState, setListState] = useState<ListState>({ kind: 'loading' });
  const [listRefreshKey, setListRefreshKey] = useState(0);
  const [selectedName, setSelectedName] = useState<string | null>(null);
  const [previewState, setPreviewState] = useState<PreviewState>({ kind: 'loading' });
  const [showCreate, setShowCreate] = useState(false);
  const [changeSetName, setChangeSetName] = useState('');
  const [changeSetType, setChangeSetType] = useState<(typeof CHANGE_SET_TYPES)[number]>('UPDATE');
  const [createState, setCreateState] = useState<CreateState>('idle');
  const [actionState, setActionState] = useState<ActionState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    setListState({ kind: 'loading' });
    getChangeSets(stackName, controller.signal)
      .then((result) => setListState({ kind: 'ready', result }))
      .catch(() => setListState({ kind: 'error' }));
    return () => controller.abort();
  }, [stackName, listRefreshKey]);

  useEffect(() => {
    if (selectedName === null) {
      return;
    }
    const controller = new AbortController();
    setPreviewState({ kind: 'loading' });
    getChangeSet(stackName, selectedName, controller.signal)
      .then((detail) => setPreviewState({ kind: 'ready', detail }))
      .catch(() => setPreviewState({ kind: 'error' }));
    return () => controller.abort();
  }, [stackName, selectedName]);

  const refreshList = useCallback(() => {
    setListRefreshKey((key) => key + 1);
  }, []);

  const togglePreview = useCallback((name: string) => {
    setSelectedName((current) => (current === name ? null : name));
  }, []);

  const handleCreate = useCallback(
    (value: StackFormValue) => {
      setCreateState('saving');
      createChangeSet(
        stackName,
        changeSetName,
        changeSetType,
        value.templateBody,
        toParameters(value),
        value.capabilities,
      )
        .then(() => {
          setCreateState('created');
          setShowCreate(false);
          setChangeSetName('');
          refreshList();
        })
        .catch(() => setCreateState('error'));
    },
    [stackName, changeSetName, changeSetType, refreshList],
  );

  const handleExecute = useCallback(
    (name: string) => {
      setActionState('working');
      executeChangeSet(stackName, name)
        .then(() => {
          setActionState('done');
          setSelectedName(null);
          refreshList();
        })
        .catch(() => setActionState('error'));
    },
    [stackName, refreshList],
  );

  const handleDelete = useCallback(
    (name: string) => {
      setActionState('working');
      deleteChangeSet(stackName, name)
        .then(() => {
          setActionState('done');
          setSelectedName((current) => (current === name ? null : current));
          refreshList();
        })
        .catch(() => setActionState('error'));
    },
    [stackName, refreshList],
  );

  return (
    <div style={sectionStyle}>
      <Heading
        as="h3"
        data-testid="cloudformation-changesets-heading"
        style={sectionHeadingStyle}
      >
        Change sets
      </Heading>
      <div style={{ display: 'flex', gap: 6 }}>
        <button
          type="button"
          data-testid="cloudformation-changesets-refresh"
          style={buttonStyle}
          onClick={refreshList}
        >
          Refresh
        </button>
        <button
          type="button"
          data-testid="cloudformation-changesets-create-toggle"
          style={buttonStyle}
          onClick={() => setShowCreate((current) => !current)}
        >
          {showCreate ? 'Cancel' : 'Create change set'}
        </button>
      </div>
      {showCreate ? (
        <div data-testid="cloudformation-changesets-create" style={sectionStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cloudformation-changesets-name">
              Change set name
            </label>
            <input
              id="cloudformation-changesets-name"
              type="text"
              data-testid="cloudformation-changesets-name"
              style={inputStyle}
              value={changeSetName}
              onChange={(event) => setChangeSetName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cloudformation-changesets-type">
              Change set type
            </label>
            <select
              id="cloudformation-changesets-type"
              data-testid="cloudformation-changesets-type"
              style={inputStyle}
              value={changeSetType}
              onChange={(event) =>
                setChangeSetType(event.target.value as (typeof CHANGE_SET_TYPES)[number])
              }
            >
              {CHANGE_SET_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <CloudFormationStackForm
            testIdPrefix="cloudformation-changeset"
            submitLabel="Create change set"
            saving={createState === 'saving'}
            initialTemplateBody={initialTemplateBody}
            initialParameters={initialParameters}
            initialCapabilities={initialCapabilities}
            onSubmit={handleCreate}
          />
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="cloudformation-changesets-create-status" style={messageStyle}>
          Change set created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="cloudformation-changesets-create-error" style={messageStyle}>
          Unable to create the change set.
        </p>
      ) : null}
      {actionState === 'error' ? (
        <p data-testid="cloudformation-changesets-action-error" style={messageStyle}>
          Unable to complete the change set action.
        </p>
      ) : null}
      {listState.kind === 'loading' ? (
        <p data-testid="cloudformation-changesets-loading" style={messageStyle}>
          Loading change sets&hellip;
        </p>
      ) : listState.kind === 'error' ? (
        <p data-testid="cloudformation-changesets-error" style={messageStyle}>
          Unable to load the change sets.
        </p>
      ) : listState.result.changeSets.length === 0 ? (
        <p data-testid="cloudformation-changesets-empty" style={messageStyle}>
          No change sets.
        </p>
      ) : (
        <table data-testid="cloudformation-changesets" style={tableStyle}>
          <thead>
            <tr>
              <th style={headerCellStyle}>Name</th>
              <th style={headerCellStyle}>Status</th>
              <th style={headerCellStyle}>Execution status</th>
              <th style={headerCellStyle}>Created</th>
              <th style={headerCellStyle}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {listState.result.changeSets.map((changeSet) => (
              <tr key={changeSet.changeSetId}>
                <td style={cellStyle}>{changeSet.changeSetName}</td>
                <td style={cellStyle}>{changeSet.status}</td>
                <td style={cellStyle}>{changeSet.executionStatus}</td>
                <td style={cellStyle}>{changeSet.creationTime}</td>
                <td style={actionsCellStyle}>
                  <button
                    type="button"
                    data-testid={`cloudformation-changesets-preview-${changeSet.changeSetName}`}
                    style={buttonStyle}
                    onClick={() => togglePreview(changeSet.changeSetName)}
                  >
                    {selectedName === changeSet.changeSetName ? 'Hide' : 'Preview'}
                  </button>
                  <ConfirmationHost
                    actionLabel="Execute"
                    prompt={`Execute change set "${changeSet.changeSetName}"?`}
                    confirmLabel="Execute"
                    onConfirm={() => handleExecute(changeSet.changeSetName)}
                  />
                  <ConfirmationHost
                    actionLabel="Delete"
                    prompt={`Delete change set "${changeSet.changeSetName}"?`}
                    confirmLabel="Delete"
                    onConfirm={() => handleDelete(changeSet.changeSetName)}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      {selectedName !== null ? (
        <div data-testid="cloudformation-changesets-preview" style={previewStyle}>
          {previewState.kind === 'loading' ? (
            <p data-testid="cloudformation-changesets-preview-loading" style={messageStyle}>
              Loading preview&hellip;
            </p>
          ) : previewState.kind === 'error' ? (
            <p data-testid="cloudformation-changesets-preview-error" style={messageStyle}>
              Unable to load the change set preview.
            </p>
          ) : previewState.detail.changes.length === 0 ? (
            <p data-testid="cloudformation-changesets-preview-empty" style={messageStyle}>
              No resource changes.
            </p>
          ) : (
            <table data-testid="cloudformation-changesets-preview-table" style={tableStyle}>
              <thead>
                <tr>
                  <th style={headerCellStyle}>Action</th>
                  <th style={headerCellStyle}>Logical ID</th>
                  <th style={headerCellStyle}>Type</th>
                  <th style={headerCellStyle}>Replacement</th>
                </tr>
              </thead>
              <tbody>
                {previewState.detail.changes.map((change) => (
                  <tr key={change.logicalResourceId}>
                    <td style={cellStyle}>{change.action}</td>
                    <td style={cellStyle}>{change.logicalResourceId}</td>
                    <td style={cellStyle}>{change.resourceType}</td>
                    <td style={cellStyle}>{change.replacement ?? '\u2014'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      ) : null}
    </div>
  );
}

export default CloudFormationChangeSetPanel;
