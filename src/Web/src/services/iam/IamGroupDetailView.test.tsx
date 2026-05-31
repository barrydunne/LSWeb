import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { IamGroupDetailView } from './IamGroupDetailView';
import {
  addIamGroupMember,
  attachIamGroupPolicy,
  deleteIamGroupInlinePolicy,
  detachIamGroupPolicy,
  getIamGroup,
  putIamGroupInlinePolicy,
  removeIamGroupMember,
} from '../../api/client';
import type { IamGroupDetail } from '../../api/client';

vi.mock('../../api/client');

const getIamGroupMock = vi.mocked(getIamGroup);
const addIamGroupMemberMock = vi.mocked(addIamGroupMember);
const removeIamGroupMemberMock = vi.mocked(removeIamGroupMember);
const attachIamGroupPolicyMock = vi.mocked(attachIamGroupPolicy);
const detachIamGroupPolicyMock = vi.mocked(detachIamGroupPolicy);
const putIamGroupInlinePolicyMock = vi.mocked(putIamGroupInlinePolicy);
const deleteIamGroupInlinePolicyMock = vi.mocked(deleteIamGroupInlinePolicy);

const emptyDetail: IamGroupDetail = {
  groupName: 'Admins',
  arn: 'arn:aws:iam::000000000000:group/Admins',
  groupId: 'AGP0001',
  path: '/',
  createDate: null,
  members: [],
  attachedPolicies: [],
  inlinePolicies: [],
};

const fullDetail: IamGroupDetail = {
  groupName: 'Admins',
  arn: 'arn:aws:iam::000000000000:group/Admins',
  groupId: 'AGP0001',
  path: '/',
  createDate: '2024-01-01T00:00:00Z',
  members: ['alice', 'bob'],
  attachedPolicies: [{ policyName: 'ReadOnly', policyArn: 'arn:aws:iam::aws:policy/ReadOnly' }],
  inlinePolicies: [
    {
      policyName: 'inline-1',
      policyDocument: JSON.stringify({
        Version: '2012-10-17',
        Statement: [{ Effect: 'Allow', Action: '*', Resource: '*' }],
      }),
    },
    { policyName: 'inline-bad', policyDocument: 'not-json' },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <IamGroupDetailView groupName="Admins" serviceKey="iam" />
    </MemoryRouter>,
  );
}

