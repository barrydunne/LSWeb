import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { LambdaLayersTab } from './LambdaLayersTab';
import { getLambdaLayers, resolveReference } from '../../api/client';
import type { LambdaLayerItem } from '../../api/client';

vi.mock('../../api/client');

const getLayersMock = vi.mocked(getLambdaLayers);
const resolveReferenceMock = vi.mocked(resolveReference);

const layer: LambdaLayerItem = {
  arn: 'arn:aws:lambda:eu-west-1:123456789012:layer:shared-utils:7',
  name: 'shared-utils',
  version: '7',
};

function renderTab() {
  return render(
    <MemoryRouter>
      <LambdaLayersTab functionName="process-orders" />
    </MemoryRouter>,
  );
}

describe('LambdaLayersTab', () => {
  beforeEach(() => {
    resolveReferenceMock.mockRejectedValue(new Error('unresolved'));
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before layers arrive', () => {
    getLayersMock.mockReturnValue(new Promise(() => {}));

    renderTab();

    expect(screen.getByTestId('lambda-layers-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getLayersMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-layers-error')).toBeInTheDocument());
  });

  it('shows an empty state when there are no layers', async () => {
    getLayersMock.mockResolvedValue({ layers: [] });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-layers-empty')).toBeInTheDocument());
    expect(getLayersMock).toHaveBeenCalledWith('process-orders', expect.anything());
  });

  it('renders attached layers with their details', async () => {
    getLayersMock.mockResolvedValue({ layers: [layer] });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-layers-tab')).toBeInTheDocument());

    const card = screen.getByTestId(`lambda-layer-${layer.arn}`);
    expect(card).toHaveTextContent('shared-utils');
    expect(card).toHaveTextContent('7');
  });
});
