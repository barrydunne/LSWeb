import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, cleanup, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { S3ConfigurationPanel } from './S3ConfigurationPanel';
import {
  deleteS3BucketPolicy,
  deleteS3ObjectVersion,
  getS3BucketConfiguration,
  getS3ObjectVersions,
  putS3BucketNotifications,
  putS3BucketPolicy,
  resolveReference,
  setS3BucketVersioning,
} from '../../api/client';
import type { S3BucketConfigurationResult } from '../../api/client';

vi.mock('../../api/client');

const getConfigurationMock = vi.mocked(getS3BucketConfiguration);
const putPolicyMock = vi.mocked(putS3BucketPolicy);
const deletePolicyMock = vi.mocked(deleteS3BucketPolicy);
const setVersioningMock = vi.mocked(setS3BucketVersioning);
const getVersionsMock = vi.mocked(getS3ObjectVersions);
const deleteVersionMock = vi.mocked(deleteS3ObjectVersion);
const putNotificationsMock = vi.mocked(putS3BucketNotifications);
const resolveReferenceMock = vi.mocked(resolveReference);

const fullConfiguration: S3BucketConfigurationResult = {
  versioningStatus: 'Enabled',
  encryptionAlgorithm: 'aws:kms',
  encryptionKeyId: 'arn:aws:kms:us-east-1:000000000000:key/abc',
  lifecycleRules: [
    { id: 'archive', status: 'Enabled', prefix: 'logs/' },
    { id: 'expire', status: 'Disabled', prefix: '' },
  ],
  notifications: [
    {
      type: 'Lambda',
      targetArn: 'arn:aws:lambda:us-east-1:000000000000:function:process',
      events: ['s3:ObjectCreated:*'],
      prefix: 'uploads/',
      suffix: '.json',
    },
  ],
  policy: '{"Version":"2012-10-17"}',
};

const emptyConfiguration: S3BucketConfigurationResult = {
  versioningStatus: 'Disabled',
  encryptionAlgorithm: '',
  encryptionKeyId: '',
  lifecycleRules: [],
  notifications: [],
  policy: '',
};

function renderPanel() {
  return render(
    <MemoryRouter>
      <S3ConfigurationPanel bucketName="data" />
    </MemoryRouter>,
  );
}

