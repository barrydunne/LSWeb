import { useEffect, useState } from 'react';
import { getCircuitStatus, type CircuitStatusResult } from '../api/client';

const pollIntervalMs = 8000;

const bannerStyle = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  padding: '12px 16px',
  borderRadius: 6,
  border: '1px solid #f85149',
  background: '#3d1417',
  color: '#ffdcd7',
} as const;

const titleStyle = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
  fontSize: 14,
  fontWeight: 600,
} as const;

const detailStyle = { fontSize: 13, lineHeight: 1.5 } as const;

function describeServices(services: string[]): string {
  if (services.length === 0) {
    return 'a backend service';
  }
  if (services.length === 1) {
    return services[0];
  }
  if (services.length === 2) {
    return `${services[0]} and ${services[1]}`;
  }
  return `${services.slice(0, -1).join(', ')} and ${services[services.length - 1]}`;
}

/**
 * Site-wide warning banner that appears on every page while an AWS gateway circuit breaker is
 * open. It names the affected service(s) and tells the user how to recover (restart the
 * container), and auto-clears once the breaker closes. Polls the circuit-status endpoint so it
 * appears and disappears without a page reload.
 */
export function CircuitBreakerBanner() {
  const [status, setStatus] = useState<CircuitStatusResult | null>(null);

  useEffect(() => {
    let active = true;
    const controller = new AbortController();

    const poll = () => {
      getCircuitStatus(controller.signal)
        .then((result) => {
          if (active) {
            setStatus(result);
          }
        })
        .catch(() => {
          // A failed status check should not itself raise an alarm; leave the last known state.
        });
    };

    poll();
    const timer = setInterval(poll, pollIntervalMs);
    return () => {
      active = false;
      controller.abort();
      clearInterval(timer);
    };
  }, []);

  if (status === null || !status.isOpen) {
    return null;
  }

  const services = describeServices(status.affectedServices);

  return (
    <div data-testid="circuit-breaker-banner" role="alert" style={bannerStyle}>
      <span data-testid="circuit-breaker-banner-title" style={titleStyle}>
        <span aria-hidden="true">&#9888;</span>
        Connection to {services} temporarily suspended
      </span>
      <span data-testid="circuit-breaker-banner-detail" style={detailStyle}>
        Repeated failures have tripped the circuit breaker, so calls to {services}{' '}
        {status.affectedServices.length > 1 ? 'are' : 'is'} being rejected. Try restarting the
        application container and reloading. This banner clears automatically once the connection
        recovers.
      </span>
    </div>
  );
}
