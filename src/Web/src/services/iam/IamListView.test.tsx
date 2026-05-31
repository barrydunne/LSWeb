import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { IamListView } from './IamListView';

vi.mock('../../api/client', () => ({
  getIamUsers: vi.fn(() => Promise.resolve({ users: [] })),
  createIamUser: vi.fn(() => Promise.resolve()),
  deleteIamUser: vi.fn(() => Promise.resolve()),
  getIamGroups: vi.fn(() => Promise.resolve({ groups: [] })),
  createIamGroup: vi.fn(() => Promise.resolve()),
  deleteIamGroup: vi.fn(() => Promise.resolve()),
  getIamRoles: vi.fn(() => Promise.resolve({ roles: [] })),
  createIamRole: vi.fn(() => Promise.resolve()),
  deleteIamRole: vi.fn(() => Promise.resolve()),
  getIamPolicies: vi.fn(() => Promise.resolve({ policies: [] })),
  createIamPolicy: vi.fn(() => Promise.resolve()),
  deleteIamPolicy: vi.fn(() => Promise.resolve()),
  getIamAccountSummary: vi.fn(() => Promise.resolve({ entries: {} })),
  getIamAccountPasswordPolicy: vi.fn(() => Promise.resolve(null)),
  getIamAccountAliases: vi.fn(() => Promise.resolve({ aliases: [] })),
}));

function renderView() {
  return render(
    <MemoryRouter>
      <IamListView serviceKey="iam" />
    </MemoryRouter>,
  );
}

describe('IamListView', () => {
  it('renders the shell with the users tab active by default', async () => {
    renderView();

    expect(screen.getByTestId('iam-list-view')).toBeInTheDocument();
    expect(screen.getByTestId('iam-list-tab-users')).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByTestId('iam-list-panel-users')).toBeInTheDocument();
    await screen.findByTestId('iam-users-panel');
  });

  it('renders a tab for each IAM resource type', async () => {
    renderView();

    expect(screen.getByTestId('iam-list-tab-users')).toBeInTheDocument();
    expect(screen.getByTestId('iam-list-tab-groups')).toBeInTheDocument();
    expect(screen.getByTestId('iam-list-tab-roles')).toBeInTheDocument();
    expect(screen.getByTestId('iam-list-tab-policies')).toBeInTheDocument();
    expect(screen.getByTestId('iam-list-tab-account')).toBeInTheDocument();
    await screen.findByTestId('iam-users-panel');
  });

  it('switches the active panel when another tab is selected', async () => {
    renderView();

    fireEvent.click(screen.getByTestId('iam-list-tab-roles'));

    expect(screen.getByTestId('iam-list-tab-roles')).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByTestId('iam-list-panel-roles')).toBeInTheDocument();
    expect(screen.queryByTestId('iam-list-panel-users')).not.toBeInTheDocument();
    await screen.findByTestId('iam-roles-panel');
  });

  it('shows the groups and policies panels when selected', async () => {
    renderView();

    fireEvent.click(screen.getByTestId('iam-list-tab-groups'));
    expect(screen.getByTestId('iam-list-panel-groups')).toBeInTheDocument();
    await screen.findByTestId('iam-groups-panel');

    fireEvent.click(screen.getByTestId('iam-list-tab-policies'));
    expect(screen.getByTestId('iam-list-panel-policies')).toBeInTheDocument();
    await screen.findByTestId('iam-policies-panel');
  });

  it('shows the account panel when selected', async () => {
    renderView();

    fireEvent.click(screen.getByTestId('iam-list-tab-account'));
    expect(screen.getByTestId('iam-list-panel-account')).toBeInTheDocument();
    await screen.findByTestId('iam-account-panel');
  });
});
