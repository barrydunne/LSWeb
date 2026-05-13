import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { HomePage } from './HomePage';
import { getCatalogue, type CatalogueServiceItem } from '../api/client';

vi.mock('../api/client');

const getCatalogueMock = vi.mocked(getCatalogue);

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
    <ThemeProvider colorMode="night">
      <HomePage />
    </ThemeProvider>,
  );
}

describe('HomePage', () => {
  beforeEach(() => {
    getCatalogueMock.mockResolvedValue({ services: [] });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading message while the catalogue is pending', () => {
    getCatalogueMock.mockReturnValue(new Promise<never>(() => {}));

    renderHome();

    expect(screen.getByTestId('home-loading')).toBeInTheDocument();
    expect(screen.getByTestId('home-recent-empty')).toBeInTheDocument();
  });

  it('renders quick links capped at the limit, sourced from the catalogue', async () => {
    getCatalogueMock.mockResolvedValue({
      services: Array.from({ length: 8 }, (_, index) =>
        service({ key: `svc-${index}`, displayName: `Service ${index}`, route: `/services/svc-${index}` }),
      ),
    });

    renderHome();

    await waitFor(() => expect(screen.getByTestId('home-quick-links')).toBeInTheDocument());

    const links = screen.getAllByTestId('home-quick-link');
    expect(links).toHaveLength(6);
    expect(links[0]).toHaveAttribute('href', '/services/svc-0');
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
});
