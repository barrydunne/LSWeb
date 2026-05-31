import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import {
  createIamPolicyVersion,
  deleteIamPolicyVersion,
  getIamPolicy,
  setIamPolicyDefaultVersion,
  tagIamPolicy,
  untagIamPolicy,
} from '../../api/client';
import type { IamPolicyDetail } from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { PolicyDocumentEditor, PolicyDocumentViewer } from './components/PolicyDocumentEditor';
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

const actionGroupStyle: CSSProperties = { display: 'flex', alignItems: 'center', gap: 8 };

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
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

const checkboxRowStyle: CSSProperties = { display: 'flex', alignItems: 'center', gap: 6 };

const badgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #2ea043',
  color: '#2ea043',
};

type LoadState = 'loading' | 'ready' | 'error';

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

/**
 * AWS-managed policies (account alias "aws") are read-only: their versions cannot be created,
 * promoted or deleted.
 */
function isAwsManaged(arn: string): boolean {
  return arn.startsWith('arn:aws:iam::aws:');
}

interface IamPolicyDetailViewProps {
  policyArn: string;
}

/**
 * Full IAM managed-policy detail view: default-version document, version history with
 * set-default / delete actions, an add-version form and an "Attached to" placeholder. AWS-managed
 * policies render read-only.
 */
