import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { IamPoliciesPanel } from './IamPoliciesPanel';
import { createIamPolicy, deleteIamPolicy, getIamPolicies } from '../../api/client';
import type { IamPolicyListResult, IamPolicyScope } from '../../api/client';

vi.mock('../../api/client');

const getIamPoliciesMock = vi.mocked(getIamPolicies);
const createIamPolicyMock = vi.mocked(createIamPolicy);
const deleteIamPolicyMock = vi.mocked(deleteIamPolicy);

const localResult: IamPolicyListResult = {
  policies: [
    {
      policyName: 'AppReadWrite',
      arn: 'arn:aws:iam::000000000000:policy/AppReadWrite',
      policyId: 'ANPA0001',
      path: '/',
      defaultVersionId: 'v2',
      attachmentCount: 3,
      isAttachable: true,
      description: 'App access',
      createDate: '2024-01-01T00:00:00Z',
      updateDate: '2024-02-01T00:00:00Z',
    },
  ],
};

const awsResult: IamPolicyListResult = {
  policies: [
    {
      policyName: 'AdministratorAccess',
      arn: 'arn:aws:iam::aws:policy/AdministratorAccess',
      policyId: 'ANPA9999',
      path: '/',
      defaultVersionId: 'v1',
      attachmentCount: 0,
      isAttachable: true,
      description: null,
      createDate: null,
      updateDate: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <IamPoliciesPanel serviceKey="iam" />
    </MemoryRouter>,
  );
}

function submitDocument() {
  fireEvent.click(screen.getByTestId('iam-policies-create-document-edit'));
  fireEvent.click(screen.getByTestId('iam-policies-create-document-save'));
}

describe('IamPoliciesPanel', () => {
  beforeEach(() => {
    getIamPoliciesMock.mockImplementation((scope: IamPolicyScope) =>
      Promise.resolve(scope === 'aws' ? awsResult : localResult),
    );
    createIamPolicyMock.mockResolvedValue();
    deleteIamPolicyMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before policies arrive', () => {
    getIamPoliciesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('iam-policies-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getIamPoliciesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-policies-error')).toBeInTheDocument());
  });

  it('renders customer-managed policies with links to the detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('iam-policies-panel')).toBeInTheDocument());
    await screen.findByTestId('iam-policies-link');

    const link = screen.getByTestId('iam-policies-link');
    expect(link).toHaveTextContent('AppReadWrite');
    expect(link).toHaveAttribute(
      'href',
      `/services/iam/${encodeURIComponent('policy/arn:aws:iam::000000000000:policy/AppReadWrite')}`,
    );
    expect(getIamPoliciesMock).toHaveBeenCalledWith('local', expect.anything());
  });

  it('switches to read-only AWS-managed policies', async () => {
    renderView();
    await screen.findByTestId('iam-policies-link');

    fireEvent.click(screen.getByTestId('iam-policies-scope-aws'));

    await waitFor(() =>
      expect(getIamPoliciesMock).toHaveBeenCalledWith('aws', expect.anything()),
    );
    await screen.findByTestId('iam-policies-readonly');
    expect(screen.queryByTestId('iam-policies-create-toggle')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('iam-policies-scope-local'));
    await waitFor(() =>
      expect(getIamPoliciesMock).toHaveBeenLastCalledWith('local', expect.anything()),
    );
    await screen.findByTestId('iam-policies-create-toggle');
  });

  it('toggles the create form', async () => {
    renderView();
    await screen.findByTestId('iam-policies-link');

    expect(screen.queryByTestId('iam-policies-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('iam-policies-create-toggle'));
    expect(screen.getByTestId('iam-policies-create-form')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('iam-policies-create-toggle'));
    expect(screen.queryByTestId('iam-policies-create-form')).not.toBeInTheDocument();
  });

  it('creates a policy with trimmed optional fields', async () => {
    renderView();
    await screen.findByTestId('iam-policies-link');
    fireEvent.click(screen.getByTestId('iam-policies-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-policies-create-name'), {
      target: { value: 'NewPolicy' },
    });
    fireEvent.change(screen.getByTestId('iam-policies-create-path'), {
      target: { value: '  /eng/  ' },
    });
    fireEvent.change(screen.getByTestId('iam-policies-create-description'), {
      target: { value: '  Engineering  ' },
    });
    submitDocument();

    await waitFor(() =>
      expect(screen.getByTestId('iam-policies-create-status')).toBeInTheDocument(),
    );
    expect(createIamPolicyMock).toHaveBeenCalledWith({
      policyName: 'NewPolicy',
      policyDocument: expect.stringContaining('2012-10-17'),
      description: 'Engineering',
      path: '/eng/',
    });
  });

  it('creates a policy with null optional fields when left blank', async () => {
    renderView();
    await screen.findByTestId('iam-policies-link');
    fireEvent.click(screen.getByTestId('iam-policies-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-policies-create-name'), {
      target: { value: 'NewPolicy' },
    });
    submitDocument();

    await waitFor(() =>
      expect(screen.getByTestId('iam-policies-create-status')).toBeInTheDocument(),
    );
    expect(createIamPolicyMock).toHaveBeenCalledWith({
      policyName: 'NewPolicy',
      policyDocument: expect.stringContaining('Statement'),
      description: null,
      path: null,
    });
  });

  it('shows an error when policy creation fails', async () => {
    createIamPolicyMock.mockRejectedValue(new Error('boom'));
    renderView();
    await screen.findByTestId('iam-policies-link');
    fireEvent.click(screen.getByTestId('iam-policies-create-toggle'));

    fireEvent.change(screen.getByTestId('iam-policies-create-name'), {
      target: { value: 'NewPolicy' },
    });
    submitDocument();

    await waitFor(() =>
      expect(screen.getByTestId('iam-policies-create-error')).toBeInTheDocument(),
    );
  });

  it('deletes a policy after confirmation', async () => {
    renderView();
    await screen.findByTestId('iam-policies-link');

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteIamPolicyMock).toHaveBeenCalledWith(
        'arn:aws:iam::000000000000:policy/AppReadWrite',
      ),
    );
  });

  it('shows an error when policy deletion fails', async () => {
    deleteIamPolicyMock.mockRejectedValue(new Error('boom'));
    renderView();
    await screen.findByTestId('iam-policies-link');

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('iam-policies-error')).toBeInTheDocument());
  });
});
