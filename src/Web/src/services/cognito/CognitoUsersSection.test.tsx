import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CognitoUsersSection } from './CognitoUsersSection';
import {
  createCognitoUser,
  deleteCognitoUser,
  getCognitoUser,
  getCognitoUsers,
  setCognitoUserEnabled,
  setCognitoUserPassword,
} from '../../api/client';
import type { CognitoUserDetailResult, CognitoUserListResult } from '../../api/client';

vi.mock('../../api/client');

const getCognitoUsersMock = vi.mocked(getCognitoUsers);
const getCognitoUserMock = vi.mocked(getCognitoUser);
const createCognitoUserMock = vi.mocked(createCognitoUser);
const deleteCognitoUserMock = vi.mocked(deleteCognitoUser);
const setCognitoUserEnabledMock = vi.mocked(setCognitoUserEnabled);
const setCognitoUserPasswordMock = vi.mocked(setCognitoUserPassword);

const usersResult: CognitoUserListResult = {
  users: [
    { username: 'alice', status: 'CONFIRMED', enabled: true, createdDate: '2024-01-01T00:00:00+00:00' },
    { username: 'bob', status: 'FORCE_CHANGE_PASSWORD', enabled: false, createdDate: null },
  ],
};

const userDetailResult: CognitoUserDetailResult = {
  username: 'alice',
  status: 'CONFIRMED',
  enabled: true,
  attributes: [
    { name: 'email', value: 'alice@example.com' },
    { name: 'custom:note', value: '' },
  ],
  createdDate: '2024-01-01T00:00:00+00:00',
  lastModifiedDate: '2024-01-02T00:00:00+00:00',
};

function renderSection(poolId = 'eu-west-1_abc123') {
  return render(<CognitoUsersSection poolId={poolId} />);
}

