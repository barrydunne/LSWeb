import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { CognitoDetailView } from './CognitoDetailView';
import {
  createUserPoolClient,
  deleteUserPoolClient,
  getCognitoUsers,
  getUserPool,
  getUserPoolClient,
  getUserPoolClients,
  regenerateUserPoolClientSecret,
  requestCognitoToken,
  updateUserPoolClient,
} from '../../api/client';
import type {
  UserPoolClientDetailResult,
  UserPoolClientListResult,
  UserPoolDetailResult,
} from '../../api/client';

vi.mock('../../api/client');

const getUserPoolMock = vi.mocked(getUserPool);
const getUserPoolClientsMock = vi.mocked(getUserPoolClients);
const getUserPoolClientMock = vi.mocked(getUserPoolClient);
const createUserPoolClientMock = vi.mocked(createUserPoolClient);
const updateUserPoolClientMock = vi.mocked(updateUserPoolClient);
const deleteUserPoolClientMock = vi.mocked(deleteUserPoolClient);
const getCognitoUsersMock = vi.mocked(getCognitoUsers);
const regenerateUserPoolClientSecretMock = vi.mocked(regenerateUserPoolClientSecret);
const requestCognitoTokenMock = vi.mocked(requestCognitoToken);

const detailResult: UserPoolDetailResult = {
  id: 'eu-west-1_abc123',
  name: 'customers',
  arn: 'arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc123',
  mfaConfiguration: 'OPTIONAL',
  estimatedNumberOfUsers: 42,
  usernameAttributes: ['email', 'phone_number'],
  autoVerifiedAttributes: ['email'],
  creationDate: '2024-01-01T00:00:00+00:00',
  lastModifiedDate: '2024-01-02T00:00:00+00:00',
  passwordPolicy: {
    minimumLength: 8,
    requireUppercase: true,
    requireLowercase: true,
    requireNumbers: false,
    requireSymbols: false,
  },
};

const clientsResult: UserPoolClientListResult = {
  clients: [{ clientId: 'client-1', clientName: 'web', userPoolId: 'eu-west-1_abc123' }],
};

const clientDetailResult: UserPoolClientDetailResult = {
  clientId: 'client-1',
  clientName: 'web',
  userPoolId: 'eu-west-1_abc123',
  clientSecret: 'super-secret',
  generateSecret: true,
  explicitAuthFlows: ['ALLOW_USER_SRP_AUTH'],
  allowedOAuthFlows: ['code'],
  allowedOAuthScopes: ['openid'],
  callbackURLs: ['https://app/callback'],
  allowedOAuthFlowsUserPoolClient: true,
  creationDate: '2024-01-01T00:00:00+00:00',
  lastModifiedDate: '2024-01-02T00:00:00+00:00',
};

const regeneratedClientDetailResult: UserPoolClientDetailResult = {
  ...clientDetailResult,
  clientId: 'client-2',
  clientSecret: 'rotated-secret',
};

function renderView(resourceId = 'eu-west-1_abc123') {
  return render(
    <MemoryRouter>
      <CognitoDetailView serviceKey="cognito" resourceId={resourceId} />
    </MemoryRouter>,
  );
}

