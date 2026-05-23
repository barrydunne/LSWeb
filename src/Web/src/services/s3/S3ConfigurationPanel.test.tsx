import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { S3ConfigurationPanel } from './S3ConfigurationPanel';
import { getS3BucketConfiguration, resolveReference } from '../../api/client';
import type { S3BucketConfigurationResult } from '../../api/client';

vi.mock('../../api/client');

const getConfigurationMock = vi.mocked(getS3BucketConfiguration);
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
    resolveReferenceMock.mockResolvedValue({
      serviceKey: 'lambda',
      resourceId: 'process',
      route: '/services/lambda/process',
    });
  });

  afterEach(() => {
    vi.resetAllMocks();
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
    expect(screen.getByTestId('s3-config-policy-document')).toHaveTextContent(
      '{"Version":"2012-10-17"}',
    );
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
});