export function IamPolicyDetailView({ policyArn }: IamPolicyDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [detail, setDetail] = useState<IamPolicyDetail | null>(null);
  const [mutationError, setMutationError] = useState(false);
  const [setAsDefault, setSetAsDefault] = useState(true);

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getIamPolicy(policyArn, signal)
        .then((data) => {
          setDetail(data);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [policyArn],
  );

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const reload = useCallback(() => load(), [load]);

  const handleAddVersion = (document: unknown) => {
    setMutationError(false);
    createIamPolicyVersion(policyArn, JSON.stringify(document), setAsDefault)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleSetDefault = (versionId: string) => {
    setMutationError(false);
    setIamPolicyDefaultVersion(policyArn, versionId)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleDeleteVersion = (versionId: string) => {
    setMutationError(false);
    deleteIamPolicyVersion(policyArn, versionId)
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleAddTag = (key: string, value: string) => {
    setMutationError(false);
    tagIamPolicy(policyArn, [{ key, value }])
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  const handleRemoveTag = (key: string) => {
    setMutationError(false);
    untagIamPolicy(policyArn, [key])
      .then(() => reload())
      .catch(() => setMutationError(true));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="iam-policy-detail-loading" style={messageStyle}>
        Loading policy&hellip;
      </p>
    );
  }

  if (loadState === 'error' || detail === null) {
    return (
      <p data-testid="iam-policy-detail-error" style={messageStyle}>
        Unable to load this IAM policy.
      </p>
    );
  }

  const readOnly = isAwsManaged(detail.arn);

  return (
    <div data-testid="iam-policy-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="iam-policy-detail-name" style={{ fontSize: 16 }}>
        {detail.policyName}
      </Heading>
      {readOnly ? (
        <Text data-testid="iam-policy-detail-readonly" style={messageStyle}>
          AWS-managed policy (read-only).
        </Text>
      ) : null}
      <div data-testid="iam-policy-detail-arn" style={rowStyle}>
        <Text style={labelStyle}>ARN</Text>
        <Text style={valueStyle}>{detail.arn}</Text>
      </div>
      <div data-testid="iam-policy-detail-policyId" style={rowStyle}>
        <Text style={labelStyle}>Policy ID</Text>
        <Text style={valueStyle}>{detail.policyId}</Text>
      </div>
      <div data-testid="iam-policy-detail-path" style={rowStyle}>
        <Text style={labelStyle}>Path</Text>
        <Text style={valueStyle}>{detail.path}</Text>
      </div>
      <div data-testid="iam-policy-detail-default-version" style={rowStyle}>
        <Text style={labelStyle}>Default version</Text>
        <Text style={valueStyle}>{detail.defaultVersionId}</Text>
      </div>
      <div data-testid="iam-policy-detail-attachments" style={rowStyle}>
        <Text style={labelStyle}>Attachment count</Text>
        <Text style={valueStyle}>{detail.attachmentCount}</Text>
      </div>
      <div data-testid="iam-policy-detail-description" style={rowStyle}>
        <Text style={labelStyle}>Description</Text>
        <Text style={valueStyle}>{detail.description ?? '\u2014'}</Text>
      </div>
      <div data-testid="iam-policy-detail-created" style={rowStyle}>
        <Text style={labelStyle}>Created</Text>
        <Text style={valueStyle}>{detail.createDate ?? '\u2014'}</Text>
      </div>
      <div data-testid="iam-policy-detail-updated" style={rowStyle}>
        <Text style={labelStyle}>Updated</Text>
        <Text style={valueStyle}>{detail.updateDate ?? '\u2014'}</Text>
      </div>

      {mutationError ? (
        <p data-testid="iam-policy-detail-mutation-error" style={messageStyle}>
          The last action could not be completed.
        </p>
      ) : null}

      <PolicyDocumentViewer
        value={parsePolicyDocument(detail.defaultVersionDocument)}
        title="Default version document"
        testId="iam-policy-detail-document"
      />

      <Text style={labelStyle}>Version history</Text>
      {detail.versions.length === 0 ? (
        <Text data-testid="iam-policy-detail-versions-empty" style={messageStyle}>
          No versions found.
        </Text>
      ) : (
        <ul data-testid="iam-policy-detail-versions-list" style={listStyle}>
          {detail.versions.map((version) => (
            <li
              key={version.versionId}
              data-testid="iam-policy-detail-version-item"
              style={itemRowStyle}
            >
              <div style={rowStyle}>
                <div style={actionGroupStyle}>
                  <Text style={valueStyle}>{version.versionId}</Text>
                  {version.isDefaultVersion ? (
                    <span data-testid="iam-policy-detail-version-default" style={badgeStyle}>
                      Default
                    </span>
                  ) : null}
                </div>
                <Text style={labelStyle}>{version.createDate ?? '\u2014'}</Text>
              </div>
              {readOnly || version.isDefaultVersion ? null : (
                <div style={actionGroupStyle}>
                  <button
                    type="button"
                    data-testid="iam-policy-detail-version-set-default"
                    style={buttonStyle}
                    onClick={() => handleSetDefault(version.versionId)}
                  >
                    Set default
                  </button>
                  <ConfirmationHost
                    actionLabel="Delete"
                    prompt={`Delete version ${version.versionId}?`}
                    confirmLabel="Confirm"
                    onConfirm={() => handleDeleteVersion(version.versionId)}
                  />
                </div>
              )}
            </li>
          ))}
        </ul>
      )}

      {readOnly ? null : (
        <div data-testid="iam-policy-detail-add-version-form" style={formStyle}>
          <Text style={labelStyle}>Add a new version</Text>
          <label style={checkboxRowStyle} htmlFor="iam-policy-detail-set-default">
            <input
              id="iam-policy-detail-set-default"
              type="checkbox"
              data-testid="iam-policy-detail-set-default"
              checked={setAsDefault}
              onChange={(event) => setSetAsDefault(event.target.checked)}
            />
            <span style={labelStyle}>Set as default version</span>
          </label>
          <PolicyDocumentEditor
            value={parsePolicyDocument(detail.defaultVersionDocument)}
            title="New version document"
            onSave={handleAddVersion}
            testId="iam-policy-detail-add-version"
          />
        </div>
      )}

      <div data-testid="iam-policy-detail-tags" style={rowStyle}>
        <Text style={labelStyle}>Tags</Text>
        <TagEditor
          tags={detail.tags}
          onAdd={handleAddTag}
          onRemove={handleRemoveTag}
          testId="iam-policy-detail-tag-editor"
        />
      </div>

      <div data-testid="iam-policy-detail-attachedto" style={rowStyle}>
        <Text style={labelStyle}>Attached to</Text>
        <Text data-testid="iam-policy-detail-attachedto-placeholder" style={messageStyle}>
          Resources attached to this policy are not available yet.
        </Text>
      </div>
    </div>
  );
}

export default IamPolicyDetailView;
