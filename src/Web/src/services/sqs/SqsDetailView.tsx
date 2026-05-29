import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { RawJsonViewer } from '../../components/RawJsonViewer';
import { ResourceLink } from '../../components/ResourceLink';
import { deleteSqsMessage, getSqsQueueAttributes, getSqsQueueConsumerLambdas, getSqsQueueRedrive, getSqsQueueSubscriptions, pollSqsMessages, purgeSqsQueue, redriveSqsQueue, sendSqsMessage, updateSqsQueueAttributes } from '../../api/client';
import type { SqsConsumerLambdaItem, SqsMessageItem, SqsPollMode, SqsQueueAttributesItem, SqsRedriveResult, SqsSubscriptionItem } from '../../api/client';
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

const tabBarStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  borderBottom: '1px solid #30363d',
  paddingBottom: 8,
};

const tabButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid transparent',
  background: 'transparent',
  color: 'inherit',
  cursor: 'pointer',
};

const activeTabButtonStyle: CSSProperties = {
  ...tabButtonStyle,
  border: '1px solid #30363d',
  background: '#21262d',
};

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

const toolbarButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
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

const attributeRowStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  alignItems: 'flex-end',
  flexWrap: 'wrap',
};

const attributeRemoveButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
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

const messageToggleStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
  flex: 1,
  minWidth: 0,
  padding: 0,
  border: 'none',
  background: 'none',
  color: 'inherit',
  cursor: 'pointer',
  textAlign: 'left',
};

const messageChevronStyle: CSSProperties = {
  fontSize: 10,
  opacity: 0.7,
  flexShrink: 0,
};

const messageDetailStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
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

const messageCountsStyle: CSSProperties = {
  display: 'flex',
  gap: 24,
  flexWrap: 'wrap',
};

const messageCountStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const messageCountValueStyle: CSSProperties = {
  fontSize: 20,
  fontWeight: 600,
  fontVariantNumeric: 'tabular-nums',
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

type TabKey = 'overview' | 'send' | 'poll';

const peekHint = 'Peek keeps messages visible to other consumers (visibility timeout 0).';
const consumeHint = 'Consume hides messages for the default visibility timeout while you inspect them.';

export function SqsDetailView({ resourceId }: ServiceDetailViewProps) {
  const queueName = resourceId;
  const isFifo = queueName.endsWith('.fifo');
  const [tab, setTab] = useState<TabKey>('overview');
  const [mode, setMode] = useState<SqsPollMode>('peek');
  const [maxMessages, setMaxMessages] = useState(10);
  const [state, setState] = useState<PollState>({ kind: 'idle' });
  const [expandedMessages, setExpandedMessages] = useState<Set<string>>(new Set());
  const [subscriptions, setSubscriptions] = useState<SqsSubscriptionItem[]>([]);
  const [lambdaTriggers, setLambdaTriggers] = useState<SqsConsumerLambdaItem[]>([]);
  const [sendBody, setSendBody] = useState('');
  const [messageGroupId, setMessageGroupId] = useState('');
  const [messageDeduplicationId, setMessageDeduplicationId] = useState('');
  const [sendAttributes, setSendAttributes] = useState<{ key: string; value: string }[]>([]);
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
    getSqsQueueConsumerLambdas(queueName, controller.signal)
      .then((result) => setLambdaTriggers(result.lambdas))
      .catch(() => {
        /* Lambda triggers are best-effort metadata; ignore failures. */
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
    setExpandedMessages(new Set());
    pollSqsMessages(queueName, mode, maxMessages)
      .then((result) => setState({ kind: 'ready', messages: result.messages, mode }))
      .catch(() => setState({ kind: 'error' }));
  }, [queueName, mode, maxMessages]);

  const toggleMessage = useCallback((receiptHandle: string) => {
    setExpandedMessages((current) => {
      const next = new Set(current);
      if (next.has(receiptHandle)) {
        next.delete(receiptHandle);
      } else {
        next.add(receiptHandle);
      }
      return next;
    });
  }, []);

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

  const addSendAttribute = useCallback(() => {
    setSendAttributes((current) => [...current, { key: '', value: '' }]);
  }, []);

  const updateSendAttribute = useCallback(
    (index: number, field: 'key' | 'value', value: string) => {
      setSendAttributes((current) =>
        current.map((attribute, position) =>
          position === index ? { ...attribute, [field]: value } : attribute));
    },
    [],
  );

  const removeSendAttribute = useCallback((index: number) => {
    setSendAttributes((current) => current.filter((_, position) => position !== index));
  }, []);

  const handleSend = useCallback(() => {
    setSendState({ kind: 'sending' });
    const messageAttributes = sendAttributes.reduce<Record<string, string>>((accumulator, attribute) => {
      const key = attribute.key.trim();
      if (key !== '') {
        accumulator[key] = attribute.value;
      }
      return accumulator;
    }, {});
    sendSqsMessage(queueName, {
      body: sendBody,
      messageAttributes: Object.keys(messageAttributes).length > 0 ? messageAttributes : undefined,
      messageGroupId: isFifo && messageGroupId !== '' ? messageGroupId : undefined,
      messageDeduplicationId:
        isFifo && messageDeduplicationId !== '' ? messageDeduplicationId : undefined,
    })
      .then(() => {
        setSendState({ kind: 'sent' });
        setSendBody('');
        setMessageGroupId('');
        setMessageDeduplicationId('');
        setSendAttributes([]);
      })
      .catch(() => setSendState({ kind: 'error' }));
  }, [queueName, sendBody, sendAttributes, isFifo, messageGroupId, messageDeduplicationId]);

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

      <div style={tabBarStyle}>
        <button
          type="button"
          data-testid="sqs-detail-tab-overview"
          style={tab === 'overview' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('overview')}
        >
          Overview
        </button>
        <button
          type="button"
          data-testid="sqs-detail-tab-send"
          style={tab === 'send' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('send')}
        >
          Send
        </button>
        <button
          type="button"
          data-testid="sqs-detail-tab-poll"
          style={tab === 'poll' ? activeTabButtonStyle : tabButtonStyle}
          onClick={() => setTab('poll')}
        >
          Poll messages
        </button>
      </div>

      {tab === 'poll' ? (
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
          <div style={fieldRowStyle}>
            <label htmlFor="sqs-poll-max" style={labelStyle}>
              Max messages
            </label>
            <select
              id="sqs-poll-max"
              data-testid="sqs-poll-max"
              style={selectStyle}
              value={maxMessages}
              onChange={(event) => setMaxMessages(Number(event.target.value))}
            >
              {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((count) => (
                <option key={count} value={count}>
                  {count}
                </option>
              ))}
            </select>
          </div>
          <button type="button" data-testid="sqs-poll-button" style={toolbarButtonStyle} onClick={poll}>
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
      ) : null}

      {tab === 'send' ? (
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
        <label style={labelStyle}>Message attributes</label>
        {sendAttributes.length > 0 ? (
          <div data-testid="sqs-send-attributes" style={fieldRowStyle}>
            {sendAttributes.map((attribute, index) => (
              <div key={index} data-testid="sqs-send-attribute-row" style={attributeRowStyle}>
                <div style={fieldRowStyle}>
                  <label htmlFor={`sqs-send-attribute-key-${index}`} style={labelStyle}>
                    Name
                  </label>
                  <input
                    id={`sqs-send-attribute-key-${index}`}
                    data-testid="sqs-send-attribute-key"
                    style={inputStyle}
                    value={attribute.key}
                    onChange={(event) => updateSendAttribute(index, 'key', event.target.value)}
                  />
                </div>
                <div style={fieldRowStyle}>
                  <label htmlFor={`sqs-send-attribute-value-${index}`} style={labelStyle}>
                    Value
                  </label>
                  <input
                    id={`sqs-send-attribute-value-${index}`}
                    data-testid="sqs-send-attribute-value"
                    style={inputStyle}
                    value={attribute.value}
                    onChange={(event) => updateSendAttribute(index, 'value', event.target.value)}
                  />
                </div>
                <button
                  type="button"
                  data-testid="sqs-send-attribute-remove"
                  style={attributeRemoveButtonStyle}
                  onClick={() => removeSendAttribute(index)}
                >
                  Remove
                </button>
              </div>
            ))}
          </div>
        ) : null}
        <button
          type="button"
          data-testid="sqs-send-attribute-add"
          style={buttonStyle}
          onClick={addSendAttribute}
        >
          Add attribute
        </button>
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
      ) : null}

      {tab === 'overview' && subscriptions.length > 0 ? (
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

      {tab === 'overview' && lambdaTriggers.length > 0 ? (
        <section data-testid="sqs-lambda-triggers" style={subscriptionsStyle}>
          <h3 style={headingStyle}>Lambda triggers</h3>
          <ul data-testid="sqs-lambda-trigger-list" style={subscriptionListStyle}>
            {lambdaTriggers.map((lambda) => (
              <li key={lambda.functionName} data-testid="sqs-lambda-trigger-item">
                <ResourceLink reference={lambda.functionArn} service="lambda" label={lambda.functionName} />
              </li>
            ))}
          </ul>
        </section>
      ) : null}

      {tab === 'overview' && (redrive.deadLetterTarget !== null || redrive.sources.length > 0) ? (
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

      {tab === 'overview' && attributes !== null ? (
        <section data-testid="sqs-attributes" style={attributesSectionStyle}>
          <h3 style={headingStyle}>Queue attributes</h3>
          <div data-testid="sqs-message-counts" style={messageCountsStyle}>
            <div style={messageCountStyle}>
              <span style={labelStyle}>Available</span>
              <span data-testid="sqs-count-available" style={messageCountValueStyle}>
                {attributes.approximateMessageCount}
              </span>
            </div>
            <div style={messageCountStyle}>
              <span style={labelStyle}>In flight</span>
              <span data-testid="sqs-count-inflight" style={messageCountValueStyle}>
                {attributes.approximateInFlightCount}
              </span>
            </div>
            <div style={messageCountStyle}>
              <span style={labelStyle}>Delayed</span>
              <span data-testid="sqs-count-delayed" style={messageCountValueStyle}>
                {attributes.approximateDelayedCount}
              </span>
            </div>
          </div>
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

      {tab === 'poll' && state.kind === 'loading' ? (
        <p data-testid="sqs-detail-loading" style={messageStyle}>
          Polling queue&hellip;
        </p>
      ) : null}

      {tab === 'poll' && state.kind === 'error' ? (
        <p data-testid="sqs-detail-error" style={messageStyle}>
          Unable to poll this queue.
        </p>
      ) : null}

      {tab === 'poll' && state.kind === 'ready' && state.messages.length === 0 ? (
        <p data-testid="sqs-detail-empty" style={messageStyle}>
          No messages were returned. The queue may be empty.
        </p>
      ) : null}

      {tab === 'poll' && state.kind === 'ready' && state.messages.length > 0 ? (
        <div data-testid="sqs-message-list" style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {state.messages.map((message) => {
            const expanded = expandedMessages.has(message.receiptHandle);
            return (
              <div key={message.receiptHandle} data-testid="sqs-message-item" style={messageCardStyle}>
                <div style={messageHeaderStyle}>
                  <button
                    type="button"
                    data-testid="sqs-message-toggle"
                    style={messageToggleStyle}
                    aria-expanded={expanded}
                    onClick={() => toggleMessage(message.receiptHandle)}
                  >
                    <span aria-hidden="true" style={messageChevronStyle}>
                      {expanded ? '▾' : '▸'}
                    </span>
                    <span data-testid="sqs-message-id" style={messageIdStyle}>
                      {message.messageId}
                    </span>
                  </button>
                  <ConfirmationHost
                    actionLabel="Delete"
                    prompt={`Delete message ${message.messageId}?`}
                    confirmLabel="Delete"
                    onConfirm={() => handleDelete(message.receiptHandle)}
                  />
                </div>
                {expanded ? (
                  <div data-testid="sqs-message-detail" style={messageDetailStyle}>
                    <RawJsonViewer value={message.body} title="Body" />
                    <RawJsonViewer value={message.attributes} title="System attributes" />
                    <RawJsonViewer value={message.messageAttributes} title="Message attributes" />
                  </div>
                ) : null}
              </div>
            );
          })}
        </div>
      ) : null}
    </div>
  );
}

export default SqsDetailView;
