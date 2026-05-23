import { afterEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { S3StorageSummaryCard } from './S3StorageSummaryCard';
import { getS3BucketStorageSummary } from '../../api/client';

vi.mock('../../api/client');

const getSummaryMock = vi.mocked(getS3BucketStorageSummary);

describe('S3StorageSummaryCard', () => {
  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the summary arrives', () => {
    getSummaryMock.mockReturnValue(new Promise(() => {}));

    render(<S3StorageSummaryCard bucketName="data" />);

    expect(screen.getByTestId('s3-storage-summary-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSummaryMock.mockRejectedValue(new Error('boom'));

    render(<S3StorageSummaryCard bucketName="data" />);

    await waitFor(() =>
      expect(screen.getByTestId('s3-storage-summary-error')).toBeInTheDocument(),
    );
  });

  it('renders the object count and formatted total size', async () => {
    getSummaryMock.mockResolvedValue({ objectCount: 1234, totalSizeBytes: 2048 });

    render(<S3StorageSummaryCard bucketName="data" />);

    await waitFor(() =>
      expect(screen.getByTestId('s3-storage-summary')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('s3-storage-summary-object-count')).toHaveTextContent('1,234');
    expect(screen.getByTestId('s3-storage-summary-total-size')).toHaveTextContent('2.0 KB');
    expect(getSummaryMock).toHaveBeenCalledWith('data', expect.any(AbortSignal));
  });
});
