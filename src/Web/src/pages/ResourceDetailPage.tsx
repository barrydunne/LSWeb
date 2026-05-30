import { Suspense, useEffect } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { useParams } from 'react-router-dom';
import { EmptyState } from '../components/EmptyState';
import { recordRecentlyViewed } from '../api/client';
import { useCatalogueService } from '../hooks/useCatalogueService';
import { getServiceView } from '../services/serviceViewRegistry';

const sectionStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 16 };

export function ResourceDetailPage() {
  const params = useParams();
  const serviceKey = params.serviceKey ?? '';
  // The detail route uses a trailing splat so resource ids containing slashes
  // (ARNs, S3 keys, parameter paths) survive routing. Decode for display/use.
  const rawResourceId = params['*'] ?? '';
  const resourceId = rawResourceId ? decodeURIComponent(rawResourceId) : '';
  const state = useCatalogueService(serviceKey);

  // Record the visit once the resource has resolved so it surfaces in the
  // home page's recently-viewed list. Recording is best-effort: a failure must
  // never disrupt viewing the resource.
  useEffect(() => {
    if (state.kind !== 'ready' || resourceId === '') {
      return;
    }
    const controller = new AbortController();
    void recordRecentlyViewed(`${serviceKey}://${resourceId}`, controller.signal).catch(() => {});
    return () => controller.abort();
  }, [state.kind, serviceKey, resourceId]);

  if (state.kind === 'loading') {
    return (
      <section data-testid="resource-detail-page" style={sectionStyle}>
        <Text data-testid="resource-detail-loading" style={{ fontSize: 14 }}>
          Loading resource&hellip;
        </Text>
      </section>
    );
  }

  if (state.kind === 'error') {
    return (
      <section data-testid="resource-detail-page" style={sectionStyle}>
        <EmptyState
          variant="no-resources"
          message="Unable to load this resource right now. Check the connection and try again."
        />
      </section>
    );
  }

  if (state.kind === 'unknown') {
    return (
      <section data-testid="resource-detail-page" data-state="unknown" style={sectionStyle}>
        <Heading as="h2" data-testid="resource-detail-heading" style={{ fontSize: 20 }}>
          Resource not found
        </Heading>
        <EmptyState
          variant="no-matches"
          message={`No service is registered for "${serviceKey}", so this resource can't be shown.`}
        />
      </section>
    );
  }

  const { service } = state;
  const view = getServiceView(serviceKey);
  const DetailView = view?.detail;

  return (
    <section data-testid="resource-detail-page" data-state="ready" style={sectionStyle}>
      <Heading as="h2" data-testid="resource-detail-heading" style={{ fontSize: 20 }}>
        {service.displayName}
      </Heading>
      {DetailView ? (
        <Suspense
          fallback={
            <Text data-testid="resource-detail-view-loading" style={{ fontSize: 14 }}>
              Loading&hellip;
            </Text>
          }
        >
          <DetailView serviceKey={serviceKey} resourceId={resourceId} />
        </Suspense>
      ) : (
        <EmptyState
          variant="no-resources"
          message={`The ${service.displayName} detail view hasn't been built yet. It's on the roadmap.`}
        />
      )}
    </section>
  );
}
