import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { SqsDetailView } from './SqsDetailView';
import {
  deleteSqsMessage,
  getSqsQueueAttributes,
  getSqsQueueConsumerLambdas,
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
const getSqsQueueConsumerLambdasMock = vi.mocked(getSqsQueueConsumerLambdas);
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
  approximateMessageCount: 7,
  approximateInFlightCount: 3,
  approximateDelayedCount: 2,
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

function goToSendTab() {
  fireEvent.click(screen.getByTestId('sqs-detail-tab-send'));
}

function goToPollTab() {
  fireEvent.click(screen.getByTestId('sqs-detail-tab-poll'));
}

describe('SqsDetailView', () => {
  beforeEach(() => {
    pollSqsMessagesMock.mockResolvedValue(pollResult);
    deleteSqsMessageMock.mockResolvedValue();
    purgeSqsQueueMock.mockResolvedValue();
    getSqsQueueSubscriptionsMock.mockResolvedValue({ subscriptions: [] });
    getSqsQueueConsumerLambdasMock.mockResolvedValue({ lambdas: [] });
    resolveReferenceMock.mockResolvedValue(null as never);
    sendSqsMessageMock.mockResolvedValue();
    getSqsQueueAttributesMock.mockResolvedValue(attributesResult);
    updateSqsQueueAttributesMock.mockResolvedValue();
    getSqsQueueRedriveMock.mockResolvedValue({ deadLetterTarget: null, sources: [] });
    redriveSqsQueueMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders the queue name and peek hint by default', () => {
    renderView();

    expect(screen.getByTestId('sqs-detail-title')).toHaveTextContent('orders');
    goToPollTab();
    expect(screen.getByTestId('sqs-poll-hint')).toHaveTextContent('Peek keeps messages visible');
  });

  it('switches the hint when consume mode is selected', () => {
    renderView();
    goToPollTab();

    fireEvent.change(screen.getByTestId('sqs-poll-mode'), { target: { value: 'consume' } });

    expect(screen.getByTestId('sqs-poll-hint')).toHaveTextContent('Consume hides messages');
  });

  it('switches between the overview, send and poll tabs', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-attributes')).toBeInTheDocument());
    expect(screen.queryByTestId('sqs-send-form')).not.toBeInTheDocument();
    expect(screen.queryByTestId('sqs-poll-mode')).not.toBeInTheDocument();

    goToSendTab();
    expect(screen.getByTestId('sqs-send-form')).toBeInTheDocument();
    expect(screen.queryByTestId('sqs-attributes')).not.toBeInTheDocument();

    goToPollTab();
    expect(screen.getByTestId('sqs-poll-mode')).toBeInTheDocument();
    expect(screen.queryByTestId('sqs-send-form')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('sqs-detail-tab-overview'));
    expect(screen.getByTestId('sqs-attributes')).toBeInTheDocument();
    expect(screen.queryByTestId('sqs-poll-mode')).not.toBeInTheDocument();
  });

  it('polls in peek mode and lists the returned messages', async () => {
    renderView();
    goToPollTab();

    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());

    expect(pollSqsMessagesMock).toHaveBeenCalledWith('orders', 'peek', 10);
    expect(screen.getAllByTestId('sqs-message-item')).toHaveLength(2);
    expect(screen.getAllByTestId('sqs-message-id')[0]).toHaveTextContent('id-1');
  });

  it('polls in consume mode when selected', async () => {
    renderView();
    goToPollTab();

    fireEvent.change(screen.getByTestId('sqs-poll-mode'), { target: { value: 'consume' } });
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());

    expect(pollSqsMessagesMock).toHaveBeenCalledWith('orders', 'consume', 10);
  });

  it('polls with the selected maximum message count', async () => {
    renderView();
    goToPollTab();

    fireEvent.change(screen.getByTestId('sqs-poll-max'), { target: { value: '3' } });
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());

    expect(pollSqsMessagesMock).toHaveBeenCalledWith('orders', 'peek', 3);
  });

  it('keeps message detail panels collapsed until a row is expanded', async () => {
    renderView();
    goToPollTab();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());

    expect(screen.queryAllByTestId('sqs-message-detail')).toHaveLength(0);

    fireEvent.click(screen.getAllByTestId('sqs-message-toggle')[0]);

    const detail = screen.getByTestId('sqs-message-detail');
    expect(detail).toBeInTheDocument();
    expect(within(detail).getByText('Body')).toBeInTheDocument();
    expect(screen.getAllByTestId('sqs-message-toggle')[0]).toHaveAttribute('aria-expanded', 'true');
  });

  it('collapses an expanded message row when toggled again', async () => {
    renderView();
    goToPollTab();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());

    const toggle = screen.getAllByTestId('sqs-message-toggle')[0];
    fireEvent.click(toggle);
    expect(screen.getByTestId('sqs-message-detail')).toBeInTheDocument();

    fireEvent.click(toggle);
    expect(screen.queryByTestId('sqs-message-detail')).not.toBeInTheDocument();
    expect(toggle).toHaveAttribute('aria-expanded', 'false');
  });

  it('collapses all rows again after a new poll', async () => {
    renderView();
    goToPollTab();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('sqs-message-toggle')[0]);
    expect(screen.getByTestId('sqs-message-detail')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.queryAllByTestId('sqs-message-detail')).toHaveLength(0));
  });

  it('shows a loading state while polling', async () => {
    let resolvePoll: (value: SqsMessageListResult) => void = () => {};
    pollSqsMessagesMock.mockReturnValue(
      new Promise<SqsMessageListResult>((resolve) => {
        resolvePoll = resolve;
      }),
    );

    renderView();
    goToPollTab();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    expect(screen.getByTestId('sqs-detail-loading')).toBeInTheDocument();

    resolvePoll(pollResult);
    await waitFor(() => expect(screen.getByTestId('sqs-message-list')).toBeInTheDocument());
  });

  it('shows an empty state when no messages are returned', async () => {
    pollSqsMessagesMock.mockResolvedValue({ messages: [] });

    renderView();
    goToPollTab();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-detail-empty')).toBeInTheDocument());
  });

  it('shows an error state when polling fails', async () => {
    pollSqsMessagesMock.mockRejectedValue(new Error('boom'));

    renderView();
    goToPollTab();
    fireEvent.click(screen.getByTestId('sqs-poll-button'));

    await waitFor(() => expect(screen.getByTestId('sqs-detail-error')).toBeInTheDocument());
  });

  it('deletes a message and removes it from the list', async () => {
    renderView();
    goToPollTab();
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
    goToPollTab();
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
    goToPollTab();
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
    goToPollTab();
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
    goToPollTab();
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
    goToPollTab();
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
    await waitFor(() =>
      expect(resolveReferenceMock).toHaveBeenCalledWith(
        'arn:aws:sns:eu-west-1:000000000000:order-events',
        'sns',
        expect.anything(),
      ),
    );
  });

  it('ignores subscription lookup failures', async () => {
    getSqsQueueSubscriptionsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(getSqsQueueSubscriptionsMock).toHaveBeenCalled());
    expect(screen.queryByTestId('sqs-subscriptions')).not.toBeInTheDocument();
  });

  it('does not render the Lambda triggers section when there are none', async () => {
    renderView();

    await waitFor(() => expect(getSqsQueueConsumerLambdasMock).toHaveBeenCalledWith('orders', expect.anything()));
    expect(screen.queryByTestId('sqs-lambda-triggers')).not.toBeInTheDocument();
  });

  it('renders consumer Lambdas as cross-resource links', async () => {
    getSqsQueueConsumerLambdasMock.mockResolvedValue({
      lambdas: [
        {
          functionName: 'order-processor',
          functionArn: 'arn:aws:lambda:eu-west-1:000000000000:function:order-processor',
          state: 'Enabled',
        },
      ],
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('sqs-lambda-triggers')).toBeInTheDocument());
    const items = screen.getAllByTestId('sqs-lambda-trigger-item');
    expect(items).toHaveLength(1);
    expect(within(items[0]).getByTestId('resource-link')).toHaveTextContent('order-processor');
    await waitFor(() =>
      expect(resolveReferenceMock).toHaveBeenCalledWith(
        'arn:aws:lambda:eu-west-1:000000000000:function:order-processor',
        'lambda',
        expect.anything(),
      ),
    );
  });

  it('ignores Lambda trigger lookup failures', async () => {
    getSqsQueueConsumerLambdasMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(getSqsQueueConsumerLambdasMock).toHaveBeenCalled());
    expect(screen.queryByTestId('sqs-lambda-triggers')).not.toBeInTheDocument();
  });

  it('sends a message body for a standard queue', async () => {
    renderView();
    goToSendTab();

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

  it('sends custom message attributes and clears them after a successful send', async () => {
    renderView();
    goToSendTab();

    fireEvent.change(screen.getByTestId('sqs-send-body'), { target: { value: 'order created' } });
    fireEvent.click(screen.getByTestId('sqs-send-attribute-add'));
    fireEvent.click(screen.getByTestId('sqs-send-attribute-add'));

    const keys = screen.getAllByTestId('sqs-send-attribute-key');
    const values = screen.getAllByTestId('sqs-send-attribute-value');
    fireEvent.change(keys[0], { target: { value: 'trace' } });
    fireEvent.change(values[0], { target: { value: 'abc-123' } });
    fireEvent.change(keys[1], { target: { value: 'source' } });
    fireEvent.change(values[1], { target: { value: 'web' } });

    fireEvent.click(screen.getByTestId('sqs-send-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-send-status')).toBeInTheDocument());
    expect(sendSqsMessageMock).toHaveBeenCalledWith('orders', {
      body: 'order created',
      messageAttributes: { trace: 'abc-123', source: 'web' },
      messageGroupId: undefined,
      messageDeduplicationId: undefined,
    });
    expect(screen.queryByTestId('sqs-send-attribute-row')).not.toBeInTheDocument();
  });

  it('omits attribute rows with a blank name and removed rows from the send', async () => {
    renderView();
    goToSendTab();

    fireEvent.change(screen.getByTestId('sqs-send-body'), { target: { value: 'order created' } });
    fireEvent.click(screen.getByTestId('sqs-send-attribute-add'));
    fireEvent.click(screen.getByTestId('sqs-send-attribute-add'));
    fireEvent.click(screen.getByTestId('sqs-send-attribute-add'));

    const keys = screen.getAllByTestId('sqs-send-attribute-key');
    const values = screen.getAllByTestId('sqs-send-attribute-value');
    fireEvent.change(keys[0], { target: { value: 'keep' } });
    fireEvent.change(values[0], { target: { value: 'yes' } });
    fireEvent.change(keys[1], { target: { value: '  ' } });
    fireEvent.change(values[1], { target: { value: 'blank-name' } });
    fireEvent.change(keys[2], { target: { value: 'remove-me' } });
    fireEvent.change(values[2], { target: { value: 'gone' } });

    fireEvent.click(screen.getAllByTestId('sqs-send-attribute-remove')[2]);
    fireEvent.click(screen.getByTestId('sqs-send-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-send-status')).toBeInTheDocument());
    expect(sendSqsMessageMock).toHaveBeenCalledWith('orders', {
      body: 'order created',
      messageAttributes: { keep: 'yes' },
      messageGroupId: undefined,
      messageDeduplicationId: undefined,
    });
  });

  it('sends without a message attributes payload when only blank-name rows exist', async () => {
    renderView();
    goToSendTab();

    fireEvent.change(screen.getByTestId('sqs-send-body'), { target: { value: 'order created' } });
    fireEvent.click(screen.getByTestId('sqs-send-attribute-add'));
    fireEvent.change(screen.getByTestId('sqs-send-attribute-value'), { target: { value: 'orphan' } });

    fireEvent.click(screen.getByTestId('sqs-send-submit'));

    await waitFor(() => expect(screen.getByTestId('sqs-send-status')).toBeInTheDocument());
    expect(sendSqsMessageMock).toHaveBeenCalledWith('orders', {
      body: 'order created',
      messageAttributes: undefined,
      messageGroupId: undefined,
      messageDeduplicationId: undefined,
    });
  });

  it('does not render the FIFO id fields for a standard queue', () => {
    renderView();
    goToSendTab();

    expect(screen.queryByTestId('sqs-send-group-id')).not.toBeInTheDocument();
  });

  it('requires a group id before a FIFO send is enabled', async () => {
    renderFifoView();
    goToSendTab();

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
    goToSendTab();

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
    expect(screen.getByTestId('sqs-count-available')).toHaveTextContent('7');
    expect(screen.getByTestId('sqs-count-inflight')).toHaveTextContent('3');
    expect(screen.getByTestId('sqs-count-delayed')).toHaveTextContent('2');
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
