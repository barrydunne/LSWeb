import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ApiGatewayAuthorizersSection } from './ApiGatewayAuthorizersSection';
import {
  createApiGatewayRestAuthorizer,
  createApiGatewayRestTokenAuthorizer,
  deleteApiGatewayRestAuthorizer,
  getApiGatewayRestAuthorizer,
  getApiGatewayRestAuthorizers,
  getUserPool,
  getUserPools,
} from '../../api/client';
import type {
  ApiGatewayRestAuthorizerItem,
  UserPoolSummaryItem,
} from '../../api/client';

vi.mock('../../api/client');

const getAuthorizersMock = vi.mocked(getApiGatewayRestAuthorizers);
const getAuthorizerMock = vi.mocked(getApiGatewayRestAuthorizer);
const createAuthorizerMock = vi.mocked(createApiGatewayRestAuthorizer);
const createTokenAuthorizerMock = vi.mocked(createApiGatewayRestTokenAuthorizer);
const deleteAuthorizerMock = vi.mocked(deleteApiGatewayRestAuthorizer);
const getUserPoolsMock = vi.mocked(getUserPools);
const getUserPoolMock = vi.mocked(getUserPool);

const POOL_ARN = 'arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc';

const authorizer: ApiGatewayRestAuthorizerItem = {
  id: 'auth1',
  name: 'pool-authorizer',
  type: 'COGNITO_USER_POOLS',
};

const userPool: UserPoolSummaryItem = {
  id: 'pool1',
  name: 'my-pool',
  creationDate: null,
};

function renderSection() {
  return render(<ApiGatewayAuthorizersSection restApiId="api-1" />);
}

