import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ResourceDetailPage } from './ResourceDetailPage';
import { getCatalogue, type CatalogueServiceItem } from '../api/client';
import {
  clearServiceViews,
  registerServiceView,
} from '../services/serviceViewRegistry';

vi.mock('../api/client');

const getCatalogueMock = vi.mocked(getCatalogue);

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
  });

  afterEach(() => {
    clearServiceViews();
    vi.resetAllMocks();
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

  it('shows a not-found state for an unknown service', async () => {
    renderAt('/services/unknown/abc');

    await waitFor(() =>
      expect(screen.getByTestId('resource-detail-heading')).toHaveTextContent('Resource not found'),
    );
  });
});