describe('IamGroupDetailView', () => {
  beforeEach(() => {
    getIamGroupMock.mockResolvedValue(emptyDetail);
    addIamGroupMemberMock.mockResolvedValue();
    removeIamGroupMemberMock.mockResolvedValue();
    attachIamGroupPolicyMock.mockResolvedValue();
    detachIamGroupPolicyMock.mockResolvedValue();
    putIamGroupInlinePolicyMock.mockResolvedValue();
    deleteIamGroupInlinePolicyMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before the group arrives', () => {
    getIamGroupMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('iam-group-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getIamGroupMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-group-detail-error')).toBeInTheDocument());
  });

  it('renders the identity fields and empty member panel', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());
    expect(screen.getByTestId('iam-group-detail-name')).toHaveTextContent('Admins');
    expect(screen.getByTestId('iam-group-detail-arn')).toHaveTextContent('arn:aws:iam');
    expect(screen.getByTestId('iam-group-detail-groupId')).toHaveTextContent('AGP0001');
    expect(screen.getByTestId('iam-group-detail-path')).toHaveTextContent('/');
    expect(screen.getByTestId('iam-group-detail-created')).toHaveTextContent('\u2014');
    expect(screen.getByTestId('iam-group-detail-members-empty')).toBeInTheDocument();
  });

  it('renders member links to the user detail view', async () => {
    getIamGroupMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    const links = screen.getAllByTestId('iam-group-detail-member-link');
    expect(links[0]).toHaveTextContent('alice');
    expect(links[0]).toHaveAttribute('href', '/services/iam/user%2Falice');
    expect(links[1]).toHaveTextContent('bob');
  });

  it('adds a member', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('iam-group-detail-member-name'), {
      target: { value: 'carol' },
    });
    fireEvent.click(screen.getByTestId('iam-group-detail-member-submit'));

    await waitFor(() => expect(addIamGroupMemberMock).toHaveBeenCalledWith('Admins', 'carol'));
  });

  it('shows a mutation error when adding a member fails', async () => {
    addIamGroupMemberMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-member-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-group-detail-mutation-error')).toBeInTheDocument(),
    );
  });

  it('removes a member after confirmation', async () => {
    getIamGroupMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(removeIamGroupMemberMock).toHaveBeenCalledWith('Admins', 'alice'));
  });

  it('attaches a managed policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-tab-permissions'));
    fireEvent.change(screen.getByTestId('iam-group-detail-attach-arn'), {
      target: { value: 'arn:aws:iam::aws:policy/AdministratorAccess' },
    });
    fireEvent.click(screen.getByTestId('iam-group-detail-attach-submit'));

    await waitFor(() =>
      expect(attachIamGroupPolicyMock).toHaveBeenCalledWith(
        'Admins',
        'arn:aws:iam::aws:policy/AdministratorAccess',
      ),
    );
  });

  it('shows a mutation error when attaching a policy fails', async () => {
    attachIamGroupPolicyMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-tab-permissions'));
    fireEvent.click(screen.getByTestId('iam-group-detail-attach-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-group-detail-mutation-error')).toBeInTheDocument(),
    );
  });

  it('renders attached and inline policies and the inline viewer', async () => {
    getIamGroupMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-tab-permissions'));
    expect(screen.getByTestId('iam-group-detail-attached-item')).toHaveTextContent('ReadOnly');
    const viewers = screen.getAllByTestId('iam-group-detail-inline-viewer-content');
    expect(viewers[0]).toHaveTextContent('Version');
    expect(viewers[1]).toHaveTextContent('not-json');
  });

  it('detaches a managed policy after confirmation', async () => {
    getIamGroupMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-tab-permissions'));
    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(detachIamGroupPolicyMock).toHaveBeenCalledWith('Admins', 'arn:aws:iam::aws:policy/ReadOnly'),
    );
  });

  it('requires a name before adding an inline policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-tab-permissions'));
    fireEvent.click(screen.getByTestId('iam-group-detail-inline-editor-edit'));
    fireEvent.click(screen.getByTestId('iam-group-detail-inline-editor-save'));

    expect(screen.getByTestId('iam-group-detail-inline-name-error')).toBeInTheDocument();
    expect(putIamGroupInlinePolicyMock).not.toHaveBeenCalled();
  });

  it('adds an inline policy with a name and document', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-tab-permissions'));
    fireEvent.change(screen.getByTestId('iam-group-detail-inline-name'), {
      target: { value: 'my-policy' },
    });
    fireEvent.click(screen.getByTestId('iam-group-detail-inline-editor-edit'));
    fireEvent.click(screen.getByTestId('iam-group-detail-inline-editor-save'));

    await waitFor(() =>
      expect(putIamGroupInlinePolicyMock).toHaveBeenCalledWith(
        'Admins',
        'my-policy',
        expect.stringContaining('Version'),
      ),
    );
  });

  it('deletes an inline policy after confirmation', async () => {
    getIamGroupMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-tab-permissions'));
    fireEvent.click(screen.getAllByTestId('confirm-trigger')[1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteIamGroupInlinePolicyMock).toHaveBeenCalledWith('Admins', 'inline-1'),
    );
  });

  it('shows empty permission panels when nothing is configured', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-group-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-group-detail-tab-permissions'));
    expect(screen.getByTestId('iam-group-detail-attached-empty')).toBeInTheDocument();
    expect(screen.getByTestId('iam-group-detail-inline-empty')).toBeInTheDocument();
  });
});
