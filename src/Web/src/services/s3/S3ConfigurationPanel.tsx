import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { getS3BucketConfiguration } from '../../api/client';
import type { S3BucketConfigurationResult } from '../../api/client';
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

export function S3ConfigurationPanel({ bucketName }: { bucketName: string }) {
  const [state, setState] = useState<ConfigState>({ kind: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getS3BucketConfiguration(bucketName, controller.signal)
      .then((configuration) => setState({ kind: 'ready', configuration }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [bucketName]);

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
              </tr>
            </thead>
            <tbody>
              {configuration.notifications.map((notification) => (
                <tr key={notification.targetArn} data-testid="s3-config-notification-row">
                  <td style={cellStyle}>{notification.type}</td>
                  <td style={cellStyle} data-testid="s3-config-notification-target">
                    <ResourceLink
                      reference={notification.targetArn}
                      service={serviceByNotificationType[notification.type]}
                    />
                  </td>
                  <td style={cellStyle}>{notification.events.join(', ')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
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
      </section>
    </div>
  );
}

export default S3ConfigurationPanel;
