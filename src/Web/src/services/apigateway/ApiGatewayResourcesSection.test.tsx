import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ApiGatewayResourcesSection } from './ApiGatewayResourcesSection';
import {
  createApiGatewayRestResource,
  deleteApiGatewayRestMethod,
  deleteApiGatewayRestResource,
  getApiGatewayRestMethod,
  getApiGatewayRestResources,
  getApiGatewayRestAuthorizers,
  putApiGatewayRestMethod,
} from '../../api/client';
import type { ApiGatewayRestResourceItem } from '../../api/client';

vi.mock('../../api/client');

const getApiGatewayRestResourcesMock = vi.mocked(getApiGatewayRestResources);
const createApiGatewayRestResourceMock = vi.mocked(createApiGatewayRestResource);
const deleteApiGatewayRestResourceMock = vi.mocked(deleteApiGatewayRestResource);
const getApiGatewayRestMethodMock = vi.mocked(getApiGatewayRestMethod);
const getApiGatewayRestAuthorizersMock = vi.mocked(getApiGatewayRestAuthorizers);
const putApiGatewayRestMethodMock = vi.mocked(putApiGatewayRestMethod);
const deleteApiGatewayRestMethodMock = vi.mocked(deleteApiGatewayRestMethod);

const rootResource: ApiGatewayRestResourceItem = {
  id: 'root1',
  parentId: null,
  pathPart: null,
  path: '/',
  resourceMethods: [],
};

const childResource: ApiGatewayRestResourceItem = {
  id: 'res2',
  parentId: 'root1',
  pathPart: 'items',
  path: '/items',
  resourceMethods: ['GET'],
};

function renderSection() {
  return render(<ApiGatewayResourcesSection restApiId="api-1" />);
}

