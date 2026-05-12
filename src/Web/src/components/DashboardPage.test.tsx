import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@primer/react';
import { DashboardPage } from './DashboardPage';
import { getCatalogue, getHealth, refreshCatalogue } from '../api/client';

vi.mock('../api/client');

const getCatalogueMock = vi.mocked(getCatalogue);
const getHealthMock = vi.mocked(getHealth);
const refreshCatalogueMock = vi.mocked(refreshCatalogue);

function renderDashboard() {
  return render(
    <ThemeProvider colorMode="night">
      <DashboardPage />
    </ThemeProvider>,
  );
}

describe('DashboardPage', () => {
  beforeEach(() => {
    getHealthMock.mockResolvedValue({ services: [] });
    getCatalogueMock.mockResolvedValue({ services: [] });
    refreshCatalogueMock.mockResolvedValue(undefined);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('renders the dashboard heading, health banner and the catalogue grid', async () => {
    getCatalogueMock.mockResolvedValue({
      services: [
        {
          key: 's3',
          displayName: 'S3',
          category: 'Storage',
          iconHint: 'archive',
          route: '/services/s3',
          supported: true,
          supportDetail: null,
        },
      ],
    });

    renderDashboard();

    expect(screen.getByTestId('dashboard-page')).toBeInTheDocument();
    expect(screen.getByTestId('dashboard-heading')).toHaveTextContent('Services');
    expect(screen.getByTestId('health-banner')).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.getByTestId('service-card-s3')).toBeInTheDocument();
    });
  });

  it('refreshes the catalogue after the user confirms the single-click action', async () => {
    const user = userEvent.setup();
    renderDashboard();

    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    expect(refreshCatalogueMock).toHaveBeenCalledTimes(1);
  });
});
