import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { Link } from 'react-router-dom';
import {
  addIamGroupMember,
  attachIamGroupPolicy,
  deleteIamGroupInlinePolicy,
  detachIamGroupPolicy,
  getIamGroup,
  putIamGroupInlinePolicy,
  removeIamGroupMember,
} from '../../api/client';
import type { IamGroupDetail } from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { PolicyDocumentEditor, PolicyDocumentViewer } from './components/PolicyDocumentEditor';

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

type LoadState = 'loading' | 'ready' | 'error';
type TabKey = 'members' | 'permissions';

interface TabDescriptor {
  key: TabKey;
  label: string;
}

const tabs: TabDescriptor[] = [
  { key: 'members', label: 'Members' },
  { key: 'permissions', label: 'Permissions' },
];

const defaultInlineDocument = {
  Version: '2012-10-17',
  Statement: [{ Effect: 'Allow', Action: '*', Resource: '*' }],
};

/**
 * Parse a stored inline policy document string into an object for display, falling back to
 * the raw string when it is not valid JSON.
 */
function parsePolicyDocument(document: string): unknown {
  try {
    return JSON.parse(document);
  } catch {
    return document;
  }
}

interface IamGroupDetailViewProps {
  groupName: string;
  serviceKey: string;
}

/**
 * Full IAM group detail view with sub-tabs for members and permissions.
 */
