import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { IamUserDetailView } from './IamUserDetailView';
import {
  addIamUserToGroup,
  attachIamUserPolicy,
  createIamAccessKey,
  deleteIamAccessKey,
  deleteIamUserInlinePolicy,
  deleteIamUserPermissionsBoundary,
  detachIamUserPolicy,
  getIamUser,
  putIamUserInlinePolicy,
  putIamUserPermissionsBoundary,
  removeIamUserFromGroup,
  resolveReference,
  tagIamUser,
  untagIamUser,
  updateIamAccessKeyStatus,
} from '../../api/client';
import type { IamUserDetail } from '../../api/client';

vi.mock('../../api/client');

const getIamUserMock = vi.mocked(getIamUser);
const attachIamUserPolicyMock = vi.mocked(attachIamUserPolicy);
const detachIamUserPolicyMock = vi.mocked(detachIamUserPolicy);
const putIamUserInlinePolicyMock = vi.mocked(putIamUserInlinePolicy);
const deleteIamUserInlinePolicyMock = vi.mocked(deleteIamUserInlinePolicy);
const addIamUserToGroupMock = vi.mocked(addIamUserToGroup);
const removeIamUserFromGroupMock = vi.mocked(removeIamUserFromGroup);
const createIamAccessKeyMock = vi.mocked(createIamAccessKey);
const updateIamAccessKeyStatusMock = vi.mocked(updateIamAccessKeyStatus);
const deleteIamAccessKeyMock = vi.mocked(deleteIamAccessKey);
const tagIamUserMock = vi.mocked(tagIamUser);
const untagIamUserMock = vi.mocked(untagIamUser);
const putIamUserPermissionsBoundaryMock = vi.mocked(putIamUserPermissionsBoundary);
const deleteIamUserPermissionsBoundaryMock = vi.mocked(deleteIamUserPermissionsBoundary);
const resolveReferenceMock = vi.mocked(resolveReference);

const emptyDetail: IamUserDetail = {
  userName: 'Alice',
  arn: 'arn:aws:iam::000000000000:user/Alice',
  userId: 'AID0001',
  path: '/',
  createDate: null,
  groups: [],
  attachedPolicies: [],
  inlinePolicyNames: [],
  accessKeys: [],
  tags: [],
  permissionsBoundaryArn: null,
};

const fullDetail: IamUserDetail = {
  userName: 'Alice',
  arn: 'arn:aws:iam::000000000000:user/Alice',
  userId: 'AID0001',
  path: '/',
  createDate: '2024-01-01T00:00:00Z',
  groups: ['Admins'],
  attachedPolicies: [{ policyName: 'ReadOnly', policyArn: 'arn:aws:iam::aws:policy/ReadOnly' }],
  inlinePolicyNames: ['inline-1'],
  accessKeys: [
    {
      accessKeyId: 'AKIAACTIVE',
      status: 'Active',
      createDate: '2024-01-02T00:00:00Z',
      lastUsedDate: null,
      lastUsedService: null,
      lastUsedRegion: null,
    },
    {
      accessKeyId: 'AKIAINACTIVE',
      status: 'Inactive',
      createDate: null,
      lastUsedDate: null,
      lastUsedService: null,
      lastUsedRegion: null,
    },
  ],
  tags: [{ key: 'env', value: 'prod' }],
  permissionsBoundaryArn: 'arn:aws:iam::aws:policy/Boundary',
};

function renderView() {
  return render(<IamUserDetailView userName="Alice" />);
}

