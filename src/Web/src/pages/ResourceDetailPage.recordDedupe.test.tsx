import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { StrictMode } from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ResourceDetailPage } from './ResourceDetailPage';
import { recordRecentlyViewed, type CatalogueServiceItem } from '../api/client';
import { useCatalogueService } from '../hooks/useCatalogueService';
import { clearServiceViews, registerServiceView } from '../services/serviceViewRegistry';

vi.mock('../api/client');
vi.mock('../hooks/useCatalogueService');

const recordRecentlyViewedMock = vi.mocked(recordRecentlyViewed);
const useCatalogueServiceMock = vi.mocked(useCatalogueService);

const sqs: CatalogueServiceItem = {
  key: 'sqs',
  displayName: 'SQS',
  category: 'Messaging',
  iconHint: 'inbox',
  route: '/services/sqs',
  supported: true,
  supportDetail: null,
};

describe('ResourceDetailPage recently-viewed dedupe', () => {
  beforeEach(() => {
    recordRecentlyViewedMock.mockResolvedValue();
    // The resource is already resolved on first render so the record effect fires immediately.
    useCatalogueServiceMock.mockReturnValue({ kind: 'ready', service: sqs });
    registerServiceView('sqs', {
      detail: ({ resourceId }) => <div data-testid="sqs-detail">{resourceId}</div>,
    });
  });

  afterEach(() => {
    clearServiceViews();
    vi.clearAllMocks();
  });

  it('records a resource only once even when the record effect re-runs for the same resource', async () => {
    // StrictMode runs the effect setup twice on mount with the ref preserved, so the second run
    // exercises the same-resource guard that skips a duplicate record.
    render(
      <StrictMode>
        <MemoryRouter initialEntries={['/services/sqs/orders']}>
          <ThemeProvider colorMode="night">
            <Routes>
              <Route path="services/:serviceKey/*" element={<ResourceDetailPage />} />
            </Routes>
          </ThemeProvider>
        </MemoryRouter>
      </StrictMode>,
    );

    await waitFor(() => expect(screen.getByTestId('sqs-detail')).toBeInTheDocument());
    await waitFor(() => expect(recordRecentlyViewedMock).toHaveBeenCalledTimes(1));
    expect(recordRecentlyViewedMock).toHaveBeenCalledWith('sqs://orders');
  });
});
