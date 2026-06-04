import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { IamRoleDetailView } from './IamRoleDetailView';
import {
  attachIamRolePolicy,
  deleteIamRoleInlinePolicy,
  deleteIamRolePermissionsBoundary,
  detachIamRolePolicy,
  getIamRole,
  getIamRoleUsedBy,
  putIamRoleInlinePolicy,
  putIamRolePermissionsBoundary,
  resolveReference,
  tagIamRole,
  untagIamRole,
  updateIamRole,
} from '../../api/client';
import type { IamRoleDetail } from '../../api/client';

vi.mock('../../api/client');

const getIamRoleMock = vi.mocked(getIamRole);
const getIamRoleUsedByMock = vi.mocked(getIamRoleUsedBy);
const resolveReferenceMock = vi.mocked(resolveReference);
const updateIamRoleMock = vi.mocked(updateIamRole);
const attachIamRolePolicyMock = vi.mocked(attachIamRolePolicy);
const detachIamRolePolicyMock = vi.mocked(detachIamRolePolicy);
const putIamRoleInlinePolicyMock = vi.mocked(putIamRoleInlinePolicy);
const deleteIamRoleInlinePolicyMock = vi.mocked(deleteIamRoleInlinePolicy);
const tagIamRoleMock = vi.mocked(tagIamRole);
const untagIamRoleMock = vi.mocked(untagIamRole);
const putIamRolePermissionsBoundaryMock = vi.mocked(putIamRolePermissionsBoundary);
const deleteIamRolePermissionsBoundaryMock = vi.mocked(deleteIamRolePermissionsBoundary);

const trustDocument = JSON.stringify({
  Version: '2012-10-17',
  Statement: [
    {
      Effect: 'Allow',
      Principal: { Service: 'lambda.amazonaws.com' },
      Action: 'sts:AssumeRole',
    },
  ],
});

const emptyDetail: IamRoleDetail = {
  roleName: 'LambdaExec',
  arn: 'arn:aws:iam::000000000000:role/LambdaExec',
  roleId: 'AROA0001',
  path: '/',
  createDate: null,
  description: null,
  maxSessionDuration: null,
  assumeRolePolicyDocument: trustDocument,
  attachedPolicies: [],
  inlinePolicies: [],
  tags: [],
  permissionsBoundaryArn: null,
};

const fullDetail: IamRoleDetail = {
  roleName: 'LambdaExec',
  arn: 'arn:aws:iam::000000000000:role/LambdaExec',
  roleId: 'AROA0001',
  path: '/',
  createDate: '2024-01-01T00:00:00Z',
  description: 'Lambda execution role',
  maxSessionDuration: 3600,
  assumeRolePolicyDocument: trustDocument,
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
  tags: [{ key: 'env', value: 'prod' }],
  permissionsBoundaryArn: 'arn:aws:iam::aws:policy/Boundary',
};

function renderView() {
  return render(
    <MemoryRouter>
      <IamRoleDetailView roleName="LambdaExec" />
    </MemoryRouter>,
  );
}