describe('CognitoDetailView', () => {
  beforeEach(() => {
    getUserPoolMock.mockResolvedValue(detailResult);
    getUserPoolClientsMock.mockResolvedValue(clientsResult);
    getUserPoolClientMock.mockResolvedValue(clientDetailResult);
    createUserPoolClientMock.mockResolvedValue(clientDetailResult);
    updateUserPoolClientMock.mockResolvedValue(undefined);
    deleteUserPoolClientMock.mockResolvedValue(undefined);
    getCognitoUsersMock.mockResolvedValue({ users: [] });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before the user pool arrives', () => {
    getUserPoolMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('cognito-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getUserPoolMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-detail-error')).toBeInTheDocument());
  });

  it('requests the user pool by its resource id', async () => {
    renderView('eu-west-1_xyz');

    await waitFor(() => expect(getUserPoolMock).toHaveBeenCalled());

    expect(getUserPoolMock).toHaveBeenCalledWith('eu-west-1_xyz', expect.any(AbortSignal));
  });

  it('renders the user pool metadata', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('cognito-detail-name')).toHaveTextContent('customers');
    expect(screen.getByTestId('cognito-detail-id')).toHaveTextContent('eu-west-1_abc123');
    expect(screen.getByTestId('cognito-detail-arn')).toHaveTextContent(
      'arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc123',
    );
    expect(screen.getByTestId('cognito-detail-mfa')).toHaveTextContent('OPTIONAL');
    expect(screen.getByTestId('cognito-detail-users')).toHaveTextContent('42');
    expect(screen.getByTestId('cognito-detail-username-attributes')).toHaveTextContent(
      'email, phone_number',
    );
    expect(screen.getByTestId('cognito-detail-auto-verified-attributes')).toHaveTextContent(
      'email',
    );
    expect(screen.getByTestId('cognito-detail-created')).toHaveTextContent(
      '2024-01-01T00:00:00+00:00',
    );
    expect(screen.getByTestId('cognito-detail-modified')).toHaveTextContent(
      '2024-01-02T00:00:00+00:00',
    );
    expect(screen.getByTestId('cognito-detail-password-policy')).toHaveTextContent(
      'Min 8; uppercase, lowercase',
    );
  });

  it('renders placeholders when optional fields are absent', async () => {
    getUserPoolMock.mockResolvedValue({
      ...detailResult,
      arn: null,
      mfaConfiguration: null,
      estimatedNumberOfUsers: null,
      usernameAttributes: [],
      autoVerifiedAttributes: [],
      creationDate: null,
      lastModifiedDate: null,
      passwordPolicy: null,
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('cognito-detail-arn')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-detail-mfa')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-detail-users')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-detail-username-attributes')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-detail-auto-verified-attributes')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-detail-created')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-detail-modified')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-detail-password-policy')).toHaveTextContent('—');
  });

  it('shows the password policy with no complexity rules', async () => {
    getUserPoolMock.mockResolvedValue({
      ...detailResult,
      passwordPolicy: {
        minimumLength: 6,
        requireUppercase: false,
        requireLowercase: false,
        requireNumbers: false,
        requireSymbols: false,
      },
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('cognito-detail-password-policy')).toHaveTextContent(
      'Min 6; no complexity rules',
    );
  });

  it('shows the password policy requiring numbers and symbols', async () => {
    getUserPoolMock.mockResolvedValue({
      ...detailResult,
      passwordPolicy: {
        minimumLength: 12,
        requireUppercase: false,
        requireLowercase: false,
        requireNumbers: true,
        requireSymbols: true,
      },
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('cognito-detail-password-policy')).toHaveTextContent(
      'Min 12; numbers, symbols',
    );
  });
});

