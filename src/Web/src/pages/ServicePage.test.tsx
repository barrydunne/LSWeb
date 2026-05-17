import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ServicePage } from './ServicePage';
import { getCatalogue, type CatalogueServiceItem } from '../api/client';
import {
  clearServiceViews,
  registerServiceView,
} from '../services/serviceViewRegistry';

vi.mock('../api/client');

const getCatalogueMock = vi.mocked(getCatalogue);

const lambda: CatalogueServiceItem = {
  key: 'lambda',
  displayName: 'Lambda',
  category: 'Compute',
  iconHint: 'zap',
  route: '/services/lambda',
  supported: true,
  supportDetail: null,
};

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <ThemeProvider colorMode="night">
        <Routes>
          <Route path="services/:serviceKey" element={<ServicePage />} />
        </Routes>
      </ThemeProvider>
    </MemoryRouter>,
  );
}

describe('ServicePage', () => {
  beforeEach(() => {
    getCatalogueMock.mockResolvedValue({ services: [lambda] });
  });

  afterEach(() => {
    clearServiceViews();
    vi.resetAllMocks();
  });

  it('shows a not-built placeholder for a known service without a registered view', async () => {
    renderAt('/services/lambda');

    await waitFor(() => expect(screen.getByTestId('service-page-heading')).toHaveTextContent('Lambda'));
    expect(screen.getByTestId('empty-state')).toHaveAttribute('data-variant', 'no-resources');
    expect(screen.getByTestId('empty-state-message')).toHaveTextContent("hasn't been built yet");
  });

  it('renders the registered list view when one exists', async () => {
    registerServiceView('lambda', {
      list: () => <div data-testid="lambda-list">functions</div>,
    });

    renderAt('/services/lambda');

    await waitFor(() => expect(screen.getByTestId('lambda-list')).toBeInTheDocument());
  });

  it('shows a not-found state for an unknown service', async () => {
    renderAt('/services/unknown');

    await waitFor(() =>
      expect(screen.getByTestId('service-page-heading')).toHaveTextContent('Service not found'),
    );
    expect(screen.getByTestId('empty-state')).toHaveAttribute('data-variant', 'no-matches');
  });

  it('shows an error state when the catalogue cannot be loaded', async () => {
    getCatalogueMock.mockRejectedValue(new Error('offline'));

    renderAt('/services/lambda');

    await waitFor(() => expect(screen.getByTestId('empty-state')).toBeInTheDocument());
    expect(screen.getByTestId('empty-state-message')).toHaveTextContent('Unable to load this service');
  });
});