describe('S3ConfigurationPanel', () => {
  beforeEach(() => {
    putPolicyMock.mockResolvedValue();
    deletePolicyMock.mockResolvedValue();
    setVersioningMock.mockResolvedValue();
    deleteVersionMock.mockResolvedValue();
    putNotificationsMock.mockResolvedValue();
    getVersionsMock.mockResolvedValue({ versions: [] });
    resolveReferenceMock.mockResolvedValue({
      serviceKey: 'lambda',
      resourceId: 'process',
      route: '/services/lambda/process',
    });
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before the configuration arrives', () => {
    getConfigurationMock.mockReturnValue(new Promise(() => {}));

    renderPanel();

    expect(screen.getByTestId('s3-config-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getConfigurationMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('s3-config-error')).toBeInTheDocument());
  });

  it('renders versioning, encryption, lifecycle, notifications and policy', async () => {
    getConfigurationMock.mockResolvedValue(fullConfiguration);

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    expect(screen.getByTestId('s3-config-versioning-status')).toHaveTextContent('Enabled');
    expect(screen.getByTestId('s3-config-encryption-algorithm')).toHaveTextContent('aws:kms');
    expect(screen.getByTestId('s3-config-encryption-key')).toHaveTextContent(
      'arn:aws:kms:us-east-1:000000000000:key/abc',
    );

    const lifecycleRows = screen.getAllByTestId('s3-config-lifecycle-row');
    expect(lifecycleRows).toHaveLength(2);
    expect(lifecycleRows[1]).toHaveTextContent('(all objects)');

    expect(screen.getByTestId('s3-config-notification-row')).toHaveTextContent('s3:ObjectCreated:*');
    expect(screen.getByTestId('s3-config-notification-prefix')).toHaveTextContent('Prefix: uploads/');
    expect(screen.getByTestId('s3-config-notification-suffix')).toHaveTextContent('Suffix: .json');
    expect(screen.getByTestId('s3-config-policy-document')).toHaveTextContent(
      '{"Version":"2012-10-17"}',
    );
    await waitFor(() => expect(resolveReferenceMock).toHaveBeenCalled());
  });

  it('resolves the notification target as a cross-resource link', async () => {
    getConfigurationMock.mockResolvedValue(fullConfiguration);

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('resource-link')).toHaveAttribute('href', '/services/lambda/process'),
    );

    expect(resolveReferenceMock).toHaveBeenCalledWith(
      'arn:aws:lambda:us-east-1:000000000000:function:process',
      'lambda',
      expect.any(AbortSignal),
    );
  });

  it('renders empty states when nothing is configured', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    expect(screen.getByTestId('s3-config-versioning-status')).toHaveTextContent('Disabled');
    expect(screen.getByTestId('s3-config-encryption-none')).toBeInTheDocument();
    expect(screen.getByTestId('s3-config-lifecycle-empty')).toBeInTheDocument();
    expect(screen.getByTestId('s3-config-notifications-empty')).toBeInTheDocument();
    expect(screen.getByTestId('s3-config-policy-empty')).toBeInTheDocument();
  });

  it('renders encryption without a KMS key when none is configured', async () => {
    getConfigurationMock.mockResolvedValue({
      ...emptyConfiguration,
      encryptionAlgorithm: 'AES256',
    });

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    expect(screen.getByTestId('s3-config-encryption-algorithm')).toHaveTextContent('AES256');
    expect(screen.queryByTestId('s3-config-encryption-key')).not.toBeInTheDocument();
  });

  it('shows only the prefix when a notification has no suffix filter', async () => {
    getConfigurationMock.mockResolvedValue({
      ...emptyConfiguration,
      notifications: [
        {
          type: 'Lambda',
          targetArn: 'arn:aws:lambda:us-east-1:000000000000:function:process',
          events: ['s3:ObjectCreated:*'],
          prefix: 'uploads/',
          suffix: '',
        },
      ],
    });

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    expect(screen.getByTestId('s3-config-notification-prefix')).toHaveTextContent('Prefix: uploads/');
    expect(screen.queryByTestId('s3-config-notification-suffix')).not.toBeInTheDocument();
    await waitFor(() => expect(resolveReferenceMock).toHaveBeenCalled());
  });

  it('shows only the suffix when a notification has no prefix filter', async () => {
    getConfigurationMock.mockResolvedValue({
      ...emptyConfiguration,
      notifications: [
        {
          type: 'Lambda',
          targetArn: 'arn:aws:lambda:us-east-1:000000000000:function:process',
          events: ['s3:ObjectCreated:*'],
          prefix: '',
          suffix: '.json',
        },
      ],
    });

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    expect(screen.queryByTestId('s3-config-notification-prefix')).not.toBeInTheDocument();
    expect(screen.getByTestId('s3-config-notification-suffix')).toHaveTextContent('Suffix: .json');
    await waitFor(() => expect(resolveReferenceMock).toHaveBeenCalled());
  });

  it('shows no filter when a notification has neither a prefix nor a suffix', async () => {
    getConfigurationMock.mockResolvedValue({
      ...emptyConfiguration,
      notifications: [
        {
          type: 'Lambda',
          targetArn: 'arn:aws:lambda:us-east-1:000000000000:function:process',
          events: ['s3:ObjectCreated:*'],
          prefix: '',
          suffix: '',
        },
      ],
    });

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    const filterCell = screen.getByTestId('s3-config-notification-filter');
    expect(filterCell).toHaveTextContent('(no filter)');
    expect(screen.queryByTestId('s3-config-notification-prefix')).not.toBeInTheDocument();
    expect(screen.queryByTestId('s3-config-notification-suffix')).not.toBeInTheDocument();
    await waitFor(() => expect(resolveReferenceMock).toHaveBeenCalled());
  });

  it('inserts the template and applies a bucket policy', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-policy-template'));
    expect((screen.getByTestId('s3-policy-editor') as HTMLTextAreaElement).value).toContain(
      'PublicRead',
    );
    fireEvent.click(screen.getByTestId('s3-policy-apply'));

    await waitFor(() => expect(screen.getByTestId('s3-policy-status')).toBeInTheDocument());
    expect(putPolicyMock).toHaveBeenCalledWith('data', expect.stringContaining('PublicRead'));
  });

  it('blocks applying an invalid JSON policy', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-policy-editor'), { target: { value: 'not json' } });
    fireEvent.click(screen.getByTestId('s3-policy-apply'));

    expect(screen.getByTestId('s3-policy-error')).toHaveTextContent('valid JSON');
    expect(putPolicyMock).not.toHaveBeenCalled();
  });

  it('blocks applying a policy without Version and Statement', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-policy-editor'), {
      target: { value: '{"Version":"2012-10-17"}' },
    });
    fireEvent.click(screen.getByTestId('s3-policy-apply'));

    expect(screen.getByTestId('s3-policy-error')).toHaveTextContent('Version');
    expect(putPolicyMock).not.toHaveBeenCalled();
  });

  it('blocks applying a policy that is a JSON array', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-policy-editor'), { target: { value: '[]' } });
    fireEvent.click(screen.getByTestId('s3-policy-apply'));

    expect(screen.getByTestId('s3-policy-error')).toBeInTheDocument();
    expect(putPolicyMock).not.toHaveBeenCalled();
  });

  it('shows an error when applying the policy fails', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);
    putPolicyMock.mockRejectedValue(new Error('boom'));

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-policy-editor'), {
      target: { value: '{"Version":"2012-10-17","Statement":[]}' },
    });
    fireEvent.click(screen.getByTestId('s3-policy-apply'));

    await waitFor(() =>
      expect(screen.getByTestId('s3-policy-error')).toHaveTextContent('Unable to apply the policy.'),
    );
  });

  it('removes an existing bucket policy', async () => {
    getConfigurationMock.mockResolvedValue(fullConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-policy-remove'));

    await waitFor(() => expect(deletePolicyMock).toHaveBeenCalledWith('data'));
  });

  it('enables versioning when it is disabled', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-versioning-enable'));

    await waitFor(() => expect(setVersioningMock).toHaveBeenCalledWith('data', true));
  });

  it('suspends versioning when it is enabled', async () => {
    getConfigurationMock.mockResolvedValue(fullConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-versioning-suspend'));

    await waitFor(() => expect(setVersioningMock).toHaveBeenCalledWith('data', false));
  });

  it('shows an error when the versioning action fails', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);
    setVersioningMock.mockRejectedValue(new Error('boom'));

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-versioning-enable'));

    await waitFor(() => expect(screen.getByTestId('s3-versioning-error')).toBeInTheDocument());
  });

  it('lists object versions and deletes one', async () => {
    getConfigurationMock.mockResolvedValue(fullConfiguration);
    getVersionsMock.mockResolvedValue({
      versions: [
        { key: 'report.pdf', versionId: 'v2', isLatest: true, isDeleteMarker: false, size: 10, lastModified: 't' },
        { key: 'report.pdf', versionId: 'v1', isLatest: false, isDeleteMarker: true, size: 0, lastModified: 't' },
      ],
    });

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    expect(screen.getAllByTestId('s3-version-row')).toHaveLength(2);
    expect(screen.getByTestId('s3-version-latest')).toBeInTheDocument();
    expect(screen.getByTestId('s3-version-delete-marker')).toBeInTheDocument();

    fireEvent.click(screen.getAllByTestId('s3-version-delete')[0]);
    await waitFor(() => expect(deleteVersionMock).toHaveBeenCalledWith('data', 'report.pdf', 'v2'));
  });

  it('shows an empty versions state when there are none', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-versions-empty')).toBeInTheDocument());
  });

  it('shows an error when the versions request fails', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);
    getVersionsMock.mockRejectedValue(new Error('boom'));

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-versions-error')).toBeInTheDocument());
  });

  it('shows a loading state while the versions are loading', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);
    getVersionsMock.mockReturnValue(new Promise(() => {}));

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());
    expect(screen.getByTestId('s3-versions-loading')).toBeInTheDocument();
  });

  it('adds an event notification', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-notification-arn'), {
      target: { value: 'arn:aws:lambda:eu-west-1:000000000000:function:p' },
    });
    fireEvent.change(screen.getByTestId('s3-notification-prefix'), { target: { value: 'logs/' } });
    fireEvent.change(screen.getByTestId('s3-notification-suffix'), { target: { value: '.json' } });
    fireEvent.click(screen.getByTestId('s3-notification-add'));

    await waitFor(() => expect(putNotificationsMock).toHaveBeenCalled());
    expect(putNotificationsMock).toHaveBeenCalledWith('data', [
      {
        type: 'Lambda',
        targetArn: 'arn:aws:lambda:eu-west-1:000000000000:function:p',
        events: ['s3:ObjectCreated:*'],
        prefix: 'logs/',
        suffix: '.json',
      },
    ]);
  });

  it('blocks adding a notification when the ARN does not match the type', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-notification-arn'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:q' },
    });
    fireEvent.click(screen.getByTestId('s3-notification-add'));

    expect(screen.getByTestId('s3-notification-error')).toHaveTextContent('Lambda ARN');
    expect(putNotificationsMock).not.toHaveBeenCalled();
  });

  it('blocks adding a notification when there are no events', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-notification-type'), { target: { value: 'Topic' } });
    fireEvent.change(screen.getByTestId('s3-notification-arn'), {
      target: { value: 'arn:aws:sns:eu-west-1:000000000000:t' },
    });
    fireEvent.change(screen.getByTestId('s3-notification-events'), { target: { value: '  ' } });
    fireEvent.click(screen.getByTestId('s3-notification-add'));

    expect(screen.getByTestId('s3-notification-error')).toHaveTextContent('at least one event');
    expect(putNotificationsMock).not.toHaveBeenCalled();
  });

  it('removes an existing notification', async () => {
    getConfigurationMock.mockResolvedValue(fullConfiguration);

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-notification-remove'));

    await waitFor(() => expect(putNotificationsMock).toHaveBeenCalledWith('data', []));
  });

  it('shows an error when updating notifications fails', async () => {
    getConfigurationMock.mockResolvedValue(emptyConfiguration);
    putNotificationsMock.mockRejectedValue(new Error('boom'));

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-notification-arn'), {
      target: { value: 'arn:aws:lambda:eu-west-1:000000000000:function:p' },
    });
    fireEvent.click(screen.getByTestId('s3-notification-add'));

    await waitFor(() =>
      expect(screen.getByTestId('s3-notification-error')).toHaveTextContent(
        'Unable to update the notifications.',
      ),
    );
  });
});
