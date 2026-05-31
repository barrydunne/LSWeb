import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { IamPolicyDetailView } from './IamPolicyDetailView';
import {
  createIamPolicyVersion,
  deleteIamPolicyVersion,
  getIamPolicy,
  setIamPolicyDefaultVersion,
  tagIamPolicy,
  untagIamPolicy,
} from '../../api/client';
import type { IamPolicyDetail } from '../../api/client';

vi.mock('../../api/client');

const getIamPolicyMock = vi.mocked(getIamPolicy);
const createIamPolicyVersionMock = vi.mocked(createIamPolicyVersion);
const setIamPolicyDefaultVersionMock = vi.mocked(setIamPolicyDefaultVersion);
const deleteIamPolicyVersionMock = vi.mocked(deleteIamPolicyVersion);
const tagIamPolicyMock = vi.mocked(tagIamPolicy);
const untagIamPolicyMock = vi.mocked(untagIamPolicy);

const localArn = 'arn:aws:iam::000000000000:policy/AppReadWrite';
const awsArn = 'arn:aws:iam::aws:policy/AdministratorAccess';

const localDetail: IamPolicyDetail = {
  policyName: 'AppReadWrite',
  arn: localArn,
  policyId: 'ANPA0001',
  path: '/',
  defaultVersionId: 'v2',
  attachmentCount: 3,
  isAttachable: true,
  description: 'App access',
  createDate: '2024-01-01T00:00:00Z',
  updateDate: '2024-02-01T00:00:00Z',
  defaultVersionDocument: '{"Version":"2012-10-17","Statement":[]}',
  versions: [
    { versionId: 'v2', isDefaultVersion: true, createDate: '2024-02-01T00:00:00Z' },
    { versionId: 'v1', isDefaultVersion: false, createDate: null },
  ],
  tags: [],
};

const awsDetail: IamPolicyDetail = {
  policyName: 'AdministratorAccess',
  arn: awsArn,
  policyId: 'ANPA9999',
  path: '/',
  defaultVersionId: 'v1',
  attachmentCount: 0,
  isAttachable: true,
  description: null,
  createDate: null,
  updateDate: null,
  defaultVersionDocument: '{}',
  versions: [],
  tags: [],
};

function renderView(arn: string) {
  return render(<IamPolicyDetailView policyArn={arn} />);
}

function submitNewVersion() {
  fireEvent.click(screen.getByTestId('iam-policy-detail-add-version-edit'));
  fireEvent.click(screen.getByTestId('iam-policy-detail-add-version-save'));
}

