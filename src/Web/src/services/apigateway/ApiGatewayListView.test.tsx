import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ApiGatewayListView } from './ApiGatewayListView';
import { getApiGatewayRestApis } from '../../api/client';
import type { ApiGatewayRestApiListResult } from '../../api/client';

vi.mock('../../api/client');

const getApiGatewayRestApisMock = vi.mocked(getApiGatewayRestApis);

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
  });

  afterEach(() => {
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
});
