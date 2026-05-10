import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { HealthBanner } from './HealthBanner';
import { getHealth } from '../api/client';

vi.mock('../api/client');

const getHealthMock = vi.mocked(getHealth);

function renderBanner() {
  return render(
    <ThemeProvider colorMode="night">
      <HealthBanner />
    </ThemeProvider>,
  );
}

describe('HealthBanner', () => {
  beforeEach(() => {
    getHealthMock.mockReturnValue(new Promise(() => {}));
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a checking state before health resolves', () => {
    renderBanner();

    expect(screen.getByTestId('health-banner-summary')).toHaveTextContent('checking');
  });

  it('reports a healthy summary when all services are available', async () => {
    getHealthMock.mockResolvedValue({
      services: [
        { key: 's3', availability: 'Available' },
        { key: 'lambda', availability: 'Available' },
      ],
    });

    renderBanner();

    await waitFor(() => {
      expect(screen.getByTestId('health-banner-status')).toHaveTextContent('Healthy');
    });
    expect(screen.getByTestId('health-banner-summary')).toHaveTextContent('All 2 services available.');
  });

  it('reports a degraded summary when services are unavailable', async () => {
    getHealthMock.mockResolvedValue({
      services: [
        { key: 's3', availability: 'Available' },
        { key: 'lambda', availability: 'Unavailable' },
      ],
    });

    renderBanner();

    await waitFor(() => {
      expect(screen.getByTestId('health-banner-status')).toHaveTextContent('Degraded');
    });
    expect(screen.getByTestId('health-banner-summary')).toHaveTextContent('1 of 2 services unavailable.');
  });

  it('reports a pending summary when statuses are still unknown', async () => {
    getHealthMock.mockResolvedValue({
      services: [
        { key: 's3', availability: 'Available' },
        { key: 'lambda', availability: 'Unknown' },
      ],
    });

    renderBanner();

    await waitFor(() => {
      expect(screen.getByTestId('health-banner-status')).toHaveTextContent('Pending');
    });
    expect(screen.getByTestId('health-banner-summary')).toHaveTextContent('Awaiting status for 1 of 2 services.');
  });

  it('reports an unknown summary when there is nothing to report', async () => {
    getHealthMock.mockResolvedValue({ services: [] });

    renderBanner();

    await waitFor(() => {
      expect(screen.getByTestId('health-banner-status')).toHaveTextContent('Unknown');
    });
    expect(screen.getByTestId('health-banner-summary')).toHaveTextContent('No service health to report.');
  });

  it('shows an error state when the health request fails', async () => {
    getHealthMock.mockRejectedValue(new Error('boom'));

    renderBanner();

    await waitFor(() => {
      expect(screen.getByTestId('health-banner-status')).toHaveTextContent('Unavailable');
    });
    expect(screen.getByTestId('health-banner-summary')).toHaveTextContent('Unable to load service health.');
  });
});
