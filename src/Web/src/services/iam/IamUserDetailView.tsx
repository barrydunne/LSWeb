import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import {
  addIamUserToGroup,
  attachIamUserPolicy,
  createIamAccessKey,
  deleteIamAccessKey,
  deleteIamUserInlinePolicy,
  deleteIamUserPermissionsBoundary,
  detachIamUserPolicy,
  getIamUser,
  putIamUserInlinePolicy,
  putIamUserPermissionsBoundary,
  removeIamUserFromGroup,
  tagIamUser,
  untagIamUser,
  updateIamAccessKeyStatus,
} from '../../api/client';
import type { IamAccessKey, IamAccessKeySecret, IamUserDetail } from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { PolicyDocumentEditor } from './components/PolicyDocumentEditor';
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

const secretBoxStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #d29922',
  background: '#1c1808',
};

type LoadState = 'loading' | 'ready' | 'error';
type TabKey = 'permissions' | 'groups' | 'access-keys' | 'tags';

interface TabDescriptor {
  key: TabKey;
  label: string;
}

const tabs: TabDescriptor[] = [
  { key: 'permissions', label: 'Permissions' },
  { key: 'groups', label: 'Groups' },
  { key: 'access-keys', label: 'Access keys' },
  { key: 'tags', label: 'Tags' },
];

const defaultInlineDocument = {
  Version: '2012-10-17',
  Statement: [{ Effect: 'Allow', Action: '*', Resource: '*' }],
};

interface IamUserDetailViewProps {
  userName: string;
}

/**
 * Full IAM user detail view with sub-tabs for permissions, group membership and access keys.
 */
