import { Suspense } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { useParams } from 'react-router-dom';
import { EmptyState } from '../components/EmptyState';
import { useCatalogueService } from '../hooks/useCatalogueService';
import { getServiceView } from '../services/serviceViewRegistry';

const sectionStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 16 };

export function ServicePage() {
  const { serviceKey = '' } = useParams();
  const state = useCatalogueService(serviceKey);

  if (state.kind === 'loading') {
    return (
      <section data-testid="service-page" style={sectionStyle}>
        <Text data-testid="service-page-loading" style={{ fontSize: 14 }}>
          Loading service&hellip;
        </Text>
      </section>
    );
  }

  if (state.kind === 'error') {
    return (
      <section data-testid="service-page" style={sectionStyle}>
        <EmptyState
          variant="no-resources"
          message="Unable to load this service right now. Check the connection and try again."
        />
      </section>
    );
  }

  if (state.kind === 'unknown') {
    return (
      <section data-testid="service-page" data-state="unknown" style={sectionStyle}>
        <Heading as="h2" data-testid="service-page-heading" style={{ fontSize: 20 }}>
          Service not found
        </Heading>
        <EmptyState
          variant="no-matches"
          message={`No service is registered for "${serviceKey}". It may not be supported by the current backend.`}
        />
      </section>
    );
  }

  const { service } = state;
  const view = getServiceView(serviceKey);
  const ListView = view?.list;

  return (
    <section data-testid="service-page" data-state="ready" style={sectionStyle}>
      <Heading as="h2" data-testid="service-page-heading" style={{ fontSize: 20 }}>
        {service.displayName}
      </Heading>
      {ListView ? (
        <Suspense
          fallback={
            <Text data-testid="service-page-view-loading" style={{ fontSize: 14 }}>
              Loading&hellip;
            </Text>
          }
        >
          <ListView serviceKey={serviceKey} />
        </Suspense>
      ) : (
        <EmptyState
          variant="no-resources"
          message={`The ${service.displayName} experience hasn't been built yet. It's on the roadmap.`}
        />
      )}
    </section>
  );
}