describe('ApiGatewayAuthorizersSection', () => {
  beforeEach(() => {
    getAuthorizersMock.mockResolvedValue({ authorizers: [authorizer] });
    getUserPoolsMock.mockResolvedValue({ userPools: [userPool] });
    getAuthorizerMock.mockResolvedValue({
      id: 'auth1',
      name: 'pool-authorizer',
      type: 'COGNITO_USER_POOLS',
      providerARNs: [POOL_ARN],
      identitySource: 'method.request.header.Authorization',
      authType: 'COGNITO_USER_POOLS',
    });
    getUserPoolMock.mockResolvedValue({
      id: 'pool1',
      name: 'my-pool',
      arn: POOL_ARN,
      mfaConfiguration: null,
      estimatedNumberOfUsers: null,
      usernameAttributes: [],
      autoVerifiedAttributes: [],
      creationDate: null,
      lastModifiedDate: null,
    });
    createAuthorizerMock.mockResolvedValue({ id: 'auth9' });
    createTokenAuthorizerMock.mockResolvedValue({ id: 'auth7' });
    deleteAuthorizerMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before data arrives', () => {
    getAuthorizersMock.mockReturnValue(new Promise(() => {}));

    renderSection();

    expect(screen.getByTestId('apigateway-authorizers-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getAuthorizersMock.mockRejectedValue(new Error('boom'));

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizers-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty message when there are no authorizers', async () => {
    getAuthorizersMock.mockResolvedValue({ authorizers: [] });

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizers-empty')).toBeInTheDocument(),
    );
  });

  it('lists authorizers when the request succeeds', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-auth1')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-authorizer-name-auth1')).toHaveTextContent(
      'pool-authorizer',
    );
    expect(screen.getByTestId('apigateway-authorizer-type-auth1')).toHaveTextContent(
      'COGNITO_USER_POOLS',
    );
  });

  it('shows authorizer detail when View is clicked', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-view-auth1')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-authorizer-view-auth1'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-detail')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-authorizer-detail-arns')).toHaveTextContent(POOL_ARN);
    expect(screen.getByTestId('apigateway-authorizer-detail-identity')).toHaveTextContent(
      'method.request.header.Authorization',
    );
    expect(screen.getByTestId('apigateway-authorizer-detail-authtype')).toHaveTextContent(
      'COGNITO_USER_POOLS',
    );
    expect(getAuthorizerMock).toHaveBeenCalledWith('api-1', 'auth1');
  });

  it('renders placeholders when the detail has null identity source and auth type', async () => {
    getAuthorizerMock.mockResolvedValue({
      id: 'auth1',
      name: 'pool-authorizer',
      type: 'COGNITO_USER_POOLS',
      providerARNs: [POOL_ARN],
      identitySource: null,
      authType: null,
    });
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-view-auth1')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-authorizer-view-auth1'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-detail-identity')).toHaveTextContent(
        '\u2014',
      ),
    );
    expect(screen.getByTestId('apigateway-authorizer-detail-authtype')).toHaveTextContent(
      '\u2014',
    );
  });

  it('keeps the detail hidden when the view request fails', async () => {
    getAuthorizerMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-view-auth1')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-authorizer-view-auth1'));

    await waitFor(() => expect(getAuthorizerMock).toHaveBeenCalled());
    expect(screen.queryByTestId('apigateway-authorizer-detail')).not.toBeInTheDocument();
  });

  it('disables the add button until a name and pool are chosen', async () => {
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-add')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-authorizer-add')).toBeDisabled();

    const user = userEvent.setup();
    await user.type(screen.getByTestId('apigateway-authorizer-name'), 'new-authorizer');
    expect(screen.getByTestId('apigateway-authorizer-add')).toBeDisabled();

    await user.selectOptions(screen.getByTestId('apigateway-authorizer-pool'), 'pool1');
    expect(screen.getByTestId('apigateway-authorizer-add')).toBeEnabled();
  });

  it('creates an authorizer from the selected user pool', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-authorizer-name'), 'new-authorizer');
    await user.selectOptions(screen.getByTestId('apigateway-authorizer-pool'), 'pool1');
    await user.type(
      screen.getByTestId('apigateway-authorizer-identity'),
      'method.request.header.Authorization',
    );
    await user.click(screen.getByTestId('apigateway-authorizer-add'));

    await waitFor(() => expect(createAuthorizerMock).toHaveBeenCalled());
    expect(getUserPoolMock).toHaveBeenCalledWith('pool1');
    expect(createAuthorizerMock).toHaveBeenCalledWith('api-1', {
      name: 'new-authorizer',
      type: 'COGNITO_USER_POOLS',
      providerARNs: [POOL_ARN],
      identitySource: 'method.request.header.Authorization',
    });
    expect(getAuthorizersMock).toHaveBeenCalledTimes(2);
  });

  it('creates an authorizer with a null identity source when left blank', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-authorizer-name'), 'new-authorizer');
    await user.selectOptions(screen.getByTestId('apigateway-authorizer-pool'), 'pool1');
    await user.click(screen.getByTestId('apigateway-authorizer-add'));

    await waitFor(() => expect(createAuthorizerMock).toHaveBeenCalled());
    expect(createAuthorizerMock).toHaveBeenCalledWith('api-1', {
      name: 'new-authorizer',
      type: 'COGNITO_USER_POOLS',
      providerARNs: [POOL_ARN],
      identitySource: null,
    });
  });

  it('shows an add error when the selected pool has no ARN', async () => {
    getUserPoolMock.mockResolvedValue({
      id: 'pool1',
      name: 'my-pool',
      arn: null,
      mfaConfiguration: null,
      estimatedNumberOfUsers: null,
      usernameAttributes: [],
      autoVerifiedAttributes: [],
      creationDate: null,
      lastModifiedDate: null,
    });
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-authorizer-name'), 'new-authorizer');
    await user.selectOptions(screen.getByTestId('apigateway-authorizer-pool'), 'pool1');
    await user.click(screen.getByTestId('apigateway-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-add-error')).toBeInTheDocument(),
    );
    expect(createAuthorizerMock).not.toHaveBeenCalled();
  });

  it('shows an add error when the create request fails', async () => {
    createAuthorizerMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-authorizer-name'), 'new-authorizer');
    await user.selectOptions(screen.getByTestId('apigateway-authorizer-pool'), 'pool1');
    await user.click(screen.getByTestId('apigateway-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-add-error')).toBeInTheDocument(),
    );
  });

  it('creates a token authorizer from the guided form', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-name'), 'jwt-authorizer');
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-issuer'),
      'https://issuer.example.com',
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-audience'), 'api://default');
    await user.clear(screen.getByTestId('apigateway-token-authorizer-identity'));
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-identity'),
      'method.request.header.Auth',
    );
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-uri'),
      'arn:aws:apigateway:eu-west-1:lambda:path/invocations',
    );
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() => expect(createTokenAuthorizerMock).toHaveBeenCalled());
    expect(createTokenAuthorizerMock).toHaveBeenCalledWith('api-1', {
      name: 'jwt-authorizer',
      issuer: 'https://issuer.example.com',
      audience: 'api://default',
      identitySource: 'method.request.header.Auth',
      authorizerUri: 'arn:aws:apigateway:eu-west-1:lambda:path/invocations',
    });
    expect(getAuthorizersMock).toHaveBeenCalledTimes(2);
  });

  it('rejects a token authorizer with no name', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-error')).toHaveTextContent(
        'Name is required.',
      ),
    );
    expect(createTokenAuthorizerMock).not.toHaveBeenCalled();
  });

  it('rejects a token authorizer with a non-absolute issuer', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-name'), 'jwt-authorizer');
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-error')).toHaveTextContent(
        'Issuer must be an absolute https URL.',
      ),
    );
    expect(createTokenAuthorizerMock).not.toHaveBeenCalled();
  });

  it('rejects a token authorizer with a non-https issuer', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-name'), 'jwt-authorizer');
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-issuer'),
      'http://issuer.example.com',
    );
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-error')).toHaveTextContent(
        'Issuer must be an absolute https URL.',
      ),
    );
    expect(createTokenAuthorizerMock).not.toHaveBeenCalled();
  });

  it('rejects a token authorizer with no audience', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-name'), 'jwt-authorizer');
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-issuer'),
      'https://issuer.example.com',
    );
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-error')).toHaveTextContent(
        'Audience is required.',
      ),
    );
    expect(createTokenAuthorizerMock).not.toHaveBeenCalled();
  });

  it('rejects a token authorizer with an identity source that is not a request value', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-name'), 'jwt-authorizer');
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-issuer'),
      'https://issuer.example.com',
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-audience'), 'api://default');
    await user.clear(screen.getByTestId('apigateway-token-authorizer-identity'));
    await user.type(screen.getByTestId('apigateway-token-authorizer-identity'), 'header.Auth');
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-error')).toHaveTextContent(
        'Identity source must reference a request value',
      ),
    );
    expect(createTokenAuthorizerMock).not.toHaveBeenCalled();
  });

  it('rejects a token authorizer with an identity source that is only the prefix', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-name'), 'jwt-authorizer');
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-issuer'),
      'https://issuer.example.com',
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-audience'), 'api://default');
    await user.clear(screen.getByTestId('apigateway-token-authorizer-identity'));
    await user.type(screen.getByTestId('apigateway-token-authorizer-identity'), 'method.request.');
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-error')).toHaveTextContent(
        'Identity source must reference a request value',
      ),
    );
    expect(createTokenAuthorizerMock).not.toHaveBeenCalled();
  });

  it('rejects a token authorizer with no authorizer URI', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-name'), 'jwt-authorizer');
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-issuer'),
      'https://issuer.example.com',
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-audience'), 'api://default');
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-error')).toHaveTextContent(
        'Authorizer URI is required.',
      ),
    );
    expect(createTokenAuthorizerMock).not.toHaveBeenCalled();
  });

  it('shows an error when the token authorizer create request fails', async () => {
    createTokenAuthorizerMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-add')).toBeInTheDocument(),
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-name'), 'jwt-authorizer');
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-issuer'),
      'https://issuer.example.com',
    );
    await user.type(screen.getByTestId('apigateway-token-authorizer-audience'), 'api://default');
    await user.type(
      screen.getByTestId('apigateway-token-authorizer-uri'),
      'arn:aws:apigateway:eu-west-1:lambda:path/invocations',
    );
    await user.click(screen.getByTestId('apigateway-token-authorizer-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-token-authorizer-error')).toHaveTextContent(
        'Unable to add the token authorizer.',
      ),
    );
  });

  it('deletes an authorizer after confirmation', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-delete-auth1')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-authorizer-delete-auth1'));

    await waitFor(() => expect(deleteAuthorizerMock).toHaveBeenCalledWith('api-1', 'auth1'));
    expect(getAuthorizersMock).toHaveBeenCalledTimes(2);
    confirmSpy.mockRestore();
  });

  it('does not delete an authorizer when confirmation is declined', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-delete-auth1')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-authorizer-delete-auth1'));

    expect(deleteAuthorizerMock).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('shows an error state when the delete request fails', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    deleteAuthorizerMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizer-delete-auth1')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-authorizer-delete-auth1'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-authorizers-error')).toBeInTheDocument(),
    );
  });
});
