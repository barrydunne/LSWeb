import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SchedulerListView } from './SchedulerListView';
import {
  getSchedules,
  createSchedule,
  deleteSchedule,
  getScheduleGroups,
  createScheduleGroup,
  deleteScheduleGroup,
} from '../../api/client';
import type { ScheduleGroupListResult, ScheduleListResult } from '../../api/client';

vi.mock('../../api/client');

const getSchedulesMock = vi.mocked(getSchedules);
const createScheduleMock = vi.mocked(createSchedule);
const deleteScheduleMock = vi.mocked(deleteSchedule);
const getScheduleGroupsMock = vi.mocked(getScheduleGroups);
const createScheduleGroupMock = vi.mocked(createScheduleGroup);
const deleteScheduleGroupMock = vi.mocked(deleteScheduleGroup);

const listResult: ScheduleListResult = {
  schedules: [
    {
      name: 'nightly',
      groupName: 'default',
      state: 'ENABLED',
      targetArn: 'arn:aws:lambda:eu-west-1:000000000000:function:job',
      arn: 'arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly',
    },
    {
      name: 'hourly',
      groupName: 'jobs',
      state: 'DISABLED',
      targetArn: 'arn:aws:sqs:eu-west-1:000000000000:queue',
      arn: 'arn:aws:scheduler:eu-west-1:000000000000:schedule/jobs/hourly',
    },
  ],
};

