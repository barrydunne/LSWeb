import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SnsListView } from './SnsListView';
import { createSnsTopic, deleteSnsTopic, getSnsTopics } from '../../api/client';
import type { SnsTopicListResult } from '../../api/client';

vi.mock('../../api/client');

const getSnsTopicsMock = vi.mocked(getSnsTopics);
const createSnsTopicMock = vi.mocked(createSnsTopic);
const deleteSnsTopicMock = vi.mocked(deleteSnsTopic);

const listResult: SnsTopicListResult = {
  topics: [
    {
      name: 'orders-topic',
      topicArn: 'arn:aws:sns:eu-west-1:000000000000:orders-topic',
    },
    {
      name: 'invoices-topic',
      topicArn: 'arn:aws:sns:eu-west-1:000000000000:invoices-topic',
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <SnsListView serviceKey="sns" />
    </MemoryRouter>,
  );
}

describe('SnsListView', () => {
  beforeEach(() => {
    getSnsTopicsMock.mockResolvedValue(listResult);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before topics arrive', () => {
    getSnsTopicsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('sns-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSnsTopicsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-error')).toBeInTheDocument());
  });

  it('renders a row per topic', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());

    expect(
      screen.getByTestId('data-list-row-arn:aws:sns:eu-west-1:000000000000:orders-topic'),
    ).toBeInTheDocument();
    expect(
      screen.getByTestId('data-list-row-arn:aws:sns:eu-west-1:000000000000:invoices-topic'),
    ).toBeInTheDocument();
  });

  it('shows the topic name and arn for each topic', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());

    const names = screen.getAllByTestId('sns-list-name');
    const arns = screen.getAllByTestId('sns-list-arn');
    expect(names[0]).toHaveTextContent('orders-topic');
    expect(arns[0]).toHaveTextContent('arn:aws:sns:eu-west-1:000000000000:orders-topic');
  });

  it('links each topic name to its arn-keyed detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());

    const names = screen.getAllByTestId('sns-list-name');
    expect(names[0]).toHaveAttribute(
      'href',
      '/services/sns/arn%3Aaws%3Asns%3Aeu-west-1%3A000000000000%3Aorders-topic',
    );
  });

  it('reloads the topics when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());
      expect(getSnsTopicsMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getSnsTopicsMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('creates a topic from the form and refreshes the list', async () => {
    createSnsTopicMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sns-create-toggle'));

    fireEvent.change(screen.getByTestId('sns-create-topicName'), {
      target: { value: 'new-topic' },
    });

    fireEvent.click(screen.getByTestId('sns-create-submit'));

    await waitFor(() => expect(screen.getByTestId('sns-create-status')).toBeInTheDocument());

    expect(createSnsTopicMock).toHaveBeenCalledWith({ name: 'new-topic' });
    expect(getSnsTopicsMock).toHaveBeenCalledTimes(2);
    expect(screen.queryByTestId('sns-create-form')).not.toBeInTheDocument();
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sns-create-toggle'));
    expect(screen.getByTestId('sns-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('sns-create-toggle'));
    expect(screen.queryByTestId('sns-create-form')).not.toBeInTheDocument();
  });

  it('shows an error when topic creation fails', async () => {
    createSnsTopicMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('sns-create-toggle'));
    fireEvent.click(screen.getByTestId('sns-create-submit'));

    await waitFor(() => expect(screen.getByTestId('sns-create-error')).toBeInTheDocument());
    expect(screen.getByTestId('sns-create-form')).toBeInTheDocument();
  });

  it('deletes a topic after confirmation and refreshes the list', async () => {
    deleteSnsTopicMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteSnsTopicMock).toHaveBeenCalledWith(
        'arn:aws:sns:eu-west-1:000000000000:orders-topic',
      ),
    );
    await waitFor(() => expect(getSnsTopicsMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when topic deletion fails', async () => {
    deleteSnsTopicMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('sns-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('sns-list-error')).toBeInTheDocument());
  });
});
