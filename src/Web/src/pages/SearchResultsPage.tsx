import { useMemo } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { ResourceLink } from '../components/ResourceLink';
import type { SearchMatchItem, SearchStateResult } from '../api/client';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 16,
};

const groupStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const matchListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  margin: 0,
  padding: 0,
  listStyle: 'none',
};

const hintStyle: CSSProperties = {
  fontSize: 12,
  opacity: 0.8,
};

interface ServiceGroup {
  serviceKey: string;
  matches: SearchMatchItem[];
}

function groupByService(matches: SearchMatchItem[]): ServiceGroup[] {
  const groups = new Map<string, SearchMatchItem[]>();
  for (const match of matches) {
    const existing = groups.get(match.serviceKey);
    if (existing) {
      existing.push(match);
    } else {
      groups.set(match.serviceKey, [match]);
    }
  }
  return Array.from(groups.entries())
    .map(([serviceKey, items]) => ({ serviceKey, matches: items }))
    .sort((left, right) => left.serviceKey.localeCompare(right.serviceKey));
}

function describeState(state: SearchStateResult): string {
  if (state.isBuilding) {
    return 'Search index is rebuilding\u2026';
  }
  return `Search index: ${state.entryCount} resources.`;
}

export function SearchResultsPage({
  query,
  matches,
  state,
}: {
  query: string;
  matches: SearchMatchItem[];
  state: SearchStateResult | null;
}) {
  const groups = useMemo(() => groupByService(matches), [matches]);

  return (
    <section data-testid="search-results" style={containerStyle}>
      <Heading as="h2" data-testid="search-results-heading" style={{ fontSize: 18 }}>
        Search results
      </Heading>

      {state ? (
        <Text data-testid="search-results-state" style={hintStyle}>
          {describeState(state)}
        </Text>
      ) : null}

      {matches.length === 0 ? (
        <Text data-testid="search-results-empty" style={{ fontSize: 14 }}>
          {`No resources match \u201c${query}\u201d.`}
        </Text>
      ) : null}

      {groups.map((group) => (
        <div key={group.serviceKey} data-testid="search-results-group" style={groupStyle}>
          <Heading as="h3" data-testid="search-results-group-name" style={{ fontSize: 15 }}>
            {group.serviceKey}
          </Heading>
          <ul data-testid="search-results-group-matches" style={matchListStyle}>
            {group.matches.map((match) => (
              <li key={`${match.serviceKey}:${match.resourceId}`} data-testid="search-results-match">
                <ResourceLink
                  reference={match.resourceId}
                  service={match.serviceKey}
                  label={match.displayName}
                />
              </li>
            ))}
          </ul>
        </div>
      ))}
    </section>
  );
}
