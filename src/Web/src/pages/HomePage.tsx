import { useEffect, useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { Link } from 'react-router-dom';
import {
  getCatalogue,
  getRecentlyViewed,
  resolveReference,
  type CatalogueServiceItem,
  type ResolvedReferenceResult,
} from '../api/client';
import { ResourceLink } from '../components/ResourceLink';
import { SeedTemplatesPanel } from '../components/SeedTemplatesPanel';
import { SnapshotPanel } from '../components/SnapshotPanel';

type HomeState =
  | { kind: 'loading' }
  | { kind: 'ready'; services: CatalogueServiceItem[] }
  | { kind: 'error' };

type RecentResource = ResolvedReferenceResult & { reference: string };

const maxRecentPerService = 3;

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

const referenceListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  listStyle: 'none',
  margin: 0,
  padding: 0,
};

const referenceItemStyle: CSSProperties = {
  padding: '8px 12px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const categoryPillStyle: CSSProperties = {
  alignSelf: 'flex-start',
  fontSize: 11,
  padding: '1px 8px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
  color: '#c9d1d9',
};

const quickLinkHeaderStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  textDecoration: 'none',
  color: 'inherit',
};

const cardResourceListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  listStyle: 'none',
  margin: 0,
  marginTop: 4,
  padding: 0,
  paddingTop: 8,
  borderTop: '1px solid #30363d',
};

const cardResourceLinkStyle: CSSProperties = {
  color: '#58a6ff',
  textDecoration: 'none',
  fontSize: 13,
};

export function HomePage() {
  const [state, setState] = useState<HomeState>({ kind: 'loading' });
  const [query, setQuery] = useState('');
  const [recent, setRecent] = useState<string[]>([]);
  const [recentByService, setRecentByService] = useState<Map<string, RecentResource[]>>(new Map());

  useEffect(() => {
    const controller = new AbortController();
    getCatalogue(controller.signal)
      .then((catalogue) => setState({ kind: 'ready', services: catalogue.services }))
      .catch(() => setState({ kind: 'error' }));
    getRecentlyViewed(controller.signal)
      .then((result) => setRecent(result.references))
      .catch(() => setRecent([]));
    return () => controller.abort();
  }, []);

  useEffect(() => {
    if (recent.length === 0) {
      setRecentByService(new Map());
      return;
    }
    const controller = new AbortController();
    const buildGroups = async () => {
      const resolved = await Promise.all(
        recent.map(async (reference): Promise<RecentResource | null> => {
          try {
            const result = await resolveReference(reference, undefined, controller.signal);
            return { reference, ...result };
          } catch {
            return null;
          }
        }),
      );
      const grouped = new Map<string, RecentResource[]>();
      for (const item of resolved) {
        if (item === null) {
          continue;
        }
        const bucket = grouped.get(item.serviceKey) ?? [];
        if (bucket.length < maxRecentPerService) {
          bucket.push(item);
          grouped.set(item.serviceKey, bucket);
        }
      }
      setRecentByService(grouped);
    };
    void buildGroups();
    return () => controller.abort();
  }, [recent]);

  const serviceCount = state.kind === 'ready' ? state.services.length : 0;

  const quickLinks = useMemo(() => {
    if (state.kind !== 'ready') {
      return [];
    }
    const needle = query.trim().toLowerCase();
    return state.services
      .filter((service) =>
        `${service.displayName} ${service.category}`.toLowerCase().includes(needle),
      )
      .sort((a, b) =>
        a.displayName.localeCompare(b.displayName, undefined, { sensitivity: 'base' }),
      );
  }, [state, query]);

  return (
    <section data-testid="home-page" style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <Heading as="h2" data-testid="home-heading" style={{ fontSize: 20 }}>
        Welcome to LocalStack Web
      </Heading>
      <Text data-testid="home-subtitle" style={{ fontSize: 14, opacity: 0.8 }}>
        Jump straight to a service or pick up where you left off.
      </Text>

      <SnapshotPanel />

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
          {quickLinks.map((service) => {
            const resources = recentByService.get(service.key) ?? [];
            return (
              <div key={service.key} data-testid="home-quick-link-card" style={cardStyle}>
                <Link to={service.route} data-testid="home-quick-link" style={quickLinkHeaderStyle}>
                  <Heading as="h4" data-testid="home-quick-link-name" style={{ fontSize: 15 }}>
                    {service.displayName}
                  </Heading>
                  <span data-testid="home-quick-link-category" style={categoryPillStyle}>
                    {service.category}
                  </span>
                </Link>
                {resources.length > 0 ? (
                  <ul data-testid="home-quick-link-resources" style={cardResourceListStyle}>
                    {resources.map((resource) => (
                      <li key={resource.reference} data-testid="home-quick-link-resource">
                        <Link to={resource.route} style={cardResourceLinkStyle}>
                          {resource.resourceId}
                        </Link>
                      </li>
                    ))}
                  </ul>
                ) : null}
              </div>
            );
          })}
        </div>
      ) : null}

      <Heading as="h3" data-testid="home-recent-heading" style={{ fontSize: 16 }}>
        Recent destinations
      </Heading>
      {recent.length > 0 ? (
        <ul data-testid="home-recent-list" style={referenceListStyle}>
          {recent.map((reference) => (
            <li key={reference} data-testid="home-recent-item" style={referenceItemStyle}>
              <ResourceLink reference={reference} />
            </li>
          ))}
        </ul>
      ) : (
        <Text data-testid="home-recent-empty" style={{ fontSize: 14, opacity: 0.8 }}>
          Your recently viewed resources will appear here.
        </Text>
      )}

      <SeedTemplatesPanel />
    </section>
  );
}
