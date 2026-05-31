import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { IamRolesPanel } from './IamRolesPanel';
import { createIamRole, deleteIamRole, getIamRoles } from '../../api/client';
import type { IamRoleListResult } from '../../api/client';

vi.mock('../../api/client');

const getIamRolesMock = vi.mocked(getIamRoles);
const createIamRoleMock = vi.mocked(createIamRole);
const deleteIamRoleMock = vi.mocked(deleteIamRole);

const listResult: IamRoleListResult = {
  roles: [
    {
      roleName: 'LambdaExec',
      arn: 'arn:aws:iam::000000000000:role/LambdaExec',
      roleId: 'AROA0001',
      path: '/',
      createDate: '2024-01-01T00:00:00Z',
      description: 'Lambda execution role',
    },
    {
      roleName: 'ReadOnly',
      arn: 'arn:aws:iam::000000000000:role/ReadOnly',
      roleId: 'AROA0002',
      path: '/team/',
      createDate: null,
      description: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <IamRolesPanel serviceKey="iam" />
    </MemoryRouter>,
  );
}

function submitTrustPolicy() {
  fireEvent.click(screen.getByTestId('iam-roles-create-trust-policy-edit'));
  fireEvent.click(screen.getByTestId('iam-roles-create-trust-policy-save'));
}

describe('IamRolesPanel', () => {
  beforeEach(() => {
    getIamRolesMock.mockResolvedValue(listResult);
    createIamRoleMock.mockResolvedValue();
    deleteIamRoleMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before roles arrive', () => {
    getIamRolesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('iam-roles-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getIamRolesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-roles-error')).toBeInTheDocument());
  });

  it('renders a row per role with links to the detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-roles-panel')).toBeInTheDocument());

    const links = screen.getAllByTestId('iam-roles-link');
    expect(links[0]).toHaveTextContent('LambdaExec');
    expect(links[0]).toHaveAttribute('href', '/services/iam/role%2FLambdaExec');
    expect(links[1]).toHaveTextContent('ReadOnly');
    expect(links[1]).toHaveAttribute('href', '/services/iam/role%2FReadOnly');
  });

  it('toggles the create form', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-roles-panel')).toBeInTheDocument());

    expect(screen.queryByTestId('iam-roles-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('iam-roles-create-toggle'));
    expect(screen.getByTestId('iam-roles-create-form')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('iam-roles-create-toggle'));
    expect(screen.queryByTestId('iam-roles-create-form')).not.toBeInTheDocument();
  });

  it('creates a role with trimmed optional fields and the default trust policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-roles-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-roles-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-roles-create-name'), {
      target: { value: 'Deploy' },
    });
    fireEvent.change(screen.getByTestId('iam-roles-create-path'), {
      target: { value: '  /eng/  ' },
    });
    fireEvent.change(screen.getByTestId('iam-roles-create-description'), {
      target: { value: '  Deployment role  ' },
    });
    fireEvent.change(screen.getByTestId('iam-roles-create-max-session'), {
      target: { value: '7200' },
    });
    submitTrustPolicy();

    await waitFor(() =>
      expect(screen.getByTestId('iam-roles-create-status')).toBeInTheDocument(),
    );
    expect(createIamRoleMock).toHaveBeenCalledWith({
      roleName: 'Deploy',
      assumeRolePolicyDocument: expect.stringContaining('lambda.amazonaws.com'),
      path: '/eng/',
      description: 'Deployment role',
      maxSessionDuration: 7200,
    });
  });

  it('creates a role with null optional fields when left blank', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-roles-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-roles-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-roles-create-name'), {
      target: { value: 'Deploy' },
    });
    submitTrustPolicy();

    await waitFor(() =>
      expect(screen.getByTestId('iam-roles-create-status')).toBeInTheDocument(),
    );
    expect(createIamRoleMock).toHaveBeenCalledWith({
      roleName: 'Deploy',
      assumeRolePolicyDocument: expect.stringContaining('sts:AssumeRole'),
      path: null,
      description: null,
      maxSessionDuration: null,
    });
  });

  it('shows an error when role creation fails', async () => {
    createIamRoleMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-roles-panel')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-roles-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-roles-create-name'), {
      target: { value: 'Deploy' },
    });
    submitTrustPolicy();

    await waitFor(() =>
      expect(screen.getByTestId('iam-roles-create-error')).toBeInTheDocument(),
    );
  });

  it('deletes a role after confirmation', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-roles-panel')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteIamRoleMock).toHaveBeenCalledWith('LambdaExec'));
  });

  it('shows an error when role deletion fails', async () => {
    deleteIamRoleMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-roles-panel')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('iam-roles-error')).toBeInTheDocument());
  });
});