describe('IamUserDetailView', () => {
  beforeEach(() => {
    getIamUserMock.mockResolvedValue(emptyDetail);
    attachIamUserPolicyMock.mockResolvedValue();
    detachIamUserPolicyMock.mockResolvedValue();
    putIamUserInlinePolicyMock.mockResolvedValue();
    deleteIamUserInlinePolicyMock.mockResolvedValue();
    addIamUserToGroupMock.mockResolvedValue();
    removeIamUserFromGroupMock.mockResolvedValue();
    createIamAccessKeyMock.mockResolvedValue({
      accessKeyId: 'AKIANEW',
      secretAccessKey: 'sekret',
      status: 'Active',
      createDate: '2024-01-03T00:00:00Z',
    });
    updateIamAccessKeyStatusMock.mockResolvedValue();
    deleteIamAccessKeyMock.mockResolvedValue();
    tagIamUserMock.mockResolvedValue();
    untagIamUserMock.mockResolvedValue();
    putIamUserPermissionsBoundaryMock.mockResolvedValue();
    deleteIamUserPermissionsBoundaryMock.mockResolvedValue();
    resolveReferenceMock.mockRejectedValue(new Error('unresolved'));
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before the user arrives', () => {
    getIamUserMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('iam-user-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getIamUserMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-user-detail-error')).toBeInTheDocument());
  });

  it('renders the identity fields and empty permission panels', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());
    expect(screen.getByTestId('iam-user-detail-name')).toHaveTextContent('Alice');
    expect(screen.getByTestId('iam-user-detail-arn')).toHaveTextContent('arn:aws:iam');
    expect(screen.getByTestId('iam-user-detail-userId')).toHaveTextContent('AID0001');
    expect(screen.getByTestId('iam-user-detail-attached-empty')).toBeInTheDocument();
    expect(screen.getByTestId('iam-user-detail-inline-empty')).toBeInTheDocument();
  });

  it('attaches a managed policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('iam-user-detail-attach-arn'), {
      target: { value: 'arn:aws:iam::aws:policy/AdministratorAccess' },
    });
    fireEvent.click(screen.getByTestId('iam-user-detail-attach-submit'));

    await waitFor(() =>
      expect(attachIamUserPolicyMock).toHaveBeenCalledWith(
        'Alice',
        'arn:aws:iam::aws:policy/AdministratorAccess',
      ),
    );
  });

  it('shows a mutation error when attaching a policy fails', async () => {
    attachIamUserPolicyMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-user-detail-attach-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-user-detail-mutation-error')).toBeInTheDocument(),
    );
  });

  it('detaches a managed policy after confirmation', async () => {
    getIamUserMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-attached-item')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(detachIamUserPolicyMock).toHaveBeenCalledWith('Alice', 'arn:aws:iam::aws:policy/ReadOnly'),
    );
  });

  it('requires a name before adding an inline policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-user-detail-inline-editor-edit'));
    fireEvent.click(screen.getByTestId('iam-user-detail-inline-editor-save'));

    expect(screen.getByTestId('iam-user-detail-inline-name-error')).toBeInTheDocument();
    expect(putIamUserInlinePolicyMock).not.toHaveBeenCalled();
  });

  it('adds an inline policy with a name and document', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('iam-user-detail-inline-name'), {
      target: { value: 'my-policy' },
    });
    fireEvent.click(screen.getByTestId('iam-user-detail-inline-editor-edit'));
    fireEvent.click(screen.getByTestId('iam-user-detail-inline-editor-save'));

    await waitFor(() =>
      expect(putIamUserInlinePolicyMock).toHaveBeenCalledWith(
        'Alice',
        'my-policy',
        expect.stringContaining('Version'),
      ),
    );
  });

  it('deletes an inline policy after confirmation', async () => {
    getIamUserMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-inline-item')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteIamUserInlinePolicyMock).toHaveBeenCalledWith('Alice', 'inline-1'),
    );
  });

  it('shows an empty groups panel and adds a group', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-user-detail-tab-groups'));
    expect(screen.getByTestId('iam-user-detail-groups-empty')).toBeInTheDocument();

    fireEvent.change(screen.getByTestId('iam-user-detail-group-name'), {
      target: { value: 'Developers' },
    });
    fireEvent.click(screen.getByTestId('iam-user-detail-group-submit'));

    await waitFor(() => expect(addIamUserToGroupMock).toHaveBeenCalledWith('Alice', 'Developers'));
  });

  it('removes a group after confirmation', async () => {
    getIamUserMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-user-detail-tab-groups'));
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(removeIamUserFromGroupMock).toHaveBeenCalledWith('Alice', 'Admins'));
  });

  it('shows an empty access keys panel and creates a key revealing the secret once', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-user-detail-tab-access-keys'));
    expect(screen.getByTestId('iam-user-detail-keys-empty')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('iam-user-detail-key-create'));

    await waitFor(() => expect(createIamAccessKeyMock).toHaveBeenCalledWith('Alice'));
    expect(await screen.findByTestId('iam-user-detail-key-secret-value')).toHaveTextContent('sekret');

    fireEvent.click(screen.getByTestId('iam-user-detail-key-secret-dismiss'));
    expect(screen.queryByTestId('iam-user-detail-key-secret')).not.toBeInTheDocument();
  });

  it('toggles access key status for active and inactive keys', async () => {
    getIamUserMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-user-detail-tab-access-keys'));
    const toggles = screen.getAllByTestId('iam-user-detail-key-toggle');
    fireEvent.click(toggles[0]);

    await waitFor(() =>
      expect(updateIamAccessKeyStatusMock).toHaveBeenCalledWith('Alice', 'AKIAACTIVE', 'Inactive'),
    );

    fireEvent.click(screen.getByTestId('iam-user-detail-tab-access-keys'));
    const togglesAgain = screen.getAllByTestId('iam-user-detail-key-toggle');
    fireEvent.click(togglesAgain[1]);

    await waitFor(() =>
      expect(updateIamAccessKeyStatusMock).toHaveBeenCalledWith('Alice', 'AKIAINACTIVE', 'Active'),
    );
  });

  it('deletes an access key after confirmation', async () => {
    getIamUserMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-user-detail-tab-access-keys'));
    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteIamAccessKeyMock).toHaveBeenCalledWith('Alice', 'AKIAACTIVE'));
  });

  it('renders tags and the permissions boundary', async () => {
    getIamUserMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-user-detail-tab-tags'));
    expect(screen.getByTestId('iam-user-detail-panel-tags')).toBeInTheDocument();
    expect(screen.getByTestId('iam-user-detail-tags-list')).toBeInTheDocument();
    expect(screen.getByTestId('iam-user-detail-boundary-current')).toBeInTheDocument();
  });

  it('adds a tag', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-user-detail-tab-tags'));

    fireEvent.change(screen.getByTestId('iam-user-detail-tags-key'), { target: { value: 'env' } });
    fireEvent.change(screen.getByTestId('iam-user-detail-tags-value'), { target: { value: 'prod' } });
    fireEvent.click(screen.getByTestId('iam-user-detail-tags-submit'));

    await waitFor(() =>
      expect(tagIamUserMock).toHaveBeenCalledWith('Alice', [{ key: 'env', value: 'prod' }]),
    );
  });

  it('removes a tag after confirmation', async () => {
    getIamUserMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-user-detail-tab-tags'));

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(untagIamUserMock).toHaveBeenCalledWith('Alice', ['env']));
  });

  it('sets a permissions boundary', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-user-detail-tab-tags'));

    fireEvent.change(screen.getByTestId('iam-user-detail-boundary-arn'), {
      target: { value: 'arn:aws:iam::aws:policy/Boundary' },
    });
    fireEvent.click(screen.getByTestId('iam-user-detail-boundary-submit'));

    await waitFor(() =>
      expect(putIamUserPermissionsBoundaryMock).toHaveBeenCalledWith(
        'Alice',
        'arn:aws:iam::aws:policy/Boundary',
      ),
    );
  });

  it('removes a permissions boundary after confirmation', async () => {
    getIamUserMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-user-detail-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-user-detail-tab-tags'));

    const triggers = screen.getAllByTestId('confirm-trigger');
    fireEvent.click(triggers[triggers.length - 1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteIamUserPermissionsBoundaryMock).toHaveBeenCalledWith('Alice'));
  });
});
