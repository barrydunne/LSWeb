import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ApiGatewayV2DetailView } from './ApiGatewayV2DetailView';
import {
  getHttpApi,
  updateHttpApi,
  getHttpRoutes,
  createHttpRoute,
  updateHttpRoute,
  deleteHttpRoute,
  getHttpIntegrations,
  createHttpIntegration,
  getHttpAuthorizers,
  createHttpAuthorizer,
  updateHttpAuthorizer,
  deleteHttpAuthorizer,
  getHttpStages,
  createHttpStage,
  updateHttpStage,
  deleteHttpStage,
} from '../../api/client';
import type {
  HttpApiDetailResult,
  HttpRouteSummaryItem,
  HttpIntegrationSummaryItem,
  HttpAuthorizerSummaryItem,
  HttpStageSummaryItem,
} from '../../api/client';

vi.mock('../../api/client');

const getHttpApiMock = vi.mocked(getHttpApi);
const updateHttpApiMock = vi.mocked(updateHttpApi);
const getHttpRoutesMock = vi.mocked(getHttpRoutes);
const createHttpRouteMock = vi.mocked(createHttpRoute);
const updateHttpRouteMock = vi.mocked(updateHttpRoute);
const deleteHttpRouteMock = vi.mocked(deleteHttpRoute);
const getHttpIntegrationsMock = vi.mocked(getHttpIntegrations);
const createHttpIntegrationMock = vi.mocked(createHttpIntegration);
const getHttpAuthorizersMock = vi.mocked(getHttpAuthorizers);
const createHttpAuthorizerMock = vi.mocked(createHttpAuthorizer);
const updateHttpAuthorizerMock = vi.mocked(updateHttpAuthorizer);
const deleteHttpAuthorizerMock = vi.mocked(deleteHttpAuthorizer);
const getHttpStagesMock = vi.mocked(getHttpStages);
const createHttpStageMock = vi.mocked(createHttpStage);
const updateHttpStageMock = vi.mocked(updateHttpStage);
const deleteHttpStageMock = vi.mocked(deleteHttpStage);

const detailResult: HttpApiDetailResult = {
  apiId: 'abc123',
  name: 'orders',
  protocolType: 'HTTP',
  apiEndpoint: 'https://abc123.execute-api.localhost',
  description: 'order service',
  version: '1.0',
  routeSelectionExpression: '$request.method',
  corsConfiguration: {
    allowCredentials: true,
    allowHeaders: ['content-type'],
    allowMethods: ['GET', 'POST'],
    allowOrigins: ['*'],
    exposeHeaders: [],
    maxAge: 600,
  },
  createdDate: '2024-01-01T00:00:00+00:00',
};

const routeSummary: HttpRouteSummaryItem = {
  routeId: 'route1',
  routeKey: 'GET /items',
  target: 'integrations/int1',
  authorizationType: 'NONE',
};

const integrationSummary: HttpIntegrationSummaryItem = {
  integrationId: 'int1',
  integrationType: 'HTTP_PROXY',
  integrationMethod: 'GET',
  integrationUri: 'https://example.test',
  payloadFormatVersion: '1.0',
  description: 'proxy',
};

const authorizerSummary: HttpAuthorizerSummaryItem = {
  authorizerId: 'auth1',
  name: 'jwt-authorizer',
  authorizerType: 'JWT',
};

const stageSummary: HttpStageSummaryItem = {
  stageName: 'dev',
  autoDeploy: true,
  deploymentId: 'deploy1',
  createdDate: '2024-01-01T00:00:00+00:00',
};

function renderView() {
  return render(<ApiGatewayV2DetailView serviceKey="apigatewayv2" resourceId="abc123" />);
}

