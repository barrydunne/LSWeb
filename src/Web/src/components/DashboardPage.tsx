import { Heading } from '@primer/react';
import { HealthBanner } from './HealthBanner';
import { ServiceCatalogueGrid } from './ServiceCatalogueGrid';

export function DashboardPage() {
  return (
    <section
      data-testid="dashboard-page"
      style={{ display: 'flex', flexDirection: 'column', gap: 16 }}
    >
      <HealthBanner />
      <Heading as="h2" data-testid="dashboard-heading" style={{ fontSize: 20 }}>
        Services
      </Heading>
      <ServiceCatalogueGrid />
    </section>
  );
}
