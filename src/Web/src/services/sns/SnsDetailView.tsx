import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import {
  getSnsSubscriptions,
  getSnsSubscriptionFilterPolicy,
  publishSnsMessage,
  setSnsSubscriptionFilterPolicy,
} from '../../api/client';
import type { SnsSubscriptionItem } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { ResourceLink } from '../../components/ResourceLink';
import { RawJsonViewer } from '../../components/RawJsonViewer';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };

const subscriptionListStyle: CSSProperties = {
  listStyle: 'none',
  margin: 0,
  padding: 0,
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const subscriptionRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
};

const protocolBadgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
  fontFamily: 'monospace',
};

const publishSectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const textareaStyle: CSSProperties = {
  fontSize: 13,
  fontFamily: 'monospace',
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  minHeight: 72,
  resize: 'vertical',
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

const attributeRowStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  alignItems: 'flex-end',
  flexWrap: 'wrap',
};

const attributeRemoveButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const hintStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const subscriptionItemStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
};

const filterPolicyStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const linkButtonStyle: CSSProperties = {
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

type PublishState =
  | { kind: 'idle' }
  | { kind: 'sending' }
  | { kind: 'sent' }
  | { kind: 'error' };

type FilterPolicyState =
  | { kind: 'collapsed' }
  | { kind: 'loading' }
  | { kind: 'ready' }
  | { kind: 'loadError' }
  | { kind: 'saving' }
  | { kind: 'saved' }
  | { kind: 'saveError' };

function deriveTopicName(topicArn: string): string {
  return topicArn.split(':').pop() || topicArn;
}

function parseFilterPolicy(filterPolicy: string): unknown | null {
  const trimmed = filterPolicy.trim();
  if (trimmed === '') {
    return null;
  }
  try {
    return JSON.parse(trimmed) as unknown;
  } catch {
    return null;
  }
}

function SubscriptionFilterPolicy({ subscriptionArn }: { subscriptionArn: string }) {
  const [state, setState] = useState<FilterPolicyState>({ kind: 'collapsed' });
  const [policy, setPolicy] = useState('');

  const load = useCallback(() => {
    setState({ kind: 'loading' });
    getSnsSubscriptionFilterPolicy(subscriptionArn)
      .then((result) => {
        setPolicy(result.filterPolicy);
        setState({ kind: 'ready' });
      })
      .catch(() => setState({ kind: 'loadError' }));
  }, [subscriptionArn]);

  const handleSave = useCallback(() => {
    setState({ kind: 'saving' });
    setSnsSubscriptionFilterPolicy(subscriptionArn, policy)
      .then(() => setState({ kind: 'saved' }))
      .catch(() => setState({ kind: 'saveError' }));
  }, [subscriptionArn, policy]);

  if (state.kind === 'collapsed') {
    return (
      <button
        type="button"
        data-testid="sns-filter-policy-toggle"
        style={linkButtonStyle}
        onClick={load}
      >
        Edit filter policy
      </button>
    );
  }

  if (state.kind === 'loading') {
    return (
      <p data-testid="sns-filter-policy-loading" style={hintStyle}>
        Loading filter policy&hellip;
      </p>
    );
  }

  if (state.kind === 'loadError') {
    return (
      <div style={filterPolicyStyle}>
        <span data-testid="sns-filter-policy-load-error" style={hintStyle}>
          Unable to load the filter policy.
        </span>
        <button
          type="button"
          data-testid="sns-filter-policy-retry"
          style={linkButtonStyle}
          onClick={load}
        >
          Retry
        </button>
      </div>
    );
  }

  const parsed = parseFilterPolicy(policy);
  const canSave = state.kind !== 'saving';

  return (
    <div data-testid="sns-filter-policy" style={filterPolicyStyle}>
      {parsed !== null ? (
        <RawJsonViewer value={parsed} title="Filter policy" initiallyExpanded />
      ) : (
        <span data-testid="sns-filter-policy-empty" style={hintStyle}>
          {policy.trim() === '' ? 'No filter policy set.' : 'Filter policy is not valid JSON.'}
        </span>
      )}
      <label htmlFor={`sns-filter-policy-input-${subscriptionArn}`} style={labelStyle}>
        Policy JSON
      </label>
      <textarea
        id={`sns-filter-policy-input-${subscriptionArn}`}
        data-testid="sns-filter-policy-input"
        style={textareaStyle}
        value={policy}
        onChange={(event) => setPolicy(event.target.value)}
      />
      <span style={hintStyle}>Leave empty to clear the filter policy.</span>
      <button
        type="button"
        data-testid="sns-filter-policy-save"
        style={buttonStyle}
        disabled={!canSave}
        onClick={handleSave}
      >
        Save filter policy
      </button>
      {state.kind === 'saved' ? (
        <span data-testid="sns-filter-policy-saved" style={hintStyle}>
          Filter policy saved.
        </span>
      ) : null}
      {state.kind === 'saveError' ? (
        <span data-testid="sns-filter-policy-save-error" style={hintStyle}>
          Unable to save the filter policy.
        </span>
      ) : null}
    </div>
  );
}

export function SnsDetailView({ resourceId }: ServiceDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [subscriptions, setSubscriptions] = useState<SnsSubscriptionItem[]>([]);
  const [subject, setSubject] = useState('');
  const [message, setMessage] = useState('');
  const [attributes, setAttributes] = useState<{ key: string; value: string }[]>([]);
  const [publishState, setPublishState] = useState<PublishState>({ kind: 'idle' });

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getSnsSubscriptions(resourceId, signal)
        .then((result) => {
          setSubscriptions(result.subscriptions);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [resourceId],
  );

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const addAttribute = useCallback(() => {
    setAttributes((current) => [...current, { key: '', value: '' }]);
  }, []);

  const updateAttribute = useCallback(
    (index: number, field: 'key' | 'value', value: string) => {
      setAttributes((current) =>
        current.map((attribute, position) =>
          position === index ? { ...attribute, [field]: value } : attribute));
    },
    [],
  );

  const removeAttribute = useCallback((index: number) => {
    setAttributes((current) => current.filter((_, position) => position !== index));
  }, []);

  const handlePublish = useCallback(() => {
    setPublishState({ kind: 'sending' });
    const messageAttributes = attributes.reduce<Record<string, string>>((accumulator, attribute) => {
      const key = attribute.key.trim();
      if (key !== '') {
        accumulator[key] = attribute.value;
      }
      return accumulator;
    }, {});
    publishSnsMessage(resourceId, {
      subject: subject.trim() !== '' ? subject : undefined,
      message,
      messageAttributes: Object.keys(messageAttributes).length > 0 ? messageAttributes : undefined,
    })
      .then(() => {
        setPublishState({ kind: 'sent' });
        setSubject('');
        setMessage('');
        setAttributes([]);
      })
      .catch(() => setPublishState({ kind: 'error' }));
  }, [resourceId, subject, message, attributes]);

  if (loadState === 'loading') {
    return (
      <p data-testid="sns-detail-loading" style={messageStyle}>
        Loading subscriptions&hellip;
      </p>
    );
  }

  if (loadState === 'error') {
    return (
      <p data-testid="sns-detail-error" style={messageStyle}>
        Unable to load this topic.
      </p>
    );
  }

  const topicName = deriveTopicName(resourceId);
  const canPublish = message.trim() !== '' && publishState.kind !== 'sending';

  return (
    <div data-testid="sns-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="sns-detail-name" style={{ fontSize: 16 }}>
        {topicName}
      </Heading>
      <div data-testid="sns-detail-arn" style={rowStyle}>
        <Text style={labelStyle}>ARN</Text>
        <Text style={valueStyle}>{resourceId}</Text>
      </div>
      <section data-testid="sns-subscriptions" style={rowStyle}>
        <Text style={labelStyle}>Subscriptions</Text>
        {subscriptions.length === 0 ? (
          <p data-testid="sns-detail-empty" style={messageStyle}>
            No subscriptions for this topic.
          </p>
        ) : (
          <ul data-testid="sns-subscription-list" style={subscriptionListStyle}>
            {subscriptions.map((subscription) => (
              <li
                key={`${subscription.subscriptionArn}-${subscription.endpoint}`}
                data-testid="sns-subscription-item"
                style={subscriptionItemStyle}
              >
                <div style={subscriptionRowStyle}>
                  <span data-testid="sns-subscription-protocol" style={protocolBadgeStyle}>
                    {subscription.protocol}
                  </span>
                  <ResourceLink reference={subscription.endpoint} service={subscription.protocol} />
                </div>
                <SubscriptionFilterPolicy subscriptionArn={subscription.subscriptionArn} />
              </li>
            ))}
          </ul>
        )}
      </section>
      <section data-testid="sns-detail-publish" style={publishSectionStyle}>
        <Text style={labelStyle}>Publish a message</Text>
        <div style={fieldRowStyle}>
          <label htmlFor="sns-publish-subject" style={labelStyle}>
            Subject
          </label>
          <input
            id="sns-publish-subject"
            data-testid="sns-detail-publish-subject"
            style={inputStyle}
            value={subject}
            onChange={(event) => setSubject(event.target.value)}
          />
        </div>
        <div style={fieldRowStyle}>
          <label htmlFor="sns-publish-message" style={labelStyle}>
            Message
          </label>
          <textarea
            id="sns-publish-message"
            data-testid="sns-detail-publish-message"
            style={textareaStyle}
            value={message}
            onChange={(event) => setMessage(event.target.value)}
          />
        </div>
        <label style={labelStyle}>Message attributes</label>
        {attributes.length > 0 ? (
          <div data-testid="sns-detail-publish-attributes" style={fieldRowStyle}>
            {attributes.map((attribute, index) => (
              <div key={index} data-testid="sns-detail-publish-attribute-row" style={attributeRowStyle}>
                <div style={fieldRowStyle}>
                  <label htmlFor={`sns-publish-attribute-key-${index}`} style={labelStyle}>
                    Name
                  </label>
                  <input
                    id={`sns-publish-attribute-key-${index}`}
                    data-testid="sns-detail-publish-attribute-key"
                    style={inputStyle}
                    value={attribute.key}
                    onChange={(event) => updateAttribute(index, 'key', event.target.value)}
                  />
                </div>
                <div style={fieldRowStyle}>
                  <label htmlFor={`sns-publish-attribute-value-${index}`} style={labelStyle}>
                    Value
                  </label>
                  <input
                    id={`sns-publish-attribute-value-${index}`}
                    data-testid="sns-detail-publish-attribute-value"
                    style={inputStyle}
                    value={attribute.value}
                    onChange={(event) => updateAttribute(index, 'value', event.target.value)}
                  />
                </div>
                <button
                  type="button"
                  data-testid="sns-detail-publish-attribute-remove"
                  style={attributeRemoveButtonStyle}
                  onClick={() => removeAttribute(index)}
                >
                  Remove
                </button>
              </div>
            ))}
          </div>
        ) : null}
        <button
          type="button"
          data-testid="sns-detail-publish-attribute-add"
          style={buttonStyle}
          onClick={addAttribute}
        >
          Add attribute
        </button>
        <button
          type="button"
          data-testid="sns-detail-publish-submit"
          style={buttonStyle}
          disabled={!canPublish}
          onClick={handlePublish}
        >
          Publish message
        </button>
        {publishState.kind === 'sent' ? (
          <span data-testid="sns-detail-publish-status" style={hintStyle}>
            Message published.
          </span>
        ) : null}
        {publishState.kind === 'error' ? (
          <span data-testid="sns-detail-publish-error" style={hintStyle}>
            Unable to publish the message.
          </span>
        ) : null}
      </section>
    </div>
  );
}

export default SnsDetailView;