describe('ApiGatewayV2DetailView', () => {
  beforeEach(() => {
    getHttpApiMock.mockResolvedValue(detailResult);
    getHttpRoutesMock.mockResolvedValue({ routes: [routeSummary] });
    getHttpIntegrationsMock.mockResolvedValue({ integrations: [integrationSummary] });
    getHttpAuthorizersMock.mockResolvedValue({ authorizers: [authorizerSummary] });
    getHttpStagesMock.mockResolvedValue({ stages: [stageSummary] });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the API arrives', () => {
    getHttpApiMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('apigatewayv2-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getHttpApiMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-error')).toBeInTheDocument());
  });

  it('requests the API by its resource id', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    expect(getHttpApiMock).toHaveBeenCalledWith('abc123', expect.any(AbortSignal));
  });

  it('renders the API detail fields', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('apigatewayv2-detail-name')).toHaveTextContent('orders');
    expect(screen.getByTestId('apigatewayv2-detail-id')).toHaveTextContent('abc123');
    expect(screen.getByTestId('apigatewayv2-detail-protocol')).toHaveTextContent('HTTP');
    expect(screen.getByTestId('apigatewayv2-detail-endpoint')).toHaveTextContent(
      'https://abc123.execute-api.localhost',
    );
    expect(screen.getByTestId('apigatewayv2-detail-description')).toHaveTextContent('order service');
    expect(screen.getByTestId('apigatewayv2-detail-version')).toHaveTextContent('1.0');
    expect(screen.getByTestId('apigatewayv2-detail-route-selection')).toHaveTextContent(
      '$request.method',
    );
    expect(screen.getByTestId('apigatewayv2-detail-created')).toHaveTextContent(
      '2024-01-01T00:00:00+00:00',
    );
  });

  it('renders the CORS configuration when present', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('apigatewayv2-detail-cors-origins')).toHaveTextContent('*');
    expect(screen.getByTestId('apigatewayv2-detail-cors-methods')).toHaveTextContent('GET, POST');
    expect(screen.getByTestId('apigatewayv2-detail-cors-headers')).toHaveTextContent('content-type');
    expect(screen.getByTestId('apigatewayv2-detail-cors-expose')).toHaveTextContent('—');
    expect(screen.getByTestId('apigatewayv2-detail-cors-credentials')).toHaveTextContent('true');
    expect(screen.getByTestId('apigatewayv2-detail-cors-max-age')).toHaveTextContent('600');
  });

  it('renders dashes for null optional fields and hides CORS when absent', async () => {
    getHttpApiMock.mockResolvedValue({
      ...detailResult,
      apiEndpoint: null,
      description: null,
      version: null,
      routeSelectionExpression: null,
      corsConfiguration: null,
      createdDate: null,
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('apigatewayv2-detail-endpoint')).toHaveTextContent('—');
    expect(screen.getByTestId('apigatewayv2-detail-description')).toHaveTextContent('—');
    expect(screen.getByTestId('apigatewayv2-detail-version')).toHaveTextContent('—');
    expect(screen.getByTestId('apigatewayv2-detail-route-selection')).toHaveTextContent('—');
    expect(screen.getByTestId('apigatewayv2-detail-created')).toHaveTextContent('—');
    expect(screen.queryByTestId('apigatewayv2-detail-cors')).not.toBeInTheDocument();
  });

  it('renders a dash for a null allowCredentials and null maxAge', async () => {
    getHttpApiMock.mockResolvedValue({
      ...detailResult,
      corsConfiguration: {
        allowCredentials: null,
        allowHeaders: [],
        allowMethods: [],
        allowOrigins: [],
        exposeHeaders: [],
        maxAge: null,
      },
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('apigatewayv2-detail-cors-credentials')).toHaveTextContent('—');
    expect(screen.getByTestId('apigatewayv2-detail-cors-max-age')).toHaveTextContent('—');
    expect(screen.getByTestId('apigatewayv2-detail-cors-origins')).toHaveTextContent('—');
  });

  it('opens and closes the edit form', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('apigatewayv2-edit-toggle'));
    expect(screen.getByTestId('apigatewayv2-edit-form')).toBeInTheDocument();
    expect(screen.getByTestId('apigatewayv2-edit-name')).toHaveValue('orders');

    fireEvent.click(screen.getByTestId('apigatewayv2-edit-toggle'));
    expect(screen.queryByTestId('apigatewayv2-edit-form')).not.toBeInTheDocument();
  });

  it('seeds the edit form with blank values when the API fields are null', async () => {
    getHttpApiMock.mockResolvedValue({
      ...detailResult,
      description: null,
      version: null,
      routeSelectionExpression: null,
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('apigatewayv2-edit-toggle'));

    expect(screen.getByTestId('apigatewayv2-edit-description')).toHaveValue('');
    expect(screen.getByTestId('apigatewayv2-edit-version')).toHaveValue('');
    expect(screen.getByTestId('apigatewayv2-edit-route-selection')).toHaveValue('');
  });

  it('updates the API from the form and refreshes the detail', async () => {
    updateHttpApiMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('apigatewayv2-edit-toggle'));

    fireEvent.change(screen.getByTestId('apigatewayv2-edit-name'), {
      target: { value: 'orders-v2' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-edit-protocol'), {
      target: { value: 'WEBSOCKET' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-edit-description'), {
      target: { value: '' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-edit-version'), {
      target: { value: '2.0' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-edit-route-selection'), {
      target: { value: '$request.path' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-edit-submit'));

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-edit-status')).toBeInTheDocument());

    expect(updateHttpApiMock).toHaveBeenCalledWith('abc123', {
      name: 'orders-v2',
      protocolType: 'WEBSOCKET',
      description: null,
      version: '2.0',
      routeSelectionExpression: '$request.path',
    });
    expect(getHttpApiMock).toHaveBeenCalledTimes(2);
    expect(screen.queryByTestId('apigatewayv2-edit-form')).not.toBeInTheDocument();
  });

  it('shows an error when the update fails', async () => {
    updateHttpApiMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('apigatewayv2-edit-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-edit-submit'));

    await waitFor(() => expect(screen.getByTestId('apigatewayv2-edit-error')).toBeInTheDocument());
    expect(screen.getByTestId('apigatewayv2-edit-form')).toBeInTheDocument();
  });

  it('renders the routes for the API', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-route1')).toBeInTheDocument(),
    );

    expect(getHttpRoutesMock).toHaveBeenCalledWith('abc123', expect.any(AbortSignal));
    expect(screen.getByTestId('apigatewayv2-route-key')).toHaveTextContent('GET /items');
  });

  it('shows a loading state while the routes are loading', async () => {
    getHttpRoutesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-routes-loading')).toBeInTheDocument(),
    );
  });

  it('shows an empty message when there are no routes', async () => {
    getHttpRoutesMock.mockResolvedValue({ routes: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-routes-empty')).toBeInTheDocument(),
    );
  });

  it('shows an error when the routes fail to load', async () => {
    getHttpRoutesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-routes-error')).toBeInTheDocument(),
    );
  });

  it('creates a route from the form and refreshes the lists', async () => {
    createHttpRouteMock.mockResolvedValue({ routeId: 'route2' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-route-new-key'), {
      target: { value: 'POST /orders' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-route-new-target'), {
      target: { value: 'integrations/int1' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-route-new-auth'), {
      target: { value: 'JWT' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-route-new-authorizer'), {
      target: { value: 'auth1' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-route-new-scopes'), {
      target: { value: 'scope.read, scope.write' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-route-new-submit'));

    await waitFor(() =>
      expect(createHttpRouteMock).toHaveBeenCalledWith('abc123', {
        routeKey: 'POST /orders',
        target: 'integrations/int1',
        authorizationType: 'JWT',
        authorizerId: 'auth1',
        authorizationScopes: ['scope.read', 'scope.write'],
      }),
    );
    await waitFor(() => expect(getHttpRoutesMock).toHaveBeenCalledTimes(2));
  });

  it('creates a route with null target and scopes when blank', async () => {
    createHttpRouteMock.mockResolvedValue({ routeId: 'route2' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-route-new-key'), {
      target: { value: '$default' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-route-new-submit'));

    await waitFor(() =>
      expect(createHttpRouteMock).toHaveBeenCalledWith('abc123', {
        routeKey: '$default',
        target: null,
        authorizationType: 'NONE',
        authorizerId: null,
        authorizationScopes: null,
      }),
    );
    await waitFor(() => expect(getHttpRoutesMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when creating a route fails', async () => {
    createHttpRouteMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-route-new-key'), {
      target: { value: 'GET /x' },
    });
    fireEvent.click(screen.getByTestId('apigatewayv2-route-new-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-new-error')).toBeInTheDocument(),
    );
  });

  it('disables the add-route button when the route key is blank', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-create-form')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigatewayv2-route-new-submit')).toBeDisabled();
  });

  it('edits a route and refreshes the lists', async () => {
    updateHttpRouteMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-route1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-route-edit-toggle'));
    expect(screen.getByTestId('apigatewayv2-route-edit-key')).toHaveValue('GET /items');

    fireEvent.change(screen.getByTestId('apigatewayv2-route-edit-key'), {
      target: { value: 'PUT /items' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-route-edit-target'), {
      target: { value: '' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-route-edit-auth'), {
      target: { value: 'JWT' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-route-edit-authorizer'), {
      target: { value: 'auth1' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-route-edit-scopes'), {
      target: { value: 'read, write' },
    });
    fireEvent.click(screen.getByTestId('apigatewayv2-route-edit-submit'));

    await waitFor(() =>
      expect(updateHttpRouteMock).toHaveBeenCalledWith('abc123', 'route1', {
        routeKey: 'PUT /items',
        target: null,
        authorizationType: 'JWT',
        authorizerId: 'auth1',
        authorizationScopes: ['read', 'write'],
      }),
    );
    await waitFor(() => expect(getHttpRoutesMock).toHaveBeenCalledTimes(2));
  });

  it('cancels editing a route', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-route1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-route-edit-toggle'));
    expect(screen.getByTestId('apigatewayv2-route-edit-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigatewayv2-route-edit-cancel'));
    expect(screen.queryByTestId('apigatewayv2-route-edit-form')).not.toBeInTheDocument();
  });

  it('seeds the route edit form defaults when fields are null', async () => {
    getHttpRoutesMock.mockResolvedValue({
      routes: [{ routeId: 'route1', routeKey: 'GET /x', target: null, authorizationType: null }],
    });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-route1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-route-edit-toggle'));

    expect(screen.getByTestId('apigatewayv2-route-edit-target')).toHaveValue('');
    expect(screen.getByTestId('apigatewayv2-route-edit-auth')).toHaveValue('NONE');
  });

  it('shows an error when editing a route fails', async () => {
    updateHttpRouteMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-route1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-route-edit-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-route-edit-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-edit-error')).toBeInTheDocument(),
    );
  });

  it('deletes a route after confirmation and refreshes the lists', async () => {
    deleteHttpRouteMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-route1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-route-delete-toggle'));
    expect(screen.getByTestId('apigatewayv2-route-delete-confirm')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigatewayv2-route-delete-confirm-yes'));

    await waitFor(() =>
      expect(deleteHttpRouteMock).toHaveBeenCalledWith('abc123', 'route1'),
    );
    await waitFor(() => expect(getHttpRoutesMock).toHaveBeenCalledTimes(2));
  });

  it('cancels a route deletion', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-route1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-route-delete-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-route-delete-confirm-no'));

    expect(screen.queryByTestId('apigatewayv2-route-delete-confirm')).not.toBeInTheDocument();
    expect(deleteHttpRouteMock).not.toHaveBeenCalled();
  });

  it('shows an error when deleting a route fails', async () => {
    deleteHttpRouteMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-route1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-route-delete-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-route-delete-confirm-yes'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-delete-error')).toBeInTheDocument(),
    );
  });

  it('renders the integrations for the API', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integration-int1')).toBeInTheDocument(),
    );

    expect(getHttpIntegrationsMock).toHaveBeenCalledWith('abc123', expect.any(AbortSignal));
    expect(screen.getByTestId('apigatewayv2-integration-id')).toHaveTextContent('int1');
  });

  it('renders dashes for null integration fields', async () => {
    getHttpIntegrationsMock.mockResolvedValue({
      integrations: [
        {
          integrationId: 'int2',
          integrationType: 'MOCK',
          integrationMethod: null,
          integrationUri: null,
          payloadFormatVersion: null,
          description: null,
        },
      ],
    });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integration-int2')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigatewayv2-integration-int2')).toHaveTextContent('Method: —');
  });

  it('shows a loading state while the integrations are loading', async () => {
    getHttpIntegrationsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integrations-loading')).toBeInTheDocument(),
    );
  });

  it('shows an empty message when there are no integrations', async () => {
    getHttpIntegrationsMock.mockResolvedValue({ integrations: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integrations-empty')).toBeInTheDocument(),
    );
  });

  it('shows an error when the integrations fail to load', async () => {
    getHttpIntegrationsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integrations-error')).toBeInTheDocument(),
    );
  });

  it('creates an integration from the form and refreshes the lists', async () => {
    createHttpIntegrationMock.mockResolvedValue({ integrationId: 'int2' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integration-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-integration-new-type'), {
      target: { value: 'MOCK' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-integration-new-method'), {
      target: { value: 'POST' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-integration-new-uri'), {
      target: { value: 'https://orders.test' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-integration-new-payload'), {
      target: { value: '2.0' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-integration-new-description'), {
      target: { value: 'order proxy' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-integration-new-submit'));

    await waitFor(() =>
      expect(createHttpIntegrationMock).toHaveBeenCalledWith('abc123', {
        integrationType: 'MOCK',
        integrationMethod: 'POST',
        integrationUri: 'https://orders.test',
        payloadFormatVersion: '2.0',
        description: 'order proxy',
      }),
    );
    await waitFor(() => expect(getHttpIntegrationsMock).toHaveBeenCalledTimes(2));
  });

  it('creates an integration with null optional fields when blank', async () => {
    createHttpIntegrationMock.mockResolvedValue({ integrationId: 'int2' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integration-create-form')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-integration-new-submit'));

    await waitFor(() =>
      expect(createHttpIntegrationMock).toHaveBeenCalledWith('abc123', {
        integrationType: 'HTTP_PROXY',
        integrationMethod: null,
        integrationUri: null,
        payloadFormatVersion: null,
        description: null,
      }),
    );
    await waitFor(() => expect(getHttpIntegrationsMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when creating an integration fails', async () => {
    createHttpIntegrationMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integration-create-form')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-integration-new-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-integration-new-error')).toBeInTheDocument(),
    );
  });

  it('renders the authorizers for the API', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-auth1')).toBeInTheDocument(),
    );

    expect(getHttpAuthorizersMock).toHaveBeenCalledWith('abc123', expect.any(AbortSignal));
    expect(screen.getByTestId('apigatewayv2-authorizer-name')).toHaveTextContent('jwt-authorizer');
  });

  it('shows a loading state while the authorizers are loading', async () => {
    getHttpAuthorizersMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizers-loading')).toBeInTheDocument(),
    );
  });

  it('shows an empty message when there are no authorizers', async () => {
    getHttpAuthorizersMock.mockResolvedValue({ authorizers: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizers-empty')).toBeInTheDocument(),
    );
  });

  it('shows an error when the authorizers fail to load', async () => {
    getHttpAuthorizersMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizers-error')).toBeInTheDocument(),
    );
  });

  it('creates an authorizer from the form and refreshes the lists', async () => {
    createHttpAuthorizerMock.mockResolvedValue({ authorizerId: 'auth2' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-new-name'), {
      target: { value: 'orders-authorizer' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-new-identity'), {
      target: { value: '$request.header.Authorization, $request.header.X-Token' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-new-issuer'), {
      target: { value: 'https://issuer.test' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-new-audience'), {
      target: { value: 'audience1, audience2' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-new-submit'));

    await waitFor(() =>
      expect(createHttpAuthorizerMock).toHaveBeenCalledWith('abc123', {
        name: 'orders-authorizer',
        authorizerType: 'JWT',
        identitySource: ['$request.header.Authorization', '$request.header.X-Token'],
        jwtIssuer: 'https://issuer.test',
        jwtAudience: ['audience1', 'audience2'],
      }),
    );
    await waitFor(() => expect(getHttpAuthorizersMock).toHaveBeenCalledTimes(2));
  });

  it('creates an authorizer with a null issuer when blank', async () => {
    createHttpAuthorizerMock.mockResolvedValue({ authorizerId: 'auth2' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-new-name'), {
      target: { value: 'orders-authorizer' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-new-submit'));

    await waitFor(() =>
      expect(createHttpAuthorizerMock).toHaveBeenCalledWith('abc123', {
        name: 'orders-authorizer',
        authorizerType: 'JWT',
        identitySource: ['$request.header.Authorization'],
        jwtIssuer: null,
        jwtAudience: [],
      }),
    );
    await waitFor(() => expect(getHttpAuthorizersMock).toHaveBeenCalledTimes(2));
  });

  it('disables the add-authorizer button when the name is blank', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-create-form')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigatewayv2-authorizer-new-submit')).toBeDisabled();
  });

  it('shows an error when creating an authorizer fails', async () => {
    createHttpAuthorizerMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-new-name'), {
      target: { value: 'orders-authorizer' },
    });
    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-new-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-new-error')).toBeInTheDocument(),
    );
  });

  it('edits an authorizer and refreshes the lists', async () => {
    updateHttpAuthorizerMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-auth1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-edit-toggle'));
    expect(screen.getByTestId('apigatewayv2-authorizer-edit-name')).toHaveValue('jwt-authorizer');

    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-edit-name'), {
      target: { value: 'renamed-authorizer' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-edit-identity'), {
      target: { value: '$request.header.Authorization, $request.header.X-Token' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-edit-issuer'), {
      target: { value: 'https://issuer.test' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-authorizer-edit-audience'), {
      target: { value: 'audience1' },
    });
    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-edit-submit'));

    await waitFor(() =>
      expect(updateHttpAuthorizerMock).toHaveBeenCalledWith('abc123', 'auth1', {
        name: 'renamed-authorizer',
        authorizerType: 'JWT',
        identitySource: ['$request.header.Authorization', '$request.header.X-Token'],
        jwtIssuer: 'https://issuer.test',
        jwtAudience: ['audience1'],
      }),
    );
    await waitFor(() => expect(getHttpAuthorizersMock).toHaveBeenCalledTimes(2));
  });

  it('cancels editing an authorizer', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-auth1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-edit-toggle'));
    expect(screen.getByTestId('apigatewayv2-authorizer-edit-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-edit-cancel'));
    expect(screen.queryByTestId('apigatewayv2-authorizer-edit-form')).not.toBeInTheDocument();
  });

  it('shows an error when editing an authorizer fails', async () => {
    updateHttpAuthorizerMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-auth1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-edit-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-edit-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-edit-error')).toBeInTheDocument(),
    );
  });

  it('deletes an authorizer after confirmation and refreshes the lists', async () => {
    deleteHttpAuthorizerMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-auth1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-delete-toggle'));
    expect(screen.getByTestId('apigatewayv2-authorizer-delete-confirm')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-delete-confirm-yes'));

    await waitFor(() =>
      expect(deleteHttpAuthorizerMock).toHaveBeenCalledWith('abc123', 'auth1'),
    );
    await waitFor(() => expect(getHttpAuthorizersMock).toHaveBeenCalledTimes(2));
  });

  it('cancels an authorizer deletion', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-auth1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-delete-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-delete-confirm-no'));

    expect(
      screen.queryByTestId('apigatewayv2-authorizer-delete-confirm'),
    ).not.toBeInTheDocument();
    expect(deleteHttpAuthorizerMock).not.toHaveBeenCalled();
  });

  it('shows an error when deleting an authorizer fails', async () => {
    deleteHttpAuthorizerMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-auth1')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-delete-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-authorizer-delete-confirm-yes'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-authorizer-delete-error')).toBeInTheDocument(),
    );
  });

  it('lists the stages with their invoke URL', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigatewayv2-stage-name')).toHaveTextContent('dev');
    expect(screen.getByTestId('apigatewayv2-stage-invoke-url')).toHaveTextContent(
      'https://abc123.execute-api.localhost/dev',
    );
  });

  it('shows an empty message when there are no stages', async () => {
    getHttpStagesMock.mockResolvedValue({ stages: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stages-empty')).toBeInTheDocument(),
    );
  });

  it('shows a loading message while the stages are loading', async () => {
    getHttpStagesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stages-loading')).toBeInTheDocument(),
    );
  });

  it('renders a manual stage without a deployment id', async () => {
    getHttpStagesMock.mockResolvedValue({
      stages: [{ stageName: 'dev', autoDeploy: false, deploymentId: null, createdDate: null }],
    });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigatewayv2-stage-dev')).toHaveTextContent('Auto deploy: No');
    expect(screen.getByTestId('apigatewayv2-stage-dev')).toHaveTextContent(
      'Deployment ID: \u2014',
    );
  });

  it('shows a dash invoke URL when the API has no endpoint', async () => {
    getHttpApiMock.mockResolvedValue({ ...detailResult, apiEndpoint: null });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigatewayv2-stage-invoke-url')).toHaveTextContent(
      'Invoke URL: \u2014',
    );
  });

  it('shows an error when the stages fail to load', async () => {
    getHttpStagesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stages-error')).toBeInTheDocument(),
    );
  });

  it('creates a stage from the form and refreshes the lists', async () => {
    createHttpStageMock.mockResolvedValue({ stageName: 'prod' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-stage-new-name'), {
      target: { value: 'prod' },
    });
    fireEvent.click(screen.getByTestId('apigatewayv2-stage-new-auto-deploy'));
    fireEvent.change(screen.getByTestId('apigatewayv2-stage-new-description'), {
      target: { value: 'production stage' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-stage-new-burst'), {
      target: { value: '100' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-stage-new-rate'), {
      target: { value: '50' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-new-submit'));

    await waitFor(() =>
      expect(createHttpStageMock).toHaveBeenCalledWith('abc123', {
        stageName: 'prod',
        autoDeploy: false,
        description: 'production stage',
        defaultRouteThrottlingBurstLimit: 100,
        defaultRouteThrottlingRateLimit: 50,
        stageVariables: {},
      }),
    );
    await waitFor(() => expect(getHttpStagesMock).toHaveBeenCalledTimes(2));
  });

  it('creates a stage with null optional values when blank', async () => {
    createHttpStageMock.mockResolvedValue({ stageName: 'prod' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-stage-new-name'), {
      target: { value: 'prod' },
    });

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-new-submit'));

    await waitFor(() =>
      expect(createHttpStageMock).toHaveBeenCalledWith('abc123', {
        stageName: 'prod',
        autoDeploy: true,
        description: null,
        defaultRouteThrottlingBurstLimit: null,
        defaultRouteThrottlingRateLimit: null,
        stageVariables: {},
      }),
    );
    await waitFor(() => expect(getHttpStagesMock).toHaveBeenCalledTimes(2));
  });

  it('disables the add-stage button when the name is blank', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-create-form')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigatewayv2-stage-new-submit')).toBeDisabled();
  });

  it('shows an error when creating a stage fails', async () => {
    createHttpStageMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-create-form')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('apigatewayv2-stage-new-name'), {
      target: { value: 'prod' },
    });
    fireEvent.click(screen.getByTestId('apigatewayv2-stage-new-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-new-error')).toBeInTheDocument(),
    );
  });

  it('edits a stage and refreshes the lists', async () => {
    updateHttpStageMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-edit-toggle'));
    expect(screen.getByTestId('apigatewayv2-stage-edit-auto-deploy')).toBeChecked();

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-edit-auto-deploy'));
    fireEvent.change(screen.getByTestId('apigatewayv2-stage-edit-description'), {
      target: { value: 'updated stage' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-stage-edit-burst'), {
      target: { value: '200' },
    });
    fireEvent.change(screen.getByTestId('apigatewayv2-stage-edit-rate'), {
      target: { value: '75' },
    });
    fireEvent.click(screen.getByTestId('apigatewayv2-stage-edit-submit'));

    await waitFor(() =>
      expect(updateHttpStageMock).toHaveBeenCalledWith('abc123', 'dev', {
        autoDeploy: false,
        description: 'updated stage',
        defaultRouteThrottlingBurstLimit: 200,
        defaultRouteThrottlingRateLimit: 75,
        stageVariables: {},
      }),
    );
    await waitFor(() => expect(getHttpStagesMock).toHaveBeenCalledTimes(2));
  });

  it('cancels editing a stage', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-edit-toggle'));
    expect(screen.getByTestId('apigatewayv2-stage-edit-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-edit-cancel'));
    expect(screen.queryByTestId('apigatewayv2-stage-edit-form')).not.toBeInTheDocument();
  });

  it('shows an error when editing a stage fails', async () => {
    updateHttpStageMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-edit-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-stage-edit-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-edit-error')).toBeInTheDocument(),
    );
  });

  it('deletes a stage after confirmation and refreshes the lists', async () => {
    deleteHttpStageMock.mockResolvedValue();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-delete-toggle'));
    expect(screen.getByTestId('apigatewayv2-stage-delete-confirm')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-delete-confirm-yes'));

    await waitFor(() => expect(deleteHttpStageMock).toHaveBeenCalledWith('abc123', 'dev'));
    await waitFor(() => expect(getHttpStagesMock).toHaveBeenCalledTimes(2));
  });

  it('cancels a stage deletion', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-delete-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-stage-delete-confirm-no'));

    expect(screen.queryByTestId('apigatewayv2-stage-delete-confirm')).not.toBeInTheDocument();
    expect(deleteHttpStageMock).not.toHaveBeenCalled();
  });

  it('shows an error when deleting a stage fails', async () => {
    deleteHttpStageMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-dev')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('apigatewayv2-stage-delete-toggle'));
    fireEvent.click(screen.getByTestId('apigatewayv2-stage-delete-confirm-yes'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-stage-delete-error')).toBeInTheDocument(),
    );
  });
});
