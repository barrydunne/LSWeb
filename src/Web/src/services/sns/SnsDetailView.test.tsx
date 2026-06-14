import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SnsDetailView } from './SnsDetailView';
import {
  getSnsSubscriptions,
  getSnsSubscriptionFilterPolicy,
  publishSnsMessage,
  resolveReference,
  setSnsSubscriptionFilterPolicy,
  subscribeSnsTopic,
  unsubscribeSnsTopic,
} from '../../api/client';
import type { SnsSubscriptionListResult } from '../../api/client';

vi.mock('../../api/client');

const getSnsSubscriptionsMock = vi.mocked(getSnsSubscriptions);
const getSnsSubscriptionFilterPolicyMock = vi.mocked(getSnsSubscriptionFilterPolicy);
const publishSnsMessageMock = vi.mocked(publishSnsMessage);
const resolveReferenceMock = vi.mocked(resolveReference);
const setSnsSubscriptionFilterPolicyMock = vi.mocked(setSnsSubscriptionFilterPolicy);
const subscribeSnsTopicMock = vi.mocked(subscribeSnsTopic);
const unsubscribeSnsTopicMock = vi.mocked(unsubscribeSnsTopic);

const topicArn = 'arn:aws:sns:eu-west-1:000000000000:orders-topic';
const subscriptionArn = 'arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f';

