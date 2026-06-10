import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ApiGatewayCorsSection } from './ApiGatewayCorsSection';
import {
  configureApiGatewayRestCors,
  getApiGatewayRestCors,
  getApiGatewayRestResources,
} from '../../api/client';

vi.mock('../../api/client');

const getApiGatewayRestResourcesMock = vi.mocked(getApiGatewayRestResources);
const getApiGatewayRestCorsMock = vi.mocked(getApiGatewayRestCors);
const configureApiGatewayRestCorsMock = vi.mocked(configureApiGatewayRestCors);

function renderSection() {
  return render(<ApiGatewayCorsSection restApiId="api-1" />);
}

describe('ApiGatewayCorsSection', () => {
  beforeEach(() => {
    getApiGatewayRestResourcesMock.mockResolvedValue({
      resources: [
        {
          id: 'res-root',
          parentId: null,
          pathPart: null,
          path: '/',
          resourceMethods: ['GET'],
        },
        {
          id: 'res-orders',
          parentId: 'res-root',
          pathPart: 'orders',
          path: '/orders',
          resourceMethods: ['GET', 'POST'],
        },
      ],
    });
    getApiGatewayRestCorsMock.mockResolvedValue({
      resourceId: 'res-root',
      enabled: false,
      allowOrigins: [],
      allowMethods: [],
      allowHeaders: [],
    });
    configureApiGatewayRestCorsMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows loading and then renders the CORS form', async () => {
    renderSection();

    expect(screen.getByTestId('apigateway-cors-loading')).toBeInTheDocument();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-save')).toBeInTheDocument(),
    );

    expect(getApiGatewayRestResourcesMock).toHaveBeenCalledWith('api-1', expect.anything());
    await waitFor(() =>
      expect(getApiGatewayRestCorsMock).toHaveBeenCalledWith(
        'api-1',
        'res-root',
        expect.anything(),
      ),
    );
    expect(screen.getByTestId('apigateway-cors-status')).toHaveTextContent(
      'CORS is not configured for this resource.',
    );
  });

  it('renders the load error when resources cannot be loaded', async () => {
    getApiGatewayRestResourcesMock.mockRejectedValue(new Error('boom'));
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-load-error')).toBeInTheDocument(),
    );
  });

  it('renders the empty state when there are no resources', async () => {
    getApiGatewayRestResourcesMock.mockResolvedValue({ resources: [] });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-empty')).toBeInTheDocument(),
    );
    expect(getApiGatewayRestCorsMock).not.toHaveBeenCalled();
  });

  it('populates fields from an enabled CORS configuration', async () => {
    getApiGatewayRestCorsMock.mockResolvedValue({
      resourceId: 'res-root',
      enabled: true,
      allowOrigins: ['*'],
      allowMethods: ['GET', 'POST'],
      allowHeaders: ['Content-Type'],
    });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-status')).toHaveTextContent(
        'CORS is enabled for this resource.',
      ),
    );
    expect(screen.getByTestId('apigateway-cors-allow-origins')).toHaveValue('*');
    expect(screen.getByTestId('apigateway-cors-allow-methods')).toHaveValue('GET, POST');
    expect(screen.getByTestId('apigateway-cors-allow-headers')).toHaveValue('Content-Type');
  });

  it('shows the config error when the CORS configuration cannot be loaded', async () => {
    getApiGatewayRestCorsMock.mockRejectedValue(new Error('nope'));
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-config-error')).toBeInTheDocument(),
    );
  });

  it('reloads CORS when a different resource is selected', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-save')).toBeInTheDocument(),
    );
    getApiGatewayRestCorsMock.mockClear();

    fireEvent.change(screen.getByTestId('apigateway-cors-resource'), {
      target: { value: 'res-orders' },
    });

    await waitFor(() =>
      expect(getApiGatewayRestCorsMock).toHaveBeenCalledWith(
        'api-1',
        'res-orders',
        expect.anything(),
      ),
    );
  });

  it('applies the permissive preset to the inputs', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-preset')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-cors-preset'));

    expect(screen.getByTestId('apigateway-cors-allow-origins')).toHaveValue('*');
    expect(screen.getByTestId('apigateway-cors-allow-methods')).toHaveValue(
      'GET, POST, PUT, DELETE, OPTIONS',
    );
    expect(screen.getByTestId('apigateway-cors-allow-headers')).toHaveValue(
      'Content-Type, Authorization, X-Amz-Date, X-Api-Key, X-Amz-Security-Token',
    );
  });

  it('saves the CORS configuration and shows success', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-save')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigateway-cors-allow-origins'), {
      target: { value: 'https://app.example.com, ' },
    });
    fireEvent.change(screen.getByTestId('apigateway-cors-allow-methods'), {
      target: { value: 'GET, POST' },
    });
    fireEvent.change(screen.getByTestId('apigateway-cors-allow-headers'), {
      target: { value: 'Content-Type' },
    });
    fireEvent.click(screen.getByTestId('apigateway-cors-save'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-save-success')).toBeInTheDocument(),
    );
    expect(configureApiGatewayRestCorsMock).toHaveBeenCalledWith('api-1', 'res-root', {
      allowOrigins: ['https://app.example.com'],
      allowMethods: ['GET', 'POST'],
      allowHeaders: ['Content-Type'],
    });
    expect(screen.getByTestId('apigateway-cors-status')).toHaveTextContent(
      'CORS is enabled for this resource.',
    );
  });

  it('shows the save error when the configuration request fails', async () => {
    configureApiGatewayRestCorsMock.mockRejectedValue(new Error('fail'));
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-save')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-cors-save'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-cors-save-error')).toBeInTheDocument(),
    );
  });
});
