import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ApiGatewayTestInvokeSection } from './ApiGatewayTestInvokeSection';
import {
  getApiGatewayRestResources,
  testInvokeApiGatewayRestMethod,
} from '../../api/client';

vi.mock('../../api/client');

const getApiGatewayRestResourcesMock = vi.mocked(getApiGatewayRestResources);
const testInvokeApiGatewayRestMethodMock = vi.mocked(testInvokeApiGatewayRestMethod);

function renderSection() {
  return render(<ApiGatewayTestInvokeSection restApiId="api-1" />);
}

describe('ApiGatewayTestInvokeSection', () => {
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
    testInvokeApiGatewayRestMethodMock.mockResolvedValue({
      statusCode: 200,
      latencyMilliseconds: 18,
      headers: { 'Content-Type': 'application/json' },
      body: '{"ok":true}',
      log: 'execution log',
    });
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows loading and then renders invoke controls', async () => {
    renderSection();

    expect(screen.getByTestId('apigateway-test-invoke-loading')).toBeInTheDocument();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-submit')).toBeInTheDocument(),
    );

    expect(getApiGatewayRestResourcesMock).toHaveBeenCalledWith('api-1', expect.anything());
  });

  it('submits invoke request and displays response details', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-submit')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigateway-test-invoke-resource'), {
      target: { value: 'res-orders' },
    });
    fireEvent.change(screen.getByTestId('apigateway-test-invoke-method'), {
      target: { value: 'POST' },
    });
    fireEvent.change(screen.getByTestId('apigateway-test-invoke-path'), {
      target: { value: '/orders?limit=5' },
    });
    fireEvent.change(screen.getByTestId('apigateway-test-invoke-headers'), {
      target: { value: 'Content-Type: application/json\nAccept: application/json' },
    });
    fireEvent.change(screen.getByTestId('apigateway-test-invoke-query'), {
      target: { value: 'debug: true' },
    });
    fireEvent.change(screen.getByTestId('apigateway-test-invoke-stage-variables'), {
      target: { value: 'env: local' },
    });
    fireEvent.change(screen.getByTestId('apigateway-test-invoke-body'), {
      target: { value: '{"orderId":"123"}' },
    });

    fireEvent.click(screen.getByTestId('apigateway-test-invoke-submit'));

    await waitFor(() =>
      expect(testInvokeApiGatewayRestMethodMock).toHaveBeenCalledWith(
        'api-1',
        'res-orders',
        'POST',
        {
          pathWithQueryString: '/orders?limit=5',
          headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
          queryStringParameters: { debug: 'true' },
          body: '{"orderId":"123"}',
          stageVariables: { env: 'local' },
        },
      ),
    );

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-result')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-test-invoke-status')).toHaveTextContent('200');
    expect(screen.getByTestId('apigateway-test-invoke-latency')).toHaveTextContent('18');
    expect(screen.getByTestId('apigateway-test-invoke-response-body')).toHaveTextContent(
      '{"ok":true}',
    );
  });

  it('shows an error when invocation fails', async () => {
    testInvokeApiGatewayRestMethodMock.mockRejectedValue(new Error('boom'));

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-submit')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigateway-test-invoke-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-error')).toBeInTheDocument(),
    );
  });

  it('ignores header and query lines without a separator and renders empty response sections', async () => {
    testInvokeApiGatewayRestMethodMock.mockResolvedValue({
      statusCode: 204,
      latencyMilliseconds: 5,
      headers: {},
      body: '',
      log: '',
    });

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-submit')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigateway-test-invoke-headers'), {
      target: { value: 'novalue\nAccept: application/json' },
    });

    fireEvent.click(screen.getByTestId('apigateway-test-invoke-submit'));

    await waitFor(() =>
      expect(testInvokeApiGatewayRestMethodMock).toHaveBeenCalledWith(
        'api-1',
        'res-root',
        'GET',
        expect.objectContaining({ headers: { Accept: 'application/json' } }),
      ),
    );

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-result')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-test-invoke-response-body')).toHaveTextContent('');
    expect(screen.getByTestId('apigateway-test-invoke-response-log')).toHaveTextContent('');
  });

  it('falls back to defaults when the first resource has no methods or path', async () => {
    getApiGatewayRestResourcesMock.mockResolvedValue({
      resources: [
        {
          id: 'res-empty',
          parentId: null,
          pathPart: null,
          path: '',
          resourceMethods: [],
        },
      ],
    });

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-submit')).toBeInTheDocument(),
    );

    expect(
      screen.getByTestId('apigateway-test-invoke-method').querySelectorAll('option'),
    ).toHaveLength(0);
    expect(screen.getByTestId('apigateway-test-invoke-path')).toHaveValue('/');
    expect(screen.getByTestId('apigateway-test-invoke-submit')).toBeDisabled();
  });

  it('shows a load error when resources cannot be retrieved', async () => {
    getApiGatewayRestResourcesMock.mockRejectedValue(new Error('load failed'));

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-load-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty state when there are no resources', async () => {
    getApiGatewayRestResourcesMock.mockResolvedValue({ resources: [] });

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-test-invoke-empty')).toBeInTheDocument(),
    );
  });
});