export function IamUserDetailView({ userName }: IamUserDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [detail, setDetail] = useState<IamUserDetail | null>(null);
  const [tab, setTab] = useState<TabKey>('permissions');
  const [mutationError, setMutationError] = useState(false);
  const [attachArn, setAttachArn] = useState('');
  const [inlineName, setInlineName] = useState('');
  const [inlineNameError, setInlineNameError] = useState(false);
  const [groupName, setGroupName] = useState('');
  const [newKeySecret, setNewKeySecret] = useState<IamAccessKeySecret | null>(null);

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getIamUser(userName, signal)
        .then((data) => {
          setDetail(data);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [userName],
  );

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const reload = useCallback(() => load(), [load]);

  const handleAttachPolicy = () => {
    setMutationError(false);
    attachIamUserPolicy(userName, attachArn)
      .then(() => {
        setAttachArn('');
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleDetachPolicy = (policyArn: string) => {
    setMutationError(false);
    detachIamUserPolicy(userName, policyArn)
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
    putIamUserInlinePolicy(userName, trimmed, JSON.stringify(document))
      .then(() => {
        setInlineName('');
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleDeleteInlinePolicy = (policyName: string) => {
    setMutationError(false);
    deleteIamUserInlinePolicy(userName, policyName)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleAddGroup = () => {
    setMutationError(false);
    addIamUserToGroup(userName, groupName)
      .then(() => {
        setGroupName('');
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleRemoveGroup = (group: string) => {
    setMutationError(false);
    removeIamUserFromGroup(userName, group)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleCreateAccessKey = () => {
    setMutationError(false);
    createIamAccessKey(userName)
      .then((secret) => {
        setNewKeySecret(secret);
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleToggleKeyStatus = (key: IamAccessKey) => {
    setMutationError(false);
    const next = key.status === 'Active' ? 'Inactive' : 'Active';
    updateIamAccessKeyStatus(userName, key.accessKeyId, next)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleDeleteAccessKey = (accessKeyId: string) => {
    setMutationError(false);
    deleteIamAccessKey(userName, accessKeyId)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleAddTag = (key: string, value: string) => {
    setMutationError(false);
    tagIamUser(userName, [{ key, value }])
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleRemoveTag = (key: string) => {
    setMutationError(false);
    untagIamUser(userName, [key])
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleSetBoundary = (arn: string) => {
    setMutationError(false);
    putIamUserPermissionsBoundary(userName, arn)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleRemoveBoundary = () => {
    setMutationError(false);
    deleteIamUserPermissionsBoundary(userName)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="iam-user-detail-loading" style={messageStyle}>
        Loading user&hellip;
      </p>
    );
  }

  if (loadState === 'error' || detail === null) {
    return (
      <p data-testid="iam-user-detail-error" style={messageStyle}>
        Unable to load this IAM user.
      </p>
    );
  }

  return (
    <div data-testid="iam-user-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="iam-user-detail-name" style={{ fontSize: 16 }}>
        {detail.userName}
      </Heading>
      <div data-testid="iam-user-detail-arn" style={rowStyle}>
        <Text style={labelStyle}>ARN</Text>
        <Text style={valueStyle}>{detail.arn}</Text>
      </div>
      <div data-testid="iam-user-detail-userId" style={rowStyle}>
        <Text style={labelStyle}>User ID</Text>
        <Text style={valueStyle}>{detail.userId}</Text>
      </div>
      <div data-testid="iam-user-detail-path" style={rowStyle}>
        <Text style={labelStyle}>Path</Text>
        <Text style={valueStyle}>{detail.path}</Text>
      </div>
      <div data-testid="iam-user-detail-created" style={rowStyle}>
        <Text style={labelStyle}>Created</Text>
        <Text style={valueStyle}>{detail.createDate ?? '\u2014'}</Text>
      </div>

      {mutationError ? (
        <p data-testid="iam-user-detail-mutation-error" style={messageStyle}>
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
            data-testid={`iam-user-detail-tab-${descriptor.key}`}
            style={tab === descriptor.key ? activeTabButtonStyle : tabButtonStyle}
            onClick={() => setTab(descriptor.key)}
          >
            {descriptor.label}
          </button>
        ))}
      </div>

      {tab === 'permissions' ? (
        <div data-testid="iam-user-detail-panel-permissions" style={rowStyle}>
          <Text style={labelStyle}>Attached managed policies</Text>
          {detail.attachedPolicies.length === 0 ? (
            <Text data-testid="iam-user-detail-attached-empty" style={messageStyle}>
              No managed policies attached.
            </Text>
          ) : (
            <ul data-testid="iam-user-detail-attached-list" style={listStyle}>
              {detail.attachedPolicies.map((policy) => (
                <li key={policy.policyArn} data-testid="iam-user-detail-attached-item" style={itemRowStyle}>
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
          <div data-testid="iam-user-detail-attach-form" style={formStyle}>
            <label style={labelStyle} htmlFor="iam-user-detail-attach-arn">
              Policy ARN
            </label>
            <input
              id="iam-user-detail-attach-arn"
              type="text"
              data-testid="iam-user-detail-attach-arn"
              style={inputStyle}
              value={attachArn}
              onChange={(event) => setAttachArn(event.target.value)}
            />
            <button
              type="button"
              data-testid="iam-user-detail-attach-submit"
              style={buttonStyle}
              onClick={handleAttachPolicy}
            >
              Attach policy
            </button>
          </div>

          <Text style={labelStyle}>Inline policies</Text>
          {detail.inlinePolicyNames.length === 0 ? (
            <Text data-testid="iam-user-detail-inline-empty" style={messageStyle}>
              No inline policies.
            </Text>
          ) : (
            <ul data-testid="iam-user-detail-inline-list" style={listStyle}>
              {detail.inlinePolicyNames.map((name) => (
                <li key={name} data-testid="iam-user-detail-inline-item" style={itemRowStyle}>
                  <Text style={valueStyle}>{name}</Text>
                  <ConfirmationHost
                    actionLabel="Delete"
                    prompt={`Delete inline policy ${name}?`}
                    confirmLabel="Confirm"
                    onConfirm={() => handleDeleteInlinePolicy(name)}
                  />
                </li>
              ))}
            </ul>
          )}
          <div data-testid="iam-user-detail-inline-form" style={formStyle}>
            <label style={labelStyle} htmlFor="iam-user-detail-inline-name">
              Inline policy name
            </label>
            <input
              id="iam-user-detail-inline-name"
              type="text"
              data-testid="iam-user-detail-inline-name"
              style={inputStyle}
              value={inlineName}
              onChange={(event) => setInlineName(event.target.value)}
            />
            {inlineNameError ? (
              <Text data-testid="iam-user-detail-inline-name-error" style={messageStyle}>
                Enter a name for the inline policy.
              </Text>
            ) : null}
            <PolicyDocumentEditor
              value={defaultInlineDocument}
              title="Inline policy document"
              onSave={handleAddInlinePolicy}
              testId="iam-user-detail-inline-editor"
            />
          </div>
        </div>
      ) : null}

      {tab === 'groups' ? (
        <div data-testid="iam-user-detail-panel-groups" style={rowStyle}>
          {detail.groups.length === 0 ? (
            <Text data-testid="iam-user-detail-groups-empty" style={messageStyle}>
              Not a member of any groups.
            </Text>
          ) : (
            <ul data-testid="iam-user-detail-groups-list" style={listStyle}>
              {detail.groups.map((group) => (
                <li key={group} data-testid="iam-user-detail-group-item" style={itemRowStyle}>
                  <Text style={valueStyle}>{group}</Text>
                  <ConfirmationHost
                    actionLabel="Remove"
                    prompt={`Remove from ${group}?`}
                    confirmLabel="Confirm"
                    onConfirm={() => handleRemoveGroup(group)}
                  />
                </li>
              ))}
            </ul>
          )}
          <div data-testid="iam-user-detail-group-form" style={formStyle}>
            <label style={labelStyle} htmlFor="iam-user-detail-group-name">
              Group name
            </label>
            <input
              id="iam-user-detail-group-name"
              type="text"
              data-testid="iam-user-detail-group-name"
              style={inputStyle}
              value={groupName}
              onChange={(event) => setGroupName(event.target.value)}
            />
            <button
              type="button"
              data-testid="iam-user-detail-group-submit"
              style={buttonStyle}
              onClick={handleAddGroup}
            >
              Add to group
            </button>
          </div>
        </div>
      ) : null}

      {tab === 'access-keys' ? (
        <div data-testid="iam-user-detail-panel-access-keys" style={rowStyle}>
          {newKeySecret ? (
            <div data-testid="iam-user-detail-key-secret" style={secretBoxStyle}>
              <Text style={labelStyle}>New access key (copy the secret now &mdash; it is shown once)</Text>
              <Text data-testid="iam-user-detail-key-secret-id" style={valueStyle}>
                {newKeySecret.accessKeyId}
              </Text>
              <Text data-testid="iam-user-detail-key-secret-value" style={valueStyle}>
                {newKeySecret.secretAccessKey}
              </Text>
              <button
                type="button"
                data-testid="iam-user-detail-key-secret-dismiss"
                style={buttonStyle}
                onClick={() => setNewKeySecret(null)}
              >
                Dismiss
              </button>
            </div>
          ) : null}
          {detail.accessKeys.length === 0 ? (
            <Text data-testid="iam-user-detail-keys-empty" style={messageStyle}>
              No access keys.
            </Text>
          ) : (
            <ul data-testid="iam-user-detail-keys-list" style={listStyle}>
              {detail.accessKeys.map((key) => (
                <li key={key.accessKeyId} data-testid="iam-user-detail-key-item" style={itemRowStyle}>
                  <div style={rowStyle}>
                    <Text style={valueStyle}>{key.accessKeyId}</Text>
                    <Text style={labelStyle}>
                      {key.status} &middot; created {key.createDate ?? '\u2014'}
                    </Text>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <button
                      type="button"
                      data-testid="iam-user-detail-key-toggle"
                      style={buttonStyle}
                      onClick={() => handleToggleKeyStatus(key)}
                    >
                      {key.status === 'Active' ? 'Deactivate' : 'Activate'}
                    </button>
                    <ConfirmationHost
                      actionLabel="Delete"
                      prompt={`Delete access key ${key.accessKeyId}?`}
                      confirmLabel="Confirm"
                      onConfirm={() => handleDeleteAccessKey(key.accessKeyId)}
                    />
                  </div>
                </li>
              ))}
            </ul>
          )}
          <button
            type="button"
            data-testid="iam-user-detail-key-create"
            style={buttonStyle}
            onClick={handleCreateAccessKey}
          >
            Create access key
          </button>
        </div>
      ) : null}

      {tab === 'tags' ? (
        <div data-testid="iam-user-detail-panel-tags" style={rowStyle}>
          <TagEditor
            tags={detail.tags}
            onAdd={handleAddTag}
            onRemove={handleRemoveTag}
            testId="iam-user-detail-tags"
          />
          <PermissionsBoundaryControl
            boundaryArn={detail.permissionsBoundaryArn}
            onSet={handleSetBoundary}
            onRemove={handleRemoveBoundary}
            testId="iam-user-detail-boundary"
          />
        </div>
      ) : null}
    </div>
  );
}

export default IamUserDetailView;
