import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ApiGatewayDetailView } from './ApiGatewayDetailView';
import {
  getApiGatewayRestApi,
  getApiGatewayRestResources,
  updateApiGatewayRestApi,
} from '../../api/client';
import type { ApiGatewayRestApiDetailResult } from '../../api/client';

vi.mock('../../api/client');

const getApiGatewayRestApiMock = vi.mocked(getApiGatewayRestApi);
const updateApiGatewayRestApiMock = vi.mocked(updateApiGatewayRestApi);
const getApiGatewayRestResourcesMock = vi.mocked(getApiGatewayRestResources);

const detailResult: ApiGatewayRestApiDetailResult = {
  id: 'api-1',
  name: 'orders-api',
  description: 'Orders service',
  version: '1.0',
  apiKeySource: 'HEADER',
  endpointConfigurationTypes: ['REGIONAL'],
  binaryMediaTypes: ['application/octet-stream'],
  createdDate: '2024-01-01T00:00:00+00:00',
};

const emptyDetailResult: ApiGatewayRestApiDetailResult = {
  id: 'api-2',
  name: 'audit-api',
  description: null,
  version: null,
  apiKeySource: null,
  endpointConfigurationTypes: [],
  binaryMediaTypes: [],
  createdDate: null,
};

function renderView() {
  return render(<ApiGatewayDetailView serviceKey="apigateway" resourceId="api-1" />);
}

describe('ApiGatewayDetailView', () => {
  beforeEach(() => {
    getApiGatewayRestApiMock.mockResolvedValue(detailResult);
    updateApiGatewayRestApiMock.mockResolvedValue();
    getApiGatewayRestResourcesMock.mockResolvedValue({ resources: [] });
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before the REST API arrives', () => {
    getApiGatewayRestApiMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('apigateway-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getApiGatewayRestApiMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-detail-error')).toBeInTheDocument(),
    );
  });

  it('renders the REST API detail fields', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-detail-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigateway-detail-name')).toHaveTextContent('orders-api');
    expect(screen.getByTestId('apigateway-detail-id')).toHaveTextContent('api-1');
    expect(screen.getByTestId('apigateway-detail-description')).toHaveTextContent(
      'Orders service',
    );
    expect(screen.getByTestId('apigateway-detail-version')).toHaveTextContent('1.0');
    expect(screen.getByTestId('apigateway-detail-api-key-source')).toHaveTextContent(
      'HEADER',
    );
    expect(screen.getByTestId('apigateway-detail-endpoint-types')).toHaveTextContent(
      'REGIONAL',
    );
    expect(screen.getByTestId('apigateway-detail-binary-media-types')).toHaveTextContent(
      'application/octet-stream',
    );
    expect(screen.getByTestId('apigateway-detail-created')).toHaveTextContent(
      '2024-01-01T00:00:00+00:00',
    );
  });

  it('renders the embedded resources section', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resources-section')).toBeInTheDocument(),
    );

    expect(getApiGatewayRestResourcesMock).toHaveBeenCalledWith('api-1', expect.anything());
  });

  it('falls back to a dash for missing optional fields', async () => {
    getApiGatewayRestApiMock.mockResolvedValue(emptyDetailResult);

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-detail-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigateway-detail-description')).toHaveTextContent('—');
    expect(screen.getByTestId('apigateway-detail-version')).toHaveTextContent('—');
    expect(screen.getByTestId('apigateway-detail-api-key-source')).toHaveTextContent('—');
    expect(screen.getByTestId('apigateway-detail-endpoint-types')).toHaveTextContent('—');
    expect(screen.getByTestId('apigateway-detail-binary-media-types')).toHaveTextContent(
      '—',
    );
    expect(screen.getByTestId('apigateway-detail-created')).toHaveTextContent('—');
  });

  it('updates the REST API name and description and reloads', async () => {
    updateApiGatewayRestApiMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-detail-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-edit-toggle'));

    fireEvent.change(screen.getByTestId('apigateway-edit-name'), {
      target: { value: 'renamed-api' },
    });
    fireEvent.change(screen.getByTestId('apigateway-edit-description'), {
      target: { value: 'updated description' },
    });

    fireEvent.click(screen.getByTestId('apigateway-edit-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-edit-status')).toBeInTheDocument(),
    );

    expect(updateApiGatewayRestApiMock).toHaveBeenCalledWith('api-1', {
      name: 'renamed-api',
      description: 'updated description',
    });
    await waitFor(() =>
      expect(getApiGatewayRestApiMock).toHaveBeenCalledTimes(2),
    );
    expect(screen.queryByTestId('apigateway-edit-form')).not.toBeInTheDocument();
  });

  it('sends null when the description is cleared on update', async () => {
    updateApiGatewayRestApiMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-detail-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-edit-toggle'));
    fireEvent.change(screen.getByTestId('apigateway-edit-description'), {
      target: { value: '' },
    });
    fireEvent.click(screen.getByTestId('apigateway-edit-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-edit-status')).toBeInTheDocument(),
    );

    expect(updateApiGatewayRestApiMock).toHaveBeenCalledWith('api-1', {
      name: 'orders-api',
      description: null,
    });
  });

  it('prefills an empty description when starting to edit an API without one', async () => {
    getApiGatewayRestApiMock.mockResolvedValue(emptyDetailResult);

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-detail-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-edit-toggle'));

    expect(screen.getByTestId('apigateway-edit-description')).toHaveValue('');
  });

  it('hides the edit form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-detail-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-edit-toggle'));
    expect(screen.getByTestId('apigateway-edit-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigateway-edit-toggle'));
    expect(screen.queryByTestId('apigateway-edit-form')).not.toBeInTheDocument();
  });

  it('shows an error when the update fails', async () => {
    updateApiGatewayRestApiMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-detail-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-edit-toggle'));
    fireEvent.click(screen.getByTestId('apigateway-edit-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-edit-error')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-edit-form')).toBeInTheDocument();
  });
});
