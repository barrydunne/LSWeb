import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { EventBridgeListView } from './EventBridgeListView';
import {
  getEventBridgeRules,
  getEventBridgeTargets,
  putEventBridgeEvent,
  getScheduledRules,
  getScheduledRule,
  putScheduledRule,
  updateScheduledRule,
  deleteScheduledRule,
  setScheduledRuleState,
  putScheduledRuleTargets,
  removeScheduledRuleTargets,
  resolveReference,
} from '../../api/client';
import type {
  EventBridgeRuleListResult,
  EventBridgeTargetListResult,
  ScheduledRuleListResult,
  ScheduledRuleDetail,
} from '../../api/client';

vi.mock('../../api/client');

const getEventBridgeRulesMock = vi.mocked(getEventBridgeRules);
const getEventBridgeTargetsMock = vi.mocked(getEventBridgeTargets);
const putEventBridgeEventMock = vi.mocked(putEventBridgeEvent);
const getScheduledRulesMock = vi.mocked(getScheduledRules);
const getScheduledRuleMock = vi.mocked(getScheduledRule);
const putScheduledRuleMock = vi.mocked(putScheduledRule);
const updateScheduledRuleMock = vi.mocked(updateScheduledRule);
const deleteScheduledRuleMock = vi.mocked(deleteScheduledRule);
const setScheduledRuleStateMock = vi.mocked(setScheduledRuleState);
const putScheduledRuleTargetsMock = vi.mocked(putScheduledRuleTargets);
const removeScheduledRuleTargetsMock = vi.mocked(removeScheduledRuleTargets);
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

const scheduledRulesResult: ScheduledRuleListResult = {
  rules: [
    {
      name: 'hourly-report',
      arn: 'arn:aws:events:eu-west-1:000000000000:rule/hourly-report',
      eventBusName: 'default',
      state: 'ENABLED',
      description: 'Runs hourly',
      scheduleExpression: 'rate(1 hour)',
    },
    {
      name: 'no-expression',
      arn: 'arn:aws:events:eu-west-1:000000000000:rule/no-expression',
      eventBusName: 'default',
      state: 'DISABLED',
      description: null,
      scheduleExpression: null,
    },
  ],
};

const scheduledRuleDetail: ScheduledRuleDetail = {
  name: 'hourly-report',
  arn: 'arn:aws:events:eu-west-1:000000000000:rule/hourly-report',
  eventBusName: 'default',
  state: 'ENABLED',
  scheduleExpression: 'rate(1 hour)',
  description: 'Runs hourly',
  roleArn: 'arn:aws:iam::000000000000:role/scheduler',
  managedBy: null,
};

function renderView() {
  return render(
    <MemoryRouter>
      <EventBridgeListView serviceKey="eventbridge" />
    </MemoryRouter>,
  );
}

async function openScheduledDetail() {
  renderView();
  await waitFor(() =>
    expect(screen.getByTestId('data-list-row-hourly-report')).toBeInTheDocument(),
  );
  const row = screen.getByTestId('data-list-row-hourly-report');
  fireEvent.click(within(row).getByTestId('eventbridge-scheduled-view'));
  await waitFor(() =>
    expect(screen.getByTestId('eventbridge-scheduled-actions')).toBeInTheDocument(),
  );
}

