import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ApiGatewayListView } from './ApiGatewayListView';
import {
  getApiGatewayRestApis,
  createApiGatewayRestApi,
  deleteApiGatewayRestApi,
} from '../../api/client';
import type { ApiGatewayRestApiListResult } from '../../api/client';

vi.mock('../../api/client');

const getApiGatewayRestApisMock = vi.mocked(getApiGatewayRestApis);
const createApiGatewayRestApiMock = vi.mocked(createApiGatewayRestApi);
const deleteApiGatewayRestApiMock = vi.mocked(deleteApiGatewayRestApi);

const result: ApiGatewayRestApiListResult = {
  restApis: [
    {
      id: 'api-1',
      name: 'orders-api',
      description: 'Orders service',
      createdDate: '2024-01-01T00:00:00+00:00',
    },
    {
      id: 'api-2',
      name: 'audit-api',
      description: null,
      createdDate: null,
    },
    {
      id: 'api-3',
      name: 'broken-api',
      description: 'Has an unparseable date',
      createdDate: 'not-a-date',
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <ApiGatewayListView serviceKey="apigateway" />
    </MemoryRouter>,
  );
}

describe('ApiGatewayListView', () => {
  beforeEach(() => {
    getApiGatewayRestApisMock.mockResolvedValue(result);
    createApiGatewayRestApiMock.mockResolvedValue({ id: 'new-id' });
    deleteApiGatewayRestApiMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before REST APIs arrive', () => {
    getApiGatewayRestApisMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('apigateway-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getApiGatewayRestApisMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-error')).toBeInTheDocument(),
    );
  });

  it('renders a row per REST API', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('data-list-row-api-1')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-api-2')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-api-3')).toBeInTheDocument();
  });

  it('formats the created date and falls back when missing or unparseable', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    const names = screen.getAllByTestId('apigateway-list-name');
    expect(names[0]).toHaveTextContent('orders-api');
    const descriptions = screen.getAllByTestId('apigateway-list-description');
    expect(descriptions[0]).toHaveTextContent('Orders service');
    expect(screen.getByTestId('apigateway-list-description-empty')).toBeInTheDocument();

    const created = screen.getAllByTestId('apigateway-list-created');
    expect(created[0]).toHaveTextContent('2024-01-01T00:00:00.000Z');
    expect(created[1]).toHaveTextContent('not-a-date');
    expect(screen.getByTestId('apigateway-list-created-empty')).toBeInTheDocument();
  });

  it('reloads the REST APIs when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
      );
      expect(getApiGatewayRestApisMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getApiGatewayRestApisMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('links each REST API name to its detail route', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    const names = screen.getAllByTestId('apigateway-list-name');
    expect(names[0]).toHaveAttribute('href', '/services/apigateway/api-1');
  });

  it('creates a REST API from the form and refreshes the list', async () => {
    createApiGatewayRestApiMock.mockResolvedValue({ id: 'new123' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-create-toggle'));

    fireEvent.change(screen.getByTestId('apigateway-create-name'), {
      target: { value: 'new-api' },
    });
    fireEvent.change(screen.getByTestId('apigateway-create-description'), {
      target: { value: 'my api' },
    });
    fireEvent.change(screen.getByTestId('apigateway-create-version'), {
      target: { value: '1.0' },
    });
    fireEvent.change(screen.getByTestId('apigateway-create-endpoint-type'), {
      target: { value: 'EDGE' },
    });

    fireEvent.click(screen.getByTestId('apigateway-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-create-status')).toBeInTheDocument(),
    );

    expect(createApiGatewayRestApiMock).toHaveBeenCalledWith({
      name: 'new-api',
      description: 'my api',
      version: '1.0',
      apiKeySource: null,
      endpointConfigurationTypes: ['EDGE'],
    });
    await waitFor(() =>
      expect(getApiGatewayRestApisMock).toHaveBeenCalledTimes(2),
    );
    expect(screen.queryByTestId('apigateway-create-form')).not.toBeInTheDocument();
  });

  it('sends null for blank optional fields when creating a REST API', async () => {
    createApiGatewayRestApiMock.mockResolvedValue({ id: 'new123' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-create-toggle'));
    fireEvent.change(screen.getByTestId('apigateway-create-name'), {
      target: { value: 'minimal' },
    });
    fireEvent.click(screen.getByTestId('apigateway-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-create-status')).toBeInTheDocument(),
    );

    expect(createApiGatewayRestApiMock).toHaveBeenCalledWith({
      name: 'minimal',
      description: null,
      version: null,
      apiKeySource: null,
      endpointConfigurationTypes: ['REGIONAL'],
    });
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-create-toggle'));
    expect(screen.getByTestId('apigateway-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigateway-create-toggle'));
    expect(screen.queryByTestId('apigateway-create-form')).not.toBeInTheDocument();
  });

  it('shows an error when REST API creation fails', async () => {
    createApiGatewayRestApiMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-create-toggle'));
    fireEvent.click(screen.getByTestId('apigateway-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-create-error')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-create-form')).toBeInTheDocument();
  });

  it('deletes a REST API after confirmation and refreshes the list', async () => {
    deleteApiGatewayRestApiMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteApiGatewayRestApiMock).toHaveBeenCalledWith('api-1'));
    await waitFor(() => expect(getApiGatewayRestApisMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when REST API deletion fails', async () => {
    deleteApiGatewayRestApiMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-list-error')).toBeInTheDocument(),
    );
  });
});