describe('CognitoUsersSection', () => {
  beforeEach(() => {
    getCognitoUsersMock.mockResolvedValue(usersResult);
    getCognitoUserMock.mockResolvedValue(userDetailResult);
    createCognitoUserMock.mockResolvedValue(userDetailResult);
    deleteCognitoUserMock.mockResolvedValue(undefined);
    setCognitoUserEnabledMock.mockResolvedValue(undefined);
    setCognitoUserPasswordMock.mockResolvedValue(undefined);
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before the users arrive', () => {
    getCognitoUsersMock.mockReturnValue(new Promise(() => {}));

    renderSection();

    expect(screen.getByTestId('cognito-users-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getCognitoUsersMock.mockRejectedValue(new Error('boom'));

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('cognito-users-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty state when there are no users', async () => {
    getCognitoUsersMock.mockResolvedValue({ users: [] });

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('cognito-users-empty')).toBeInTheDocument(),
    );
  });

  it('lists the users with status and disabled markers', async () => {
    renderSection();

    const rows = await screen.findAllByTestId('cognito-user-row');
    expect(rows).toHaveLength(2);
    expect(rows[0]).toHaveTextContent('alice (CONFIRMED)');
    expect(rows[1]).toHaveTextContent('bob (FORCE_CHANGE_PASSWORD, disabled)');
  });

  it('toggles and submits the create user form', async () => {
    const user = userEvent.setup();
    renderSection();
    await screen.findAllByTestId('cognito-user-row');

    await user.click(screen.getByTestId('cognito-user-create-toggle'));
    await user.type(screen.getByTestId('cognito-user-create-username'), 'carol');
    await user.type(screen.getByTestId('cognito-user-create-email'), 'carol@example.com');
    await user.type(screen.getByTestId('cognito-user-create-password'), 'Temp123!');
    await user.click(screen.getByTestId('cognito-user-create-submit'));

    await waitFor(() =>
      expect(createCognitoUserMock).toHaveBeenCalledWith('eu-west-1_abc123', {
        username: 'carol',
        attributes: [{ name: 'email', value: 'carol@example.com' }],
        temporaryPassword: 'Temp123!',
      }),
    );
    await waitFor(() =>
      expect(screen.queryByTestId('cognito-user-create-form')).not.toBeInTheDocument(),
    );
  });

  it('creates a user without email or temporary password', async () => {
    const user = userEvent.setup();
    renderSection();
    await screen.findAllByTestId('cognito-user-row');

    await user.click(screen.getByTestId('cognito-user-create-toggle'));
    await user.type(screen.getByTestId('cognito-user-create-username'), 'dan');
    await user.click(screen.getByTestId('cognito-user-create-submit'));

    await waitFor(() =>
      expect(createCognitoUserMock).toHaveBeenCalledWith('eu-west-1_abc123', {
        username: 'dan',
        attributes: [],
        temporaryPassword: null,
      }),
    );
  });

  it('shows an error when create fails', async () => {
    createCognitoUserMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();
    await screen.findAllByTestId('cognito-user-row');

    await user.click(screen.getByTestId('cognito-user-create-toggle'));
    await user.type(screen.getByTestId('cognito-user-create-username'), 'erin');
    await user.click(screen.getByTestId('cognito-user-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-user-create-error')).toBeInTheDocument(),
    );
  });

  it('views a user with attributes including an empty value', async () => {
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('cognito-user-view'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-user-detail-username')).toHaveTextContent('alice'),
    );
    expect(screen.getByTestId('cognito-user-detail-status')).toHaveTextContent('CONFIRMED');
    expect(screen.getByTestId('cognito-user-detail-enabled')).toHaveTextContent('Yes');
    const attributes = screen.getByTestId('cognito-user-detail-attributes');
    expect(attributes).toHaveTextContent('email: alice@example.com');
    expect(attributes).toHaveTextContent('custom:note: —');
  });

  it('shows a user with no attributes', async () => {
    getCognitoUserMock.mockResolvedValue({ ...userDetailResult, enabled: false, attributes: [] });
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('cognito-user-view'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-user-detail-attributes-empty')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('cognito-user-detail-enabled')).toHaveTextContent('No');
  });

  it('shows a loading and error state for the user detail', async () => {
    getCognitoUserMock.mockReturnValueOnce(new Promise(() => {}));
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('cognito-user-view'));
    expect(screen.getByTestId('cognito-user-detail-loading')).toBeInTheDocument();

    getCognitoUserMock.mockRejectedValueOnce(new Error('boom'));
    await user.click(within(rows[1]).getByTestId('cognito-user-view'));
    await waitFor(() =>
      expect(screen.getByTestId('cognito-user-detail-error')).toBeInTheDocument(),
    );
  });

  it('toggles a user enabled state', async () => {
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('cognito-user-toggle-enabled'));

    await waitFor(() =>
      expect(setCognitoUserEnabledMock).toHaveBeenCalledWith('eu-west-1_abc123', 'alice', false),
    );
  });

  it('shows an error when toggling enabled fails', async () => {
    setCognitoUserEnabledMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[1]).getByTestId('cognito-user-toggle-enabled'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-users-action-error')).toHaveTextContent(
        'Unable to update bob.',
      ),
    );
  });

  it('resets a user password', async () => {
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('cognito-user-reset-password'));
    await user.type(screen.getByTestId('cognito-user-password-value'), 'NewPass1!');
    await user.click(screen.getByTestId('cognito-user-password-permanent'));
    await user.click(screen.getByTestId('cognito-user-password-submit'));

    await waitFor(() =>
      expect(setCognitoUserPasswordMock).toHaveBeenCalledWith('eu-west-1_abc123', 'alice', {
        password: 'NewPass1!',
        permanent: false,
      }),
    );
    await waitFor(() =>
      expect(screen.queryByTestId('cognito-user-password-form')).not.toBeInTheDocument(),
    );
  });

  it('shows an error when setting the password fails', async () => {
    setCognitoUserPasswordMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('cognito-user-reset-password'));
    await user.type(screen.getByTestId('cognito-user-password-value'), 'NewPass1!');
    await user.click(screen.getByTestId('cognito-user-password-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-user-password-error')).toBeInTheDocument(),
    );
  });

  it('deletes a user and clears the selection', async () => {
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('cognito-user-view'));
    await waitFor(() =>
      expect(screen.getByTestId('cognito-user-detail-username')).toBeInTheDocument(),
    );

    await user.click(within(rows[0]).getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteCognitoUserMock).toHaveBeenCalledWith('eu-west-1_abc123', 'alice'),
    );
    await waitFor(() =>
      expect(screen.queryByTestId('cognito-user-detail')).not.toBeInTheDocument(),
    );
  });

  it('keeps the selection when deleting a different user', async () => {
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('cognito-user-view'));
    await waitFor(() =>
      expect(screen.getByTestId('cognito-user-detail-username')).toBeInTheDocument(),
    );

    await user.click(within(rows[1]).getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteCognitoUserMock).toHaveBeenCalledWith('eu-west-1_abc123', 'bob'),
    );
    expect(screen.getByTestId('cognito-user-detail')).toBeInTheDocument();
  });

  it('shows an action error when delete fails', async () => {
    deleteCognitoUserMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();
    const rows = await screen.findAllByTestId('cognito-user-row');

    await user.click(within(rows[0]).getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('cognito-users-action-error')).toHaveTextContent(
        'Unable to delete alice.',
      ),
    );
  });

  it('hides the create form when toggled twice', async () => {
    const user = userEvent.setup();
    renderSection();
    await screen.findAllByTestId('cognito-user-row');

    await user.click(screen.getByTestId('cognito-user-create-toggle'));
    expect(screen.getByTestId('cognito-user-create-form')).toBeInTheDocument();
    await user.click(screen.getByTestId('cognito-user-create-toggle'));
    expect(screen.queryByTestId('cognito-user-create-form')).not.toBeInTheDocument();
  });
});
