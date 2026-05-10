import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { ServiceCatalogueGrid } from './ServiceCatalogueGrid';
import { getCatalogue, getHealth, type CatalogueServiceItem } from '../api/client';

vi.mock('../api/client');

const getCatalogueMock = vi.mocked(getCatalogue);
const getHealthMock = vi.mocked(getHealth);

const services: CatalogueServiceItem[] = [
  {
    key: 's3',
    displayName: 'S3',
    category: 'Storage',
    iconHint: 'archive',
    route: '/services/s3',
    supported: true,
    supportDetail: null,
  },
  {
    key: 'lambda',
    displayName: 'Lambda',
    category: 'Compute',
    iconHint: 'zap',
    route: '/services/lambda',
    supported: true,
    supportDetail: null,
  },
];

function renderGrid() {
  return render(
    <ThemeProvider colorMode="night">
      <ServiceCatalogueGrid />
    </ThemeProvider>,
  );
}

describe('ServiceCatalogueGrid', () => {
  beforeEach(() => {
    getHealthMock.mockResolvedValue({ services: [] });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the catalogue resolves', () => {
    getCatalogueMock.mockReturnValue(new Promise(() => {}));

    renderGrid();

    expect(screen.getByTestId('catalogue-grid-loading')).toBeInTheDocument();
  });

  it('renders a card for each managed service', async () => {
    getCatalogueMock.mockResolvedValue({ services });

    renderGrid();

    await waitFor(() => {
      expect(screen.getByTestId('catalogue-grid')).toBeInTheDocument();
    });
    expect(screen.getByTestId('service-card-s3')).toBeInTheDocument();
    expect(screen.getByTestId('service-card-lambda')).toBeInTheDocument();
  });

  it('annotates each card with its reported availability', async () => {
    getCatalogueMock.mockResolvedValue({ services });
    getHealthMock.mockResolvedValue({
      services: [
        { key: 's3', availability: 'Available' },
        { key: 'lambda', availability: 'Unavailable' },
      ],
    });

    renderGrid();

    await waitFor(() => {
      expect(screen.getByTestId('catalogue-grid')).toBeInTheDocument();
    });
    expect(screen.getByTestId('service-card-lambda')).toHaveAttribute('aria-disabled', 'true');
    expect(screen.getByTestId('service-card-s3')).toHaveAttribute('aria-disabled', 'false');
  });

  it('falls back to an unknown status when the health request fails', async () => {
    getCatalogueMock.mockResolvedValue({ services });
    getHealthMock.mockRejectedValue(new Error('boom'));

    renderGrid();

    await waitFor(() => {
      expect(screen.getByTestId('catalogue-grid')).toBeInTheDocument();
    });
    expect(screen.getByTestId('service-card-s3')).toHaveAttribute('aria-disabled', 'false');
  });

  it('renders unsupported services as non-actionable cards', async () => {
    getCatalogueMock.mockResolvedValue({
      services: [
        {
          key: 'kinesis',
          displayName: 'Kinesis',
          category: 'Analytics',
          iconHint: 'pulse',
          route: '/services/kinesis',
          supported: false,
          supportDetail: 'Not supported by the current backend.',
        },
      ],
    });

    renderGrid();

    await waitFor(() => {
      expect(screen.getByTestId('catalogue-grid')).toBeInTheDocument();
    });
    const card = screen.getByTestId('service-card-kinesis');
    expect(card).not.toHaveAttribute('href');
    expect(card).toHaveAttribute('aria-disabled', 'true');
    expect(screen.getByTestId('service-card-unsupported')).toBeInTheDocument();
  });

  it('shows an empty state when no services are available', async () => {
    getCatalogueMock.mockResolvedValue({ services: [] });

    renderGrid();

    await waitFor(() => {
      expect(screen.getByTestId('catalogue-grid-empty')).toBeInTheDocument();
    });
  });

  it('shows an error state when the request fails', async () => {
    getCatalogueMock.mockRejectedValue(new Error('boom'));

    renderGrid();

    await waitFor(() => {
      expect(screen.getByTestId('catalogue-grid-error')).toBeInTheDocument();
    });
  });
});
