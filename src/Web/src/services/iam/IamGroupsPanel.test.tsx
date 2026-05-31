import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { IamGroupsPanel } from './IamGroupsPanel';
import { createIamGroup, deleteIamGroup, getIamGroups } from '../../api/client';
import type { IamGroupListResult } from '../../api/client';

vi.mock('../../api/client');

const getIamGroupsMock = vi.mocked(getIamGroups);
const createIamGroupMock = vi.mocked(createIamGroup);
const deleteIamGroupMock = vi.mocked(deleteIamGroup);

const listResult: IamGroupListResult = {
  groups: [
    {
      groupName: 'Admins',
      arn: 'arn:aws:iam::000000000000:group/Admins',
      groupId: 'AGP0001',
      path: '/',
      createDate: '2024-01-01T00:00:00Z',
    },
    {
      groupName: 'Developers',
      arn: 'arn:aws:iam::000000000000:group/Developers',
      groupId: 'AGP0002',
      path: '/team/',
      createDate: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <IamGroupsPanel serviceKey="iam" />
    </MemoryRouter>,
  );
}

describe('IamGroupsPanel', () => {
  beforeEach(() => {
    getIamGroupsMock.mockResolvedValue(listResult);
    createIamGroupMock.mockResolvedValue();
    deleteIamGroupMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before groups arrive', () => {
    getIamGroupsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('iam-groups-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getIamGroupsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-groups-error')).toBeInTheDocument());
  });

  it('renders a row per group with links to the detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-groups-panel')).toBeInTheDocument());

    const links = screen.getAllByTestId('iam-groups-link');
    expect(links[0]).toHaveTextContent('Admins');
    expect(links[0]).toHaveAttribute('href', '/services/iam/group%2FAdmins');
    expect(links[1]).toHaveTextContent('Developers');
    expect(links[1]).toHaveAttribute('href', '/services/iam/group%2FDevelopers');
  });

  it('toggles the create form', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-groups-panel')).toBeInTheDocument());

    expect(screen.queryByTestId('iam-groups-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('iam-groups-create-toggle'));
    expect(screen.getByTestId('iam-groups-create-form')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('iam-groups-create-toggle'));
    expect(screen.queryByTestId('iam-groups-create-form')).not.toBeInTheDocument();
  });

  it('creates a group with a trimmed path and shows a status message', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-groups-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-groups-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-groups-create-name'), {
      target: { value: 'Ops' },
    });
    fireEvent.change(screen.getByTestId('iam-groups-create-path'), {
      target: { value: '  /eng/  ' },
    });
    fireEvent.click(screen.getByTestId('iam-groups-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-groups-create-status')).toBeInTheDocument(),
    );
    expect(createIamGroupMock).toHaveBeenCalledWith({ groupName: 'Ops', path: '/eng/' });
  });

  it('creates a group without a path as null', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-groups-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-groups-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-groups-create-name'), {
      target: { value: 'Ops' },
    });
    fireEvent.click(screen.getByTestId('iam-groups-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-groups-create-status')).toBeInTheDocument(),
    );
    expect(createIamGroupMock).toHaveBeenCalledWith({ groupName: 'Ops', path: null });
  });

  it('shows a saving label while the create is in flight', async () => {
    let resolveCreate: (() => void) | undefined;
    createIamGroupMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveCreate = resolve;
      }),
    );
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-groups-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-groups-create-toggle'));
    fireEvent.change(screen.getByTestId('iam-groups-create-name'), {
      target: { value: 'Ops' },
    });
    fireEvent.click(screen.getByTestId('iam-groups-create-submit'));

    expect(screen.getByTestId('iam-groups-create-submit')).toBeDisabled();
    expect(screen.getByTestId('iam-groups-create-submit')).toHaveTextContent('Creating');

    resolveCreate?.();
    await waitFor(() =>
      expect(screen.getByTestId('iam-groups-create-status')).toBeInTheDocument(),
    );
  });

  it('shows an error when group creation fails', async () => {
    createIamGroupMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-groups-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-groups-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-groups-create-name'), {
      target: { value: 'Ops' },
    });
    fireEvent.click(screen.getByTestId('iam-groups-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-groups-create-error')).toBeInTheDocument(),
    );
  });

  it('deletes a group after confirmation', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-groups-panel')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteIamGroupMock).toHaveBeenCalledWith('Admins'));
  });

  it('shows an error when group deletion fails', async () => {
    deleteIamGroupMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-groups-panel')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('iam-groups-error')).toBeInTheDocument());
  });
});