describe('IamRoleDetailView', () => {
  beforeEach(() => {
    getIamRoleMock.mockResolvedValue(emptyDetail);
    getIamRoleUsedByMock.mockResolvedValue({ consumers: [] });
    resolveReferenceMock.mockRejectedValue(new Error('unresolved'));
    updateIamRoleMock.mockResolvedValue();
    attachIamRolePolicyMock.mockResolvedValue();
    detachIamRolePolicyMock.mockResolvedValue();
    putIamRoleInlinePolicyMock.mockResolvedValue();
    deleteIamRoleInlinePolicyMock.mockResolvedValue();
    tagIamRoleMock.mockResolvedValue();
    untagIamRoleMock.mockResolvedValue();
    putIamRolePermissionsBoundaryMock.mockResolvedValue();
    deleteIamRolePermissionsBoundaryMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before the role arrives', () => {
    getIamRoleMock.mockReturnValue(new Promise(() => {}));
    getIamRoleUsedByMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('iam-role-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getIamRoleMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-role-detail-error')).toBeInTheDocument());
  });

  it('renders the identity fields', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());
    expect(screen.getByTestId('iam-role-detail-name')).toHaveTextContent('LambdaExec');
    expect(screen.getByTestId('iam-role-detail-arn')).toHaveTextContent('arn:aws:iam');
    expect(screen.getByTestId('iam-role-detail-roleId')).toHaveTextContent('AROA0001');
    expect(screen.getByTestId('iam-role-detail-path')).toHaveTextContent('/');
    expect(screen.getByTestId('iam-role-detail-created')).toHaveTextContent('\u2014');
  });

  it('pre-populates the settings fields from the loaded role', async () => {
    getIamRoleMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('iam-role-detail-description')).toHaveValue('Lambda execution role');
    expect(screen.getByTestId('iam-role-detail-max-session')).toHaveValue(3600);
  });

  it('saves the description and max-session settings without a trust policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('iam-role-detail-description'), {
      target: { value: '  Deployment role  ' },
    });
    fireEvent.change(screen.getByTestId('iam-role-detail-max-session'), {
      target: { value: '7200' },
    });
    fireEvent.click(screen.getByTestId('iam-role-detail-settings-submit'));

    await waitFor(() =>
      expect(updateIamRoleMock).toHaveBeenCalledWith('LambdaExec', {
        description: 'Deployment role',
        maxSessionDuration: 7200,
        trustPolicyDocument: null,
      }),
    );
  });

  it('saves blank settings as null', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-settings-submit'));

    await waitFor(() =>
      expect(updateIamRoleMock).toHaveBeenCalledWith('LambdaExec', {
        description: null,
        maxSessionDuration: null,
        trustPolicyDocument: null,
      }),
    );
  });

  it('shows a mutation error when saving settings fails', async () => {
    updateIamRoleMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-settings-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-role-detail-mutation-error')).toBeInTheDocument(),
    );
  });

  it('edits and saves the trust relationship policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-trust-editor-edit'));
    fireEvent.click(screen.getByTestId('iam-role-detail-trust-editor-save'));

    await waitFor(() =>
      expect(updateIamRoleMock).toHaveBeenCalledWith('LambdaExec', {
        description: null,
        maxSessionDuration: null,
        trustPolicyDocument: expect.stringContaining('lambda.amazonaws.com'),
      }),
    );
  });

  it('attaches a managed policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-permissions'));
    fireEvent.change(screen.getByTestId('iam-role-detail-attach-arn'), {
      target: { value: 'arn:aws:iam::aws:policy/AdministratorAccess' },
    });
    fireEvent.click(screen.getByTestId('iam-role-detail-attach-submit'));

    await waitFor(() =>
      expect(attachIamRolePolicyMock).toHaveBeenCalledWith(
        'LambdaExec',
        'arn:aws:iam::aws:policy/AdministratorAccess',
      ),
    );
  });

  it('shows a mutation error when attaching a policy fails', async () => {
    attachIamRolePolicyMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-permissions'));
    fireEvent.click(screen.getByTestId('iam-role-detail-attach-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('iam-role-detail-mutation-error')).toBeInTheDocument(),
    );
  });

  it('renders attached and inline policies and the inline viewer', async () => {
    getIamRoleMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-permissions'));
    expect(screen.getByTestId('iam-role-detail-attached-item')).toHaveTextContent('ReadOnly');
    const viewers = screen.getAllByTestId('iam-role-detail-inline-viewer-content');
    expect(viewers[0]).toHaveTextContent('Version');
    expect(viewers[1]).toHaveTextContent('not-json');
  });

  it('detaches a managed policy after confirmation', async () => {
    getIamRoleMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-permissions'));
    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(detachIamRolePolicyMock).toHaveBeenCalledWith(
        'LambdaExec',
        'arn:aws:iam::aws:policy/ReadOnly',
      ),
    );
  });

  it('requires a name before adding an inline policy', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-permissions'));
    fireEvent.click(screen.getByTestId('iam-role-detail-inline-editor-edit'));
    fireEvent.click(screen.getByTestId('iam-role-detail-inline-editor-save'));

    expect(screen.getByTestId('iam-role-detail-inline-name-error')).toBeInTheDocument();
    expect(putIamRoleInlinePolicyMock).not.toHaveBeenCalled();
  });

  it('adds an inline policy with a name and document', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-permissions'));
    fireEvent.change(screen.getByTestId('iam-role-detail-inline-name'), {
      target: { value: 'my-policy' },
    });
    fireEvent.click(screen.getByTestId('iam-role-detail-inline-editor-edit'));
    fireEvent.click(screen.getByTestId('iam-role-detail-inline-editor-save'));

    await waitFor(() =>
      expect(putIamRoleInlinePolicyMock).toHaveBeenCalledWith(
        'LambdaExec',
        'my-policy',
        expect.stringContaining('Version'),
      ),
    );
  });

  it('deletes an inline policy after confirmation', async () => {
    getIamRoleMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-permissions'));
    fireEvent.click(screen.getAllByTestId('confirm-trigger')[1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteIamRoleInlinePolicyMock).toHaveBeenCalledWith('LambdaExec', 'inline-1'),
    );
  });

  it('shows empty permission panels when nothing is configured', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-permissions'));
    expect(screen.getByTestId('iam-role-detail-attached-empty')).toBeInTheDocument();
    expect(screen.getByTestId('iam-role-detail-inline-empty')).toBeInTheDocument();
  });

  it('shows a loading state while the used-by lookup is in flight', async () => {
    getIamRoleUsedByMock.mockReturnValue(new Promise(() => {}));

    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-usedby'));
    await waitFor(() =>
      expect(screen.getByTestId('iam-role-detail-usedby-loading')).toBeInTheDocument(),
    );
  });

  it('shows an empty used-by state when no resources reference the role', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-usedby'));
    await waitFor(() =>
      expect(screen.getByTestId('iam-role-detail-usedby-empty')).toBeInTheDocument(),
    );
  });

  it('lists the resources that use the role as navigable references', async () => {
    getIamRoleUsedByMock.mockResolvedValue({
      consumers: [
        { consumerType: 'Lambda function', resourceName: 'orders', serviceKey: 'lambda' },
      ],
    });

    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-usedby'));
    await waitFor(() =>
      expect(screen.getByTestId('iam-role-detail-usedby-item')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('iam-role-detail-usedby-item')).toHaveTextContent('Lambda function');
    expect(getIamRoleUsedByMock).toHaveBeenCalledWith('LambdaExec', expect.anything());
  });

  it('shows an error state when the used-by lookup fails', async () => {
    getIamRoleUsedByMock.mockRejectedValue(new Error('boom'));

    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-usedby'));
    await waitFor(() =>
      expect(screen.getByTestId('iam-role-detail-usedby-error')).toBeInTheDocument(),
    );
  });

  it('renders tags and the permissions boundary', async () => {
    getIamRoleMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('iam-role-detail-tab-tags'));
    expect(screen.getByTestId('iam-role-detail-panel-tags')).toBeInTheDocument();
    expect(screen.getByTestId('iam-role-detail-tags-list')).toBeInTheDocument();
    expect(screen.getByTestId('iam-role-detail-boundary-current')).toBeInTheDocument();
  });

  it('adds a tag', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-role-detail-tab-tags'));

    fireEvent.change(screen.getByTestId('iam-role-detail-tags-key'), { target: { value: 'env' } });
    fireEvent.change(screen.getByTestId('iam-role-detail-tags-value'), { target: { value: 'prod' } });
    fireEvent.click(screen.getByTestId('iam-role-detail-tags-submit'));

    await waitFor(() =>
      expect(tagIamRoleMock).toHaveBeenCalledWith('LambdaExec', [{ key: 'env', value: 'prod' }]),
    );
  });

  it('removes a tag after confirmation', async () => {
    getIamRoleMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-role-detail-tab-tags'));

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(untagIamRoleMock).toHaveBeenCalledWith('LambdaExec', ['env']));
  });

  it('sets a permissions boundary', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-role-detail-tab-tags'));

    fireEvent.change(screen.getByTestId('iam-role-detail-boundary-arn'), {
      target: { value: 'arn:aws:iam::aws:policy/Boundary' },
    });
    fireEvent.click(screen.getByTestId('iam-role-detail-boundary-submit'));

    await waitFor(() =>
      expect(putIamRolePermissionsBoundaryMock).toHaveBeenCalledWith(
        'LambdaExec',
        'arn:aws:iam::aws:policy/Boundary',
      ),
    );
  });

  it('removes a permissions boundary after confirmation', async () => {
    getIamRoleMock.mockResolvedValue(fullDetail);
    renderView();
    await waitFor(() => expect(screen.getByTestId('iam-role-detail-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('iam-role-detail-tab-tags'));

    const triggers = screen.getAllByTestId('confirm-trigger');
    fireEvent.click(triggers[triggers.length - 1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteIamRolePermissionsBoundaryMock).toHaveBeenCalledWith('LambdaExec'),
    );
  });
});
