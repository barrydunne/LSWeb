import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ApiGatewayV2ListView } from './ApiGatewayV2ListView';
import { createHttpApi, deleteHttpApi, getHttpApis } from '../../api/client';
import type { HttpApiListResult } from '../../api/client';

vi.mock('../../api/client');

const getHttpApisMock = vi.mocked(getHttpApis);
const createHttpApiMock = vi.mocked(createHttpApi);
const deleteHttpApiMock = vi.mocked(deleteHttpApi);

const listResult: HttpApiListResult = {
  apis: [
    {
      apiId: 'abc123',
      name: 'orders',
      protocolType: 'HTTP',
      apiEndpoint: 'https://abc123.execute-api.localhost',
      createdDate: '2024-01-01T00:00:00+00:00',
    },
    {
      apiId: 'def456',
      name: 'events',
      protocolType: 'WEBSOCKET',
      apiEndpoint: null,
      createdDate: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <ApiGatewayV2ListView serviceKey="apigatewayv2" />
    </MemoryRouter>,
  );
}

describe('ApiGatewayV2ListView', () => {
  beforeEach(() => {
    getHttpApisMock.mockResolvedValue(listResult);
  });

  afterEach(() => {
    cleanup();
    vi.useRealTimers();
    vi.resetAllMocks();
  });

  it('shows a loading state before the APIs arrive', () => {
    getHttpApisMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('apigatewayv2-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getHttpApisMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-error')).toBeInTheDocument());
  });

  it('renders a row per API', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    expect(screen.getByTestId('data-list-row-abc123')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-def456')).toBeInTheDocument();
  });

  it('shows the name, id, protocol, and creation date for each API', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    const names = screen.getAllByTestId('apigatewayv2-list-name');
    const ids = screen.getAllByTestId('apigatewayv2-list-id');
    const protocols = screen.getAllByTestId('apigatewayv2-list-protocol');
    const created = screen.getAllByTestId('apigatewayv2-list-created');
    expect(names[0]).toHaveTextContent('orders');
    expect(ids[0]).toHaveTextContent('abc123');
    expect(protocols[0]).toHaveTextContent('HTTP');
    expect(protocols[1]).toHaveTextContent('WEBSOCKET');
    expect(created[0]).toHaveTextContent('2024-01-01T00:00:00+00:00');
    expect(created[1]).toHaveTextContent('—');
  });

  it('links each API name to its id-keyed detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    const names = screen.getAllByTestId('apigatewayv2-list-name');
    expect(names[0]).toHaveAttribute('href', '/services/apigatewayv2/abc123');
  });

  it('reloads the APIs when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());
      expect(getHttpApisMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      await act(async () => {
        await vi.advanceTimersByTimeAsync(5000);
      });

      await vi.waitFor(() => expect(getHttpApisMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('creates an API from the form and refreshes the list', async () => {
    createHttpApiMock.mockResolvedValue({ apiId: 'new123' });

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('apigatewayv2-create-toggle'));

    fireEvent.change(screen.getByTestId('apigatewayv2-create-name'), {
      target: { value: 'new-api' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-create-protocol'), {
      target: { value: 'WEBSOCKET' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-create-description'), {
      target: { value: 'my api' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-create-version'), {
      target: { value: '1.0' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-create-route-selection'), {
      target: { value: '$request.method' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-create-submit'));

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-create-status')).toBeInTheDocument());

    expect(createHttpApiMock).toHaveBeenCalledWith({
      name: 'new-api',
      protocolType: 'WEBSOCKET',
      description: 'my api',
      version: '1.0',
      routeSelectionExpression: '$request.method',
    });
    await waitFor(() => expect(getHttpApisMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('apigatewayv2-create-form')).not.toBeInTheDocument();
  });

  it('sends null for blank optional fields when creating an API', async () => {
    createHttpApiMock.mockResolvedValue({ apiId: 'new123' });

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('apigatewayv2-create-toggle'));
    fireEvent.change(screen.getByTestId('apigatewayv2-create-name'), {
      target: { value: 'minimal' },
    });
    fireEvent.click(screen.getByTestId('apigatewayv2-create-submit'));

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-create-status')).toBeInTheDocument());

    expect(createHttpApiMock).toHaveBeenCalledWith({
      name: 'minimal',
      protocolType: 'HTTP',
      description: null,
      version: null,
      routeSelectionExpression: null,
    });
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('apigatewayv2-create-toggle'));
    expect(screen.getByTestId('apigatewayv2-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigatewayv2-create-toggle'));
    expect(screen.queryByTestId('apigatewayv2-create-form')).not.toBeInTheDocument();
  });

  it('shows an error when API creation fails', async () => {
    createHttpApiMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('apigatewayv2-create-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-create-submit'));

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-create-error')).toBeInTheDocument());
    expect(screen.getByTestId('apigatewayv2-create-form')).toBeInTheDocument();
  });

  it('deletes an API after confirmation and refreshes the list', async () => {
    deleteHttpApiMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteHttpApiMock).toHaveBeenCalledWith('abc123'));
    await waitFor(() => expect(getHttpApisMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when API deletion fails', async () => {
    deleteHttpApiMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-list-error')).toBeInTheDocument());
  });
});