describe('CognitoDetailView app clients', () => {
  beforeEach(() => {
    getUserPoolMock.mockResolvedValue(detailResult);
    getUserPoolClientsMock.mockResolvedValue(clientsResult);
    getUserPoolClientMock.mockResolvedValue(clientDetailResult);
    createUserPoolClientMock.mockResolvedValue(clientDetailResult);
    updateUserPoolClientMock.mockResolvedValue(undefined);
    deleteUserPoolClientMock.mockResolvedValue(undefined);
    getCognitoUsersMock.mockResolvedValue({ users: [] });
    regenerateUserPoolClientSecretMock.mockResolvedValue(regeneratedClientDetailResult);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state for the clients list', async () => {
    getUserPoolClientsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-section')).toBeInTheDocument());
    expect(screen.getByTestId('cognito-clients-loading')).toBeInTheDocument();
  });

  it('shows an error state when the clients list fails', async () => {
    getUserPoolClientsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-error')).toBeInTheDocument());
    expect(getUserPoolClientsMock).toHaveBeenCalledWith('eu-west-1_abc123', expect.any(AbortSignal));
  });

  it('shows an empty state when there are no app clients', async () => {
    getUserPoolClientsMock.mockResolvedValue({ clients: [] });

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-empty')).toBeInTheDocument());
  });

  it('lists the app clients', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());
    expect(screen.getByTestId('cognito-client-row')).toHaveTextContent('web (client-1)');
  });

  it('creates an app client and refreshes the list', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-create-toggle'));
    await user.type(screen.getByTestId('cognito-client-create-name'), 'mobile');
    await user.click(screen.getByTestId('cognito-client-create-generate-secret'));
    await user.click(
      screen.getByTestId('cognito-client-create-auth-flow-ALLOW_USER_SRP_AUTH'),
    );
    await user.type(screen.getByTestId('cognito-client-create-oauth-flows'), 'code');
    await user.type(screen.getByTestId('cognito-client-create-oauth-scopes'), 'openid, email');
    await user.type(screen.getByTestId('cognito-client-create-callbacks'), 'https://app/callback');
    await user.click(screen.getByTestId('cognito-client-create-oauth-user-pool-client'));
    await user.click(screen.getByTestId('cognito-client-create-submit'));

    await waitFor(() =>
      expect(createUserPoolClientMock).toHaveBeenCalledWith('eu-west-1_abc123', {
        clientName: 'mobile',
        generateSecret: true,
        explicitAuthFlows: ['ALLOW_USER_SRP_AUTH'],
        allowedOAuthFlows: ['code'],
        allowedOAuthScopes: ['openid', 'email'],
        callbackURLs: ['https://app/callback'],
        allowedOAuthFlowsUserPoolClient: true,
      }),
    );

    await waitFor(() =>
      expect(screen.queryByTestId('cognito-client-create-form')).not.toBeInTheDocument(),
    );
    expect(getUserPoolClientsMock).toHaveBeenCalledTimes(2);
  });

  it('shows an error when creating an app client fails', async () => {
    createUserPoolClientMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-create-toggle'));
    await user.type(screen.getByTestId('cognito-client-create-name'), 'mobile');
    await user.click(screen.getByTestId('cognito-client-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-create-error')).toBeInTheDocument(),
    );
  });

  it('toggles the create form closed again', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-create-toggle'));
    expect(screen.getByTestId('cognito-client-create-form')).toBeInTheDocument();

    await user.click(screen.getByTestId('cognito-client-create-toggle'));
    expect(screen.queryByTestId('cognito-client-create-form')).not.toBeInTheDocument();
  });

  it('shows a loading state while a client detail is fetched', async () => {
    getUserPoolClientMock.mockReturnValue(new Promise(() => {}));
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-detail-loading')).toBeInTheDocument(),
    );
  });

  it('shows an error state when a client detail fails', async () => {
    getUserPoolClientMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-detail-error')).toBeInTheDocument(),
    );
  });

  it('shows the selected client detail and toggles the secret', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));

    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());
    expect(getUserPoolClientMock).toHaveBeenCalledWith('eu-west-1_abc123', 'client-1');
    expect(screen.getByTestId('cognito-client-detail-id')).toHaveTextContent('client-1');
    expect(screen.getByTestId('cognito-client-detail-name')).toHaveTextContent('web');
    expect(screen.getByTestId('cognito-client-detail-auth-flows')).toHaveTextContent(
      'ALLOW_USER_SRP_AUTH',
    );
    expect(screen.getByTestId('cognito-client-detail-oauth-flows')).toHaveTextContent('code');
    expect(screen.getByTestId('cognito-client-detail-oauth-scopes')).toHaveTextContent('openid');
    expect(screen.getByTestId('cognito-client-detail-callbacks')).toHaveTextContent(
      'https://app/callback',
    );

    expect(screen.getByTestId('cognito-client-detail-secret')).not.toHaveTextContent('super-secret');

    await user.click(screen.getByTestId('cognito-client-detail-secret-toggle'));
    expect(screen.getByTestId('cognito-client-detail-secret')).toHaveTextContent('super-secret');

    await user.click(screen.getByTestId('cognito-client-detail-secret-toggle'));
    expect(screen.getByTestId('cognito-client-detail-secret')).not.toHaveTextContent('super-secret');
  });

  it('renders placeholders when the client secret and lists are absent', async () => {
    getUserPoolClientMock.mockResolvedValue({
      ...clientDetailResult,
      clientSecret: null,
      explicitAuthFlows: [],
      allowedOAuthFlows: [],
      allowedOAuthScopes: [],
      callbackURLs: [],
    });
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));

    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());
    expect(screen.getByTestId('cognito-client-detail-secret')).toHaveTextContent('—');
    expect(screen.queryByTestId('cognito-client-detail-secret-toggle')).not.toBeInTheDocument();
    expect(screen.getByTestId('cognito-client-detail-auth-flows')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-client-detail-oauth-flows')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-client-detail-oauth-scopes')).toHaveTextContent('—');
    expect(screen.getByTestId('cognito-client-detail-callbacks')).toHaveTextContent('—');
  });

  it('edits the selected client', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));
    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-edit-toggle'));
    const nameInput = screen.getByTestId('cognito-client-edit-name');
    expect(nameInput).toHaveValue('web');

    await user.clear(nameInput);
    await user.type(nameInput, 'web-renamed');

    await user.click(
      screen.getByTestId('cognito-client-edit-auth-flow-ALLOW_USER_SRP_AUTH'),
    );
    await user.click(
      screen.getByTestId('cognito-client-edit-auth-flow-ALLOW_USER_PASSWORD_AUTH'),
    );

    const oauthFlows = screen.getByTestId('cognito-client-edit-oauth-flows');
    await user.clear(oauthFlows);
    await user.type(oauthFlows, 'implicit');

    const oauthScopes = screen.getByTestId('cognito-client-edit-oauth-scopes');
    await user.clear(oauthScopes);
    await user.type(oauthScopes, 'openid, profile');

    const callbacks = screen.getByTestId('cognito-client-edit-callbacks');
    await user.clear(callbacks);
    await user.type(callbacks, 'https://app/new');

    await user.click(screen.getByTestId('cognito-client-edit-oauth-user-pool-client'));
    await user.click(screen.getByTestId('cognito-client-edit-submit'));

    await waitFor(() =>
      expect(updateUserPoolClientMock).toHaveBeenCalledWith('eu-west-1_abc123', 'client-1', {
        clientName: 'web-renamed',
        explicitAuthFlows: ['ALLOW_USER_PASSWORD_AUTH'],
        allowedOAuthFlows: ['implicit'],
        allowedOAuthScopes: ['openid', 'profile'],
        callbackURLs: ['https://app/new'],
        allowedOAuthFlowsUserPoolClient: false,
      }),
    );

    await waitFor(() =>
      expect(screen.queryByTestId('cognito-client-edit-form')).not.toBeInTheDocument(),
    );
  });

  it('shows an error when editing the client fails', async () => {
    updateUserPoolClientMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));
    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-edit-toggle'));
    await user.click(screen.getByTestId('cognito-client-edit-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-edit-error')).toBeInTheDocument(),
    );
  });

  it('deletes an app client and clears the selection', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));
    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());

    const row = screen.getByTestId('cognito-client-row');
    await user.click(within(row).getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteUserPoolClientMock).toHaveBeenCalledWith('eu-west-1_abc123', 'client-1'),
    );
    await waitFor(() =>
      expect(screen.queryByTestId('cognito-client-detail')).not.toBeInTheDocument(),
    );
    expect(getUserPoolClientsMock).toHaveBeenCalledTimes(2);
  });

  it('deletes an app client when none is selected', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    const row = screen.getByTestId('cognito-client-row');
    await user.click(within(row).getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteUserPoolClientMock).toHaveBeenCalledWith('eu-west-1_abc123', 'client-1'),
    );
    expect(getUserPoolClientsMock).toHaveBeenCalledTimes(2);
  });

  it('shows an error when deleting an app client fails', async () => {
    deleteUserPoolClientMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    const row = screen.getByTestId('cognito-client-row');
    await user.click(within(row).getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('cognito-clients-error')).toBeInTheDocument());
  });

  it('regenerates the client secret and reveals the new one', async () => {
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));
    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-regenerate-secret'));

    await waitFor(() =>
      expect(regenerateUserPoolClientSecretMock).toHaveBeenCalledWith('eu-west-1_abc123', 'client-1'),
    );
    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-detail-id')).toHaveTextContent('client-2'),
    );
    expect(screen.getByTestId('cognito-client-detail-secret')).toHaveTextContent('rotated-secret');
    expect(getUserPoolClientsMock).toHaveBeenCalledTimes(2);
  });

  it('shows an error when regenerating the client secret fails', async () => {
    regenerateUserPoolClientSecretMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));
    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-regenerate-secret'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-regenerate-error')).toBeInTheDocument(),
    );
  });

  it('requests a token and shows the issued tokens and claims', async () => {
    requestCognitoTokenMock.mockResolvedValue({
      accessToken: 'access-token',
      idToken: 'id-token',
      refreshToken: 'refresh-token',
      tokenType: 'Bearer',
      expiresIn: 3600,
      claims: [{ name: 'sub', value: 'abc' }],
    });
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));
    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());

    await user.type(screen.getByTestId('cognito-client-token-username'), 'alice');
    await user.type(screen.getByTestId('cognito-client-token-password'), 'Passw0rd!');
    await user.click(screen.getByTestId('cognito-client-token-submit'));

    await waitFor(() =>
      expect(requestCognitoTokenMock).toHaveBeenCalledWith('eu-west-1_abc123', 'client-1', {
        username: 'alice',
        password: 'Passw0rd!',
      }),
    );
    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-token-result')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('cognito-client-token-access')).toHaveTextContent('access-token');
    expect(screen.getByTestId('cognito-client-token-id')).toHaveTextContent('id-token');
    expect(screen.getByTestId('cognito-client-token-refresh')).toHaveTextContent('refresh-token');
    expect(screen.getByTestId('cognito-client-token-type')).toHaveTextContent('Bearer');
    expect(screen.getByTestId('cognito-client-token-expires')).toHaveTextContent('3600');
    expect(screen.getByTestId('cognito-client-token-claims')).toHaveTextContent('sub: abc');
  });

  it('shows placeholders and no claims when the token response is empty', async () => {
    requestCognitoTokenMock.mockResolvedValue({
      accessToken: null,
      idToken: null,
      refreshToken: null,
      tokenType: null,
      expiresIn: null,
      claims: [],
    });
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));
    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());

    await user.type(screen.getByTestId('cognito-client-token-username'), 'alice');
    await user.type(screen.getByTestId('cognito-client-token-password'), 'Passw0rd!');
    await user.click(screen.getByTestId('cognito-client-token-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-token-claims-empty')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('cognito-client-token-access')).toHaveTextContent('—');
  });

  it('shows an error when the token request fails', async () => {
    requestCognitoTokenMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-clients-list')).toBeInTheDocument());

    await user.click(screen.getByTestId('cognito-client-view'));
    await waitFor(() => expect(screen.getByTestId('cognito-client-detail')).toBeInTheDocument());

    await user.type(screen.getByTestId('cognito-client-token-username'), 'alice');
    await user.type(screen.getByTestId('cognito-client-token-password'), 'Passw0rd!');
    await user.click(screen.getByTestId('cognito-client-token-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-client-token-error')).toBeInTheDocument(),
    );
  });
});