const groupsResult: ScheduleGroupListResult = {
  groups: [
    {
      name: 'default',
      state: 'ACTIVE',
      arn: 'arn:aws:scheduler:eu-west-1:000000000000:schedule-group/default',
      creationDate: '2024-01-01T00:00:00+00:00',
      lastModificationDate: '2024-01-02T00:00:00+00:00',
    },
    {
      name: 'jobs',
      state: 'ACTIVE',
      arn: 'arn:aws:scheduler:eu-west-1:000000000000:schedule-group/jobs',
      creationDate: '2024-01-01T00:00:00+00:00',
      lastModificationDate: '2024-01-02T00:00:00+00:00',
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <SchedulerListView serviceKey="scheduler" />
    </MemoryRouter>,
  );
}

describe('SchedulerListView', () => {
  beforeEach(() => {
    getSchedulesMock.mockResolvedValue(listResult);
    createScheduleMock.mockResolvedValue();
    deleteScheduleMock.mockResolvedValue();
    getScheduleGroupsMock.mockResolvedValue(groupsResult);
    createScheduleGroupMock.mockResolvedValue();
    deleteScheduleGroupMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before schedules arrive', () => {
    getSchedulesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('scheduler-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSchedulesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('scheduler-list-error')).toBeInTheDocument());
  });

  it('renders a row per schedule', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());

    expect(
      screen.getByTestId(
        'data-list-row-arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly',
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByTestId(
        'data-list-row-arn:aws:scheduler:eu-west-1:000000000000:schedule/jobs/hourly',
      ),
    ).toBeInTheDocument();
  });

  it('shows the name, group, state and target for each schedule', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());

    const names = screen.getAllByTestId('scheduler-list-name');
    const groups = screen.getAllByTestId('scheduler-list-group');
    const states = screen.getAllByTestId('scheduler-list-state');
    const targets = screen.getAllByTestId('scheduler-list-target');
    expect(names[0]).toHaveTextContent('nightly');
    expect(groups[0]).toHaveTextContent('default');
    expect(states[0]).toHaveTextContent('ENABLED');
    expect(targets[0]).toHaveTextContent('arn:aws:lambda:eu-west-1:000000000000:function:job');
  });

  it('links each schedule name to its composite-id detail view', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());

    const names = screen.getAllByTestId('scheduler-list-name');
    expect(names[0]).toHaveAttribute('href', '/services/scheduler/default%2Fnightly');
  });

  it('reloads the schedules when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument(),
      );
      expect(getSchedulesMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getSchedulesMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('toggles the create form', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());

    expect(screen.queryByTestId('scheduler-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('scheduler-create-toggle'));
    expect(screen.getByTestId('scheduler-create-form')).toBeInTheDocument();
  });

  it('creates a schedule with the OFF window and a null window value', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-create-toggle'));

    fireEvent.change(screen.getByTestId('scheduler-create-name'), {
      target: { value: 'daily-job' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-group'), {
      target: { value: 'jobs' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-expression'), {
      target: { value: 'rate(5 minutes)' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-timezone'), {
      target: { value: 'Europe/Dublin' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target-type'), {
      target: { value: 'sqs' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:queue' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/scheduler' },
    });
    fireEvent.click(screen.getByTestId('scheduler-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-create-status')).toBeInTheDocument(),
    );
    expect(createScheduleMock).toHaveBeenCalledWith({
      name: 'daily-job',
      groupName: 'jobs',
      scheduleExpression: 'rate(5 minutes)',
      scheduleExpressionTimezone: 'Europe/Dublin',
      description: null,
      startDate: null,
      endDate: null,
      targetArn: 'arn:aws:sqs:eu-west-1:000000000000:queue',
      roleArn: 'arn:aws:iam::000000000000:role/scheduler',
      flexibleTimeWindowMode: 'OFF',
      maximumWindowInMinutes: null,
      state: 'ENABLED',
      targetInput: null,
    });
  });

  it('creates a flexible schedule with the entered window minutes', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-create-toggle'));

    fireEvent.change(screen.getByTestId('scheduler-create-name'), {
      target: { value: 'flex-job' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-expression'), {
      target: { value: 'rate(1 hour)' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target-type'), {
      target: { value: 'sqs' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:queue' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/scheduler' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-mode'), {
      target: { value: 'FLEXIBLE' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-window'), {
      target: { value: '15' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-state'), {
      target: { value: 'DISABLED' },
    });
    fireEvent.click(screen.getByTestId('scheduler-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-create-status')).toBeInTheDocument(),
    );
    expect(createScheduleMock).toHaveBeenCalledWith(
      expect.objectContaining({
        name: 'flex-job',
        flexibleTimeWindowMode: 'FLEXIBLE',
        maximumWindowInMinutes: 15,
        state: 'DISABLED',
      }),
    );
  });

  it('shows a saving label while the create is in flight', async () => {
    let resolveCreate: (() => void) | undefined;
    createScheduleMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveCreate = resolve;
      }),
    );
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-create-toggle'));
    fireEvent.change(screen.getByTestId('scheduler-create-name'), {
      target: { value: 'daily-job' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target-type'), {
      target: { value: 'other' },
    });
    fireEvent.click(screen.getByTestId('scheduler-create-submit'));

    expect(screen.getByTestId('scheduler-create-submit')).toBeDisabled();
    expect(screen.getByTestId('scheduler-create-submit')).toHaveTextContent('Creating');

    resolveCreate?.();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-create-status')).toBeInTheDocument(),
    );
  });

  it('shows an error when schedule creation fails', async () => {
    createScheduleMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-create-toggle'));
    fireEvent.change(screen.getByTestId('scheduler-create-name'), {
      target: { value: 'daily-job' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target-type'), {
      target: { value: 'other' },
    });
    fireEvent.click(screen.getByTestId('scheduler-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-create-error')).toBeInTheDocument(),
    );
  });

  it('blocks creation when the ARN does not match the selected target type', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-create-toggle'));
    fireEvent.change(screen.getByTestId('scheduler-create-name'), {
      target: { value: 'daily-job' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target-type'), {
      target: { value: 'lambda' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:queue' },
    });
    fireEvent.click(screen.getByTestId('scheduler-create-submit'));

    expect(screen.getByTestId('scheduler-create-error')).toHaveTextContent(
      'does not look like a Lambda function ARN',
    );
    expect(createScheduleMock).not.toHaveBeenCalled();
  });

  it('blocks creation when the target payload is not valid JSON', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-create-toggle'));
    fireEvent.change(screen.getByTestId('scheduler-create-name'), {
      target: { value: 'daily-job' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target-type'), {
      target: { value: 'other' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-payload'), {
      target: { value: 'not-json' },
    });
    fireEvent.click(screen.getByTestId('scheduler-create-submit'));

    expect(screen.getByTestId('scheduler-create-error')).toHaveTextContent(
      'must be valid JSON',
    );
    expect(createScheduleMock).not.toHaveBeenCalled();
  });

  it('creates a schedule with a target payload', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-create-toggle'));
    fireEvent.change(screen.getByTestId('scheduler-create-name'), {
      target: { value: 'daily-job' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-expression'), {
      target: { value: 'rate(5 minutes)' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target-type'), {
      target: { value: 'lambda' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-target'), {
      target: { value: 'arn:aws:lambda:eu-west-1:000000000000:function:job' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/scheduler' },
    });
    fireEvent.change(screen.getByTestId('scheduler-create-payload'), {
      target: { value: '{"key":"value"}' },
    });
    fireEvent.click(screen.getByTestId('scheduler-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-create-status')).toBeInTheDocument(),
    );
    expect(createScheduleMock).toHaveBeenCalledWith(
      expect.objectContaining({ targetInput: '{"key":"value"}' }),
    );
  });

  it('deletes a schedule after confirmation', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteScheduleMock).toHaveBeenCalledWith('nightly', 'default'));
  });

  it('shows an error when schedule deletion fails', async () => {
    deleteScheduleMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('scheduler-list-error')).toBeInTheDocument());
  });

  it('toggles the groups panel', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());

    expect(screen.queryByTestId('scheduler-groups-panel')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));
    expect(screen.getByTestId('scheduler-groups-panel')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));
    expect(screen.queryByTestId('scheduler-groups-panel')).not.toBeInTheDocument();
  });

  it('lists the schedule groups with a delete only for non-default groups', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));

    const names = screen.getAllByTestId('scheduler-group-item-name');
    expect(names[0]).toHaveTextContent('default');
    expect(names[1]).toHaveTextContent('jobs');
    // Only the non-default group exposes a delete confirmation trigger.
    const items = screen.getAllByTestId('scheduler-group-item');
    expect(items[0].querySelector('[data-testid="confirm-trigger"]')).toBeNull();
    expect(items[1].querySelector('[data-testid="confirm-trigger"]')).not.toBeNull();
  });

  it('creates a schedule group', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));

    fireEvent.change(screen.getByTestId('scheduler-group-name'), {
      target: { value: 'reports' },
    });
    fireEvent.click(screen.getByTestId('scheduler-group-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-group-status')).toBeInTheDocument(),
    );
    expect(createScheduleGroupMock).toHaveBeenCalledWith({ name: 'reports' });
  });

  it('shows a saving label while the group create is in flight', async () => {
    let resolveCreate: (() => void) | undefined;
    createScheduleGroupMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveCreate = resolve;
      }),
    );
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));
    fireEvent.change(screen.getByTestId('scheduler-group-name'), {
      target: { value: 'reports' },
    });
    fireEvent.click(screen.getByTestId('scheduler-group-submit'));

    expect(screen.getByTestId('scheduler-group-submit')).toBeDisabled();
    expect(screen.getByTestId('scheduler-group-submit')).toHaveTextContent('Creating');

    resolveCreate?.();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-group-status')).toBeInTheDocument(),
    );
  });

  it('shows an error when schedule group creation fails', async () => {
    createScheduleGroupMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));
    fireEvent.change(screen.getByTestId('scheduler-group-name'), {
      target: { value: 'reports' },
    });
    fireEvent.click(screen.getByTestId('scheduler-group-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-group-error')).toBeInTheDocument(),
    );
  });

  it('deletes a schedule group after confirmation', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));

    const items = screen.getAllByTestId('scheduler-group-item');
    fireEvent.click(items[1].querySelector('[data-testid="confirm-trigger"]') as HTMLElement);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteScheduleGroupMock).toHaveBeenCalledWith('jobs'));
  });

  it('shows an error when schedule group deletion fails', async () => {
    deleteScheduleGroupMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));

    const items = screen.getAllByTestId('scheduler-group-item');
    fireEvent.click(items[1].querySelector('[data-testid="confirm-trigger"]') as HTMLElement);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('scheduler-list-error')).toBeInTheDocument());
  });

  it('filters the schedules by the selected group', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());

    expect(screen.getAllByTestId('scheduler-list-name')).toHaveLength(2);

    fireEvent.change(screen.getByTestId('scheduler-group-filter'), {
      target: { value: 'jobs' },
    });

    const names = screen.getAllByTestId('scheduler-list-name');
    expect(names).toHaveLength(1);
    expect(names[0]).toHaveTextContent('hourly');
  });

  it('falls back to an empty group list when the groups request fails', async () => {
    getScheduleGroupsMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('scheduler-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('scheduler-groups-toggle'));

    expect(screen.queryAllByTestId('scheduler-group-item')).toHaveLength(0);
  });
});
