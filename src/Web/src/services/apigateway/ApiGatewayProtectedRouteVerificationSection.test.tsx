import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import * as clientModule from '../../api/client';
import type {
  ApiGatewayRestResourceItem,
  ApiGatewayRestMethodDetailResult,
  ApiGatewayRestMethodTestInvokeResult,
} from '../../api/client';
import { ApiGatewayProtectedRouteVerificationSection } from './ApiGatewayProtectedRouteVerificationSection';

vi.mock('../../api/client');

describe('ApiGatewayProtectedRouteVerificationSection', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders heading', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [],
    });

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-heading')).toBeInTheDocument();
    });
  });

  it('shows loading message while discovering routes', () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockImplementation(() => new Promise(() => {}));

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    expect(screen.getByTestId('apigateway-protected-route-verification-loading')).toBeInTheDocument();
  });

  it('shows empty message when no protected methods exist', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/test',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'NONE',
    } as unknown as ApiGatewayRestMethodDetailResult);

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(
        screen.getByTestId('apigateway-protected-route-verification-empty')
      ).toBeInTheDocument();
    });
  });

  it('discovers protected methods and populates dropdown', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/users',
          parentId: null,
          resourceMethods: ['GET', 'POST'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockImplementation((_api, _resourceId, method) => {
      if (method === 'POST') {
        return Promise.resolve({
          authorizationType: 'COGNITO_USER_POOLS',
          authorizerId: 'auth1',
        } as unknown as ApiGatewayRestMethodDetailResult);
      }
      return Promise.resolve({
        authorizationType: 'NONE',
      } as unknown as ApiGatewayRestMethodDetailResult);
    });

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      const select = screen.getByTestId('apigateway-protected-route-verification-method');
      expect(select).toBeInTheDocument();
    });
  });

  it('auto-selects first protected method', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/secure',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'AWS_IAM',
    } as unknown as ApiGatewayRestMethodDetailResult);

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      const select = screen.getByTestId(
        'apigateway-protected-route-verification-method'
      ) as HTMLSelectElement;
      expect(select.value).toBe('0');
    });
  });

  it('displays authorization header with default value', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/api',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'COGNITO_USER_POOLS',
    } as unknown as ApiGatewayRestMethodDetailResult);

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      const input = screen.getByTestId(
        'apigateway-protected-route-verification-token'
      ) as HTMLInputElement;
      expect(input.value).toBe('Bearer mock-token-12345');
    });
  });

  it('verifies authorization with 401 and 200', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/api/data',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'COGNITO_USER_POOLS',
      authorizerId: 'auth1',
    } as unknown as ApiGatewayRestMethodDetailResult);
    mockClient.testInvokeApiGatewayRestMethod.mockImplementation((_api, _resourceId, _method, options) => {
      if (!options.headers || !options.headers['Authorization']) {
        return Promise.resolve({
          statusCode: 401,
          latencyMilliseconds: 12,
          headers: {},
          body: 'Unauthorized',
          log: '',
        } as unknown as ApiGatewayRestMethodTestInvokeResult);
      }
      return Promise.resolve({
        statusCode: 200,
        latencyMilliseconds: 45,
        headers: { 'Content-Type': 'application/json' },
        body: '{"data":"test"}',
        log: 'Auth check passed',
      } as unknown as ApiGatewayRestMethodTestInvokeResult);
    });

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-submit')).not.toBeDisabled();
    });

    const button = screen.getByTestId('apigateway-protected-route-verification-submit');
    fireEvent.click(button);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-results')).toBeInTheDocument();
    });

    expect(screen.getByText(/Auth enforced/)).toBeInTheDocument();
    expect(screen.getByText(/Request forwarded/)).toBeInTheDocument();
  });

  it('verifies authorization with 403 and error', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/admin',
          parentId: null,
          resourceMethods: ['DELETE'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'AWS_IAM',
    } as unknown as ApiGatewayRestMethodDetailResult);
    mockClient.testInvokeApiGatewayRestMethod.mockImplementation((_api, _resourceId, _method, options) => {
      if (!options.headers || !options.headers['Authorization']) {
        return Promise.resolve({
          statusCode: 403,
          latencyMilliseconds: 8,
          headers: {},
          body: 'Forbidden',
          log: '',
        } as unknown as ApiGatewayRestMethodTestInvokeResult);
      }
      return Promise.reject(new Error('Invocation error'));
    });

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-submit')).not.toBeDisabled();
    });

    const button = screen.getByTestId('apigateway-protected-route-verification-submit');
    fireEvent.click(button);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-results')).toBeInTheDocument();
    });

    expect(screen.getByText(/Auth enforced/)).toBeInTheDocument();
    expect(screen.getByText(/Request error \(unexpected\)/)).toBeInTheDocument();
  });

  it('verify button is enabled when protected method exists', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/api',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'COGNITO_USER_POOLS',
    } as unknown as ApiGatewayRestMethodDetailResult);

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      const button = screen.getByTestId('apigateway-protected-route-verification-submit');
      expect(button).not.toBeDisabled();
    });
  });

  it('disables verify button during verification', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/api',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'COGNITO_USER_POOLS',
    } as unknown as ApiGatewayRestMethodDetailResult);
    mockClient.testInvokeApiGatewayRestMethod.mockImplementation(
      () => new Promise(() => {})
    );

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-submit')).not.toBeDisabled();
    });

    const button = screen.getByTestId('apigateway-protected-route-verification-submit');
    fireEvent.click(button);

    await waitFor(() => {
      expect(button).toBeDisabled();
      expect(button).toHaveTextContent('Verifying');
    });
  });

  it('handles multiple protected methods', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/users',
          parentId: null,
          resourceMethods: ['GET', 'POST'],
        } as unknown as ApiGatewayRestResourceItem,
        {
          id: 'res2',
          path: '/admin',
          parentId: null,
          resourceMethods: ['DELETE'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'AWS_IAM',
    } as unknown as ApiGatewayRestMethodDetailResult);

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      const select = screen.getByTestId('apigateway-protected-route-verification-method');
      expect(select).toBeInTheDocument();
    });

    const select = screen.getByTestId(
      'apigateway-protected-route-verification-method'
    ) as HTMLSelectElement;
    expect(select.children.length).toBeGreaterThan(0);
  });

  it('shows a load error when discovery fails', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockRejectedValue(new Error('boom'));

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(
        screen.getByTestId('apigateway-protected-route-verification-load-error')
      ).toBeInTheDocument();
    });
  });

  it('skips methods whose details cannot be loaded', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/test',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockRejectedValue(new Error('no detail'));

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(
        screen.getByTestId('apigateway-protected-route-verification-empty')
      ).toBeInTheDocument();
    });
  });

  it('falls back to root path when a protected resource has no path', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'COGNITO_USER_POOLS',
    } as unknown as ApiGatewayRestMethodDetailResult);

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      const select = screen.getByTestId(
        'apigateway-protected-route-verification-method'
      ) as HTMLSelectElement;
      expect(select.children[0].textContent).toContain('/');
    });
  });

  it('reports auth not enforced and auth rejected outcomes', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/open',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'COGNITO_USER_POOLS',
    } as unknown as ApiGatewayRestMethodDetailResult);
    mockClient.testInvokeApiGatewayRestMethod.mockImplementation((_api, _resourceId, _method, options) => {
      if (!options.headers || !options.headers['Authorization']) {
        return Promise.resolve({
          statusCode: 200,
          latencyMilliseconds: 10,
          headers: {},
          body: 'OK',
          log: '',
        } as unknown as ApiGatewayRestMethodTestInvokeResult);
      }
      return Promise.resolve({
        statusCode: 403,
        latencyMilliseconds: 9,
        headers: {},
        body: 'Forbidden',
        log: '',
      } as unknown as ApiGatewayRestMethodTestInvokeResult);
    });

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-submit')).toBeEnabled();
    });
    fireEvent.click(screen.getByTestId('apigateway-protected-route-verification-submit'));

    await waitFor(() => {
      expect(screen.getByText(/Auth not enforced/)).toBeInTheDocument();
    });
    expect(screen.getByText(/Auth rejected/)).toBeInTheDocument();
  });

  it('reports a request error for the unauthorized probe', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/secure',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'COGNITO_USER_POOLS',
    } as unknown as ApiGatewayRestMethodDetailResult);
    mockClient.testInvokeApiGatewayRestMethod.mockImplementation((_api, _resourceId, _method, options) => {
      if (!options.headers || !options.headers['Authorization']) {
        return Promise.reject(new Error('network error'));
      }
      return Promise.resolve({
        statusCode: 200,
        latencyMilliseconds: 10,
        headers: {},
        body: 'OK',
        log: '',
      } as unknown as ApiGatewayRestMethodTestInvokeResult);
    });

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-submit')).toBeEnabled();
    });
    fireEvent.click(screen.getByTestId('apigateway-protected-route-verification-submit'));

    await waitFor(() => {
      expect(screen.getByText(/Request error \(expected\)/)).toBeInTheDocument();
    });
  });

  it('updates the selected method and authorization token from inputs', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/users',
          parentId: null,
          resourceMethods: ['GET', 'POST'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'AWS_IAM',
    } as unknown as ApiGatewayRestMethodDetailResult);

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-method')).toBeInTheDocument();
    });

    const select = screen.getByTestId(
      'apigateway-protected-route-verification-method'
    ) as HTMLSelectElement;
    fireEvent.change(select, { target: { value: '1' } });
    expect(select.value).toBe('1');

    const token = screen.getByTestId(
      'apigateway-protected-route-verification-token'
    ) as HTMLInputElement;
    fireEvent.change(token, { target: { value: 'Bearer replaced-token' } });
    expect(token.value).toBe('Bearer replaced-token');
  });

  it('renders a redirect status code with the redirect badge styling', async () => {
    const mockClient = vi.mocked(clientModule);
    mockClient.getApiGatewayRestResources.mockResolvedValue({
      resources: [
        {
          id: 'res1',
          path: '/redirect',
          parentId: null,
          resourceMethods: ['GET'],
        } as unknown as ApiGatewayRestResourceItem,
      ],
    });
    mockClient.getApiGatewayRestMethod.mockResolvedValue({
      authorizationType: 'COGNITO_USER_POOLS',
    } as unknown as ApiGatewayRestMethodDetailResult);
    mockClient.testInvokeApiGatewayRestMethod.mockImplementation((_api, _resourceId, _method, options) => {
      if (!options.headers || !options.headers['Authorization']) {
        return Promise.resolve({
          statusCode: 302,
          latencyMilliseconds: 10,
          headers: {},
          body: 'Found',
          log: '',
        } as unknown as ApiGatewayRestMethodTestInvokeResult);
      }
      return Promise.resolve({
        statusCode: 302,
        latencyMilliseconds: 11,
        headers: {},
        body: 'Found',
        log: '',
      } as unknown as ApiGatewayRestMethodTestInvokeResult);
    });

    render(<ApiGatewayProtectedRouteVerificationSection restApiId="test-api" />);

    await waitFor(() => {
      expect(screen.getByTestId('apigateway-protected-route-verification-submit')).toBeEnabled();
    });
    fireEvent.click(screen.getByTestId('apigateway-protected-route-verification-submit'));

    await waitFor(() => {
      expect(
        screen.getByTestId('apigateway-protected-route-verification-results')
      ).toBeInTheDocument();
    });
    expect(screen.getAllByText(/302/).length).toBeGreaterThan(0);
  });
});
