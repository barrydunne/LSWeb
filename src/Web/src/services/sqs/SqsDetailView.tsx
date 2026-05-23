import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { RawJsonViewer } from '../../components/RawJsonViewer';
import { ResourceLink } from '../../components/ResourceLink';
import { deleteSqsMessage, getSqsQueueAttributes, getSqsQueueRedrive, getSqsQueueSubscriptions, pollSqsMessages, purgeSqsQueue, redriveSqsQueue, sendSqsMessage, updateSqsQueueAttributes } from '../../api/client';
import type { SqsMessageItem, SqsPollMode, SqsQueueAttributesItem, SqsRedriveResult, SqsSubscriptionItem } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const messageStyle: CSSProperties = { fontSize: 14 };

const headingStyle: CSSProperties = { fontSize: 16, fontWeight: 600 };

const toolbarStyle: CSSProperties = {
  display: 'flex',
  gap: 12,
  alignItems: 'flex-end',
  flexWrap: 'wrap',
};

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const selectStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const hintStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const sendSectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const textareaStyle: CSSProperties = {
  fontSize: 13,
  fontFamily: 'monospace',
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  minHeight: 72,
  resize: 'vertical',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const sendRowStyle: CSSProperties = {
  display: 'flex',
  gap: 12,
  flexWrap: 'wrap',
};

const messageCardStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const messageHeaderStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  fontSize: 13,
};

const messageIdStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 12,
  wordBreak: 'break-all',
};

const subscriptionsStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const subscriptionListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  margin: 0,
  paddingLeft: 18,
};

const attributesSectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const readOnlyRowStyle: CSSProperties = {
  display: 'flex',
  gap: 6,
  fontSize: 13,
};

const readOnlyValueStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 12,
  wordBreak: 'break-all',
};

type PollState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'ready'; messages: SqsMessageItem[]; mode: SqsPollMode }
  | { kind: 'error' };

type SendState =
  | { kind: 'idle' }
  | { kind: 'sending' }
  | { kind: 'sent' }
  | { kind: 'error' };

type AttributesSaveState =
  | { kind: 'idle' }
  | { kind: 'saving' }
  | { kind: 'saved' }
  | { kind: 'error' };

type RedriveState =
  | { kind: 'idle' }
  | { kind: 'starting' }
  | { kind: 'started' }
  | { kind: 'error' };

const peekHint = 'Peek keeps messages visible to other consumers (visibility timeout 0).';
const consumeHint = 'Consume hides messages for the default visibility timeout while you inspect them.';