describe('ApiGatewayResourcesSection', () => {
  beforeEach(() => {
    getApiGatewayRestResourcesMock.mockResolvedValue({
      resources: [rootResource, childResource],
    });
    createApiGatewayRestResourceMock.mockResolvedValue({ id: 'res9' });
    deleteApiGatewayRestResourceMock.mockResolvedValue();
    getApiGatewayRestMethodMock.mockResolvedValue({
      resourceId: 'res2',
      httpMethod: 'GET',
      authorizationType: 'NONE',
      authorizerId: null,
      apiKeyRequired: false,
      authorizationScopes: [],
      integrationType: 'MOCK',
      integrationUri: null,
    });
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [],
    });
    putApiGatewayRestMethodMock.mockResolvedValue();
    deleteApiGatewayRestMethodMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before resources arrive', () => {
    getApiGatewayRestResourcesMock.mockReturnValue(new Promise(() => {}));

    renderSection();

    expect(screen.getByTestId('apigateway-resources-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getApiGatewayRestResourcesMock.mockRejectedValue(new Error('boom'));

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resources-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty message when there are no resources', async () => {
    getApiGatewayRestResourcesMock.mockResolvedValue({ resources: [] });

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resources-empty')).toBeInTheDocument(),
    );
  });

  it('renders resources with paths and methods', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resources-section')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigateway-resource-path-root1')).toHaveTextContent('/');
    expect(screen.getByTestId('apigateway-resource-path-res2')).toHaveTextContent('/items');
    expect(screen.getByTestId('apigateway-resource-no-methods-root1')).toBeInTheDocument();
    expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument();
    expect(screen.queryByTestId('apigateway-resource-delete-root1')).not.toBeInTheDocument();
    expect(screen.getByTestId('apigateway-resource-delete-res2')).toBeInTheDocument();
  });

  it('disables the add button when the path part is empty', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-add')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('apigateway-resource-add')).toBeDisabled();
  });

  it('adds a resource under the root when no parent is selected', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-add-form')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('apigateway-resource-path-part'), 'orders');
    await user.click(screen.getByTestId('apigateway-resource-add'));

    await waitFor(() =>
      expect(createApiGatewayRestResourceMock).toHaveBeenCalledWith('api-1', {
        parentId: 'root1',
        pathPart: 'orders',
      }),
    );
    expect(getApiGatewayRestResourcesMock).toHaveBeenCalledTimes(2);
  });

  it('adds a resource under the selected parent', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-add-form')).toBeInTheDocument(),
    );

    await user.selectOptions(screen.getByTestId('apigateway-resource-parent'), 'res2');
    await user.type(screen.getByTestId('apigateway-resource-path-part'), 'sub');
    await user.click(screen.getByTestId('apigateway-resource-add'));

    await waitFor(() =>
      expect(createApiGatewayRestResourceMock).toHaveBeenCalledWith('api-1', {
        parentId: 'res2',
        pathPart: 'sub',
      }),
    );
  });

  it('uses an empty parent id when there is no root resource', async () => {
    getApiGatewayRestResourcesMock.mockResolvedValue({ resources: [childResource] });
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-add-form')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('apigateway-resource-path-part'), 'orders');
    await user.click(screen.getByTestId('apigateway-resource-add'));

    await waitFor(() =>
      expect(createApiGatewayRestResourceMock).toHaveBeenCalledWith('api-1', {
        parentId: '',
        pathPart: 'orders',
      }),
    );
  });

  it('shows an error when adding a resource fails', async () => {
    createApiGatewayRestResourceMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-add-form')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('apigateway-resource-path-part'), 'orders');
    await user.click(screen.getByTestId('apigateway-resource-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-add-error')).toBeInTheDocument(),
    );
  });

  it('deletes a resource after confirmation', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-delete-res2')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-resource-delete-res2'));

    await waitFor(() =>
      expect(deleteApiGatewayRestResourceMock).toHaveBeenCalledWith('api-1', 'res2'),
    );
    confirmSpy.mockRestore();
  });

  it('does not delete a resource when confirmation is cancelled', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-delete-res2')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-resource-delete-res2'));

    expect(deleteApiGatewayRestResourceMock).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('shows an error when deleting a resource fails', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    deleteApiGatewayRestResourceMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resource-delete-res2')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-resource-delete-res2'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resources-error')).toBeInTheDocument(),
    );
    confirmSpy.mockRestore();
  });

  it('adds a method with the selected http method and authorization type', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-http-res2')).toBeInTheDocument(),
    );

    await user.selectOptions(screen.getByTestId('apigateway-method-http-res2'), 'POST');
    await user.selectOptions(
      screen.getByTestId('apigateway-method-auth-res2'),
      'COGNITO_USER_POOLS',
    );
    await user.selectOptions(screen.getByTestId('apigateway-method-integration-type-res2'), 'AWS_PROXY');
    await user.type(
      screen.getByTestId('apigateway-method-integration-uri-res2'),
      'arn:aws:lambda:eu-west-1:000000000000:function:orders',
    );
    await user.click(screen.getByTestId('apigateway-method-add-res2'));

    await waitFor(() =>
      expect(putApiGatewayRestMethodMock).toHaveBeenCalledWith('api-1', 'res2', 'POST', {
        authorizationType: 'COGNITO_USER_POOLS',
        authorizerId: null,
        apiKeyRequired: false,
        authorizationScopes: [],
        integrationType: 'AWS_PROXY',
        integrationUri: 'arn:aws:lambda:eu-west-1:000000000000:function:orders',
      }),
    );
  });

  it('sends null integration uri for mock integrations', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-add-res2')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-add-res2'));

    await waitFor(() =>
      expect(putApiGatewayRestMethodMock).toHaveBeenCalledWith('api-1', 'res2', 'GET', {
        authorizationType: 'NONE',
        authorizerId: null,
        apiKeyRequired: false,
        authorizationScopes: [],
        integrationType: 'MOCK',
        integrationUri: null,
      }),
    );
  });

  it('shows an error when adding a method fails', async () => {
    putApiGatewayRestMethodMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-add-res2')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-add-res2'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-error')).toBeInTheDocument(),
    );
  });

  it('deletes a method after confirmation', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-delete-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-delete-res2-GET'));

    await waitFor(() =>
      expect(deleteApiGatewayRestMethodMock).toHaveBeenCalledWith('api-1', 'res2', 'GET'),
    );
    confirmSpy.mockRestore();
  });

  it('does not delete a method when confirmation is cancelled', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-delete-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-delete-res2-GET'));

    expect(deleteApiGatewayRestMethodMock).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('shows an error when deleting a method fails', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    deleteApiGatewayRestMethodMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-delete-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-delete-res2-GET'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-resources-error')).toBeInTheDocument(),
    );
    confirmSpy.mockRestore();
  });

  it('views a method detail', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail')).toHaveTextContent('GET'),
    );
    expect(screen.getByTestId('apigateway-method-detail')).toHaveTextContent('NONE');
    expect(getApiGatewayRestMethodMock).toHaveBeenCalledWith('api-1', 'res2', 'GET');
  });

  it('does not show a method detail when the request fails', async () => {
    getApiGatewayRestMethodMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    await waitFor(() => expect(getApiGatewayRestMethodMock).toHaveBeenCalled());
    expect(screen.queryByTestId('apigateway-method-detail')).not.toBeInTheDocument();
  });

  it('shows authorizer dropdown when auth type is COGNITO_USER_POOLS', async () => {
    const user = userEvent.setup();
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [
        { id: 'auth-1', name: 'My Authorizer', type: 'COGNITO_USER_POOLS' },
      ],
    });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-http-res2')).toBeInTheDocument(),
    );

    // Change auth type to COGNITO_USER_POOLS
    await user.selectOptions(
      screen.getByTestId('apigateway-method-auth-res2'),
      'COGNITO_USER_POOLS',
    );

    // Authorizer dropdown should appear
    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-authorizer-res2')).toBeInTheDocument(),
    );
    expect(getApiGatewayRestAuthorizersMock).toHaveBeenCalledWith('api-1', expect.anything());
  });

  it('does not show authorizer dropdown when auth type is NONE', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-http-res2')).toBeInTheDocument(),
    );

    // Select NONE (default) - authorizer dropdown should not appear
    expect(screen.queryByTestId('apigateway-method-authorizer-res2')).not.toBeInTheDocument();
  });

  it('adds a method with an authorizer binding', async () => {
    const user = userEvent.setup();
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [
        { id: 'auth-1', name: 'My Authorizer', type: 'COGNITO_USER_POOLS' },
      ],
    });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-http-root1')).toBeInTheDocument(),
    );

    // Change auth type to COGNITO_USER_POOLS
    await user.selectOptions(
      screen.getByTestId('apigateway-method-auth-root1'),
      'COGNITO_USER_POOLS',
    );

    // Wait for authorizer dropdown
    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-authorizer-root1')).toBeInTheDocument(),
    );

    // Select authorizer
    await user.selectOptions(
      screen.getByTestId('apigateway-method-authorizer-root1'),
      'auth-1',
    );

    // Add method
    await user.click(screen.getByTestId('apigateway-method-add-root1'));

    await waitFor(() =>
      expect(putApiGatewayRestMethodMock).toHaveBeenCalled(),
    );

    // Verify the call had the authorizer
    const lastCall = putApiGatewayRestMethodMock.mock.calls[putApiGatewayRestMethodMock.mock.calls.length - 1];
    expect(lastCall[0]).toBe('api-1');
    expect(lastCall[1]).toBe('root1');
    expect(lastCall[2]).toBe('GET');
    expect(lastCall[3].authorizationType).toBe('COGNITO_USER_POOLS');
    expect(lastCall[3].authorizerId).toBe('auth-1');
  });

  it('displays bound authorizer in method detail', async () => {
    const user = userEvent.setup();
    getApiGatewayRestMethodMock.mockResolvedValue({
      resourceId: 'res2',
      httpMethod: 'GET',
      authorizationType: 'COGNITO_USER_POOLS',
      authorizerId: 'auth-1',
      apiKeyRequired: false,
      authorizationScopes: [],
      integrationType: 'MOCK',
      integrationUri: null,
    });
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [
        { id: 'auth-1', name: 'My Authorizer', type: 'COGNITO_USER_POOLS' },
      ],
    });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail')).toHaveTextContent('auth-1'),
    );
  });

  it('allows binding an authorizer to a method in detail view', async () => {
    const user = userEvent.setup();
    getApiGatewayRestMethodMock.mockResolvedValue({
      resourceId: 'res2',
      httpMethod: 'GET',
      authorizationType: 'COGNITO_USER_POOLS',
      authorizerId: null,
      apiKeyRequired: false,
      authorizationScopes: [],
      integrationType: 'MOCK',
      integrationUri: null,
    });
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [
        { id: 'auth-1', name: 'My Authorizer', type: 'COGNITO_USER_POOLS' },
      ],
    });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    // Wait for method detail and authorizer dropdown
    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail-authorizer')).toBeInTheDocument(),
    );

    // Select authorizer
    await user.selectOptions(
      screen.getByTestId('apigateway-method-detail-authorizer'),
      'auth-1',
    );

    // Save button should appear
    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail-save-authorizer')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-detail-save-authorizer'));

    // Verify the method was updated with the authorizer
    await waitFor(() =>
      expect(putApiGatewayRestMethodMock).toHaveBeenCalled(),
    );

    const lastCall = putApiGatewayRestMethodMock.mock.calls[putApiGatewayRestMethodMock.mock.calls.length - 1];
    expect(lastCall[0]).toBe('api-1');
    expect(lastCall[1]).toBe('res2');
    expect(lastCall[2]).toBe('GET');
    expect(lastCall[3].authorizationType).toBe('COGNITO_USER_POOLS');
    expect(lastCall[3].authorizerId).toBe('auth-1');
  });

  it('does not delete a method when the confirmation is cancelled', async () => {
    const user = userEvent.setup();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-delete-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-delete-res2-GET'));

    expect(deleteApiGatewayRestMethodMock).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('keeps the method detail open when loading its authorizers fails', async () => {
    const user = userEvent.setup();
    getApiGatewayRestMethodMock.mockResolvedValue({
      resourceId: 'res2',
      httpMethod: 'GET',
      authorizationType: 'COGNITO_USER_POOLS',
      authorizerId: null,
      apiKeyRequired: false,
      authorizationScopes: [],
      integrationType: 'MOCK',
      integrationUri: null,
    });
    getApiGatewayRestAuthorizersMock.mockRejectedValue(new Error('auth boom'));
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail-authorizer')).toBeInTheDocument(),
    );
  });

  it('clears the create-form authorizers when loading them fails', async () => {
    const user = userEvent.setup();
    getApiGatewayRestAuthorizersMock.mockRejectedValue(new Error('auth boom'));
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-auth-root1')).toBeInTheDocument(),
    );

    await user.selectOptions(
      screen.getByTestId('apigateway-method-auth-root1'),
      'COGNITO_USER_POOLS',
    );

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-authorizer-root1')).toBeInTheDocument(),
    );
    expect(
      screen.getByTestId('apigateway-method-authorizer-root1').querySelectorAll('option'),
    ).toHaveLength(1);
  });

  it('shows the integration URI and a loading indicator while authorizers load', async () => {
    const user = userEvent.setup();
    getApiGatewayRestMethodMock.mockResolvedValue({
      resourceId: 'res2',
      httpMethod: 'GET',
      authorizationType: 'COGNITO_USER_POOLS',
      authorizerId: 'auth-1',
      apiKeyRequired: false,
      authorizationScopes: [],
      integrationType: 'HTTP_PROXY',
      integrationUri: 'https://example.com/orders',
    });
    getApiGatewayRestAuthorizersMock.mockReturnValue(new Promise(() => {}));
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail')).toHaveTextContent(
        'Integration: https://example.com/orders',
      ),
    );
    expect(screen.getByTestId('apigateway-method-detail')).toHaveTextContent('(auth-1)');
    expect(screen.getByText('Loading authorizers…')).toBeInTheDocument();
  });

  it('tracks the selected resource when changing the HTTP method dropdown', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-http-res2')).toBeInTheDocument(),
    );

    await user.selectOptions(screen.getByTestId('apigateway-method-http-res2'), 'POST');

    expect(screen.getByTestId('apigateway-method-http-res2')).toHaveValue('POST');
    expect(screen.getByTestId('apigateway-method-http-root1')).toHaveValue('GET');
  });

  it('closes the method detail panel', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-detail-close'));

    await waitFor(() =>
      expect(screen.queryByTestId('apigateway-method-detail')).not.toBeInTheDocument(),
    );
  });

  it('keeps the save control available when saving the authorizer fails', async () => {
    const user = userEvent.setup();
    getApiGatewayRestMethodMock.mockResolvedValue({
      resourceId: 'res2',
      httpMethod: 'GET',
      authorizationType: 'COGNITO_USER_POOLS',
      authorizerId: null,
      apiKeyRequired: false,
      authorizationScopes: [],
      integrationType: 'MOCK',
      integrationUri: null,
    });
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [{ id: 'auth-1', name: 'My Authorizer', type: 'COGNITO_USER_POOLS' }],
    });
    putApiGatewayRestMethodMock.mockRejectedValue(new Error('save boom'));
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail-authorizer')).toBeInTheDocument(),
    );

    await user.selectOptions(screen.getByTestId('apigateway-method-detail-authorizer'), 'auth-1');

    await user.click(screen.getByTestId('apigateway-method-detail-save-authorizer'));

    await waitFor(() => expect(putApiGatewayRestMethodMock).toHaveBeenCalled());

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail-save-authorizer')).not.toBeDisabled(),
    );
  });

  it('resets the selected authorizer when switching to a non-authorizer auth type', async () => {
    const user = userEvent.setup();
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [{ id: 'auth-1', name: 'My Authorizer', type: 'COGNITO_USER_POOLS' }],
    });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-auth-root1')).toBeInTheDocument(),
    );

    await user.selectOptions(
      screen.getByTestId('apigateway-method-auth-root1'),
      'COGNITO_USER_POOLS',
    );

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-authorizer-root1')).toBeInTheDocument(),
    );

    await user.selectOptions(screen.getByTestId('apigateway-method-auth-root1'), 'NONE');

    expect(screen.queryByTestId('apigateway-method-authorizer-root1')).not.toBeInTheDocument();
  });

  it('clears the create-form authorizer selection back to none', async () => {
    const user = userEvent.setup();
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [{ id: 'auth-1', name: 'My Authorizer', type: 'COGNITO_USER_POOLS' }],
    });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-auth-root1')).toBeInTheDocument(),
    );

    await user.selectOptions(
      screen.getByTestId('apigateway-method-auth-root1'),
      'COGNITO_USER_POOLS',
    );

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-authorizer-root1')).toBeInTheDocument(),
    );

    await user.selectOptions(screen.getByTestId('apigateway-method-authorizer-root1'), 'auth-1');
    expect(screen.getByTestId('apigateway-method-authorizer-root1')).toHaveValue('auth-1');

    await user.selectOptions(screen.getByTestId('apigateway-method-authorizer-root1'), '');
    expect(screen.getByTestId('apigateway-method-authorizer-root1')).toHaveValue('');
  });

  it('clears the bound authorizer back to none in the method detail view', async () => {
    const user = userEvent.setup();
    getApiGatewayRestMethodMock.mockResolvedValue({
      resourceId: 'res2',
      httpMethod: 'GET',
      authorizationType: 'COGNITO_USER_POOLS',
      authorizerId: 'auth-1',
      apiKeyRequired: false,
      authorizationScopes: [],
      integrationType: 'MOCK',
      integrationUri: null,
    });
    getApiGatewayRestAuthorizersMock.mockResolvedValue({
      authorizers: [{ id: 'auth-1', name: 'My Authorizer', type: 'COGNITO_USER_POOLS' }],
    });
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-view-res2-GET')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('apigateway-method-view-res2-GET'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-method-detail-authorizer')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-method-detail-authorizer')).toHaveValue('auth-1');

    await user.selectOptions(screen.getByTestId('apigateway-method-detail-authorizer'), '');

    expect(screen.getByTestId('apigateway-method-detail-authorizer')).toHaveValue('');
  });
});
