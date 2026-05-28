import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { SqsDetailView } from './SqsDetailView';
import {
  deleteSqsMessage,
  getSqsQueueAttributes,
  getSqsQueueRedrive,
  getSqsQueueSubscriptions,
  pollSqsMessages,
  purgeSqsQueue,
  redriveSqsQueue,
  resolveReference,
  sendSqsMessage,
  updateSqsQueueAttributes,
} from '../../api/client';
import type { SqsMessageListResult, SqsQueueAttributesItem } from '../../api/client';

vi.mock('../../api/client');

const pollSqsMessagesMock = vi.mocked(pollSqsMessages);
const deleteSqsMessageMock = vi.mocked(deleteSqsMessage);
const purgeSqsQueueMock = vi.mocked(purgeSqsQueue);
const getSqsQueueSubscriptionsMock = vi.mocked(getSqsQueueSubscriptions);
const resolveReferenceMock = vi.mocked(resolveReference);
const sendSqsMessageMock = vi.mocked(sendSqsMessage);
const getSqsQueueAttributesMock = vi.mocked(getSqsQueueAttributes);
const updateSqsQueueAttributesMock = vi.mocked(updateSqsQueueAttributes);
const getSqsQueueRedriveMock = vi.mocked(getSqsQueueRedrive);
const redriveSqsQueueMock = vi.mocked(redriveSqsQueue);

const attributesResult: SqsQueueAttributesItem = {
  visibilityTimeoutSeconds: 30,
  messageRetentionPeriodSeconds: 345600,
  delaySeconds: 0,
  receiveMessageWaitTimeSeconds: 0,
  maximumMessageSizeBytes: 262144,
  queueArn: 'arn:aws:sqs:eu-west-1:000000000000:orders',
  fifoQueue: false,
};

const pollResult: SqsMessageListResult = {
  messages: [
    {
      messageId: 'id-1',
      receiptHandle: 'receipt-1',
      body: 'first body',
      attributes: { SentTimestamp: '1700000000000' },
      messageAttributes: { trace: 'abc' },
    },
    {
      messageId: 'id-2',
      receiptHandle: 'receipt-2',
      body: 'second body',
      attributes: {},
      messageAttributes: {},
    },
  ],
};

function renderView() {
  return render(<SqsDetailView serviceKey="sqs" resourceId="orders" />);
}

function renderFifoView() {
  return render(<SqsDetailView serviceKey="sqs" resourceId="orders.fifo" />);
}

