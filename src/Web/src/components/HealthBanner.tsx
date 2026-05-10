import { useEffect, useState } from 'react';
import { Label, Text } from '@primer/react';
import { getHealth, type HealthResult } from '../api/client';

type BannerState =
  | { kind: 'checking' }
  | { kind: 'ready'; health: HealthResult }
  | { kind: 'error' };

type LabelVariant = 'success' | 'danger' | 'secondary';

interface BannerSummary {
  variant: LabelVariant;
  status: string;
  summary: string;
}

function summarise(health: HealthResult): BannerSummary {
  const total = health.services.length;
  const unavailable = health.services.filter((service) => service.availability === 'Unavailable').length;
  const unknown = health.services.filter((service) => service.availability === 'Unknown').length;

  if (total === 0) {
    return { variant: 'secondary', status: 'Unknown', summary: 'No service health to report.' };
  }
  if (unavailable > 0) {
    return { variant: 'danger', status: 'Degraded', summary: `${unavailable} of ${total} services unavailable.` };
  }
  if (unknown > 0) {
    return { variant: 'secondary', status: 'Pending', summary: `Awaiting status for ${unknown} of ${total} services.` };
  }
  return { variant: 'success', status: 'Healthy', summary: `All ${total} services available.` };
}

const bannerStyle = {
  display: 'flex',
  alignItems: 'center',
  gap: 12,
  padding: '12px 16px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
} as const;

export function HealthBanner() {
  const [state, setState] = useState<BannerState>({ kind: 'checking' });

  useEffect(() => {
    const controller = new AbortController();
    getHealth(controller.signal)
      .then((health) => setState({ kind: 'ready', health }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, []);

  if (state.kind === 'checking') {
    return (
      <div data-testid="health-banner" style={bannerStyle}>
        <Text data-testid="health-banner-summary" style={{ fontSize: 14 }}>
          Service availability: checking&hellip;
        </Text>
      </div>
    );
  }

  if (state.kind === 'error') {
    return (
      <div data-testid="health-banner" style={bannerStyle}>
        <Label variant="danger" data-testid="health-banner-status">
          Unavailable
        </Label>
        <Text data-testid="health-banner-summary" style={{ fontSize: 14 }}>
          Unable to load service health.
        </Text>
      </div>
    );
  }

  const { variant, status, summary } = summarise(state.health);

  return (
    <div data-testid="health-banner" style={bannerStyle}>
      <Label variant={variant} data-testid="health-banner-status">
        {status}
      </Label>
      <Text data-testid="health-banner-summary" style={{ fontSize: 14 }}>
        {summary}
      </Text>
    </div>
  );
}
