import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { CognitoListView } from './CognitoListView';
import { createUserPool, deleteUserPool, getUserPools } from '../../api/client';
import type { UserPoolListResult } from '../../api/client';

vi.mock('../../api/client');

const getUserPoolsMock = vi.mocked(getUserPools);
const createUserPoolMock = vi.mocked(createUserPool);
const deleteUserPoolMock = vi.mocked(deleteUserPool);

const listResult: UserPoolListResult = {
  userPools: [
    {
      id: 'eu-west-1_abc123',
      name: 'customers',
      creationDate: '2024-01-01T00:00:00+00:00',
    },
    {
      id: 'eu-west-1_def456',
      name: 'admins',
      creationDate: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <CognitoListView serviceKey="cognito" />
    </MemoryRouter>,
  );
}

describe('CognitoListView', () => {
  beforeEach(() => {
    getUserPoolsMock.mockResolvedValue(listResult);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before user pools arrive', () => {
    getUserPoolsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('cognito-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getUserPoolsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-error')).toBeInTheDocument());
  });

  it('renders a row per user pool', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());

    expect(screen.getByTestId('data-list-row-eu-west-1_abc123')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-eu-west-1_def456')).toBeInTheDocument();
  });

  it('shows the name, id, and creation date for each user pool', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());

    const names = screen.getAllByTestId('cognito-list-name');
    const ids = screen.getAllByTestId('cognito-list-id');
    const created = screen.getAllByTestId('cognito-list-created');
    expect(names[0]).toHaveTextContent('customers');
    expect(ids[0]).toHaveTextContent('eu-west-1_abc123');
    expect(created[0]).toHaveTextContent('2024-01-01T00:00:00+00:00');
    expect(created[1]).toHaveTextContent('—');
  });

  it('links each user pool name to its id-keyed detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());

    const names = screen.getAllByTestId('cognito-list-name');
    expect(names[0]).toHaveAttribute('href', '/services/cognito/eu-west-1_abc123');
  });

  it('reloads the user pools when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());
      expect(getUserPoolsMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getUserPoolsMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('creates a user pool from the form and refreshes the list', async () => {
    createUserPoolMock.mockResolvedValue({ id: 'eu-west-1_new' });

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('cognito-create-toggle'));

    fireEvent.change(screen.getByTestId('cognito-create-name'), {
      target: { value: 'new-pool' },
    });
    fireEvent.change(screen.getByTestId('cognito-create-mfa'), {
      target: { value: 'OPTIONAL' },
    });
    fireEvent.change(screen.getByTestId('cognito-create-username-attributes'), {
      target: { value: 'email, phone_number' },
    });
    fireEvent.change(screen.getByTestId('cognito-create-auto-verified-attributes'), {
      target: { value: 'email' },
    });
    fireEvent.change(screen.getByTestId('cognito-create-password-length'), {
      target: { value: '10' },
    });
    fireEvent.click(screen.getByTestId('cognito-create-require-uppercase'));
    fireEvent.click(screen.getByTestId('cognito-create-require-lowercase'));
    fireEvent.click(screen.getByTestId('cognito-create-require-numbers'));
    fireEvent.click(screen.getByTestId('cognito-create-require-symbols'));

    fireEvent.click(screen.getByTestId('cognito-create-submit'));

    await waitFor(() => expect(screen.getByTestId('cognito-create-status')).toBeInTheDocument());

    expect(createUserPoolMock).toHaveBeenCalledWith({
      name: 'new-pool',
      mfaConfiguration: 'OPTIONAL',
      usernameAttributes: ['email', 'phone_number'],
      autoVerifiedAttributes: ['email'],
      passwordPolicy: {
        minimumLength: 10,
        requireUppercase: false,
        requireLowercase: false,
        requireNumbers: false,
        requireSymbols: true,
      },
    });
    await waitFor(() => expect(getUserPoolsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('cognito-create-form')).not.toBeInTheDocument();
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('cognito-create-toggle'));
    expect(screen.getByTestId('cognito-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('cognito-create-toggle'));
    expect(screen.queryByTestId('cognito-create-form')).not.toBeInTheDocument();
  });

  it('shows an error when user pool creation fails', async () => {
    createUserPoolMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('cognito-create-toggle'));
    fireEvent.change(screen.getByTestId('cognito-create-password-length'), {
      target: { value: '' },
    });
    fireEvent.click(screen.getByTestId('cognito-create-submit'));

    await waitFor(() => expect(screen.getByTestId('cognito-create-error')).toBeInTheDocument());
    expect(screen.getByTestId('cognito-create-form')).toBeInTheDocument();
  });

  it('deletes a user pool after confirmation and refreshes the list', async () => {
    deleteUserPoolMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteUserPoolMock).toHaveBeenCalledWith('eu-west-1_abc123'),
    );
    await waitFor(() => expect(getUserPoolsMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when user pool deletion fails', async () => {
    deleteUserPoolMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('cognito-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('cognito-list-error')).toBeInTheDocument());
  });
});