export function SqsDetailView({ resourceId }: ServiceDetailViewProps) {
  const queueName = resourceId;
  const isFifo = queueName.endsWith('.fifo');
  const [mode, setMode] = useState<SqsPollMode>('peek');
  const [state, setState] = useState<PollState>({ kind: 'idle' });
  const [subscriptions, setSubscriptions] = useState<SqsSubscriptionItem[]>([]);
  const [sendBody, setSendBody] = useState('');
  const [messageGroupId, setMessageGroupId] = useState('');
  const [messageDeduplicationId, setMessageDeduplicationId] = useState('');
  const [sendState, setSendState] = useState<SendState>({ kind: 'idle' });
  const [attributes, setAttributes] = useState<SqsQueueAttributesItem | null>(null);
  const [visibilityTimeout, setVisibilityTimeout] = useState('');
  const [retentionPeriod, setRetentionPeriod] = useState('');
  const [delaySeconds, setDelaySeconds] = useState('');
  const [waitTimeSeconds, setWaitTimeSeconds] = useState('');
  const [attributesSaveState, setAttributesSaveState] = useState<AttributesSaveState>({ kind: 'idle' });
  const [redrive, setRedrive] = useState<SqsRedriveResult>({ deadLetterTarget: null, sources: [] });
  const [redriveState, setRedriveState] = useState<RedriveState>({ kind: 'idle' });

  useEffect(() => {
    const controller = new AbortController();
    getSqsQueueSubscriptions(queueName, controller.signal)
      .then((result) => setSubscriptions(result.subscriptions))
      .catch(() => {
        /* Subscriptions are best-effort metadata; ignore failures. */
      });
    return () => controller.abort();
  }, [queueName]);

  useEffect(() => {
    const controller = new AbortController();
    getSqsQueueAttributes(queueName, controller.signal)
      .then((result) => {
        setAttributes(result);
        setVisibilityTimeout(String(result.visibilityTimeoutSeconds));
        setRetentionPeriod(String(result.messageRetentionPeriodSeconds));
        setDelaySeconds(String(result.delaySeconds));
        setWaitTimeSeconds(String(result.receiveMessageWaitTimeSeconds));
      })
      .catch(() => {
        /* Attributes are best-effort metadata; ignore failures. */
      });
    return () => controller.abort();
  }, [queueName]);

  useEffect(() => {
    const controller = new AbortController();
    getSqsQueueRedrive(queueName, controller.signal)
      .then((result) => setRedrive(result))
      .catch(() => {
        /* Redrive relationships are best-effort metadata; ignore failures. */
      });
    return () => controller.abort();
  }, [queueName]);

  const poll = useCallback(() => {
    setState({ kind: 'loading' });
    pollSqsMessages(queueName, mode, 10)
      .then((result) => setState({ kind: 'ready', messages: result.messages, mode }))
      .catch(() => setState({ kind: 'error' }));
  }, [queueName, mode]);

  const handleDelete = useCallback(
    (receiptHandle: string) => {
      deleteSqsMessage(queueName, receiptHandle)
        .then(() =>
          setState((current) =>
            current.kind === 'ready'
              ? {
                  ...current,
                  messages: current.messages.filter((message) => message.receiptHandle !== receiptHandle),
                }
              : current,
          ),
        )
        .catch(() => setState({ kind: 'error' }));
    },
    [queueName],
  );

  const handlePurge = useCallback(() => {
    purgeSqsQueue(queueName)
      .then(() =>
        setState((current) =>
          current.kind === 'ready' ? { ...current, messages: [] } : current,
        ),
      )
      .catch(() => setState({ kind: 'error' }));
  }, [queueName]);

  const handleSend = useCallback(() => {
    setSendState({ kind: 'sending' });
    sendSqsMessage(queueName, {
      body: sendBody,
      messageGroupId: isFifo && messageGroupId !== '' ? messageGroupId : undefined,
      messageDeduplicationId:
        isFifo && messageDeduplicationId !== '' ? messageDeduplicationId : undefined,
    })
      .then(() => {
        setSendState({ kind: 'sent' });
        setSendBody('');
        setMessageGroupId('');
        setMessageDeduplicationId('');
      })
      .catch(() => setSendState({ kind: 'error' }));
  }, [queueName, sendBody, isFifo, messageGroupId, messageDeduplicationId]);

  const handleSaveAttributes = useCallback(() => {
    setAttributesSaveState({ kind: 'saving' });
    updateSqsQueueAttributes(queueName, {
      visibilityTimeoutSeconds: Number(visibilityTimeout),
      messageRetentionPeriodSeconds: Number(retentionPeriod),
      delaySeconds: Number(delaySeconds),
      receiveMessageWaitTimeSeconds: Number(waitTimeSeconds),
    })
      .then(() => setAttributesSaveState({ kind: 'saved' }))
      .catch(() => setAttributesSaveState({ kind: 'error' }));
  }, [queueName, visibilityTimeout, retentionPeriod, delaySeconds, waitTimeSeconds]);

  const handleRedrive = useCallback(() => {
    setRedriveState({ kind: 'starting' });
    redriveSqsQueue(queueName)
      .then(() => setRedriveState({ kind: 'started' }))
      .catch(() => setRedriveState({ kind: 'error' }));
  }, [queueName]);

  const canSend = sendBody.trim() !== '' && (!isFifo || messageGroupId.trim() !== '') && sendState.kind !== 'sending';

  return (
    <div data-testid="sqs-detail-view" style={containerStyle}>
      <h2 data-testid="sqs-detail-title" style={headingStyle}>
        {queueName}
      </h2>

      <div style={toolbarStyle}>
        <div style={fieldRowStyle}>
          <label htmlFor="sqs-poll-mode" style={labelStyle}>
            Mode
          </label>
          <select
            id="sqs-poll-mode"
            data-testid="sqs-poll-mode"
            style={selectStyle}
            value={mode}
            onChange={(event) => setMode(event.target.value as SqsPollMode)}
          >
            <option value="peek">Peek</option>
            <option value="consume">Consume</option>
          </select>
        </div>
        <button type="button" data-testid="sqs-poll-button" style={buttonStyle} onClick={poll}>
          Poll messages
        </button>
        <ConfirmationHost
          actionLabel="Purge queue"
          prompt={`Purge all messages from ${queueName}? This cannot be undone.`}
          confirmLabel="Purge"
          onConfirm={handlePurge}
        />
        <span data-testid="sqs-poll-hint" style={hintStyle}>
          {mode === 'peek' ? peekHint : consumeHint}
        </span>
      </div>

      <section data-testid="sqs-send-form" style={sendSectionStyle}>
        <h3 style={headingStyle}>Send a message</h3>
        <label htmlFor="sqs-send-body" style={labelStyle}>
          Body
        </label>
        <textarea
          id="sqs-send-body"
          data-testid="sqs-send-body"
          style={textareaStyle}
          value={sendBody}
          onChange={(event) => setSendBody(event.target.value)}
        />
        {isFifo ? (
          <div style={sendRowStyle}>
            <div style={fieldRowStyle}>
              <label htmlFor="sqs-send-group-id" style={labelStyle}>
                Message group id
              </label>
              <input
                id="sqs-send-group-id"
                data-testid="sqs-send-group-id"
                style={inputStyle}
                value={messageGroupId}
                onChange={(event) => setMessageGroupId(event.target.value)}
              />
            </div>
            <div style={fieldRowStyle}>
              <label htmlFor="sqs-send-dedup-id" style={labelStyle}>
                Deduplication id
              </label>
              <input
                id="sqs-send-dedup-id"
                data-testid="sqs-send-dedup-id"
                style={inputStyle}
                value={messageDeduplicationId}
                onChange={(event) => setMessageDeduplicationId(event.target.value)}
              />
            </div>
          </div>
        ) : null}
        <button
          type="button"
          data-testid="sqs-send-submit"
          style={buttonStyle}
          disabled={!canSend}
          onClick={handleSend}
        >
          Send message
        </button>
        {sendState.kind === 'sent' ? (
          <span data-testid="sqs-send-status" style={hintStyle}>
            Message sent.
          </span>
        ) : null}
        {sendState.kind === 'error' ? (
          <span data-testid="sqs-send-error" style={hintStyle}>
            Unable to send the message.
          </span>
        ) : null}
      </section>

      {subscriptions.length > 0 ? (
        <section data-testid="sqs-subscriptions" style={subscriptionsStyle}>
          <h3 style={headingStyle}>SNS subscriptions</h3>
          <ul data-testid="sqs-subscription-list" style={subscriptionListStyle}>
            {subscriptions.map((subscription) => (
              <li key={subscription.topicArn} data-testid="sqs-subscription-item">
                <ResourceLink reference={subscription.topicArn} service="sns" label={subscription.topicName} />
              </li>
            ))}
          </ul>
        </section>
      ) : null}

      {redrive.deadLetterTarget !== null || redrive.sources.length > 0 ? (
        <section data-testid="sqs-redrive" style={subscriptionsStyle}>
          <h3 style={headingStyle}>Dead-letter queue</h3>
          {redrive.deadLetterTarget !== null ? (
            <div data-testid="sqs-redrive-target" style={readOnlyRowStyle}>
              <span style={labelStyle}>Dead-letter target</span>
              <ResourceLink
                reference={redrive.deadLetterTarget.queueArn}
                service="sqs"
                label={redrive.deadLetterTarget.queueName}
              />
              <span data-testid="sqs-redrive-max-receive" style={hintStyle}>
                after {redrive.deadLetterTarget.maxReceiveCount} receives
              </span>
            </div>
          ) : null}
          {redrive.sources.length > 0 ? (
            <>
              <span style={labelStyle}>Source queues</span>
              <ul data-testid="sqs-redrive-source-list" style={subscriptionListStyle}>
                {redrive.sources.map((source) => (
                  <li key={source.queueArn} data-testid="sqs-redrive-source-item">
                    <ResourceLink reference={source.queueArn} service="sqs" label={source.queueName} />
                  </li>
                ))}
              </ul>
              <ConfirmationHost
                actionLabel="Redrive messages"
                prompt={`Redrive messages from ${queueName} back to their source queues?`}
                confirmLabel="Redrive"
                onConfirm={handleRedrive}
              />
              {redriveState.kind === 'started' ? (
                <span data-testid="sqs-redrive-status" style={hintStyle}>
                  Redrive started.
                </span>
              ) : null}
              {redriveState.kind === 'error' ? (
                <span data-testid="sqs-redrive-error" style={hintStyle}>
                  Unable to start the redrive.
                </span>
              ) : null}
            </>
          ) : null}
        </section>
      ) : null}

      {attributes !== null ? (
        <section data-testid="sqs-attributes" style={attributesSectionStyle}>
          <h3 style={headingStyle}>Queue attributes</h3>
          <div style={readOnlyRowStyle}>
            <span style={labelStyle}>ARN</span>
            <span data-testid="sqs-attr-arn" style={readOnlyValueStyle}>
              {attributes.queueArn}
            </span>
          </div>
          <div style={readOnlyRowStyle}>
            <span style={labelStyle}>FIFO</span>
            <span data-testid="sqs-attr-fifo" style={readOnlyValueStyle}>
              {attributes.fifoQueue ? 'Yes' : 'No'}
            </span>
          </div>
          <div style={readOnlyRowStyle}>
            <span style={labelStyle}>Max message size (bytes)</span>
            <span data-testid="sqs-attr-max-size" style={readOnlyValueStyle}>
              {attributes.maximumMessageSizeBytes}
            </span>
          </div>
          <div style={sendRowStyle}>
            <div style={fieldRowStyle}>
              <label htmlFor="sqs-attr-visibility-timeout" style={labelStyle}>
                Visibility timeout (seconds)
              </label>
              <input
                id="sqs-attr-visibility-timeout"
                data-testid="sqs-attr-visibility-timeout"
                type="number"
                style={inputStyle}
                value={visibilityTimeout}
                onChange={(event) => setVisibilityTimeout(event.target.value)}
              />
            </div>
            <div style={fieldRowStyle}>
              <label htmlFor="sqs-attr-retention" style={labelStyle}>
                Retention period (seconds)
              </label>
              <input
                id="sqs-attr-retention"
                data-testid="sqs-attr-retention"
                type="number"
                style={inputStyle}
                value={retentionPeriod}
                onChange={(event) => setRetentionPeriod(event.target.value)}
              />
            </div>
            <div style={fieldRowStyle}>
              <label htmlFor="sqs-attr-delay" style={labelStyle}>
                Delivery delay (seconds)
              </label>
              <input
                id="sqs-attr-delay"
                data-testid="sqs-attr-delay"
                type="number"
                style={inputStyle}
                value={delaySeconds}
                onChange={(event) => setDelaySeconds(event.target.value)}
              />
            </div>
            <div style={fieldRowStyle}>
              <label htmlFor="sqs-attr-wait-time" style={labelStyle}>
                Receive wait time (seconds)
              </label>
              <input
                id="sqs-attr-wait-time"
                data-testid="sqs-attr-wait-time"
                type="number"
                style={inputStyle}
                value={waitTimeSeconds}
                onChange={(event) => setWaitTimeSeconds(event.target.value)}
              />
            </div>
          </div>
          <button
            type="button"
            data-testid="sqs-attr-submit"
            style={buttonStyle}
            disabled={attributesSaveState.kind === 'saving'}
            onClick={handleSaveAttributes}
          >
            Save attributes
          </button>
          {attributesSaveState.kind === 'saved' ? (
            <span data-testid="sqs-attr-status" style={hintStyle}>
              Attributes updated.
            </span>
          ) : null}
          {attributesSaveState.kind === 'error' ? (
            <span data-testid="sqs-attr-error" style={hintStyle}>
              Unable to update attributes.
            </span>
          ) : null}
        </section>
      ) : null}

      {state.kind === 'loading' ? (
        <p data-testid="sqs-detail-loading" style={messageStyle}>
          Polling queue&hellip;
        </p>
      ) : null}

      {state.kind === 'error' ? (
        <p data-testid="sqs-detail-error" style={messageStyle}>
          Unable to poll this queue.
        </p>
      ) : null}

      {state.kind === 'ready' && state.messages.length === 0 ? (
        <p data-testid="sqs-detail-empty" style={messageStyle}>
          No messages were returned. The queue may be empty.
        </p>
      ) : null}

      {state.kind === 'ready' && state.messages.length > 0 ? (
        <div data-testid="sqs-message-list" style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {state.messages.map((message) => (
            <div key={message.receiptHandle} data-testid="sqs-message-item" style={messageCardStyle}>
              <div style={messageHeaderStyle}>
                <span data-testid="sqs-message-id" style={messageIdStyle}>
                  {message.messageId}
                </span>
                <ConfirmationHost
                  actionLabel="Delete"
                  prompt={`Delete message ${message.messageId}?`}
                  confirmLabel="Delete"
                  onConfirm={() => handleDelete(message.receiptHandle)}
                />
              </div>
              <RawJsonViewer value={message.body} title="Body" />
              <RawJsonViewer value={message.attributes} title="System attributes" />
              <RawJsonViewer value={message.messageAttributes} title="Message attributes" />
            </div>
          ))}
        </div>
      ) : null}
    </div>
  );
}

export default SqsDetailView;
