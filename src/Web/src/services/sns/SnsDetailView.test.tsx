import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SnsDetailView } from './SnsDetailView';
import { getSnsSubscriptions, resolveReference } from '../../api/client';
import type { SnsSubscriptionListResult } from '../../api/client';

vi.mock('../../api/client');

const getSnsSubscriptionsMock = vi.mocked(getSnsSubscriptions);
const resolveReferenceMock = vi.mocked(resolveReference);

const topicArn = 'arn:aws:sns:eu-west-1:000000000000:orders-topic';

const listResult: SnsSubscriptionListResult = {
  subscriptions: [
    {
      subscriptionArn: 'arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f',
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
    resolveReferenceMock.mockResolvedValue(null as never);
  });

  afterEach(() => {
    vi.resetAllMocks();
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
});
