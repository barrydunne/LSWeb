import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { S3ListView } from './S3ListView';
import { createS3Bucket, deleteS3Bucket, getS3Buckets } from '../../api/client';
import type { S3BucketListResult } from '../../api/client';

vi.mock('../../api/client');

const getS3BucketsMock = vi.mocked(getS3Buckets);
const createS3BucketMock = vi.mocked(createS3Bucket);
const deleteS3BucketMock = vi.mocked(deleteS3Bucket);

const listResult: S3BucketListResult = {
  buckets: [
    { name: 'orders', creationDate: '2026-01-02T03:04:05.0000000Z' },
    { name: 'invoices', creationDate: '2026-02-03T04:05:06.0000000Z' },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <S3ListView serviceKey="s3" />
    </MemoryRouter>,
  );
}

describe('S3ListView', () => {
  beforeEach(() => {
    getS3BucketsMock.mockResolvedValue(listResult);
  });

  afterEach(() => {
    cleanup();
    vi.useRealTimers();
    vi.resetAllMocks();
  });

  it('shows a loading state before buckets arrive', () => {
    getS3BucketsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('s3-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getS3BucketsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-error')).toBeInTheDocument());
  });

  it('renders a row per bucket', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-view')).toBeInTheDocument());

    expect(screen.getByTestId('data-list-row-orders')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-invoices')).toBeInTheDocument();
  });

  it('formats the bucket creation date', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-view')).toBeInTheDocument());

    const created = screen.getByText('02 Jan 2026, 03:04:05 UTC');
    expect(created).toBeInTheDocument();
    expect(created).toHaveAttribute('title', '2026-01-02T03:04:05.0000000Z');
  });

  it('links each bucket name to its detail route', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-view')).toBeInTheDocument());

    const links = screen.getAllByTestId('s3-list-link');
    expect(links[0]).toHaveAttribute('href', '/services/s3/orders');
    expect(links[1]).toHaveAttribute('href', '/services/s3/invoices');
  });

  it('creates a bucket from the form and refreshes the list', async () => {
    createS3BucketMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-create-toggle'));

    fireEvent.change(screen.getByTestId('s3-create-bucketName'), {
      target: { value: 'new-bucket' },
    });

    fireEvent.click(screen.getByTestId('s3-create-submit'));

    await waitFor(() => expect(screen.getByTestId('s3-create-status')).toBeInTheDocument());

    expect(createS3BucketMock).toHaveBeenCalledWith('new-bucket');
    await waitFor(() => expect(getS3BucketsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('s3-create-form')).not.toBeInTheDocument();
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-create-toggle'));
    expect(screen.getByTestId('s3-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('s3-create-toggle'));
    expect(screen.queryByTestId('s3-create-form')).not.toBeInTheDocument();
  });

  it('shows an error when bucket creation fails', async () => {
    createS3BucketMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-create-toggle'));
    fireEvent.click(screen.getByTestId('s3-create-submit'));

    await waitFor(() => expect(screen.getByTestId('s3-create-error')).toBeInTheDocument());
    expect(screen.getByTestId('s3-create-form')).toBeInTheDocument();
  });

  it('deletes a bucket after confirmation and refreshes the list', async () => {
    deleteS3BucketMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteS3BucketMock).toHaveBeenCalledWith('orders'));
    await waitFor(() => expect(getS3BucketsMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when bucket deletion fails', async () => {
    deleteS3BucketMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('s3-list-error')).toBeInTheDocument());
  });

  it('reloads buckets when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await act(async () => {
        await Promise.resolve();
      });
      expect(getS3BucketsMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      await act(async () => {
        await vi.advanceTimersByTimeAsync(5_000);
      });

      await vi.waitFor(() => expect(getS3BucketsMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });
});
