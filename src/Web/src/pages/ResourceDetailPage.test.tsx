import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ResourceDetailPage } from './ResourceDetailPage';
import { getCatalogue, recordRecentlyViewed, type CatalogueServiceItem } from '../api/client';
import {
  clearServiceViews,
  registerServiceView,
} from '../services/serviceViewRegistry';

vi.mock('../api/client');

const getCatalogueMock = vi.mocked(getCatalogue);
const recordRecentlyViewedMock = vi.mocked(recordRecentlyViewed);

const sqs: CatalogueServiceItem = {
  key: 'sqs',
  displayName: 'SQS',
  category: 'Messaging',
  iconHint: 'inbox',
  route: '/services/sqs',
  supported: true,
  supportDetail: null,
};

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <ThemeProvider colorMode="night">
        <Routes>
          <Route path="services/:serviceKey/*" element={<ResourceDetailPage />} />
        </Routes>
      </ThemeProvider>
    </MemoryRouter>,
  );
}

describe('ResourceDetailPage', () => {
  beforeEach(() => {
    getCatalogueMock.mockResolvedValue({ services: [sqs] });
    recordRecentlyViewedMock.mockResolvedValue();
  });

  afterEach(() => {
    clearServiceViews();
    vi.clearAllMocks();
  });

  it('shows a not-built placeholder when no detail view is registered', async () => {
    renderAt('/services/sqs/orders');

    await waitFor(() => expect(screen.getByTestId('resource-detail-heading')).toHaveTextContent('SQS'));
    expect(screen.getByTestId('empty-state-message')).toHaveTextContent("hasn't been built yet");
  });

  it('passes the decoded splat resource id into the registered detail view', async () => {
    registerServiceView('sqs', {
      detail: ({ resourceId }) => <div data-testid="sqs-detail">{resourceId}</div>,
    });

    renderAt('/services/sqs/team%2Forders');

    await waitFor(() => expect(screen.getByTestId('sqs-detail')).toHaveTextContent('team/orders'));
  });

  it('records the resource as recently viewed once it resolves', async () => {
    registerServiceView('sqs', {
      detail: ({ resourceId }) => <div data-testid="sqs-detail">{resourceId}</div>,
    });

    renderAt('/services/sqs/orders');

    await waitFor(() => expect(screen.getByTestId('sqs-detail')).toBeInTheDocument());
    await waitFor(() =>
      expect(recordRecentlyViewedMock).toHaveBeenCalledWith('sqs://orders', expect.any(AbortSignal)),
    );
  });

  it('ignores a failure to record the recently-viewed resource', async () => {
    recordRecentlyViewedMock.mockRejectedValue(new Error('boom'));
    registerServiceView('sqs', {
      detail: ({ resourceId }) => <div data-testid="sqs-detail">{resourceId}</div>,
    });

    renderAt('/services/sqs/orders');

    await waitFor(() => expect(screen.getByTestId('sqs-detail')).toBeInTheDocument());
    await waitFor(() => expect(recordRecentlyViewedMock).toHaveBeenCalledTimes(1));
    expect(screen.getByTestId('sqs-detail')).toHaveTextContent('orders');
  });

  it('does not record a recently-viewed visit for an unknown service', async () => {
    renderAt('/services/unknown/abc');

    await waitFor(() =>
      expect(screen.getByTestId('resource-detail-heading')).toHaveTextContent('Resource not found'),
    );
    expect(recordRecentlyViewedMock).not.toHaveBeenCalled();
  });

  it('shows a not-found state for an unknown service', async () => {
    renderAt('/services/unknown/abc');

    await waitFor(() =>
      expect(screen.getByTestId('resource-detail-heading')).toHaveTextContent('Resource not found'),
    );
  });

  it('shows an error state when the catalogue request fails', async () => {
    getCatalogueMock.mockRejectedValue(new Error('boom'));

    renderAt('/services/sqs/orders');

    await waitFor(() =>
      expect(screen.getByTestId('empty-state-message')).toHaveTextContent('Unable to load this resource'),
    );
  });

  it('treats a missing splat segment as an empty resource id', async () => {
    registerServiceView('sqs', {
      detail: ({ resourceId }) => <div data-testid="sqs-detail">[{resourceId}]</div>,
    });

    render(
      <MemoryRouter initialEntries={['/services/sqs']}>
        <ThemeProvider colorMode="night">
          <Routes>
            <Route path="services/:serviceKey" element={<ResourceDetailPage />} />
          </Routes>
        </ThemeProvider>
      </MemoryRouter>,
    );

    await waitFor(() => expect(screen.getByTestId('sqs-detail')).toHaveTextContent('[]'));
    expect(recordRecentlyViewedMock).not.toHaveBeenCalled();
  });

  it('treats a missing service key param as an empty key', async () => {
    render(
      <MemoryRouter initialEntries={['/orphan']}>
        <ThemeProvider colorMode="night">
          <Routes>
            <Route path="orphan" element={<ResourceDetailPage />} />
          </Routes>
        </ThemeProvider>
      </MemoryRouter>,
    );

    await waitFor(() =>
      expect(screen.getByTestId('resource-detail-heading')).toHaveTextContent('Resource not found'),
    );
  });
});
