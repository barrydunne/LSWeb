import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import {
  attachIamRolePolicy,
  deleteIamRoleInlinePolicy,
  deleteIamRolePermissionsBoundary,
  detachIamRolePolicy,
  getIamRole,
  getIamRoleUsedBy,
  putIamRoleInlinePolicy,
  putIamRolePermissionsBoundary,
  tagIamRole,
  untagIamRole,
  updateIamRole,
} from '../../api/client';
import type { IamRoleConsumer, IamRoleDetail } from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { ResourceLink } from '../../components/ResourceLink';
import { PolicyDocumentEditor, PolicyDocumentViewer } from './components/PolicyDocumentEditor';
import { TrustPolicyBuilder } from './components/TrustPolicyBuilder';
import { PermissionsBoundaryControl } from './components/PermissionsBoundaryControl';
import { TagEditor } from './components/TagEditor';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 2 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
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

const listStyle: CSSProperties = {
  listStyle: 'none',
  margin: 0,
  padding: 0,
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const itemRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

type LoadState = 'loading' | 'ready' | 'error' | 'notFound';
type TabKey = 'trust' | 'permissions' | 'usedby' | 'tags';

interface TabDescriptor {
  key: TabKey;
  label: string;
}

const tabs: TabDescriptor[] = [
  { key: 'trust', label: 'Trust relationships' },
  { key: 'permissions', label: 'Permissions' },
  { key: 'usedby', label: 'Used by' },
  { key: 'tags', label: 'Tags' },
];

const defaultInlineDocument = {
  Version: '2012-10-17',
  Statement: [{ Effect: 'Allow', Action: '*', Resource: '*' }],
};

/**
 * Parse a stored policy document string into an object for display, falling back to the raw
 * string when it is not valid JSON.
 */
function parsePolicyDocument(document: string): unknown {
  try {
    return JSON.parse(document);
  } catch {
    return document;
  }
}

interface IamRoleDetailViewProps {
  roleName: string;
}

/**
 * Full IAM role detail view with an editable description / max-session field and sub-tabs for
 * trust relationships, permissions, and a "Used by" placeholder.
 */
export function IamRoleDetailView({ roleName }: IamRoleDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [detail, setDetail] = useState<IamRoleDetail | null>(null);
  const [tab, setTab] = useState<TabKey>('trust');
  const [mutationError, setMutationError] = useState(false);
  const [description, setDescription] = useState('');
  const [maxSession, setMaxSession] = useState('');
  const [attachArn, setAttachArn] = useState('');
  const [inlineName, setInlineName] = useState('');
  const [inlineNameError, setInlineNameError] = useState(false);
  const [usedBy, setUsedBy] = useState<IamRoleConsumer[]>([]);
  const [usedByState, setUsedByState] = useState<LoadState>('loading');

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getIamRole(roleName, signal)
        .then((data) => {
          if (data === null) {
            setDetail(null);
            setLoadState('notFound');
            return;
          }
          setDetail(data);
          setDescription(data.description ?? '');
          setMaxSession(data.maxSessionDuration === null ? '' : String(data.maxSessionDuration));
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [roleName],
  );

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  useEffect(() => {
    const controller = new AbortController();
    setUsedByState('loading');
    getIamRoleUsedBy(roleName, controller.signal)
      .then((data) => {
        setUsedBy(data.consumers);
        setUsedByState('ready');
      })
      .catch(() => setUsedByState('error'));
    return () => controller.abort();
  }, [roleName]);

  const reload = useCallback(() => load(), [load]);

  const handleSaveSettings = () => {
    setMutationError(false);
    const trimmedDescription = description.trim();
    const trimmedMaxSession = maxSession.trim();
    updateIamRole(roleName, {
      description: trimmedDescription === '' ? null : trimmedDescription,
      maxSessionDuration: trimmedMaxSession === '' ? null : Number(trimmedMaxSession),
      trustPolicyDocument: null,
    })
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleSaveTrustPolicy = (document: unknown) => {
    setMutationError(false);
    updateIamRole(roleName, {
      description: null,
      maxSessionDuration: null,
      trustPolicyDocument: JSON.stringify(document),
    })
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleAttachPolicy = () => {
    setMutationError(false);
    attachIamRolePolicy(roleName, attachArn)
      .then(() => {
        setAttachArn('');
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleDetachPolicy = (policyArn: string) => {
    setMutationError(false);
    detachIamRolePolicy(roleName, policyArn)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleAddInlinePolicy = (document: unknown) => {
    setMutationError(false);
    const trimmed = inlineName.trim();
    if (trimmed === '') {
      setInlineNameError(true);
      return;
    }
    setInlineNameError(false);
    putIamRoleInlinePolicy(roleName, trimmed, JSON.stringify(document))
      .then(() => {
        setInlineName('');
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleDeleteInlinePolicy = (policyName: string) => {
    setMutationError(false);
    deleteIamRoleInlinePolicy(roleName, policyName)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleAddTag = (key: string, value: string) => {
    setMutationError(false);
    tagIamRole(roleName, [{ key, value }])
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleRemoveTag = (key: string) => {
    setMutationError(false);
    untagIamRole(roleName, [key])
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleSetBoundary = (arn: string) => {
    setMutationError(false);
    putIamRolePermissionsBoundary(roleName, arn)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleRemoveBoundary = () => {
    setMutationError(false);
    deleteIamRolePermissionsBoundary(roleName)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="iam-role-detail-loading" style={messageStyle}>
        Loading role&hellip;
      </p>
    );
  }

  if (loadState === 'notFound') {
    return (
      <p data-testid="iam-role-detail-not-found" style={messageStyle}>
        This IAM role was not found. It may have been deleted, or it may be a placeholder role that
        was never created as a real IAM resource.
      </p>
    );
  }

  if (loadState === 'error' || detail === null) {
    return (
      <p data-testid="iam-role-detail-error" style={messageStyle}>
        Unable to load this IAM role.
      </p>
    );
  }

  return (
    <div data-testid="iam-role-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="iam-role-detail-name" style={{ fontSize: 16 }}>
        {detail.roleName}
      </Heading>
      <div data-testid="iam-role-detail-arn" style={rowStyle}>
        <Text style={labelStyle}>ARN</Text>
        <Text style={valueStyle}>{detail.arn}</Text>
      </div>
      <div data-testid="iam-role-detail-roleId" style={rowStyle}>
        <Text style={labelStyle}>Role ID</Text>
        <Text style={valueStyle}>{detail.roleId}</Text>
      </div>
      <div data-testid="iam-role-detail-path" style={rowStyle}>
        <Text style={labelStyle}>Path</Text>
        <Text style={valueStyle}>{detail.path}</Text>
      </div>
      <div data-testid="iam-role-detail-created" style={rowStyle}>
        <Text style={labelStyle}>Created</Text>
        <Text style={valueStyle}>{detail.createDate ?? '\u2014'}</Text>
      </div>

      <div data-testid="iam-role-detail-settings-form" style={formStyle}>
        <div style={rowStyle}>
          <label style={labelStyle} htmlFor="iam-role-detail-description">
            Description
          </label>
          <input
            id="iam-role-detail-description"
            type="text"
            data-testid="iam-role-detail-description"
            style={inputStyle}
            value={description}
            onChange={(event) => setDescription(event.target.value)}
          />
        </div>
        <div style={rowStyle}>
          <label style={labelStyle} htmlFor="iam-role-detail-max-session">
            Max session duration in seconds
          </label>
          <input
            id="iam-role-detail-max-session"
            type="number"
            data-testid="iam-role-detail-max-session"
            style={inputStyle}
            value={maxSession}
            onChange={(event) => setMaxSession(event.target.value)}
          />
        </div>
        <button
          type="button"
          data-testid="iam-role-detail-settings-submit"
          style={buttonStyle}
          onClick={handleSaveSettings}
        >
          Save settings
        </button>
      </div>

      {mutationError ? (
        <p data-testid="iam-role-detail-mutation-error" style={messageStyle}>
          The last action could not be completed.
        </p>
      ) : null}

      <div role="tablist" style={tabBarStyle}>
        {tabs.map((descriptor) => (
          <button
            key={descriptor.key}
            type="button"
            role="tab"
            aria-selected={tab === descriptor.key}
            data-testid={`iam-role-detail-tab-${descriptor.key}`}
            style={tab === descriptor.key ? activeTabButtonStyle : tabButtonStyle}
            onClick={() => setTab(descriptor.key)}
          >
            {descriptor.label}
          </button>
        ))}
      </div>

      {tab === 'trust' ? (
        <div data-testid="iam-role-detail-panel-trust" style={rowStyle}>
          <TrustPolicyBuilder
            value={parsePolicyDocument(detail.assumeRolePolicyDocument)}
            onSave={handleSaveTrustPolicy}
            testId="iam-role-detail-trust-builder"
          />
          <PolicyDocumentEditor
            value={parsePolicyDocument(detail.assumeRolePolicyDocument)}
            title="Trust relationship policy"
            onSave={handleSaveTrustPolicy}
            testId="iam-role-detail-trust-editor"
          />
        </div>
      ) : null}

      {tab === 'permissions' ? (
        <div data-testid="iam-role-detail-panel-permissions" style={rowStyle}>
          <Text style={labelStyle}>Attached managed policies</Text>
          {detail.attachedPolicies.length === 0 ? (
            <Text data-testid="iam-role-detail-attached-empty" style={messageStyle}>
              No managed policies attached.
            </Text>
          ) : (
            <ul data-testid="iam-role-detail-attached-list" style={listStyle}>
              {detail.attachedPolicies.map((policy) => (
                <li key={policy.policyArn} data-testid="iam-role-detail-attached-item" style={itemRowStyle}>
                  <div style={rowStyle}>
                    <Text style={valueStyle}>{policy.policyName}</Text>
                    <Text style={labelStyle}>{policy.policyArn}</Text>
                  </div>
                  <ConfirmationHost
                    actionLabel="Detach"
                    prompt={`Detach ${policy.policyName}?`}
                    confirmLabel="Confirm"
                    onConfirm={() => handleDetachPolicy(policy.policyArn)}
                  />
                </li>
              ))}
            </ul>
          )}
          <div data-testid="iam-role-detail-attach-form" style={formStyle}>
            <label style={labelStyle} htmlFor="iam-role-detail-attach-arn">
              Policy ARN
            </label>
            <input
              id="iam-role-detail-attach-arn"
              type="text"
              data-testid="iam-role-detail-attach-arn"
              style={inputStyle}
              value={attachArn}
              onChange={(event) => setAttachArn(event.target.value)}
            />
            <button
              type="button"
              data-testid="iam-role-detail-attach-submit"
              style={buttonStyle}
              onClick={handleAttachPolicy}
            >
              Attach policy
            </button>
          </div>

          <Text style={labelStyle}>Inline policies</Text>
          {detail.inlinePolicies.length === 0 ? (
            <Text data-testid="iam-role-detail-inline-empty" style={messageStyle}>
              No inline policies.
            </Text>
          ) : (
            <ul data-testid="iam-role-detail-inline-list" style={listStyle}>
              {detail.inlinePolicies.map((policy) => (
                <li
                  key={policy.policyName}
                  data-testid="iam-role-detail-inline-item"
                  style={{ ...itemRowStyle, flexDirection: 'column', alignItems: 'stretch' }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12 }}>
                    <Text style={valueStyle}>{policy.policyName}</Text>
                    <ConfirmationHost
                      actionLabel="Delete"
                      prompt={`Delete inline policy ${policy.policyName}?`}
                      confirmLabel="Confirm"
                      onConfirm={() => handleDeleteInlinePolicy(policy.policyName)}
                    />
                  </div>
                  <PolicyDocumentViewer
                    value={parsePolicyDocument(policy.policyDocument)}
                    title={policy.policyName}
                    testId="iam-role-detail-inline-viewer"
                  />
                </li>
              ))}
            </ul>
          )}
          <div data-testid="iam-role-detail-inline-form" style={formStyle}>
            <label style={labelStyle} htmlFor="iam-role-detail-inline-name">
              Inline policy name
            </label>
            <input
              id="iam-role-detail-inline-name"
              type="text"
              data-testid="iam-role-detail-inline-name"
              style={inputStyle}
              value={inlineName}
              onChange={(event) => setInlineName(event.target.value)}
            />
            {inlineNameError ? (
              <Text data-testid="iam-role-detail-inline-name-error" style={messageStyle}>
                Enter a name for the inline policy.
              </Text>
            ) : null}
            <PolicyDocumentEditor
              value={defaultInlineDocument}
              title="Inline policy document"
              onSave={handleAddInlinePolicy}
              testId="iam-role-detail-inline-editor"
            />
          </div>
        </div>
      ) : null}

      {tab === 'usedby' ? (
        <div data-testid="iam-role-detail-panel-usedby" style={rowStyle}>
          {usedByState === 'loading' ? (
            <Text data-testid="iam-role-detail-usedby-loading" style={messageStyle}>
              Loading resources that use this role…
            </Text>
          ) : usedByState === 'error' ? (
            <Text data-testid="iam-role-detail-usedby-error" style={messageStyle}>
              Resources that use this role could not be loaded.
            </Text>
          ) : usedBy.length === 0 ? (
            <Text data-testid="iam-role-detail-usedby-empty" style={messageStyle}>
              No resources currently use this role. Some references may not be reported by the local
              backend.
            </Text>
          ) : (
            <ul style={listStyle}>
              {usedBy.map((consumer) => (
                <li
                  key={`${consumer.serviceKey}:${consumer.resourceName}`}
                  data-testid="iam-role-detail-usedby-item"
                  style={itemRowStyle}
                >
                  <Text style={labelStyle}>{consumer.consumerType}</Text>
                  <ResourceLink
                    reference={consumer.resourceName}
                    service={consumer.serviceKey}
                  />
                </li>
              ))}
            </ul>
          )}
        </div>
      ) : null}

      {tab === 'tags' ? (
        <div data-testid="iam-role-detail-panel-tags" style={rowStyle}>
          <TagEditor
            tags={detail.tags}
            onAdd={handleAddTag}
            onRemove={handleRemoveTag}
            testId="iam-role-detail-tags"
          />
          <PermissionsBoundaryControl
            boundaryArn={detail.permissionsBoundaryArn}
            onSet={handleSetBoundary}
            onRemove={handleRemoveBoundary}
            testId="iam-role-detail-boundary"
          />
        </div>
      ) : null}
    </div>
  );
}

export default IamRoleDetailView;
