import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter } from 'react-router-dom';
import { ServiceCard } from './ServiceCard';
import type { CatalogueServiceItem } from '../api/client';

const service: CatalogueServiceItem = {
  key: 's3',
  displayName: 'S3',
  category: 'Storage',
  iconHint: 'archive',
  route: '/services/s3',
  supported: true,
  supportDetail: null,
};

function renderCard(availability?: string) {
  return render(
    <MemoryRouter>
      <ThemeProvider colorMode="night">
        <ServiceCard service={service} availability={availability} />
      </ThemeProvider>
    </MemoryRouter>,
  );
}

function renderUnsupportedCard(overrides?: Partial<CatalogueServiceItem>) {
  return render(
    <MemoryRouter>
      <ThemeProvider colorMode="night">
        <ServiceCard service={{ ...service, supported: false, ...overrides }} />
      </ThemeProvider>
    </MemoryRouter>,
  );
}

describe('ServiceCard', () => {
  it('renders the service name, category and icon hint', () => {
    renderCard();

    expect(screen.getByTestId('service-card-name')).toHaveTextContent('S3');
    expect(screen.getByTestId('service-card-category')).toHaveTextContent('Storage');
    expect(screen.getByTestId('service-card-icon')).toHaveTextContent('archive');
  });

  it('links to the service route', () => {
    renderCard();

    expect(screen.getByTestId('service-card-s3')).toHaveAttribute('href', '/services/s3');
  });

  it('defaults to an available status', () => {
    renderCard();

    expect(screen.getByTestId('service-card-availability')).toHaveTextContent('Available');
    expect(screen.getByTestId('service-card-s3')).toHaveAttribute('aria-disabled', 'false');
  });

  it('greys out and marks unavailable services as disabled', () => {
    renderCard('Unavailable');

    expect(screen.getByTestId('service-card-availability')).toHaveTextContent('Unavailable');
    const card = screen.getByTestId('service-card-s3');
    expect(card).toHaveAttribute('aria-disabled', 'true');
    expect(card).toHaveStyle({ opacity: '0.55' });
  });

  it('shows an unknown status without disabling the card', () => {
    renderCard('Unknown');

    expect(screen.getByTestId('service-card-availability')).toHaveTextContent('Unknown');
    expect(screen.getByTestId('service-card-s3')).toHaveAttribute('aria-disabled', 'false');
  });

  it('renders an unsupported service as a non-actionable, dimmed card', () => {
    renderUnsupportedCard({ supportDetail: 'Not supported by the current backend.' });

    const card = screen.getByTestId('service-card-s3');
    expect(card).not.toHaveAttribute('href');
    expect(card).toHaveAttribute('aria-disabled', 'true');
    expect(card).toHaveStyle({ opacity: '0.55' });
    expect(screen.getByTestId('service-card-unsupported')).toHaveTextContent('Unsupported');
    expect(screen.getByTestId('service-card-support-detail')).toHaveTextContent(
      'Not supported by the current backend.',
    );
  });

  it('falls back to a default message when an unsupported service has no detail', () => {
    renderUnsupportedCard({ supportDetail: null });

    expect(screen.getByTestId('service-card-support-detail')).toHaveTextContent(
      'This service is not supported by the current backend.',
    );
  });
});
