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
  putApiGatewayRestMethod,
} from '../../api/client';
import type { ApiGatewayRestResourceItem } from '../../api/client';

vi.mock('../../api/client');

const getApiGatewayRestResourcesMock = vi.mocked(getApiGatewayRestResources);
const createApiGatewayRestResourceMock = vi.mocked(createApiGatewayRestResource);
const deleteApiGatewayRestResourceMock = vi.mocked(deleteApiGatewayRestResource);
const getApiGatewayRestMethodMock = vi.mocked(getApiGatewayRestMethod);
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
    await user.click(screen.getByTestId('apigateway-method-add-res2'));

    await waitFor(() =>
      expect(putApiGatewayRestMethodMock).toHaveBeenCalledWith('api-1', 'res2', 'POST', {
        authorizationType: 'COGNITO_USER_POOLS',
        authorizerId: null,
        apiKeyRequired: false,
        authorizationScopes: [],
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
});