describe('IamPolicyDetailView', () => {
  beforeEach(() => {
    getIamPolicyMock.mockResolvedValue(localDetail);
    createIamPolicyVersionMock.mockResolvedValue();
    setIamPolicyDefaultVersionMock.mockResolvedValue();
    deleteIamPolicyVersionMock.mockResolvedValue();
    tagIamPolicyMock.mockResolvedValue();
    untagIamPolicyMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before the policy arrives', () => {
    getIamPolicyMock.mockReturnValue(new Promise(() => {}));

    renderView(localArn);

    expect(screen.getByTestId('iam-policy-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getIamPolicyMock.mockRejectedValue(new Error('boom'));

    renderView(localArn);

    await waitFor(() =>
      expect(screen.getByTestId('iam-policy-detail-error')).toBeInTheDocument(),
    );
  });

  it('renders the customer-managed policy with its version history', async () => {
    renderView(localArn);

    await screen.findByTestId('iam-policy-detail-view');
    expect(screen.getByTestId('iam-policy-detail-name')).toHaveTextContent('AppReadWrite');
    expect(screen.getByTestId('iam-policy-detail-description')).toHaveTextContent('App access');
    expect(screen.getByTestId('iam-policy-detail-attachments')).toHaveTextContent('3');
    expect(screen.getByTestId('iam-policy-detail-created')).toHaveTextContent('2024-01-01');
    expect(screen.getByTestId('iam-policy-detail-updated')).toHaveTextContent('2024-02-01');
    expect(screen.getAllByTestId('iam-policy-detail-version-item')).toHaveLength(2);
    expect(screen.getByTestId('iam-policy-detail-version-default')).toBeInTheDocument();
    expect(screen.getByTestId('iam-policy-detail-add-version-form')).toBeInTheDocument();
    expect(screen.queryByTestId('iam-policy-detail-readonly')).not.toBeInTheDocument();
  });

  it('renders AWS-managed policies read-only with no versions', async () => {
    getIamPolicyMock.mockResolvedValue(awsDetail);

    renderView(awsArn);

    await screen.findByTestId('iam-policy-detail-view');
    expect(screen.getByTestId('iam-policy-detail-readonly')).toBeInTheDocument();
    expect(screen.getByTestId('iam-policy-detail-versions-empty')).toBeInTheDocument();
    expect(screen.getByTestId('iam-policy-detail-description')).toHaveTextContent('\u2014');
    expect(screen.getByTestId('iam-policy-detail-created')).toHaveTextContent('\u2014');
    expect(screen.queryByTestId('iam-policy-detail-add-version-form')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('iam-policy-detail-version-set-default'),
    ).not.toBeInTheDocument();
  });

  it('adds a new version honouring the set-as-default toggle', async () => {
    renderView(localArn);
    await screen.findByTestId('iam-policy-detail-view');

    fireEvent.click(screen.getByTestId('iam-policy-detail-set-default'));
    submitNewVersion();

    await waitFor(() =>
      expect(createIamPolicyVersionMock).toHaveBeenCalledWith(
        localArn,
        expect.stringContaining('2012-10-17'),
        false,
      ),
    );
  });

  it('shows a mutation error when adding a version fails', async () => {
    createIamPolicyVersionMock.mockRejectedValue(new Error('boom'));
    renderView(localArn);
    await screen.findByTestId('iam-policy-detail-view');

    submitNewVersion();

    await waitFor(() =>
      expect(screen.getByTestId('iam-policy-detail-mutation-error')).toBeInTheDocument(),
    );
  });

  it('sets a non-default version as default', async () => {
    renderView(localArn);
    await screen.findByTestId('iam-policy-detail-view');

    fireEvent.click(screen.getByTestId('iam-policy-detail-version-set-default'));

    await waitFor(() =>
      expect(setIamPolicyDefaultVersionMock).toHaveBeenCalledWith(localArn, 'v1'),
    );
  });

  it('deletes a non-default version after confirmation', async () => {
    renderView(localArn);
    await screen.findByTestId('iam-policy-detail-view');

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteIamPolicyVersionMock).toHaveBeenCalledWith(localArn, 'v1'),
    );
  });

  it('falls back to the raw document when it is not valid JSON', async () => {
    getIamPolicyMock.mockResolvedValue({ ...localDetail, defaultVersionDocument: 'not-json' });

    renderView(localArn);

    await screen.findByTestId('iam-policy-detail-view');
    expect(screen.getByTestId('iam-policy-detail-document')).toBeInTheDocument();
  });

  it('adds a tag', async () => {
    renderView(localArn);
    await screen.findByTestId('iam-policy-detail-view');

    fireEvent.change(screen.getByTestId('iam-policy-detail-tag-editor-key'), {
      target: { value: 'env' },
    });
    fireEvent.change(screen.getByTestId('iam-policy-detail-tag-editor-value'), {
      target: { value: 'prod' },
    });
    fireEvent.click(screen.getByTestId('iam-policy-detail-tag-editor-submit'));

    await waitFor(() =>
      expect(tagIamPolicyMock).toHaveBeenCalledWith(localArn, [{ key: 'env', value: 'prod' }]),
    );
  });

  it('removes a tag after confirmation', async () => {
    getIamPolicyMock.mockResolvedValue({ ...localDetail, tags: [{ key: 'env', value: 'prod' }] });
    renderView(localArn);
    await screen.findByTestId('iam-policy-detail-view');

    const triggers = screen.getAllByTestId('confirm-trigger');
    fireEvent.click(triggers[triggers.length - 1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(untagIamPolicyMock).toHaveBeenCalledWith(localArn, ['env']));
  });
});
