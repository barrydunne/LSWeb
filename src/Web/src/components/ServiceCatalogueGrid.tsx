import { useEffect, useState } from 'react';
import { Text } from '@primer/react';
import { getCatalogue, getHealth, type CatalogueServiceItem } from '../api/client';
import { ServiceCard } from './ServiceCard';

type GridState =
  | { kind: 'loading' }
  | { kind: 'ready'; services: CatalogueServiceItem[]; availabilityByKey: Map<string, string> }
  | { kind: 'error' };

export function ServiceCatalogueGrid() {
  const [state, setState] = useState<GridState>({ kind: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    Promise.all([
      getCatalogue(controller.signal),
      getHealth(controller.signal).catch(() => ({ services: [] })),
    ])
      .then(([catalogue, health]) => {
        const availabilityByKey = new Map(health.services.map((service) => [service.key, service.availability]));
        setState({ kind: 'ready', services: catalogue.services, availabilityByKey });
      })
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, []);

  if (state.kind === 'loading') {
    return (
      <Text data-testid="catalogue-grid-loading" style={{ fontSize: 14 }}>
        Loading services&hellip;
      </Text>
    );
  }

  if (state.kind === 'error') {
    return (
      <Text data-testid="catalogue-grid-error" style={{ fontSize: 14 }}>
        Unable to load the service catalogue.
      </Text>
    );
  }

  if (state.services.length === 0) {
    return (
      <Text data-testid="catalogue-grid-empty" style={{ fontSize: 14 }}>
        No services are available.
      </Text>
    );
  }

  return (
    <div
      data-testid="catalogue-grid"
      style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
        gap: 16,
      }}
    >
      {state.services.map((service) => (
        <ServiceCard
          key={service.key}
          service={service}
          availability={state.availabilityByKey.get(service.key) ?? 'Unknown'}
        />
      ))}
    </div>
  );
}
