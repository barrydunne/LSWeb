import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { EventBridgeListView } from './EventBridgeListView';
import {
  getEventBridgeRules,
  getEventBridgeTargets,
  putEventBridgeEvent,
  resolveReference,
} from '../../api/client';
import type {
  EventBridgeRuleListResult,
  EventBridgeTargetListResult,
} from '../../api/client';

vi.mock('../../api/client');

const getEventBridgeRulesMock = vi.mocked(getEventBridgeRules);
const getEventBridgeTargetsMock = vi.mocked(getEventBridgeTargets);
const putEventBridgeEventMock = vi.mocked(putEventBridgeEvent);
const resolveReferenceMock = vi.mocked(resolveReference);

const rulesResult: EventBridgeRuleListResult = {
  rules: [
    {
      name: 'orders-rule',
      arn: 'arn:aws:events:eu-west-1:000000000000:rule/orders-rule',
      eventBusName: 'default',
      state: 'ENABLED',
      description: 'Routes order events',
      scheduleExpression: 'rate(5 minutes)',
    },
    {
      name: 'audit-rule',
      arn: 'arn:aws:events:eu-west-1:000000000000:rule/audit-rule',
      eventBusName: 'default',
      state: 'DISABLED',
      description: null,
      scheduleExpression: null,
    },
  ],
};

const ordersTargets: EventBridgeTargetListResult = {
  targets: [
    {
      id: 'target-1',
      arn: 'arn:aws:lambda:eu-west-1:000000000000:function:orders-handler',
    },
  ],
};

const auditTargets: EventBridgeTargetListResult = { targets: [] };

function renderView() {
  return render(
    <MemoryRouter>
      <EventBridgeListView serviceKey="eventbridge" />
    </MemoryRouter>,
  );
}

describe('EventBridgeListView', () => {
  beforeEach(() => {
    getEventBridgeRulesMock.mockResolvedValue(rulesResult);
    getEventBridgeTargetsMock.mockImplementation((ruleName) =>
      Promise.resolve(ruleName === 'orders-rule' ? ordersTargets : auditTargets),
    );
    resolveReferenceMock.mockResolvedValue({
      serviceKey: 'lambda',
      resourceId: 'orders-handler',
      route: '/services/lambda/orders-handler',
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before rules arrive', () => {
    getEventBridgeRulesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('eventbridge-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the rules request fails', async () => {
    getEventBridgeRulesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-error')).toBeInTheDocument(),
    );
  });

  it('renders a row per rule', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('data-list-row-orders-rule')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-audit-rule')).toBeInTheDocument();
  });

  it('shows the name, state and schedule for each rule', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    const names = screen.getAllByTestId('eventbridge-list-name');
    const states = screen.getAllByTestId('eventbridge-list-state');
    expect(names[0]).toHaveTextContent('orders-rule');
    expect(states[0]).toHaveTextContent('ENABLED');
    expect(screen.getByTestId('eventbridge-list-schedule')).toHaveTextContent('rate(5 minutes)');
    expect(screen.getByTestId('eventbridge-list-schedule-empty')).toBeInTheDocument();
  });

  it('renders each target as a resolved resource link', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    await waitFor(() =>
      expect(resolveReferenceMock).toHaveBeenCalledWith(
        'arn:aws:lambda:eu-west-1:000000000000:function:orders-handler',
        undefined,
        expect.anything(),
      ),
    );

    const link = await screen.findByRole('link', { name: 'orders-handler' });
    expect(link).toHaveAttribute('href', '/services/lambda/orders-handler');
  });

  it('shows a placeholder when a rule has no targets', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('eventbridge-list-targets-empty')).toBeInTheDocument();
  });

  it('reloads the rules when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
      );
      expect(getEventBridgeRulesMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getEventBridgeRulesMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('disables the send button until the required fields are filled', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('eventbridge-send-submit')).toBeDisabled();

    fireEvent.change(screen.getByTestId('eventbridge-send-source'), {
      target: { value: 'orders.service' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail-type'), {
      target: { value: 'OrderPlaced' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: '{"orderId":"123"}' },
    });

    expect(screen.getByTestId('eventbridge-send-submit')).not.toBeDisabled();
  });

  it('previews the parsed detail JSON', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: '{"orderId":"123"}' },
    });

    expect(screen.getByText('Detail preview')).toBeInTheDocument();
  });

  it('previews raw text when the detail is not valid JSON', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: 'not json' },
    });

    expect(screen.getByText('Detail preview')).toBeInTheDocument();
  });

  it('sends the event and shows the accepted result', async () => {
    putEventBridgeEventMock.mockResolvedValue({
      accepted: true,
      eventId: 'event-1',
      errorCode: null,
      errorMessage: null,
    });
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-send-source'), {
      target: { value: 'orders.service' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail-type'), {
      target: { value: 'OrderPlaced' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-bus'), {
      target: { value: 'orders-bus' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: '{"orderId":"123"}' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-send-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-send-accepted')).toHaveTextContent('event-1'),
    );
    expect(putEventBridgeEventMock).toHaveBeenCalledWith({
      source: 'orders.service',
      detailType: 'OrderPlaced',
      detail: '{"orderId":"123"}',
      eventBusName: 'orders-bus',
    });
  });

  it('omits the event bus name when left blank', async () => {
    putEventBridgeEventMock.mockResolvedValue({
      accepted: true,
      eventId: 'event-1',
      errorCode: null,
      errorMessage: null,
    });
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-send-source'), {
      target: { value: 'orders.service' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail-type'), {
      target: { value: 'OrderPlaced' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: '{"orderId":"123"}' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-send-submit'));

    await waitFor(() => expect(putEventBridgeEventMock).toHaveBeenCalled());
    expect(putEventBridgeEventMock.mock.calls[0][0].eventBusName).toBeNull();
  });

  it('shows a rejection message when the event is not accepted', async () => {
    putEventBridgeEventMock.mockResolvedValue({
      accepted: false,
      eventId: null,
      errorCode: 'InternalException',
      errorMessage: 'Event bus not found.',
    });
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-send-source'), {
      target: { value: 'orders.service' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail-type'), {
      target: { value: 'OrderPlaced' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: '{"orderId":"123"}' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-send-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-send-rejected')).toHaveTextContent(
        'Event bus not found.',
      ),
    );
  });

  it('shows the accepted result without an id when none is returned', async () => {
    putEventBridgeEventMock.mockResolvedValue({
      accepted: true,
      eventId: null,
      errorCode: null,
      errorMessage: null,
    });
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-send-source'), {
      target: { value: 'orders.service' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail-type'), {
      target: { value: 'OrderPlaced' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: '{"orderId":"123"}' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-send-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-send-accepted')).toHaveTextContent(
        'Event accepted.',
      ),
    );
  });

  it('falls back to a generic rejection message when no code or message is returned', async () => {
    putEventBridgeEventMock.mockResolvedValue({
      accepted: false,
      eventId: null,
      errorCode: null,
      errorMessage: null,
    });
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-send-source'), {
      target: { value: 'orders.service' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail-type'), {
      target: { value: 'OrderPlaced' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: '{"orderId":"123"}' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-send-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-send-rejected')).toHaveTextContent(
        'unknown error',
      ),
    );
  });

  it('shows an error message when the send fails', async () => {
    putEventBridgeEventMock.mockRejectedValue(new Error('boom'));
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-send-source'), {
      target: { value: 'orders.service' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail-type'), {
      target: { value: 'OrderPlaced' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-send-detail'), {
      target: { value: '{"orderId":"123"}' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-send-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-send-error')).toBeInTheDocument(),
    );
  });
});
