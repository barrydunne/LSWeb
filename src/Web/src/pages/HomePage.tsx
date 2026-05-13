import { useEffect, useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Label, Text } from '@primer/react';
import { getCatalogue, type CatalogueServiceItem } from '../api/client';

type HomeState =
  | { kind: 'loading' }
  | { kind: 'ready'; services: CatalogueServiceItem[] }
  | { kind: 'error' };

const quickLinkLimit = 6;

const gridStyle: CSSProperties = {
  display: 'grid',
  gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
  gap: 16,
};

const cardStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  textDecoration: 'none',
  color: 'inherit',
};

const searchInputStyle: CSSProperties = {
  padding: '8px 12px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  fontSize: 14,
  width: '100%',
  maxWidth: 360,
};

export function HomePage() {
  const [state, setState] = useState<HomeState>({ kind: 'loading' });
  const [query, setQuery] = useState('');

  useEffect(() => {
    const controller = new AbortController();
    getCatalogue(controller.signal)
      .then((catalogue) => setState({ kind: 'ready', services: catalogue.services }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, []);

  const serviceCount = state.kind === 'ready' ? state.services.length : 0;

  const quickLinks = useMemo(() => {
    if (state.kind !== 'ready') {
      return [];
    }
    const needle = query.trim().toLowerCase();
    return state.services
      .filter((service) => `${service.displayName} ${service.category}`.toLowerCase().includes(needle))
      .slice(0, quickLinkLimit);
  }, [state, query]);

  return (
    <section data-testid="home-page" style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <Heading as="h2" data-testid="home-heading" style={{ fontSize: 20 }}>
        Welcome to LocalStack Web
      </Heading>
      <Text data-testid="home-subtitle" style={{ fontSize: 14, opacity: 0.8 }}>
        Jump straight to a service or pick up where you left off.
      </Text>

      <input
        data-testid="home-search-input"
        type="search"
        aria-label="Search services"
        placeholder={'Search services\u2026'}
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        style={searchInputStyle}
      />

      <Heading as="h3" data-testid="home-quick-links-heading" style={{ fontSize: 16 }}>
        Quick links
      </Heading>

      {state.kind === 'loading' ? (
        <Text data-testid="home-loading" style={{ fontSize: 14 }}>
          Loading quick links&hellip;
        </Text>
      ) : null}

      {state.kind === 'error' ? (
        <Text data-testid="home-error" style={{ fontSize: 14 }}>
          Unable to load quick links.
        </Text>
      ) : null}

      {state.kind === 'ready' && serviceCount === 0 ? (
        <Text data-testid="home-empty" style={{ fontSize: 14 }}>
          No services are available.
        </Text>
      ) : null}

      {state.kind === 'ready' && serviceCount > 0 && quickLinks.length === 0 ? (
        <Text data-testid="home-no-matches" style={{ fontSize: 14 }}>
          No services match your search.
        </Text>
      ) : null}

      {quickLinks.length > 0 ? (
        <div data-testid="home-quick-links" style={gridStyle}>
          {quickLinks.map((service) => (
            <a key={service.key} href={service.route} data-testid="home-quick-link" style={cardStyle}>
              <Heading as="h4" data-testid="home-quick-link-name" style={{ fontSize: 15 }}>
                {service.displayName}
              </Heading>
              <Label data-testid="home-quick-link-category">{service.category}</Label>
            </a>
          ))}
        </div>
      ) : null}

      <Heading as="h3" data-testid="home-recent-heading" style={{ fontSize: 16 }}>
        Recent destinations
      </Heading>
      <Text data-testid="home-recent-empty" style={{ fontSize: 14, opacity: 0.8 }}>
        Your recently viewed resources will appear here.
      </Text>
    </section>
  );
}