const listResult: SnsSubscriptionListResult = {
  subscriptions: [
    {
      subscriptionArn,
      protocol: 'sqs',
      endpoint: 'arn:aws:sqs:eu-west-1:000000000000:orders',
      owner: '000000000000',
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <SnsDetailView serviceKey="sns" resourceId={topicArn} />
    </MemoryRouter>,
  );
}

describe('SnsDetailView', () => {
  beforeEach(() => {
    getSnsSubscriptionsMock.mockResolvedValue(listResult);
    getSnsSubscriptionFilterPolicyMock.mockResolvedValue({ filterPolicy: '' });
    publishSnsMessageMock.mockResolvedValue();
    resolveReferenceMock.mockResolvedValue(null as never);
    setSnsSubscriptionFilterPolicyMock.mockResolvedValue();
    subscribeSnsTopicMock.mockResolvedValue();
    unsubscribeSnsTopicMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before subscriptions arrive', () => {
    getSnsSubscriptionsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('sns-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSnsSubscriptionsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-error')).toBeInTheDocument());
  });

  it('renders the topic name and arn derived from the resource id', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('sns-detail-name')).toHaveTextContent('orders-topic');
    expect(screen.getByTestId('sns-detail-arn')).toHaveTextContent(topicArn);
    // ResourceLink resolves asynchronously; wait so the effect settles before teardown.
    await waitFor(() => expect(resolveReferenceMock).toHaveBeenCalled());
  });

  it('lists each subscription with its protocol and a cross-resource link', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-subscriptions')).toBeInTheDocument());

    expect(getSnsSubscriptionsMock).toHaveBeenCalledWith(topicArn, expect.anything());
    const items = screen.getAllByTestId('sns-subscription-item');
    expect(items).toHaveLength(1);
    expect(screen.getByTestId('sns-subscription-protocol')).toHaveTextContent('sqs');
    expect(screen.getByTestId('resource-link')).toHaveTextContent(
      'arn:aws:sqs:eu-west-1:000000000000:orders',
    );
    await waitFor(() =>
      expect(resolveReferenceMock).toHaveBeenCalledWith(
        'arn:aws:sqs:eu-west-1:000000000000:orders',
        'sqs',
        expect.anything(),
      ),
    );
  });

  it('shows an empty state when the topic has no subscriptions', async () => {
    getSnsSubscriptionsMock.mockResolvedValue({ subscriptions: [] });

    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-empty')).toBeInTheDocument());
    expect(screen.queryByTestId('sns-subscription-list')).not.toBeInTheDocument();
  });

  it('falls back to the raw resource id for the heading when no name segment is present', async () => {
    getSnsSubscriptionsMock.mockResolvedValue({ subscriptions: [] });

    render(
      <MemoryRouter>
        <SnsDetailView serviceKey="sns" resourceId="arn:aws:sns:eu-west-1:000000000000:" />
      </MemoryRouter>,
    );

    await waitFor(() => expect(screen.getByTestId('sns-detail-view')).toBeInTheDocument());
    expect(screen.getByTestId('sns-detail-name')).toHaveTextContent(
      'arn:aws:sns:eu-west-1:000000000000:',
    );
  });

  it('publishes a message with the subject, body, and attributes then clears the form', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('sns-detail-publish-subject'), {
      target: { value: 'Heads up' },
    });
    fireEvent.change(screen.getByTestId('sns-detail-publish-message'), {
      target: { value: 'hello world' },
    });
    fireEvent.click(screen.getByTestId('sns-detail-publish-attribute-add'));
    fireEvent.change(screen.getByTestId('sns-detail-publish-attribute-key'), {
      target: { value: 'source' },
    });
    fireEvent.change(screen.getByTestId('sns-detail-publish-attribute-value'), {
      target: { value: 'web' },
    });
    fireEvent.click(screen.getByTestId('sns-detail-publish-submit'));

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish-status')).toBeInTheDocument());
    expect(publishSnsMessageMock).toHaveBeenCalledWith(topicArn, {
      subject: 'Heads up',
      message: 'hello world',
      messageAttributes: { source: 'web' },
    });
    expect(screen.getByTestId('sns-detail-publish-subject')).toHaveValue('');
    expect(screen.getByTestId('sns-detail-publish-message')).toHaveValue('');
    expect(screen.queryByTestId('sns-detail-publish-attributes')).not.toBeInTheDocument();
  });

  it('publishes without a subject or attributes when none are supplied', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('sns-detail-publish-message'), {
      target: { value: 'hello world' },
    });
    fireEvent.click(screen.getByTestId('sns-detail-publish-submit'));

    await waitFor(() => expect(publishSnsMessageMock).toHaveBeenCalled());
    expect(publishSnsMessageMock).toHaveBeenCalledWith(topicArn, {
      subject: undefined,
      message: 'hello world',
      messageAttributes: undefined,
    });
  });

  it('ignores attribute rows whose name is blank', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('sns-detail-publish-message'), {
      target: { value: 'hello world' },
    });
    fireEvent.click(screen.getByTestId('sns-detail-publish-attribute-add'));
    fireEvent.change(screen.getByTestId('sns-detail-publish-attribute-value'), {
      target: { value: 'orphan' },
    });
    fireEvent.click(screen.getByTestId('sns-detail-publish-submit'));

    await waitFor(() => expect(publishSnsMessageMock).toHaveBeenCalled());
    expect(publishSnsMessageMock).toHaveBeenCalledWith(topicArn, {
      subject: undefined,
      message: 'hello world',
      messageAttributes: undefined,
    });
  });

  it('updates only the targeted attribute row when several are present', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('sns-detail-publish-message'), {
      target: { value: 'hello world' },
    });
    fireEvent.click(screen.getByTestId('sns-detail-publish-attribute-add'));
    fireEvent.click(screen.getByTestId('sns-detail-publish-attribute-add'));

    const keyInputs = screen.getAllByTestId('sns-detail-publish-attribute-key');
    fireEvent.change(keyInputs[0], { target: { value: 'first' } });
    fireEvent.change(keyInputs[1], { target: { value: 'second' } });
    const valueInputs = screen.getAllByTestId('sns-detail-publish-attribute-value');
    fireEvent.change(valueInputs[0], { target: { value: 'one' } });
    fireEvent.change(valueInputs[1], { target: { value: 'two' } });
    fireEvent.click(screen.getByTestId('sns-detail-publish-submit'));

    await waitFor(() => expect(publishSnsMessageMock).toHaveBeenCalled());
    expect(publishSnsMessageMock).toHaveBeenCalledWith(topicArn, {
      subject: undefined,
      message: 'hello world',
      messageAttributes: { first: 'one', second: 'two' },
    });
  });

  it('removes an attribute row when the remove button is clicked', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sns-detail-publish-attribute-add'));
    expect(screen.getByTestId('sns-detail-publish-attribute-row')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('sns-detail-publish-attribute-remove'));
    expect(screen.queryByTestId('sns-detail-publish-attribute-row')).not.toBeInTheDocument();
  });

  it('disables the publish button until a message is entered', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish')).toBeInTheDocument());

    expect(screen.getByTestId('sns-detail-publish-submit')).toBeDisabled();

    fireEvent.change(screen.getByTestId('sns-detail-publish-message'), {
      target: { value: 'hello' },
    });
    expect(screen.getByTestId('sns-detail-publish-submit')).toBeEnabled();
  });

  it('shows an error when publishing fails', async () => {
    publishSnsMessageMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('sns-detail-publish-message'), {
      target: { value: 'hello world' },
    });
    fireEvent.click(screen.getByTestId('sns-detail-publish-submit'));

    await waitFor(() => expect(screen.getByTestId('sns-detail-publish-error')).toBeInTheDocument());
  });

  describe('subscriptions', () => {
    it('subscribes an endpoint to the topic', async () => {
      renderView();
      await waitFor(() => expect(screen.getByTestId('sns-subscribe-form')).toBeInTheDocument());

      fireEvent.change(screen.getByTestId('sns-subscribe-protocol'), { target: { value: 'lambda' } });
      fireEvent.change(screen.getByTestId('sns-subscribe-endpoint'), {
        target: { value: 'arn:aws:lambda:eu-west-1:000000000000:function:p' },
      });
      fireEvent.click(screen.getByTestId('sns-subscribe-submit'));

      await waitFor(() =>
        expect(subscribeSnsTopicMock).toHaveBeenCalledWith(
          topicArn,
          'lambda',
          'arn:aws:lambda:eu-west-1:000000000000:function:p',
        ),
      );
    });

    it('blocks subscribing when the endpoint is empty', async () => {
      renderView();
      await waitFor(() => expect(screen.getByTestId('sns-subscribe-form')).toBeInTheDocument());

      fireEvent.click(screen.getByTestId('sns-subscribe-submit'));

      expect(screen.getByTestId('sns-subscribe-error')).toBeInTheDocument();
      expect(subscribeSnsTopicMock).not.toHaveBeenCalled();
    });

    it('shows an error when subscribing fails', async () => {
      subscribeSnsTopicMock.mockRejectedValue(new Error('boom'));
      renderView();
      await waitFor(() => expect(screen.getByTestId('sns-subscribe-form')).toBeInTheDocument());

      fireEvent.change(screen.getByTestId('sns-subscribe-endpoint'), {
        target: { value: 'arn:aws:sqs:eu-west-1:000000000000:q' },
      });
      fireEvent.click(screen.getByTestId('sns-subscribe-submit'));

      await waitFor(() => expect(screen.getByTestId('sns-subscribe-error')).toBeInTheDocument());
    });

    it('unsubscribes a confirmed subscription', async () => {
      renderView();
      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());

      fireEvent.click(screen.getByTestId('sns-subscription-unsubscribe'));

      await waitFor(() =>
        expect(unsubscribeSnsTopicMock).toHaveBeenCalledWith(subscriptionArn),
      );
    });

    it('shows an error when unsubscribing fails', async () => {
      unsubscribeSnsTopicMock.mockRejectedValue(new Error('boom'));
      renderView();
      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());

      fireEvent.click(screen.getByTestId('sns-subscription-unsubscribe'));

      await waitFor(() => expect(screen.getByTestId('sns-unsubscribe-error')).toBeInTheDocument());
    });

    it('marks an unconfirmed subscription as pending and offers no unsubscribe button', async () => {
      getSnsSubscriptionsMock.mockResolvedValue({
        subscriptions: [
          {
            subscriptionArn: 'PendingConfirmation',
            protocol: 'email',
            endpoint: 'ops@example.com',
            owner: '000000000000',
          },
        ],
      });

      renderView();
      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());

      expect(screen.getByTestId('sns-subscription-pending')).toBeInTheDocument();
      expect(screen.queryByTestId('sns-subscription-unsubscribe')).not.toBeInTheDocument();
    });
  });

  describe('filter policy', () => {
    it('shows an edit toggle for each subscription without loading the policy up front', async () => {
      getSnsSubscriptionsMock.mockResolvedValue({
        subscriptions: [
          listResult.subscriptions[0],
          {
            subscriptionArn: 'arn:aws:sns:eu-west-1:000000000000:orders-topic:9d2a',
            protocol: 'email',
            endpoint: 'ops@example.com',
            owner: '000000000000',
          },
        ],
      });

      renderView();

      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());
      expect(screen.getAllByTestId('sns-filter-policy-toggle')).toHaveLength(2);
      expect(getSnsSubscriptionFilterPolicyMock).not.toHaveBeenCalled();
    });

    it('loads and renders a JSON filter policy when the toggle is clicked', async () => {
      getSnsSubscriptionFilterPolicyMock.mockResolvedValue({
        filterPolicy: '{"store":["example_corp"]}',
      });

      renderView();

      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());
      fireEvent.click(screen.getByTestId('sns-filter-policy-toggle'));

      await waitFor(() => expect(screen.getByTestId('sns-filter-policy')).toBeInTheDocument());
      expect(getSnsSubscriptionFilterPolicyMock).toHaveBeenCalledWith(subscriptionArn);
      expect(screen.getByTestId('raw-json-viewer')).toBeInTheDocument();
      expect(screen.getByTestId('sns-filter-policy-input')).toHaveValue('{"store":["example_corp"]}');
    });

    it('shows an empty hint when no filter policy is set', async () => {
      getSnsSubscriptionFilterPolicyMock.mockResolvedValue({ filterPolicy: '' });

      renderView();

      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());
      fireEvent.click(screen.getByTestId('sns-filter-policy-toggle'));

      await waitFor(() => expect(screen.getByTestId('sns-filter-policy-empty')).toBeInTheDocument());
      expect(screen.getByTestId('sns-filter-policy-empty')).toHaveTextContent('No filter policy set.');
      expect(screen.queryByTestId('raw-json-viewer')).not.toBeInTheDocument();
    });

    it('shows an invalid-json hint when the policy cannot be parsed', async () => {
      getSnsSubscriptionFilterPolicyMock.mockResolvedValue({ filterPolicy: 'not json' });

      renderView();

      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());
      fireEvent.click(screen.getByTestId('sns-filter-policy-toggle'));

      await waitFor(() => expect(screen.getByTestId('sns-filter-policy-empty')).toBeInTheDocument());
      expect(screen.getByTestId('sns-filter-policy-empty')).toHaveTextContent(
        'Filter policy is not valid JSON.',
      );
    });

    it('shows a loading state while the policy is fetched', async () => {
      getSnsSubscriptionFilterPolicyMock.mockReturnValue(new Promise(() => {}));

      renderView();

      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());
      fireEvent.click(screen.getByTestId('sns-filter-policy-toggle'));

      expect(screen.getByTestId('sns-filter-policy-loading')).toBeInTheDocument();
    });

    it('shows a load error and retries when the fetch fails', async () => {
      getSnsSubscriptionFilterPolicyMock.mockRejectedValueOnce(new Error('boom'));

      renderView();

      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());
      fireEvent.click(screen.getByTestId('sns-filter-policy-toggle'));

      await waitFor(() =>
        expect(screen.getByTestId('sns-filter-policy-load-error')).toBeInTheDocument());

      getSnsSubscriptionFilterPolicyMock.mockResolvedValue({ filterPolicy: '{}' });
      fireEvent.click(screen.getByTestId('sns-filter-policy-retry'));

      await waitFor(() => expect(screen.getByTestId('sns-filter-policy')).toBeInTheDocument());
    });

    it('saves an edited filter policy and shows a confirmation', async () => {
      getSnsSubscriptionFilterPolicyMock.mockResolvedValue({ filterPolicy: '' });

      renderView();

      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());
      fireEvent.click(screen.getByTestId('sns-filter-policy-toggle'));

      await waitFor(() => expect(screen.getByTestId('sns-filter-policy-input')).toBeInTheDocument());
      fireEvent.change(screen.getByTestId('sns-filter-policy-input'), {
        target: { value: '{"store":["example_corp"]}' },
      });
      fireEvent.click(screen.getByTestId('sns-filter-policy-save'));

      await waitFor(() => expect(screen.getByTestId('sns-filter-policy-saved')).toBeInTheDocument());
      expect(setSnsSubscriptionFilterPolicyMock).toHaveBeenCalledWith(
        subscriptionArn,
        '{"store":["example_corp"]}',
      );
    });

    it('shows a save error when persisting the policy fails', async () => {
      getSnsSubscriptionFilterPolicyMock.mockResolvedValue({ filterPolicy: '' });
      setSnsSubscriptionFilterPolicyMock.mockRejectedValue(new Error('boom'));

      renderView();

      await waitFor(() => expect(screen.getByTestId('sns-subscription-list')).toBeInTheDocument());
      fireEvent.click(screen.getByTestId('sns-filter-policy-toggle'));

      await waitFor(() => expect(screen.getByTestId('sns-filter-policy-save')).toBeInTheDocument());
      fireEvent.click(screen.getByTestId('sns-filter-policy-save'));

      await waitFor(() =>
        expect(screen.getByTestId('sns-filter-policy-save-error')).toBeInTheDocument());
    });
  });
});
