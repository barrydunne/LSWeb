import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import {
  deleteS3BucketPolicy,
  deleteS3ObjectVersion,
  getS3BucketConfiguration,
  getS3ObjectVersions,
  putS3BucketNotifications,
  putS3BucketPolicy,
  setS3BucketVersioning,
} from '../../api/client';
import type {
  S3BucketConfigurationResult,
  S3NotificationResult,
  S3ObjectVersionItem,
} from '../../api/client';
import { ResourceLink } from '../../components/ResourceLink';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 16,
};

const messageStyle: CSSProperties = { fontSize: 14 };

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const sectionTitleStyle: CSSProperties = { fontSize: 14, fontWeight: 600 };

const tableStyle: CSSProperties = {
  width: '100%',
  borderCollapse: 'collapse',
  fontSize: 13,
};

const cellStyle: CSSProperties = {
  textAlign: 'left',
  padding: '4px 8px',
  borderBottom: '1px solid #30363d',
  verticalAlign: 'top',
};

const emptyStyle: CSSProperties = { fontSize: 13, opacity: 0.7 };

const inputStyle: CSSProperties = {
  width: '100%',
  minHeight: 160,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
  fontFamily: 'monospace',
  fontSize: 12,
  boxSizing: 'border-box',
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

const policyStyle: CSSProperties = {
  margin: 0,
  padding: 12,
  borderRadius: 6,
  background: '#161b22',
  fontFamily: 'monospace',
  fontSize: 12,
  lineHeight: 1.5,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
  overflowX: 'auto',
  maxHeight: 320,
  overflowY: 'auto',
};

type ConfigState =
  | { kind: 'loading' }
  | { kind: 'ready'; configuration: S3BucketConfigurationResult }
  | { kind: 'error' };

const serviceByNotificationType: Record<string, string> = {
  Lambda: 'lambda',
  Queue: 'sqs',
  Topic: 'sns',
};

const notificationTypeTokens: Record<string, string> = {
  Lambda: ':lambda:',
  Queue: ':sqs:',
  Topic: ':sns:',
};

export function S3ConfigurationPanel({ bucketName }: { bucketName: string }) {
  const [state, setState] = useState<ConfigState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [policyDraft, setPolicyDraft] = useState('');
  const [policyError, setPolicyError] = useState<string | null>(null);
  const [applyState, setApplyState] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle');
  const [versions, setVersions] = useState<S3ObjectVersionItem[]>([]);
  const [versionsState, setVersionsState] = useState<'loading' | 'ready' | 'error'>('loading');
  const [versioningState, setVersioningState] = useState<'idle' | 'saving' | 'error'>('idle');
  const [notifType, setNotifType] = useState('Lambda');
  const [notifArn, setNotifArn] = useState('');
  const [notifEvents, setNotifEvents] = useState('s3:ObjectCreated:*');
  const [notifPrefix, setNotifPrefix] = useState('');
  const [notifSuffix, setNotifSuffix] = useState('');
  const [notifError, setNotifError] = useState<string | null>(null);
  const [notifState, setNotifState] = useState<'idle' | 'saving' | 'error'>('idle');

  useEffect(() => {
    const controller = new AbortController();
    setVersionsState('loading');
    getS3ObjectVersions(bucketName, '', controller.signal)
      .then((result) => {
        setVersions(result.versions);
        setVersionsState('ready');
      })
      .catch(() => setVersionsState('error'));
    return () => controller.abort();
  }, [bucketName, reloadToken]);

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getS3BucketConfiguration(bucketName, controller.signal)
      .then((configuration) => {
        setState({ kind: 'ready', configuration });
        setPolicyDraft(configuration.policy);
      })
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [bucketName, reloadToken]);

  const refresh = useCallback(() => setReloadToken((token) => token + 1), []);

  const handleSetVersioning = (enabled: boolean) => {
    setVersioningState('saving');
    setS3BucketVersioning(bucketName, enabled)
      .then(() => {
        setVersioningState('idle');
        refresh();
      })
      .catch(() => setVersioningState('error'));
  };

  const handleDeleteVersion = (version: S3ObjectVersionItem) => {
    setVersioningState('saving');
    deleteS3ObjectVersion(bucketName, version.key, version.versionId)
      .then(() => {
        setVersioningState('idle');
        refresh();
      })
      .catch(() => setVersioningState('error'));
  };

  const persistNotifications = (next: S3NotificationResult[]) => {
    setNotifState('saving');
    putS3BucketNotifications(bucketName, next)
      .then(() => {
        setNotifState('idle');
        refresh();
      })
      .catch(() => setNotifState('error'));
  };

  const handleAddNotification = (current: S3NotificationResult[]) => {
    const trimmedArn = notifArn.trim();
    const token = notificationTypeTokens[notifType];
    if (token !== undefined && !trimmedArn.includes(token)) {
      setNotifError(`The target ARN does not look like a ${notifType} ARN (expected "${token}").`);
      setNotifState('error');
      return;
    }
    const events = notifEvents
      .split(',')
      .map((event) => event.trim())
      .filter((event) => event !== '');
    if (events.length === 0) {
      setNotifError('Enter at least one event.');
      setNotifState('error');
      return;
    }
    setNotifError(null);
    const rule: S3NotificationResult = {
      type: notifType,
      targetArn: trimmedArn,
      events,
      prefix: notifPrefix.trim(),
      suffix: notifSuffix.trim(),
    };
    setNotifArn('');
    setNotifPrefix('');
    setNotifSuffix('');
    persistNotifications([...current, rule]);
  };

  const handleRemoveNotification = (current: S3NotificationResult[], index: number) => {
    persistNotifications(current.filter((_, position) => position !== index));
  };

  const handleInsertTemplate = () => {
    setPolicyDraft(
      JSON.stringify(
        {
          Version: '2012-10-17',
          Statement: [
            {
              Sid: 'PublicRead',
              Effect: 'Allow',
              Principal: '*',
              Action: 's3:GetObject',
              Resource: `arn:aws:s3:::${bucketName}/*`,
            },
          ],
        },
        null,
        2,
      ),
    );
  };

  const handleApplyPolicy = () => {
    let parsed: unknown;
    try {
      parsed = JSON.parse(policyDraft);
    } catch {
      setPolicyError('Policy must be valid JSON.');
      setApplyState('error');
      return;
    }
    if (
      typeof parsed !== 'object' ||
      parsed === null ||
      Array.isArray(parsed) ||
      !('Version' in parsed) ||
      !('Statement' in parsed)
    ) {
      setPolicyError("Policy must be a JSON object with 'Version' and 'Statement'.");
      setApplyState('error');
      return;
    }
    setPolicyError(null);
    setApplyState('saving');
    putS3BucketPolicy(bucketName, policyDraft)
      .then(() => {
        setApplyState('saved');
        refresh();
      })
      .catch(() => setApplyState('error'));
  };

  const handleRemovePolicy = () => {
    setPolicyError(null);
    setApplyState('saving');
    deleteS3BucketPolicy(bucketName)
      .then(() => {
        setApplyState('saved');
        setPolicyDraft('');
        refresh();
      })
      .catch(() => setApplyState('error'));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="s3-config-loading" style={messageStyle}>
        Loading configuration&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="s3-config-error" style={messageStyle}>
        Unable to load the bucket configuration.
      </p>
    );
  }

  const { configuration } = state;

  return (
    <div data-testid="s3-config-view" style={containerStyle}>
      <section data-testid="s3-config-versioning" style={sectionStyle}>
        <span style={sectionTitleStyle}>Versioning</span>
        <span data-testid="s3-config-versioning-status" style={messageStyle}>
          {configuration.versioningStatus}
        </span>
        <div style={{ display: 'flex', gap: 8 }}>
          {configuration.versioningStatus === 'Enabled' ? (
            <button
              type="button"
              data-testid="s3-versioning-suspend"
              style={buttonStyle}
              disabled={versioningState === 'saving'}
              onClick={() => handleSetVersioning(false)}
            >
              Suspend versioning
            </button>
          ) : (
            <button
              type="button"
              data-testid="s3-versioning-enable"
              style={buttonStyle}
              disabled={versioningState === 'saving'}
              onClick={() => handleSetVersioning(true)}
            >
              Enable versioning
            </button>
          )}
        </div>
        {versioningState === 'error' ? (
          <p data-testid="s3-versioning-error" style={messageStyle}>
            The last versioning action could not be completed.
          </p>
        ) : null}
        <span style={emptyStyle}>Object versions</span>
        {versionsState === 'loading' ? (
          <span data-testid="s3-versions-loading" style={emptyStyle}>
            Loading versions&hellip;
          </span>
        ) : versionsState === 'error' ? (
          <span data-testid="s3-versions-error" style={emptyStyle}>
            Unable to load object versions.
          </span>
        ) : versions.length === 0 ? (
          <span data-testid="s3-versions-empty" style={emptyStyle}>
            No object versions are stored in this bucket.
          </span>
        ) : (
          <ul data-testid="s3-versions-list" style={{ listStyle: 'none', margin: 0, padding: 0 }}>
            {versions.map((version) => (
              <li
                key={`${version.key}|${version.versionId}`}
                data-testid="s3-version-row"
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  gap: 12,
                  padding: '4px 0',
                }}
              >
                <span style={{ fontFamily: 'monospace', fontSize: 12 }}>
                  <span data-testid="s3-version-key">{version.key}</span>{' '}
                  <span data-testid="s3-version-id">{version.versionId}</span>
                  {version.isLatest ? <span data-testid="s3-version-latest"> (latest)</span> : null}
                  {version.isDeleteMarker ? (
                    <span data-testid="s3-version-delete-marker"> (delete marker)</span>
                  ) : null}
                </span>
                <button
                  type="button"
                  data-testid="s3-version-delete"
                  style={buttonStyle}
                  onClick={() => handleDeleteVersion(version)}
                >
                  Delete
                </button>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section data-testid="s3-config-encryption" style={sectionStyle}>
        <span style={sectionTitleStyle}>Default encryption</span>
        {configuration.encryptionAlgorithm.length === 0 ? (
          <span data-testid="s3-config-encryption-none" style={emptyStyle}>
            No default encryption is configured.
          </span>
        ) : (
          <table style={tableStyle}>
            <tbody>
              <tr>
                <td style={cellStyle}>Algorithm</td>
                <td style={cellStyle} data-testid="s3-config-encryption-algorithm">
                  {configuration.encryptionAlgorithm}
                </td>
              </tr>
              {configuration.encryptionKeyId.length > 0 ? (
                <tr>
                  <td style={cellStyle}>KMS key</td>
                  <td style={cellStyle} data-testid="s3-config-encryption-key">
                    {configuration.encryptionKeyId}
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        )}
      </section>

      <section data-testid="s3-config-lifecycle" style={sectionStyle}>
        <span style={sectionTitleStyle}>Lifecycle rules</span>
        {configuration.lifecycleRules.length === 0 ? (
          <span data-testid="s3-config-lifecycle-empty" style={emptyStyle}>
            No lifecycle rules are configured.
          </span>
        ) : (
          <table data-testid="s3-config-lifecycle-table" style={tableStyle}>
            <thead>
              <tr>
                <th style={cellStyle}>Id</th>
                <th style={cellStyle}>Status</th>
                <th style={cellStyle}>Prefix</th>
              </tr>
            </thead>
            <tbody>
              {configuration.lifecycleRules.map((rule) => (
                <tr key={rule.id} data-testid="s3-config-lifecycle-row">
                  <td style={cellStyle}>{rule.id}</td>
                  <td style={cellStyle}>{rule.status}</td>
                  <td style={cellStyle}>{rule.prefix.length === 0 ? '(all objects)' : rule.prefix}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>

      <section data-testid="s3-config-notifications" style={sectionStyle}>
        <span style={sectionTitleStyle}>Event notifications</span>
        {configuration.notifications.length === 0 ? (
          <span data-testid="s3-config-notifications-empty" style={emptyStyle}>
            No event notifications are configured.
          </span>
        ) : (
          <table data-testid="s3-config-notifications-table" style={tableStyle}>
            <thead>
              <tr>
                <th style={cellStyle}>Type</th>
                <th style={cellStyle}>Target</th>
                <th style={cellStyle}>Events</th>
                <th style={cellStyle}>Filter</th>
                <th style={cellStyle}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {configuration.notifications.map((notification, index) => (
                <tr key={notification.targetArn} data-testid="s3-config-notification-row">
                  <td style={cellStyle}>{notification.type}</td>
                  <td style={cellStyle} data-testid="s3-config-notification-target">
                    <ResourceLink
                      reference={notification.targetArn}
                      service={serviceByNotificationType[notification.type]}
                    />
                  </td>
                  <td style={cellStyle}>{notification.events.join(', ')}</td>
                  <td style={cellStyle} data-testid="s3-config-notification-filter">
                    {notification.prefix.length === 0 && notification.suffix.length === 0 ? (
                      <span style={emptyStyle}>(no filter)</span>
                    ) : (
                      <span>
                        {notification.prefix.length > 0 ? (
                          <span data-testid="s3-config-notification-prefix">
                            Prefix: {notification.prefix}
                          </span>
                        ) : null}
                        {notification.prefix.length > 0 && notification.suffix.length > 0 ? ', ' : null}
                        {notification.suffix.length > 0 ? (
                          <span data-testid="s3-config-notification-suffix">
                            Suffix: {notification.suffix}
                          </span>
                        ) : null}
                      </span>
                    )}
                  </td>
                  <td style={cellStyle}>
                    <button
                      type="button"
                      data-testid="s3-notification-remove"
                      style={buttonStyle}
                      onClick={() => handleRemoveNotification(configuration.notifications, index)}
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        <div data-testid="s3-notification-form" style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          <select
            data-testid="s3-notification-type"
            style={inputStyle}
            value={notifType}
            onChange={(event) => setNotifType(event.target.value)}
          >
            <option value="Lambda">Lambda function</option>
            <option value="Queue">SQS queue</option>
            <option value="Topic">SNS topic</option>
          </select>
          <input
            type="text"
            data-testid="s3-notification-arn"
            style={inputStyle}
            placeholder="Destination ARN"
            value={notifArn}
            onChange={(event) => setNotifArn(event.target.value)}
          />
          <input
            type="text"
            data-testid="s3-notification-events"
            style={inputStyle}
            placeholder="Events (comma separated)"
            value={notifEvents}
            onChange={(event) => setNotifEvents(event.target.value)}
          />
          <input
            type="text"
            data-testid="s3-notification-prefix"
            style={inputStyle}
            placeholder="Prefix filter (optional)"
            value={notifPrefix}
            onChange={(event) => setNotifPrefix(event.target.value)}
          />
          <input
            type="text"
            data-testid="s3-notification-suffix"
            style={inputStyle}
            placeholder="Suffix filter (optional)"
            value={notifSuffix}
            onChange={(event) => setNotifSuffix(event.target.value)}
          />
          <button
            type="button"
            data-testid="s3-notification-add"
            style={buttonStyle}
            disabled={notifState === 'saving'}
            onClick={() => handleAddNotification(configuration.notifications)}
          >
            {notifState === 'saving' ? 'Saving\u2026' : 'Add notification'}
          </button>
          {notifState === 'error' ? (
            <p data-testid="s3-notification-error" style={messageStyle}>
              {notifError ?? 'Unable to update the notifications.'}
            </p>
          ) : null}
        </div>
      </section>

      <section data-testid="s3-config-policy" style={sectionStyle}>
        <span style={sectionTitleStyle}>Access policy</span>
        {configuration.policy.length === 0 ? (
          <span data-testid="s3-config-policy-empty" style={emptyStyle}>
            No bucket policy is configured.
          </span>
        ) : (
          <pre data-testid="s3-config-policy-document" style={policyStyle}>
            {configuration.policy}
          </pre>
        )}
        <button
          type="button"
          data-testid="s3-policy-template"
          style={buttonStyle}
          onClick={handleInsertTemplate}
        >
          Insert public-read template
        </button>
        <textarea
          data-testid="s3-policy-editor"
          style={inputStyle}
          value={policyDraft}
          onChange={(event) => setPolicyDraft(event.target.value)}
        />
        <span style={emptyStyle}>Preview</span>
        <pre data-testid="s3-policy-preview" style={policyStyle}>
          {policyDraft}
        </pre>
        <div style={{ display: 'flex', gap: 8 }}>
          <button
            type="button"
            data-testid="s3-policy-apply"
            style={buttonStyle}
            disabled={applyState === 'saving'}
            onClick={handleApplyPolicy}
          >
            {applyState === 'saving' ? 'Applying\u2026' : 'Apply policy'}
          </button>
          {configuration.policy.length > 0 ? (
            <button
              type="button"
              data-testid="s3-policy-remove"
              style={buttonStyle}
              onClick={handleRemovePolicy}
            >
              Remove policy
            </button>
          ) : null}
        </div>
        {applyState === 'error' ? (
          <p data-testid="s3-policy-error" style={messageStyle}>
            {policyError ?? 'Unable to apply the policy.'}
          </p>
        ) : null}
        {applyState === 'saved' ? (
          <p data-testid="s3-policy-status" style={messageStyle}>
            Policy updated.
          </p>
        ) : null}
      </section>
    </div>
  );
}

export default S3ConfigurationPanel;
