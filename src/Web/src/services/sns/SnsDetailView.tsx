import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { getSnsSubscriptions } from '../../api/client';
import type { SnsSubscriptionItem } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { ResourceLink } from '../../components/ResourceLink';

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

type LoadState = 'loading' | 'ready' | 'error';

function deriveTopicName(topicArn: string): string {
  return topicArn.split(':').pop() || topicArn;
}

export function SnsDetailView({ resourceId }: ServiceDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [subscriptions, setSubscriptions] = useState<SnsSubscriptionItem[]>([]);

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
                style={subscriptionRowStyle}
              >
                <span data-testid="sns-subscription-protocol" style={protocolBadgeStyle}>
                  {subscription.protocol}
                </span>
                <ResourceLink reference={subscription.endpoint} service={subscription.protocol} />
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}

export default SnsDetailView;