export function IamGroupDetailView({ groupName, serviceKey }: IamGroupDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [detail, setDetail] = useState<IamGroupDetail | null>(null);
  const [tab, setTab] = useState<TabKey>('members');
  const [mutationError, setMutationError] = useState(false);
  const [memberName, setMemberName] = useState('');
  const [attachArn, setAttachArn] = useState('');
  const [inlineName, setInlineName] = useState('');
  const [inlineNameError, setInlineNameError] = useState(false);

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getIamGroup(groupName, signal)
        .then((data) => {
          setDetail(data);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [groupName],
  );

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const reload = useCallback(() => load(), [load]);

  const handleAddMember = () => {
    setMutationError(false);
    addIamGroupMember(groupName, memberName)
      .then(() => {
        setMemberName('');
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleRemoveMember = (userName: string) => {
    setMutationError(false);
    removeIamGroupMember(groupName, userName)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleAttachPolicy = () => {
    setMutationError(false);
    attachIamGroupPolicy(groupName, attachArn)
      .then(() => {
        setAttachArn('');
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleDetachPolicy = (policyArn: string) => {
    setMutationError(false);
    detachIamGroupPolicy(groupName, policyArn)
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
    putIamGroupInlinePolicy(groupName, trimmed, JSON.stringify(document))
      .then(() => {
        setInlineName('');
        return reload();
      })
      .catch(() => setMutationError(true));
  };

  const handleDeleteInlinePolicy = (policyName: string) => {
    setMutationError(false);
    deleteIamGroupInlinePolicy(groupName, policyName)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="iam-group-detail-loading" style={messageStyle}>
        Loading group&hellip;
      </p>
    );
  }

  if (loadState === 'error' || detail === null) {
    return (
      <p data-testid="iam-group-detail-error" style={messageStyle}>
        Unable to load this IAM group.
      </p>
    );
  }

  return (
    <div data-testid="iam-group-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="iam-group-detail-name" style={{ fontSize: 16 }}>
        {detail.groupName}
      </Heading>
      <div data-testid="iam-group-detail-arn" style={rowStyle}>
        <Text style={labelStyle}>ARN</Text>
        <Text style={valueStyle}>{detail.arn}</Text>
      </div>
      <div data-testid="iam-group-detail-groupId" style={rowStyle}>
        <Text style={labelStyle}>Group ID</Text>
        <Text style={valueStyle}>{detail.groupId}</Text>
      </div>
      <div data-testid="iam-group-detail-path" style={rowStyle}>
        <Text style={labelStyle}>Path</Text>
        <Text style={valueStyle}>{detail.path}</Text>
      </div>
      <div data-testid="iam-group-detail-created" style={rowStyle}>
        <Text style={labelStyle}>Created</Text>
        <Text style={valueStyle}>{detail.createDate ?? '\u2014'}</Text>
      </div>

      {mutationError ? (
        <p data-testid="iam-group-detail-mutation-error" style={messageStyle}>
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
            data-testid={`iam-group-detail-tab-${descriptor.key}`}
            style={tab === descriptor.key ? activeTabButtonStyle : tabButtonStyle}
            onClick={() => setTab(descriptor.key)}
          >
            {descriptor.label}
          </button>
        ))}
      </div>

      {tab === 'members' ? (
        <div data-testid="iam-group-detail-panel-members" style={rowStyle}>
          {detail.members.length === 0 ? (
            <Text data-testid="iam-group-detail-members-empty" style={messageStyle}>
              This group has no members.
            </Text>
          ) : (
            <ul data-testid="iam-group-detail-members-list" style={listStyle}>
              {detail.members.map((member) => (
                <li key={member} data-testid="iam-group-detail-member-item" style={itemRowStyle}>
                  <Link
                    data-testid="iam-group-detail-member-link"
                    to={`/services/${serviceKey}/${encodeURIComponent(`user/${member}`)}`}
                  >
                    {member}
                  </Link>
                  <ConfirmationHost
                    actionLabel="Remove"
                    prompt={`Remove ${member} from ${detail.groupName}?`}
                    confirmLabel="Confirm"
                    onConfirm={() => handleRemoveMember(member)}
                  />
                </li>
              ))}
            </ul>
          )}
          <div data-testid="iam-group-detail-member-form" style={formStyle}>
            <label style={labelStyle} htmlFor="iam-group-detail-member-name">
              User name
            </label>
            <input
              id="iam-group-detail-member-name"
              type="text"
              data-testid="iam-group-detail-member-name"
              style={inputStyle}
              value={memberName}
              onChange={(event) => setMemberName(event.target.value)}
            />
            <button
              type="button"
              data-testid="iam-group-detail-member-submit"
              style={buttonStyle}
              onClick={handleAddMember}
            >
              Add member
            </button>
          </div>
        </div>
      ) : null}

      {tab === 'permissions' ? (
        <div data-testid="iam-group-detail-panel-permissions" style={rowStyle}>
          <Text style={labelStyle}>Attached managed policies</Text>
          {detail.attachedPolicies.length === 0 ? (
            <Text data-testid="iam-group-detail-attached-empty" style={messageStyle}>
              No managed policies attached.
            </Text>
          ) : (
            <ul data-testid="iam-group-detail-attached-list" style={listStyle}>
              {detail.attachedPolicies.map((policy) => (
                <li key={policy.policyArn} data-testid="iam-group-detail-attached-item" style={itemRowStyle}>
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
          <div data-testid="iam-group-detail-attach-form" style={formStyle}>
            <label style={labelStyle} htmlFor="iam-group-detail-attach-arn">
              Policy ARN
            </label>
            <input
              id="iam-group-detail-attach-arn"
              type="text"
              data-testid="iam-group-detail-attach-arn"
              style={inputStyle}
              value={attachArn}
              onChange={(event) => setAttachArn(event.target.value)}
            />
            <button
              type="button"
              data-testid="iam-group-detail-attach-submit"
              style={buttonStyle}
              onClick={handleAttachPolicy}
            >
              Attach policy
            </button>
          </div>

          <Text style={labelStyle}>Inline policies</Text>
          {detail.inlinePolicies.length === 0 ? (
            <Text data-testid="iam-group-detail-inline-empty" style={messageStyle}>
              No inline policies.
            </Text>
          ) : (
            <ul data-testid="iam-group-detail-inline-list" style={listStyle}>
              {detail.inlinePolicies.map((policy) => (
                <li
                  key={policy.policyName}
                  data-testid="iam-group-detail-inline-item"
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
                    testId="iam-group-detail-inline-viewer"
                  />
                </li>
              ))}
            </ul>
          )}
          <div data-testid="iam-group-detail-inline-form" style={formStyle}>
            <label style={labelStyle} htmlFor="iam-group-detail-inline-name">
              Inline policy name
            </label>
            <input
              id="iam-group-detail-inline-name"
              type="text"
              data-testid="iam-group-detail-inline-name"
              style={inputStyle}
              value={inlineName}
              onChange={(event) => setInlineName(event.target.value)}
            />
            {inlineNameError ? (
              <Text data-testid="iam-group-detail-inline-name-error" style={messageStyle}>
                Enter a name for the inline policy.
              </Text>
            ) : null}
            <PolicyDocumentEditor
              value={defaultInlineDocument}
              title="Inline policy document"
              onSave={handleAddInlinePolicy}
              testId="iam-group-detail-inline-editor"
            />
          </div>
        </div>
      ) : null}
    </div>
  );
}

export default IamGroupDetailView;