describe('EventBridgeListView', () => {
  beforeEach(() => {
    getEventBridgeRulesMock.mockResolvedValue(rulesResult);
    getEventBridgeTargetsMock.mockImplementation((ruleName) =>
      Promise.resolve(ruleName === 'orders-rule' ? ordersTargets : auditTargets),
    );
    getScheduledRulesMock.mockResolvedValue(scheduledRulesResult);
    getScheduledRuleMock.mockResolvedValue(scheduledRuleDetail);
    putScheduledRuleMock.mockResolvedValue(undefined);
    updateScheduledRuleMock.mockResolvedValue(undefined);
    deleteScheduledRuleMock.mockResolvedValue(undefined);
    setScheduledRuleStateMock.mockResolvedValue(undefined);
    putScheduledRuleTargetsMock.mockResolvedValue(undefined);
    removeScheduledRuleTargetsMock.mockResolvedValue(undefined);
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

      fireEvent.click(screen.getAllByTestId('auto-refresh-switch')[0]);
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

  it('renders the scheduled rules returned by the API', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-list-view')).toBeInTheDocument(),
    );

    await waitFor(() =>
      expect(screen.getByTestId('data-list-row-hourly-report')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('data-list-row-no-expression')).toBeInTheDocument();
    expect(getScheduledRulesMock).toHaveBeenCalledTimes(1);
  });

  it('shows a loading state for scheduled rules before they arrive', () => {
    getScheduledRulesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('eventbridge-scheduled-loading')).toBeInTheDocument();
  });

  it('shows an error state when the scheduled rules request fails', async () => {
    getScheduledRulesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-error')).toBeInTheDocument(),
    );
  });

  it('loads and shows scheduled rule detail when View is clicked', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('data-list-row-hourly-report')).toBeInTheDocument(),
    );

    const row = screen.getByTestId('data-list-row-hourly-report');
    fireEvent.click(within(row).getByTestId('eventbridge-scheduled-view'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-detail')).toBeInTheDocument(),
    );
    expect(getScheduledRuleMock).toHaveBeenCalledWith('hourly-report', 'default');
    expect(screen.getByTestId('eventbridge-scheduled-detail-schedule')).toHaveTextContent(
      'rate(1 hour)',
    );
    expect(screen.getByTestId('eventbridge-scheduled-detail-managed')).toHaveTextContent('—');
  });

  it('renders dashes for missing detail fields and the managed-by owner', async () => {
    getScheduledRuleMock.mockResolvedValue({
      name: 'no-expression',
      arn: 'arn:aws:events:us-east-1:000000000000:rule/no-expression',
      eventBusName: 'default',
      state: 'DISABLED',
      scheduleExpression: null,
      description: null,
      roleArn: null,
      managedBy: 'aws.events',
    });
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('data-list-row-no-expression')).toBeInTheDocument(),
    );

    const row = screen.getByTestId('data-list-row-no-expression');
    fireEvent.click(within(row).getByTestId('eventbridge-scheduled-view'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-detail')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('eventbridge-scheduled-detail-schedule')).toHaveTextContent('—');
    expect(screen.getByTestId('eventbridge-scheduled-detail-description')).toHaveTextContent('—');
    expect(screen.getByTestId('eventbridge-scheduled-detail-role')).toHaveTextContent('—');
    expect(screen.getByTestId('eventbridge-scheduled-detail-managed')).toHaveTextContent(
      'aws.events',
    );
  });

  it('shows an error when the scheduled rule detail request fails', async () => {
    getScheduledRuleMock.mockRejectedValue(new Error('boom'));
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('data-list-row-hourly-report')).toBeInTheDocument(),
    );

    const row = screen.getByTestId('data-list-row-hourly-report');
    fireEvent.click(within(row).getByTestId('eventbridge-scheduled-view'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-detail-error')).toBeInTheDocument(),
    );
  });

  it('refreshes the scheduled rules and clears the detail panel', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('data-list-row-hourly-report')).toBeInTheDocument(),
      );

      const row = screen.getByTestId('data-list-row-hourly-report');
      fireEvent.click(within(row).getByTestId('eventbridge-scheduled-view'));
      await vi.waitFor(() =>
        expect(screen.getByTestId('eventbridge-scheduled-detail')).toBeInTheDocument(),
      );

      const section = screen.getByTestId('eventbridge-scheduled-section');
      fireEvent.click(within(section).getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() =>
        expect(screen.queryByTestId('eventbridge-scheduled-detail')).not.toBeInTheDocument(),
      );
      expect(getScheduledRulesMock).toHaveBeenCalledTimes(2);
    } finally {
      vi.useRealTimers();
    }
  });

  it('creates a scheduled rule and refreshes the list', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-create')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-name'), {
      target: { value: 'nightly' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-schedule'), {
      target: { value: 'cron(0 0 * * ? *)' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-state'), {
      target: { value: 'DISABLED' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-description'), {
      target: { value: 'Nightly job' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-bus'), {
      target: { value: 'custom-bus' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-create-submit'));

    await waitFor(() =>
      expect(putScheduledRuleMock).toHaveBeenCalledWith({
        name: 'nightly',
        scheduleExpression: 'cron(0 0 * * ? *)',
        state: 'DISABLED',
        description: 'Nightly job',
        eventBusName: 'custom-bus',
      }),
    );
    await waitFor(() => expect(getScheduledRulesMock).toHaveBeenCalledTimes(2));
  });

  it('creates a scheduled rule without a description or bus', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-create')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-name'), {
      target: { value: 'simple' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-schedule'), {
      target: { value: 'rate(2 hours)' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-create-submit'));

    await waitFor(() =>
      expect(putScheduledRuleMock).toHaveBeenCalledWith({
        name: 'simple',
        scheduleExpression: 'rate(2 hours)',
        state: 'ENABLED',
        description: null,
        eventBusName: null,
      }),
    );
  });

  it('shows an error when creating a scheduled rule fails', async () => {
    putScheduledRuleMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-create')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-name'), {
      target: { value: 'x' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-schedule'), {
      target: { value: 'rate(1 hour)' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-create-error')).toBeInTheDocument(),
    );
  });

  it('disables the create submit until name and schedule are provided', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-create')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('eventbridge-scheduled-create-submit')).toBeDisabled();
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-name'), {
      target: { value: 'only-name' },
    });
    expect(screen.getByTestId('eventbridge-scheduled-create-submit')).toBeDisabled();
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-schedule'), {
      target: { value: 'rate(1 hour)' },
    });
    expect(screen.getByTestId('eventbridge-scheduled-create-submit')).toBeEnabled();
  });

  it('disables the create submit while a create is in flight', async () => {
    let resolveCreate: () => void = () => {};
    putScheduledRuleMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveCreate = () => resolve();
      }),
    );
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-create')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-name'), {
      target: { value: 'x' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-create-schedule'), {
      target: { value: 'rate(1 hour)' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-create-submit')).toBeDisabled(),
    );
    resolveCreate();
  });

  it('toggles an enabled rule to disabled', async () => {
    await openScheduledDetail();

    expect(screen.getByTestId('eventbridge-scheduled-toggle-state')).toHaveTextContent('Disable');
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-toggle-state'));

    await waitFor(() =>
      expect(setScheduledRuleStateMock).toHaveBeenCalledWith('hourly-report', 'DISABLED', 'default'),
    );
  });

  it('toggles a disabled rule to enabled', async () => {
    getScheduledRuleMock.mockResolvedValue({ ...scheduledRuleDetail, state: 'DISABLED' });
    await openScheduledDetail();

    expect(screen.getByTestId('eventbridge-scheduled-toggle-state')).toHaveTextContent('Enable');
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-toggle-state'));

    await waitFor(() =>
      expect(setScheduledRuleStateMock).toHaveBeenCalledWith('hourly-report', 'ENABLED', 'default'),
    );
  });

  it('saves edits to a scheduled rule', async () => {
    await openScheduledDetail();

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-edit-schedule'), {
      target: { value: 'rate(30 minutes)' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-edit-state'), {
      target: { value: 'DISABLED' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-edit-description'), {
      target: { value: 'Updated' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-edit-save'));

    await waitFor(() =>
      expect(updateScheduledRuleMock).toHaveBeenCalledWith(
        'hourly-report',
        {
          scheduleExpression: 'rate(30 minutes)',
          state: 'DISABLED',
          description: 'Updated',
        },
        'default',
      ),
    );
  });

  it('saves edits with a cleared description', async () => {
    await openScheduledDetail();

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-edit-description'), {
      target: { value: '   ' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-edit-save'));

    await waitFor(() =>
      expect(updateScheduledRuleMock).toHaveBeenCalledWith(
        'hourly-report',
        {
          scheduleExpression: 'rate(1 hour)',
          state: 'ENABLED',
          description: null,
        },
        'default',
      ),
    );
  });

  it('shows an error when saving edits fails', async () => {
    updateScheduledRuleMock.mockRejectedValue(new Error('boom'));
    await openScheduledDetail();

    fireEvent.click(screen.getByTestId('eventbridge-scheduled-edit-save'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-action-error')).toBeInTheDocument(),
    );
  });

  it('disables save when the schedule is cleared', async () => {
    await openScheduledDetail();

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-edit-schedule'), {
      target: { value: '' },
    });
    expect(screen.getByTestId('eventbridge-scheduled-edit-save')).toBeDisabled();
  });

  it('adds a target with optional role and input', async () => {
    await openScheduledDetail();

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-id'), {
      target: { value: 't1' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-arn'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:queue' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/scheduler' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-input'), {
      target: { value: '{"key":"value"}' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-target-add'));

    await waitFor(() =>
      expect(putScheduledRuleTargetsMock).toHaveBeenCalledWith(
        'hourly-report',
        [
          {
            id: 't1',
            arn: 'arn:aws:sqs:eu-west-1:000000000000:queue',
            roleArn: 'arn:aws:iam::000000000000:role/scheduler',
            input: '{"key":"value"}',
          },
        ],
        'default',
      ),
    );
  });

  it('adds a target without optional role and input', async () => {
    await openScheduledDetail();

    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-id'), {
      target: { value: 't2' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-arn'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:queue-2' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-target-add'));

    await waitFor(() =>
      expect(putScheduledRuleTargetsMock).toHaveBeenCalledWith(
        'hourly-report',
        [
          {
            id: 't2',
            arn: 'arn:aws:sqs:eu-west-1:000000000000:queue-2',
            roleArn: null,
            input: null,
          },
        ],
        'default',
      ),
    );
  });

  it('disables add target until id and arn are provided', async () => {
    await openScheduledDetail();

    expect(screen.getByTestId('eventbridge-scheduled-target-add')).toBeDisabled();
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-id'), {
      target: { value: 't' },
    });
    expect(screen.getByTestId('eventbridge-scheduled-target-add')).toBeDisabled();
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-arn'), {
      target: { value: 'arn' },
    });
    expect(screen.getByTestId('eventbridge-scheduled-target-add')).toBeEnabled();
  });

  it('removes a target from a scheduled rule', async () => {
    await openScheduledDetail();

    expect(screen.getByTestId('eventbridge-scheduled-target-remove')).toBeDisabled();
    fireEvent.change(screen.getByTestId('eventbridge-scheduled-target-remove-id'), {
      target: { value: 'target-1' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-target-remove'));

    await waitFor(() =>
      expect(removeScheduledRuleTargetsMock).toHaveBeenCalledWith(
        'hourly-report',
        ['target-1'],
        'default',
      ),
    );
  });

  it('cancels a pending delete', async () => {
    await openScheduledDetail();

    fireEvent.click(screen.getByTestId('eventbridge-scheduled-delete'));
    expect(screen.getByTestId('eventbridge-scheduled-delete-confirm')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-delete-cancel'));

    expect(
      screen.queryByTestId('eventbridge-scheduled-delete-confirm'),
    ).not.toBeInTheDocument();
    expect(deleteScheduledRuleMock).not.toHaveBeenCalled();
  });

  it('deletes a scheduled rule after confirmation', async () => {
    await openScheduledDetail();

    fireEvent.click(screen.getByTestId('eventbridge-scheduled-delete'));
    fireEvent.click(screen.getByTestId('eventbridge-scheduled-delete-yes'));

    await waitFor(() =>
      expect(deleteScheduledRuleMock).toHaveBeenCalledWith('hourly-report', 'default'),
    );
    await waitFor(() =>
      expect(screen.queryByTestId('eventbridge-scheduled-detail')).not.toBeInTheDocument(),
    );
  });

  it('disables actions while a mutation is in flight', async () => {
    let resolveToggle: () => void = () => {};
    setScheduledRuleStateMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveToggle = () => resolve();
      }),
    );
    await openScheduledDetail();

    fireEvent.click(screen.getByTestId('eventbridge-scheduled-toggle-state'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-scheduled-toggle-state')).toBeDisabled(),
    );
    expect(screen.getByTestId('eventbridge-scheduled-edit-save')).toBeDisabled();
    expect(screen.getByTestId('eventbridge-scheduled-target-add')).toBeDisabled();
    expect(screen.getByTestId('eventbridge-scheduled-target-remove')).toBeDisabled();
    expect(screen.getByTestId('eventbridge-scheduled-delete')).toBeDisabled();
    resolveToggle();
  });
});
