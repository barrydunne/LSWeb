import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter } from 'react-router-dom';
import { HomePage } from './HomePage';
import {
  getCatalogue,
  getFavourites,
  getRecentlyViewed,
  resolveReference,
  type CatalogueServiceItem,
} from '../api/client';

vi.mock('../api/client');

const getCatalogueMock = vi.mocked(getCatalogue);
const getRecentlyViewedMock = vi.mocked(getRecentlyViewed);
const getFavouritesMock = vi.mocked(getFavourites);
const resolveReferenceMock = vi.mocked(resolveReference);

function service(overrides: Partial<CatalogueServiceItem> & { key: string }): CatalogueServiceItem {
  return {
    key: overrides.key,
    displayName: overrides.displayName ?? overrides.key,
    category: overrides.category ?? 'Compute',
    iconHint: overrides.iconHint ?? overrides.key,
    route: overrides.route ?? `/services/${overrides.key}`,
    supported: overrides.supported ?? true,
    supportDetail: overrides.supportDetail ?? null,
  };
}

function renderHome() {
  return render(
    <MemoryRouter>
      <ThemeProvider colorMode="night">
        <HomePage />
      </ThemeProvider>
    </MemoryRouter>,
  );
}

describe('HomePage', () => {
  beforeEach(() => {
    getCatalogueMock.mockResolvedValue({ services: [] });
    getRecentlyViewedMock.mockResolvedValue({ references: [] });
    getFavouritesMock.mockResolvedValue({ references: [] });
    resolveReferenceMock.mockResolvedValue({ serviceKey: 'sqs', resourceId: 'orders', route: '/services/sqs/orders' });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading message while the catalogue is pending', () => {
    getCatalogueMock.mockReturnValue(new Promise<never>(() => {}));

    renderHome();

    expect(screen.getByTestId('home-loading')).toBeInTheDocument();
    expect(screen.getByTestId('home-recent-empty')).toBeInTheDocument();
  });

  it('renders a quick link for every service in the catalogue', async () => {
    getCatalogueMock.mockResolvedValue({
      services: Array.from({ length: 8 }, (_, index) =>
        service({ key: `svc-${index}`, displayName: `Service ${index}`, route: `/services/svc-${index}` }),
      ),
    });

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-quick-links')).toBeInTheDocument());

    const links = screen.getAllByTestId('home-quick-link');
    expect(links).toHaveLength(8);
    expect(links[0]).toHaveAttribute('href', '/services/svc-0');
  });

  it('groups recently-viewed resources onto their service cards, capped at three', async () => {
    getCatalogueMock.mockResolvedValue({
      services: [
        service({ key: 'sqs', displayName: 'Simple Queue Service', route: '/services/sqs' }),
        service({ key: 's3', displayName: 'Simple Storage Service', route: '/services/s3' }),
        service({ key: 'lambda', displayName: 'Lambda', route: '/services/lambda' }),
      ],
    });
    getRecentlyViewedMock.mockResolvedValue({
      references: ['sqs://a', 'sqs://b', 'sqs://c', 'sqs://d', 's3://x', 'broken'],
    });
    resolveReferenceMock.mockImplementation((reference) => {
      if (reference === 'broken') {
        return Promise.reject(new Error('unresolved'));
      }
      const [serviceKey, resourceId] = reference.split('://');
      return Promise.resolve({ serviceKey, resourceId, route: `/services/${serviceKey}/${resourceId}` });
    });

    renderHome();

    await waitFor(() => expect(screen.getAllByTestId('home-quick-link-card')).toHaveLength(3));
    await waitFor(() => expect(screen.getAllByTestId('home-quick-link-resource')).toHaveLength(4));

    const cards = screen.getAllByTestId('home-quick-link-card');
    expect(within(cards[0]).getAllByTestId('home-quick-link-resource')).toHaveLength(3);
    expect(within(cards[0]).getByText('a')).toBeInTheDocument();
    expect(within(cards[0]).queryByText('d')).not.toBeInTheDocument();
    expect(within(cards[1]).getAllByTestId('home-quick-link-resource')).toHaveLength(1);
    expect(within(cards[2]).queryByTestId('home-quick-link-resources')).not.toBeInTheDocument();
  });

  it('filters the quick links by the search query', async () => {
    getCatalogueMock.mockResolvedValue({
      services: [
        service({ key: 'sqs', displayName: 'Simple Queue Service', category: 'Messaging' }),
        service({ key: 'lambda', displayName: 'Lambda', category: 'Compute' }),
      ],
    });

    renderHome();

    await waitFor(() => expect(screen.getAllByTestId('home-quick-link')).toHaveLength(2));

    fireEvent.change(screen.getByTestId('home-search-input'), { target: { value: 'queue' } });

    const links = screen.getAllByTestId('home-quick-link');
    expect(links).toHaveLength(1);
    expect(screen.getByTestId('home-quick-link-name')).toHaveTextContent('Simple Queue Service');
  });

  it('shows a no-matches message when the search excludes every service', async () => {
    getCatalogueMock.mockResolvedValue({
      services: [service({ key: 'sqs', displayName: 'Simple Queue Service', category: 'Messaging' })],
    });

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-quick-link')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('home-search-input'), { target: { value: 'nothing-here' } });

    expect(screen.getByTestId('home-no-matches')).toBeInTheDocument();
    expect(screen.queryByTestId('home-quick-link')).not.toBeInTheDocument();
  });

  it('shows an empty message when the catalogue has no services', async () => {
    getCatalogueMock.mockResolvedValue({ services: [] });

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-empty')).toBeInTheDocument());
    expect(screen.queryByTestId('home-quick-link')).not.toBeInTheDocument();
  });

  it('shows an error message when the catalogue request fails', async () => {
    getCatalogueMock.mockRejectedValue(new Error('boom'));

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-error')).toBeInTheDocument());
  });

  it('lists recently-viewed resources when the store returns references', async () => {
    getRecentlyViewedMock.mockResolvedValue({ references: ['sqs://orders', 'sns://events'] });

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-recent-list')).toBeInTheDocument());

    expect(screen.getAllByTestId('home-recent-item')).toHaveLength(2);
    expect(screen.queryByTestId('home-recent-empty')).not.toBeInTheDocument();
    await waitFor(() => expect(resolveReferenceMock).toHaveBeenCalled());
    await waitFor(() => expect(screen.getAllByRole('link').length).toBeGreaterThan(0));
  });

  it('shows the recent placeholder when there are no recently-viewed resources', async () => {
    getRecentlyViewedMock.mockResolvedValue({ references: [] });

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-recent-empty')).toBeInTheDocument());
    expect(screen.queryByTestId('home-recent-item')).not.toBeInTheDocument();
  });

  it('lists favourites when the store returns references', async () => {
    getFavouritesMock.mockResolvedValue({ references: ['s3://reports'] });

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-favourites-list')).toBeInTheDocument());

    expect(screen.getAllByTestId('home-favourite-item')).toHaveLength(1);
    expect(screen.queryByTestId('home-favourites-empty')).not.toBeInTheDocument();
    await waitFor(() => expect(resolveReferenceMock).toHaveBeenCalled());
    await waitFor(() => expect(screen.getAllByRole('link').length).toBeGreaterThan(0));
  });

  it('shows the favourites placeholder when there are no favourites', async () => {
    getFavouritesMock.mockResolvedValue({ references: [] });

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-favourites-empty')).toBeInTheDocument());
    expect(screen.queryByTestId('home-favourite-item')).not.toBeInTheDocument();
  });

  it('falls back to empty sections when the preference requests fail', async () => {
    getRecentlyViewedMock.mockRejectedValue(new Error('boom'));
    getFavouritesMock.mockRejectedValue(new Error('boom'));

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-recent-empty')).toBeInTheDocument());
    expect(screen.getByTestId('home-favourites-empty')).toBeInTheDocument();
  });
});