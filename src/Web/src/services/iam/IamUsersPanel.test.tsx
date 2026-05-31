import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { IamUsersPanel } from './IamUsersPanel';
import { createIamUser, deleteIamUser, getIamUsers } from '../../api/client';
import type { IamUserListResult } from '../../api/client';

vi.mock('../../api/client');

const getIamUsersMock = vi.mocked(getIamUsers);
const createIamUserMock = vi.mocked(createIamUser);
const deleteIamUserMock = vi.mocked(deleteIamUser);

const listResult: IamUserListResult = {
  users: [
    {
      userName: 'Alice',
      arn: 'arn:aws:iam::000000000000:user/Alice',
      userId: 'AID0001',
      path: '/',
      createDate: '2024-01-01T00:00:00Z',
    },
    {
      userName: 'Bob',
      arn: 'arn:aws:iam::000000000000:user/Bob',
      userId: 'AID0002',
      path: '/team/',
      createDate: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <IamUsersPanel serviceKey="iam" />
    </MemoryRouter>,
  );
}

describe('IamUsersPanel', () => {
  beforeEach(() => {
    getIamUsersMock.mockResolvedValue(listResult);
    createIamUserMock.mockResolvedValue();
    deleteIamUserMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before users arrive', () => {
    getIamUsersMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('iam-users-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getIamUsersMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-users-error')).toBeInTheDocument());
  });

  it('renders a row per user with links to the detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-users-panel')).toBeInTheDocument());

    const links = screen.getAllByTestId('iam-users-link');
    expect(links[0]).toHaveTextContent('Alice');
    expect(links[0]).toHaveAttribute('href', '/services/iam/user%2FAlice');
    expect(links[1]).toHaveTextContent('Bob');
    expect(links[1]).toHaveAttribute('href', '/services/iam/user%2FBob');
  });

  it('toggles the create form', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-users-panel')).toBeInTheDocument());

    expect(screen.queryByTestId('iam-users-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('iam-users-create-toggle'));
    expect(screen.getByTestId('iam-users-create-form')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('iam-users-create-toggle'));
    expect(screen.queryByTestId('iam-users-create-form')).not.toBeInTheDocument();
  });

  it('creates a user with a trimmed path and shows a status message', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-users-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-users-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-users-create-name'), {
      target: { value: 'Carol' },
    });
    fireEvent.change(screen.getByTestId('iam-users-create-path'), {
      target: { value: '  /eng/  ' },
    });
    fireEvent.click(screen.getByTestId('iam-users-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-users-create-status')).toBeInTheDocument(),
    );
    expect(createIamUserMock).toHaveBeenCalledWith({ userName: 'Carol', path: '/eng/' });
  });

  it('creates a user without a path as null', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-users-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-users-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-users-create-name'), {
      target: { value: 'Carol' },
    });
    fireEvent.click(screen.getByTestId('iam-users-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-users-create-status')).toBeInTheDocument(),
    );
    expect(createIamUserMock).toHaveBeenCalledWith({ userName: 'Carol', path: null });
  });

  it('shows a saving label while the create is in flight', async () => {
    let resolveCreate: (() => void) | undefined;
    createIamUserMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveCreate = resolve;
      }),
    );
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-users-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-users-create-toggle'));
    fireEvent.change(screen.getByTestId('iam-users-create-name'), {
      target: { value: 'Carol' },
    });
    fireEvent.click(screen.getByTestId('iam-users-create-submit'));

    expect(screen.getByTestId('iam-users-create-submit')).toBeDisabled();
    expect(screen.getByTestId('iam-users-create-submit')).toHaveTextContent('Creating');

    resolveCreate?.();
    await waitFor(() =>
      expect(screen.getByTestId('iam-users-create-status')).toBeInTheDocument(),
    );
  });

  it('shows an error when user creation fails', async () => {
    createIamUserMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-users-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-users-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-users-create-name'), {
      target: { value: 'Carol' },
    });
    fireEvent.click(screen.getByTestId('iam-users-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-users-create-error')).toBeInTheDocument(),
    );
  });

  it('deletes a user after confirmation', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-users-panel')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteIamUserMock).toHaveBeenCalledWith('Alice'));
  });

  it('shows an error when user deletion fails', async () => {
    deleteIamUserMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-users-panel')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('iam-users-error')).toBeInTheDocument());
  });
});
