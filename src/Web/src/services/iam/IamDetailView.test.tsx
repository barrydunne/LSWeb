import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { IamDetailView } from './IamDetailView';

vi.mock('../../api/client', () => ({
  getIamUser: vi.fn(() =>
    Promise.resolve({
      userName: 'Bob',
      arn: 'arn:aws:iam::000000000000:user/Bob',
      userId: 'AID000000000000000001',
      path: '/',
      createDate: '2024-01-01T00:00:00Z',
      groups: [],
      attachedPolicies: [],
      inlinePolicyNames: [],
      accessKeys: [],
      tags: [],
      permissionsBoundaryArn: null,
    }),
  ),
  attachIamUserPolicy: vi.fn(() => Promise.resolve()),
  detachIamUserPolicy: vi.fn(() => Promise.resolve()),
  putIamUserInlinePolicy: vi.fn(() => Promise.resolve()),
  deleteIamUserInlinePolicy: vi.fn(() => Promise.resolve()),
  addIamUserToGroup: vi.fn(() => Promise.resolve()),
  removeIamUserFromGroup: vi.fn(() => Promise.resolve()),
  createIamAccessKey: vi.fn(() => Promise.resolve({})),
  updateIamAccessKeyStatus: vi.fn(() => Promise.resolve()),
  deleteIamAccessKey: vi.fn(() => Promise.resolve()),
  getIamGroup: vi.fn(() =>
    Promise.resolve({
      groupName: 'Admins',
      arn: 'arn:aws:iam::000000000000:group/Admins',
      groupId: 'AGP000000000000000001',
      path: '/',
      createDate: '2024-01-01T00:00:00Z',
      members: [],
      attachedPolicies: [],
      inlinePolicies: [],
    }),
  ),
  addIamGroupMember: vi.fn(() => Promise.resolve()),
  removeIamGroupMember: vi.fn(() => Promise.resolve()),
  attachIamGroupPolicy: vi.fn(() => Promise.resolve()),
  detachIamGroupPolicy: vi.fn(() => Promise.resolve()),
  putIamGroupInlinePolicy: vi.fn(() => Promise.resolve()),
  deleteIamGroupInlinePolicy: vi.fn(() => Promise.resolve()),
  getIamRole: vi.fn(() =>
    Promise.resolve({
      roleName: 'MyRole',
      arn: 'arn:aws:iam::000000000000:role/MyRole',
      roleId: 'AROA000000000000000001',
      path: '/',
      createDate: '2024-01-01T00:00:00Z',
      description: null,
      maxSessionDuration: null,
      assumeRolePolicyDocument: '{}',
      attachedPolicies: [],
      inlinePolicies: [],
      tags: [],
      permissionsBoundaryArn: null,
    }),
  ),
  updateIamRole: vi.fn(() => Promise.resolve()),
  attachIamRolePolicy: vi.fn(() => Promise.resolve()),
  detachIamRolePolicy: vi.fn(() => Promise.resolve()),
  putIamRoleInlinePolicy: vi.fn(() => Promise.resolve()),
  deleteIamRoleInlinePolicy: vi.fn(() => Promise.resolve()),
  getIamRoleUsedBy: vi.fn(() => Promise.resolve({ consumers: [] })),
  getIamPolicy: vi.fn(() =>
    Promise.resolve({
      policyName: 'MyPolicy',
      arn: 'arn:aws:iam::000000000000:policy/MyPolicy',
      policyId: 'ANPA000000000000000001',
      path: '/',
      defaultVersionId: 'v1',
      attachmentCount: 0,
      isAttachable: true,
      description: null,
      createDate: '2024-01-01T00:00:00Z',
      updateDate: null,
      defaultVersionDocument: '{}',
      versions: [],
      tags: [],
    }),
  ),
  createIamPolicyVersion: vi.fn(() => Promise.resolve()),
  setIamPolicyDefaultVersion: vi.fn(() => Promise.resolve()),
  deleteIamPolicyVersion: vi.fn(() => Promise.resolve()),
}));

function renderView(resourceId: string) {
  return render(<IamDetailView serviceKey="iam" resourceId={resourceId} />);
}

describe('IamDetailView', () => {
  it('renders the full role detail view for a role resource', async () => {
    renderView('role/MyRole');

    expect(await screen.findByTestId('iam-role-detail-view')).toBeInTheDocument();
    expect(screen.getByTestId('iam-role-detail-name')).toHaveTextContent('MyRole');
  });

  it('renders the full user detail view for a user resource', async () => {
    renderView('user/Bob');

    expect(await screen.findByTestId('iam-user-detail-view')).toBeInTheDocument();
    expect(screen.getByTestId('iam-user-detail-name')).toHaveTextContent('Bob');
  });

  it('renders the full group detail view for a group resource', async () => {
    renderView('group/Admins');

    expect(await screen.findByTestId('iam-group-detail-view')).toBeInTheDocument();
    expect(screen.getByTestId('iam-group-detail-name')).toHaveTextContent('Admins');
  });

  it('renders the full policy detail view for a policy resource', async () => {
    renderView('policy/arn:aws:iam::000000000000:policy/MyPolicy');

    expect(await screen.findByTestId('iam-policy-detail-view')).toBeInTheDocument();
    expect(screen.getByTestId('iam-policy-detail-name')).toHaveTextContent('MyPolicy');
  });

  it('shows an unknown state for a resource without a recognised prefix', () => {
    renderView('something-else');

    expect(screen.getByTestId('iam-detail-name')).toHaveTextContent('something-else');
    expect(screen.getByTestId('iam-detail-unknown')).toBeInTheDocument();
  });

  it('treats an unrecognised prefix as unknown', () => {
    renderView('widget/thing');

    expect(screen.getByTestId('iam-detail-unknown')).toBeInTheDocument();
  });
});
