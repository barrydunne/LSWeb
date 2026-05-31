import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter } from 'react-router-dom';
import { SearchResultsPage } from './SearchResultsPage';
import {
  resolveReference,
  type SearchMatchItem,
  type SearchStateResult,
} from '../api/client';

vi.mock('../api/client');

const resolveReferenceMock = vi.mocked(resolveReference);

function match(overrides: Partial<SearchMatchItem> & { serviceKey: string; resourceId: string }): SearchMatchItem {
  return {
    serviceKey: overrides.serviceKey,
    resourceId: overrides.resourceId,
    displayName: overrides.displayName ?? overrides.resourceId,
    route: overrides.route ?? `/services/${overrides.serviceKey}/${overrides.resourceId}`,
  };
}

function renderPage(props: {
  query: string;
  matches: SearchMatchItem[];
  state: SearchStateResult | null;
}) {
  return render(
    <MemoryRouter>
      <ThemeProvider colorMode="night">
        <SearchResultsPage {...props} />
      </ThemeProvider>
    </MemoryRouter>,
  );
}

describe('SearchResultsPage', () => {
  beforeEach(() => {
    resolveReferenceMock.mockImplementation((reference, service) =>
      Promise.resolve({
        serviceKey: service ?? 'unknown',
        resourceId: reference,
        route: `/services/${service ?? 'unknown'}/${reference}`,
      }),
    );
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('groups matches by service and sorts the groups by service key', () => {
    renderPage({
      query: 'or',
      state: null,
      matches: [
        match({ serviceKey: 'sqs', resourceId: 'orders' }),
        match({ serviceKey: 'lambda', resourceId: 'order-processor' }),
        match({ serviceKey: 'sqs', resourceId: 'order-events' }),
      ],
    });

    const groups = screen.getAllByTestId('search-results-group');
    expect(groups).toHaveLength(2);

    const names = screen.getAllByTestId('search-results-group-name').map((node) => node.textContent);
    expect(names).toEqual(['lambda', 'sqs']);

    expect(screen.getAllByTestId('search-results-match')).toHaveLength(3);
  });

  it('renders a resource link per match using the display name as the label', async () => {
    renderPage({
      query: 'orders',
      state: null,
      matches: [match({ serviceKey: 'sqs', resourceId: 'orders', displayName: 'Orders queue' })],
    });

    await waitFor(() =>
      expect(screen.getByTestId('resource-link')).toHaveAttribute('href', '/services/sqs/orders'),
    );
    expect(screen.getByTestId('resource-link')).toHaveTextContent('Orders queue');
    expect(resolveReferenceMock).toHaveBeenCalledWith('orders', 'sqs', expect.any(AbortSignal));
  });

  it('renders an IAM match grouped under iam and navigates to its type-prefixed route', async () => {
    renderPage({
      query: 'lambda',
      state: null,
      matches: [
        match({ serviceKey: 'iam', resourceId: 'role/LambdaExec', displayName: 'LambdaExec' }),
      ],
    });

    expect(screen.getByTestId('search-results-group-name')).toHaveTextContent('iam');
    await waitFor(() =>
      expect(screen.getByTestId('resource-link')).toHaveAttribute(
        'href',
        '/services/iam/role/LambdaExec',
      ),
    );
    expect(screen.getByTestId('resource-link')).toHaveTextContent('LambdaExec');
    expect(resolveReferenceMock).toHaveBeenCalledWith('role/LambdaExec', 'iam', expect.any(AbortSignal));
  });

  it('shows an empty message naming the query when there are no matches', () => {
    renderPage({ query: 'zzz', state: null, matches: [] });

    expect(screen.getByTestId('search-results-empty')).toHaveTextContent('No resources match \u201czzz\u201d.');
    expect(screen.queryByTestId('search-results-group')).not.toBeInTheDocument();
  });

  it('shows the index entry count when the index is not building', () => {
    renderPage({
      query: 'or',
      state: { builtAt: '2026-01-02T03:04:05Z', entryCount: 12, isBuilding: false },
      matches: [],
    });

    expect(screen.getByTestId('search-results-state')).toHaveTextContent('Search index: 12 resources.');
  });

  it('shows a rebuilding hint when the index is building', () => {
    renderPage({
      query: 'or',
      state: { builtAt: '2026-01-02T03:04:05Z', entryCount: 0, isBuilding: true },
      matches: [],
    });

    expect(screen.getByTestId('search-results-state')).toHaveTextContent('Search index is rebuilding\u2026');
  });

  it('omits the state hint when no state is available', () => {
    renderPage({ query: 'or', state: null, matches: [] });

    expect(screen.queryByTestId('search-results-state')).not.toBeInTheDocument();
  });
});