describe('SqsDetailView', () => {
  beforeEach(() => {
    pollSqsMessagesMock.mockResolvedValue(pollResult);
    deleteSqsMessageMock.mockResolvedValue();
    purgeSqsQueueMock.mockResolvedValue();
    getSqsQueueSubscriptionsMock.mockResolvedValue({ subscriptions: [] });
    resolveReferenceMock.mockResolvedValue(null as never);
    sendSqsMessageMock.mockResolvedValue();
    getSqsQueueAttributesMock.mockResolvedValue(attributesResult);
    updateSqsQueueAttributesMock.mockResolvedValue();
    getSqsQueueRedriveMock.mockResolvedValue({ deadLetterTarget: null, sources: [] });
    redriveSqsQueueMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('renders the queue name and peek hint by default', () => {
    renderView();

    expect(screen.getByTestId('sqs-detail-title')).toHaveTextContent('orders');
    expect(screen.getByTestId('sqs-poll-hint')).toHaveTextContent('Peek keeps messages visible');
  });

  it('switches the hint when consume mode is selected', () => {
    renderView();

    fireEvent.change(screen.getByTestId('sqs-poll-mode'), { target: { value: 'consume' } });

    expect(screen.getByTestId('sqs-poll-hint')).toHaveTextContent('Consume hides messages');
  });

  it('polls in peek mode and lists the returned messages', async () => {
    renderView();

    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());

    expect(pollSqsMessagesMock).toHaveBeenCalledWith('orders', 'peek', 10);
    expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(2);
    expect(screen.getAllByTestId('sqs-message-id')[0]).toHaveTextContent('id-1');
  });

  it('polls in consume mode when selected', async () => {
    renderView();

    fireEvent.change(screen.getByTestId('sqs-poll-mode'), { target: { value: 'consume' } });
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());

    expect(pollSqsMessagesMock).toHaveBeenCalledWith('orders', 'consume', 10);
  });

  it('shows a loading state while polling', async () => {
    let resolvePoll: (value: SqsMessageListResult) => void = () => {};
    pollSqsMessagesMock.mockReturnValue(
      new Promise<SqsMessageListResult>((resolve) => {
        resolvePoll = resolve;
      }),
    );

    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    expect(screen.getByTestId('sqs-detail-loading')).toBeInTheDocument();

    resolvePoll(pollResult);
    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());
  });

  it('shows an empty state when no messages are returned', async () => {
    pollSqsMessagesMock.mockResolvedValue({ messages: [] });

    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-detail-empty')).toBeInTheDocument());
  });

  it('shows an error state when polling fails', async () => {
    pollSqsMessagesMock.mockRejectedValue(new Error('boom'));

    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-detail-error')).toBeInTheDocument());
  });

  it('deletes a message and removes it from the list', async () => {
    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(2));

    const triggers = screen.getAllByRole('button', { name: 'Delete' });
    fireEvent.click(triggers[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(1));
    expect(deleteSqsMessageMock).toHaveBeenCalledWith('orders', 'receipt-1');
    expect(screen.getByTestId('sqs-message-id')).toHaveTextContent('id-2');
  });

  it('shows an error state when deleting fails', async () => {
    deleteSqsMessageMock.mockRejectedValue(new Error('nope'));

    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(2));

    fireEvent.click(screen.getAllByRole('button', { name: 'Delete' })[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('sqs-detail-error')).toBeInTheDocument());
  });

  it('ignores a completed delete when the view is no longer showing messages', async () => {
    let resolveDelete: () => void = () => {};
    deleteSqsMessageMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveDelete = resolve;
      }),
    );
    let resolveSecondPoll: (value: SqsMessageListResult) => void = () => {};

    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));
    await waitFor(() => expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(2));

    // Arm and accept the delete, but it stays pending.
    fireEvent.click(screen.getAllByRole('button', { name: 'Delete' })[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    // Start a second poll that remains pending, moving the view into the loading state.
    pollSqsMessagesMock.mockReturnValue(
      new Promise<SqsMessageListResult>((resolve) => {
        resolveSecondPoll = resolve;
      }),
    );
    fireEvent.click(screen.getByTestId('sqs-poll-button'));
    await waitFor(() => expect(screen.getByTestId('sqs-detail-loading')).toBeInTheDocument());

    // The delete now resolves while the view is loading; the message list must not reappear.
    resolveDelete();
    await waitFor(() => expect(deleteSqsMessageMock).toHaveBeenCalledWith('orders', 'receipt-1'));
    expect(screen.getByTestId('sqs-detail-loading')).toBeInTheDocument();
    expect(screen.queryByTestId('sqs-message-list')).not.toBeInTheDocument();

    resolveSecondPoll({ messages: [] });
    await waitFor(() => expect(screen.getByTestId('sqs-detail-empty')).toBeInTheDocument());
  });

  it('purges the queue and clears the message list', async () => {
    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(2));

    fireEvent.click(screen.getByRole('button', { name: 'Purge queue' }));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('sqs-detail-empty')).toBeInTheDocument());
    expect(purgeSqsQueueMock).toHaveBeenCalledWith('orders');
    expect(screen.queryByTestId('sqs-message-list')).not.toBeInTheDocument();
  });

  it('shows an error state when purging fails', async () => {
    purgeSqsQueueMock.mockRejectedValue(new Error('nope'));

    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(2));

    fireEvent.click(screen.getByRole('button', { name: 'Purge queue' }));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('sqs-detail-error')).toBeInTheDocument());
  });

  it('ignores a completed purge when the view is no longer showing messages', async () => {
    let resolvePurge: () => void = () => {};
    purgeSqsQueueMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolvePurge = resolve;
      }),
    );
    let resolveSecondPoll: (value: SqsMessageListResult) => void = () => {};

    renderView();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));
    await waitFor(() => expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(2));

    // Arm and accept the purge, but it stays pending.
    fireEvent.click(screen.getByRole('button', { name: 'Purge queue' }));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    // Start a second poll that remains pending, moving the view into the loading state.
    pollSqsMessagesMock.mockReturnValue(
      new Promise<SqsMessageListResult>((resolve) => {
        resolveSecondPoll = resolve;
      }),
    );
    fireEvent.click(screen.getByTestId('sqs-poll-button'));
    await waitFor(() => expect(screen.getByTestId('sqs-detail-loading')).toBeInTheDocument());

    // The purge now resolves while the view is loading; the message list must not reappear.
    resolvePurge();
    await waitFor(() => expect(purgeSqsQueueMock).toHaveBeenCalledWith('orders'));
    expect(screen.getByTestId('sqs-detail-loading')).toBeInTheDocument();
    expect(screen.queryByTestId('sqs-message-list')).not.toBeInTheDocument();

    resolveSecondPoll({ messages: [] });
    await waitFor(() => expect(screen.getByTestId('sqs-detail-empty')).toBeInTheDocument());
  });

  it('does not render the subscriptions section when there are none', async () => {
    renderView();

    await waitFor(() => expect(getSqsQueueSubscriptionsMock).toHaveBeenCalledWith('orders', expect.anything()));
    expect(screen.queryByTestId('sqs-subscriptions')).not.toBeInTheDocument();
  });

  it('renders SNS subscriptions as cross-resource links', async () => {
    getSqsQueueSubscriptionsMock.mockResolvedValue({
      subscriptions: [
        { topicArn: 'arn:aws:sns:eu-west-1:000000000000:order-events', topicName: 'order-events' },
      ],
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-subscriptions')).toBeInTheDocument());
    const items = screen.getAllByTestId('sqs-subscription-item');
    expect(items).toHaveLength(1);
    expect(screen.getByTestId('resource-link')).toHaveTextContent('order-events');
    expect(resolveReferenceMock).toHaveBeenCalledWith(
      'arn:aws:sns:eu-west-1:000000000000:order-events',
      'sns',
      expect.anything(),
    );
  });

  it('ignores subscription lookup failures', async () => {
    getSqsQueueSubscriptionsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(getSqsQueueSubscriptionsMock).toHaveBeenCalled());
    expect(screen.queryByTestId('sqs-subscriptions')).not.toBeInTheDocument();
  });

  it('sends a message body for a standard queue', async () => {
    renderView();

    fireEvent.change(screen.getByTestId('sqs-send-body'), { target: { value: 'order created' } });
    fireEvent.click(screen.getByTestId('sqs-send-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-send-status')).toBeInTheDocument());
    expect(sendSqsMessageMock).toHaveBeenCalledWith('orders', {
      body: 'order created',
      messageGroupId: undefined,
      messageDeduplicationId: undefined,
    });
    expect(screen.getByTestId('sqs-send-body')).toHaveValue('');
  });

  it('does not render the FIFO id fields for a standard queue', () => {
    renderView();

    expect(screen.queryByTestId('sqs-send-group-id')).not.toBeInTheDocument();
  });

  it('requires a group id before a FIFO send is enabled', async () => {
    renderFifoView();

    fireEvent.change(screen.getByTestId('sqs-send-body'), { target: { value: 'fifo body' } });
    expect(screen.getByTestId('sqs-send-submit')).toBeDisabled();

    fireEvent.change(screen.getByTestId('sqs-send-group-id'), { target: { value: 'group-7' } });
    fireEvent.change(screen.getByTestId('sqs-send-dedup-id'), { target: { value: 'dedup-7' } });
    fireEvent.click(screen.getByTestId('sqs-send-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-send-status')).toBeInTheDocument());
    expect(sendSqsMessageMock).toHaveBeenCalledWith('orders.fifo', {
      body: 'fifo body',
      messageGroupId: 'group-7',
      messageDeduplicationId: 'dedup-7',
    });
  });

  it('shows an error when the send fails', async () => {
    sendSqsMessageMock.mockRejectedValue(new Error('nope'));

    renderView();

    fireEvent.change(screen.getByTestId('sqs-send-body'), { target: { value: 'will fail' } });
    fireEvent.click(screen.getByTestId('sqs-send-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-send-error')).toBeInTheDocument());
  });

  it('loads and displays the queue attributes', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-attributes')).toBeInTheDocument());
    expect(getSqsQueueAttributesMock).toHaveBeenCalledWith('orders', expect.anything());
    expect(screen.getByTestId('sqs-attr-arn')).toHaveTextContent('arn:aws:sqs:eu-west-1:000000000000:orders');
    expect(screen.getByTestId('sqs-attr-fifo')).toHaveTextContent('No');
    expect(screen.getByTestId('sqs-attr-max-size')).toHaveTextContent('262144');
    expect(screen.getByTestId('sqs-attr-visibility-timeout')).toHaveValue(30);
    expect(screen.getByTestId('sqs-attr-retention')).toHaveValue(345600);
  });

  it('marks the queue as FIFO when the attribute is set', async () => {
    getSqsQueueAttributesMock.mockResolvedValue({ ...attributesResult, fifoQueue: true });

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-attr-fifo')).toHaveTextContent('Yes'));
  });

  it('saves edited attributes and confirms success', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-attributes')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('sqs-attr-visibility-timeout'), { target: { value: '60' } });
    fireEvent.change(screen.getByTestId('sqs-attr-retention'), { target: { value: '86400' } });
    fireEvent.change(screen.getByTestId('sqs-attr-delay'), { target: { value: '10' } });
    fireEvent.change(screen.getByTestId('sqs-attr-wait-time'), { target: { value: '5' } });
    fireEvent.click(screen.getByTestId('sqs-attr-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-attr-status')).toBeInTheDocument());
    expect(updateSqsQueueAttributesMock).toHaveBeenCalledWith('orders', {
      visibilityTimeoutSeconds: 60,
      messageRetentionPeriodSeconds: 86400,
      delaySeconds: 10,
      receiveMessageWaitTimeSeconds: 5,
    });
  });

  it('shows an error when saving attributes fails', async () => {
    updateSqsQueueAttributesMock.mockRejectedValue(new Error('nope'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-attributes')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sqs-attr-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-attr-error')).toBeInTheDocument());
  });

  it('ignores attribute lookup failures', async () => {
    getSqsQueueAttributesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(getSqsQueueAttributesMock).toHaveBeenCalled());
    expect(screen.queryByTestId('sqs-attributes')).not.toBeInTheDocument();
  });

  it('does not render the redrive section when there are no relationships', async () => {
    renderView();

    await waitFor(() => expect(getSqsQueueRedriveMock).toHaveBeenCalledWith('orders', expect.anything()));
    expect(screen.queryByTestId('sqs-redrive')).not.toBeInTheDocument();
  });

  it('renders the dead-letter target without a redrive control', async () => {
    getSqsQueueRedriveMock.mockResolvedValue({
      deadLetterTarget: {
        queueArn: 'arn:aws:sqs:eu-west-1:000000000000:orders-dlq',
        queueName: 'orders-dlq',
        maxReceiveCount: 5,
      },
      sources: [],
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-redrive')).toBeInTheDocument());
    expect(screen.getByTestId('sqs-redrive-target')).toBeInTheDocument();
    await waitFor(() =>
      expect(resolveReferenceMock).toHaveBeenCalledWith(
        'arn:aws:sqs:eu-west-1:000000000000:orders-dlq',
        'sqs',
        expect.anything(),
      ),
    );
    expect(screen.getByTestId('sqs-redrive-max-receive')).toHaveTextContent('after 5 receives');
    expect(screen.queryByTestId('sqs-redrive-source-list')).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Redrive messages' })).not.toBeInTheDocument();
  });

  it('renders source queues and starts a redrive', async () => {
    getSqsQueueRedriveMock.mockResolvedValue({
      deadLetterTarget: null,
      sources: [
        { queueArn: 'arn:aws:sqs:eu-west-1:000000000000:orders', queueName: 'orders' },
      ],
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-redrive-source-list')).toBeInTheDocument());
    expect(screen.getAllByTestId('sqs-redrive-source-item')).toHaveLength(1);

    fireEvent.click(screen.getByRole('button', { name: 'Redrive messages' }));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('sqs-redrive-status')).toBeInTheDocument());
    expect(redriveSqsQueueMock).toHaveBeenCalledWith('orders');
  });

  it('shows an error when the redrive fails', async () => {
    redriveSqsQueueMock.mockRejectedValue(new Error('nope'));
    getSqsQueueRedriveMock.mockResolvedValue({
      deadLetterTarget: null,
      sources: [
        { queueArn: 'arn:aws:sqs:eu-west-1:000000000000:orders', queueName: 'orders' },
      ],
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-redrive-source-list')).toBeInTheDocument());

    fireEvent.click(screen.getByRole('button', { name: 'Redrive messages' }));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('sqs-redrive-error')).toBeInTheDocument());
  });

  it('ignores redrive lookup failures', async () => {
    getSqsQueueRedriveMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(getSqsQueueRedriveMock).toHaveBeenCalled());
    expect(screen.queryByTestId('sqs-redrive')).not.toBeInTheDocument();
  });
});
