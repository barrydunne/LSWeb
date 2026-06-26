import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SqsListView } from './SqsListView';
import { createSqsQueue, deleteSqsQueue, getSqsQueues } from '../../api/client';
import type { SqsQueueListResult } from '../../api/client';

vi.mock('../../api/client');

const getSqsQueuesMock = vi.mocked(getSqsQueues);
const createSqsQueueMock = vi.mocked(createSqsQueue);
const deleteSqsQueueMock = vi.mocked(deleteSqsQueue);

const listResult: SqsQueueListResult = {
  queues: [
    {
      name: 'orders',
      url: 'http://localhost:4566/000000000000/orders',
      approximateMessageCount: 3,
      approximateInFlightCount: 1,
      approximateDelayedCount: 2,
    },
    {
      name: 'invoices',
      url: 'http://localhost:4566/000000000000/invoices',
      approximateMessageCount: 0,
      approximateInFlightCount: 0,
      approximateDelayedCount: 0,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <SqsListView serviceKey="sqs" />
    </MemoryRouter>,
  );
}

describe('SqsListView', () => {
  beforeEach(() => {
    getSqsQueuesMock.mockResolvedValue(listResult);
  });

  afterEach(() => {
    cleanup();
    vi.useRealTimers();
    vi.resetAllMocks();
  });

  it('shows a loading state before queues arrive', () => {
    getSqsQueuesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('sqs-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSqsQueuesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-error')).toBeInTheDocument());
  });

  it('renders a row per queue', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    expect(screen.getByTestId('data-list-row-orders')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-invoices')).toBeInTheDocument();
  });

  it('shows the approximate message counts for each queue', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    const available = screen.getAllByTestId('sqs-list-available');
    const inFlight = screen.getAllByTestId('sqs-list-inflight');
    const delayed = screen.getAllByTestId('sqs-list-delayed');
    expect(available[0]).toHaveTextContent('3');
    expect(inFlight[0]).toHaveTextContent('1');
    expect(delayed[0]).toHaveTextContent('2');
  });

  it('links each queue to its detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    const links = screen.getAllByTestId('sqs-list-link');
    expect(links[0]).toHaveAttribute('href', '/services/sqs/orders');
  });

  it('reloads the queues when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());
      expect(getSqsQueuesMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      await act(async () => {
        await vi.advanceTimersByTimeAsync(5000);
      });

      await vi.waitFor(() => expect(getSqsQueuesMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('creates a standard queue from the form and refreshes the list', async () => {
    createSqsQueueMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sqs-create-toggle'));

    fireEvent.change(screen.getByTestId('sqs-create-queueName'), {
      target: { value: 'new-queue' },
    });

    fireEvent.click(screen.getByTestId('sqs-create-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-create-status')).toBeInTheDocument());

    expect(createSqsQueueMock).toHaveBeenCalledWith('new-queue', false);
    await waitFor(() => expect(getSqsQueuesMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('sqs-create-form')).not.toBeInTheDocument();
  });

  it('creates a FIFO queue when the checkbox is ticked', async () => {
    createSqsQueueMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sqs-create-toggle'));

    fireEvent.change(screen.getByTestId('sqs-create-queueName'), {
      target: { value: 'new-queue.fifo' },
    });
    fireEvent.click(screen.getByTestId('sqs-create-fifo'));

    fireEvent.click(screen.getByTestId('sqs-create-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-create-status')).toBeInTheDocument());
    expect(createSqsQueueMock).toHaveBeenCalledWith('new-queue.fifo', true);
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sqs-create-toggle'));
    expect(screen.getByTestId('sqs-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('sqs-create-toggle'));
    expect(screen.queryByTestId('sqs-create-form')).not.toBeInTheDocument();
  });

  it('shows an error when queue creation fails', async () => {
    createSqsQueueMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sqs-create-toggle'));
    fireEvent.click(screen.getByTestId('sqs-create-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-create-error')).toBeInTheDocument());
    expect(screen.getByTestId('sqs-create-form')).toBeInTheDocument();
  });

  it('deletes a queue after confirmation and refreshes the list', async () => {
    deleteSqsQueueMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteSqsQueueMock).toHaveBeenCalledWith('orders'));
    await waitFor(() => expect(getSqsQueuesMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when queue deletion fails', async () => {
    deleteSqsQueueMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('sqs-list-error')).toBeInTheDocument());
  });
});