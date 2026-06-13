import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SchedulerDetailView } from './SchedulerDetailView';
import { getSchedule, updateSchedule, resolveReference } from '../../api/client';
import type { ScheduleDetailResult } from '../../api/client';

vi.mock('../../api/client');

const getScheduleMock = vi.mocked(getSchedule);
const updateScheduleMock = vi.mocked(updateSchedule);
const resolveReferenceMock = vi.mocked(resolveReference);

const detailResult: ScheduleDetailResult = {
  name: 'nightly',
  groupName: 'default',
  state: 'ENABLED',
  scheduleExpression: 'rate(1 day)',
  scheduleExpressionTimezone: 'UTC',
  description: 'Nightly job',
  startDate: '2024-01-01T00:00:00+00:00',
  endDate: null,
  targetArn: 'arn:aws:lambda:eu-west-1:000000000000:function:job',
  roleArn: 'arn:aws:iam::000000000000:role/scheduler',
  flexibleTimeWindowMode: 'FLEXIBLE',
  maximumWindowInMinutes: 15,
  arn: 'arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly',
  creationDate: '2024-01-01T00:00:00+00:00',
  lastModificationDate: '2024-01-02T00:00:00+00:00',
  targetInput: null,
};

function renderView(resourceId = 'default/nightly') {
  return render(
    <MemoryRouter>
      <SchedulerDetailView serviceKey="scheduler" resourceId={resourceId} />
    </MemoryRouter>,
  );
}

describe('SchedulerDetailView', () => {
  beforeEach(() => {
    getScheduleMock.mockResolvedValue(detailResult);
    updateScheduleMock.mockResolvedValue();
    resolveReferenceMock.mockResolvedValue(null as never);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before the schedule arrives', () => {
    getScheduleMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('scheduler-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getScheduleMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-error')).toBeInTheDocument(),
    );
  });

  it('renders the schedule metadata', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('scheduler-detail-name')).toHaveTextContent('nightly');
    expect(screen.getByTestId('scheduler-detail-group')).toHaveTextContent('default');
    expect(screen.getByTestId('scheduler-detail-state')).toHaveTextContent('ENABLED');
    expect(screen.getByTestId('scheduler-detail-expression')).toHaveTextContent('rate(1 day)');
    expect(screen.getByTestId('scheduler-detail-timezone')).toHaveTextContent('UTC');
    expect(screen.getByTestId('scheduler-detail-description')).toHaveTextContent('Nightly job');
    expect(screen.getByTestId('scheduler-detail-start')).toHaveTextContent(
      '2024-01-01T00:00:00+00:00',
    );
    expect(screen.getByTestId('scheduler-detail-target')).toHaveTextContent(
      'arn:aws:lambda:eu-west-1:000000000000:function:job',
    );
    expect(screen.getByTestId('scheduler-detail-arn')).toHaveTextContent(
      'arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly',
    );
    expect(screen.getByTestId('scheduler-detail-created')).toHaveTextContent(
      '2024-01-01T00:00:00+00:00',
    );
    expect(screen.getByTestId('scheduler-detail-modified')).toHaveTextContent(
      '2024-01-02T00:00:00+00:00',
    );
  });

  it('renders the flexible time window with its maximum window', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-flexible')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('scheduler-detail-flexible')).toHaveTextContent('FLEXIBLE (15 min)');
  });

  it('renders placeholders when optional fields are absent', async () => {
    getScheduleMock.mockResolvedValue({
      ...detailResult,
      scheduleExpressionTimezone: null,
      description: null,
      startDate: null,
      endDate: null,
      flexibleTimeWindowMode: 'OFF',
      maximumWindowInMinutes: null,
      creationDate: null,
      lastModificationDate: null,
    });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('scheduler-detail-timezone')).toHaveTextContent('—');
    expect(screen.getByTestId('scheduler-detail-description')).toHaveTextContent('—');
    expect(screen.getByTestId('scheduler-detail-start')).toHaveTextContent('—');
    expect(screen.getByTestId('scheduler-detail-end')).toHaveTextContent('—');
    expect(screen.getByTestId('scheduler-detail-flexible')).toHaveTextContent('OFF');
    expect(screen.getByTestId('scheduler-detail-created')).toHaveTextContent('—');
    expect(screen.getByTestId('scheduler-detail-modified')).toHaveTextContent('—');
  });

  it('requests the schedule identified by the composite resource id', async () => {
    renderView('default/nightly');

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );

    expect(getScheduleMock).toHaveBeenCalledWith('nightly', 'default', expect.anything());
  });

  it('falls back to the default group when the resource id has no separator', async () => {
    renderView('lonely');

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );

    expect(getScheduleMock).toHaveBeenCalledWith('lonely', 'default', expect.anything());
  });

  it('renders the schedule role as a resource link reference', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-role')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('scheduler-detail-role')).toHaveTextContent(
      'arn:aws:iam::000000000000:role/scheduler',
    );
  });

  it('toggles the edit form and prefills it from the schedule', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('scheduler-detail-edit-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));

    expect(screen.getByTestId('scheduler-detail-edit-form')).toBeInTheDocument();
    expect(screen.getByTestId('scheduler-detail-edit-expression')).toHaveValue('rate(1 day)');
    expect(screen.getByTestId('scheduler-detail-edit-target')).toHaveValue(
      'arn:aws:lambda:eu-west-1:000000000000:function:job',
    );
    expect(screen.getByTestId('scheduler-detail-edit-role')).toHaveValue(
      'arn:aws:iam::000000000000:role/scheduler',
    );
    expect(screen.getByTestId('scheduler-detail-edit-mode')).toHaveValue('FLEXIBLE');
    expect(screen.getByTestId('scheduler-detail-edit-window')).toHaveValue(15);
    expect(screen.getByTestId('scheduler-detail-edit-state')).toHaveValue('ENABLED');
  });

  it('saves the edited schedule after confirmation', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));

    fireEvent.change(screen.getByTestId('scheduler-detail-edit-expression'), {
      target: { value: 'rate(2 days)' },
    });
    fireEvent.change(screen.getByTestId('scheduler-detail-edit-target'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:queue' },
    });
    fireEvent.change(screen.getByTestId('scheduler-detail-edit-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/other' },
    });
    fireEvent.change(screen.getByTestId('scheduler-detail-edit-mode'), {
      target: { value: 'OFF' },
    });
    fireEvent.change(screen.getByTestId('scheduler-detail-edit-state'), {
      target: { value: 'DISABLED' },
    });
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-save-status')).toBeInTheDocument(),
    );
    expect(updateScheduleMock).toHaveBeenCalledWith('nightly', 'default', {
      scheduleExpression: 'rate(2 days)',
      scheduleExpressionTimezone: 'UTC',
      description: null,
      startDate: null,
      endDate: null,
      targetArn: 'arn:aws:sqs:eu-west-1:000000000000:queue',
      roleArn: 'arn:aws:iam::000000000000:role/other',
      flexibleTimeWindowMode: 'OFF',
      maximumWindowInMinutes: 15,
      state: 'DISABLED',
      targetInput: null,
    });
  });

  it('prefills an empty window and saves null when the schedule has no window', async () => {
    getScheduleMock.mockResolvedValue({
      ...detailResult,
      flexibleTimeWindowMode: 'OFF',
      maximumWindowInMinutes: null,
    });
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));

    expect(screen.getByTestId('scheduler-detail-edit-mode')).toHaveValue('OFF');
    expect(screen.queryByTestId('scheduler-detail-edit-window')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-save-status')).toBeInTheDocument(),
    );
    expect(updateScheduleMock).toHaveBeenCalledWith(
      'nightly',
      'default',
      expect.objectContaining({
        flexibleTimeWindowMode: 'OFF',
        maximumWindowInMinutes: null,
      }),
    );
  });

  it('saves a flexible schedule with the entered window minutes', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));

    fireEvent.change(screen.getByTestId('scheduler-detail-edit-window'), {
      target: { value: '30' },
    });
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-save-status')).toBeInTheDocument(),
    );
    expect(updateScheduleMock).toHaveBeenCalledWith(
      'nightly',
      'default',
      expect.objectContaining({
        flexibleTimeWindowMode: 'FLEXIBLE',
        maximumWindowInMinutes: 30,
      }),
    );
  });

  it('shows an error when the update fails', async () => {
    updateScheduleMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-save-error')).toBeInTheDocument(),
    );
  });

  it('renders the target payload when present', async () => {
    getScheduleMock.mockResolvedValue({
      ...detailResult,
      targetInput: '{"key":"value"}',
    });
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-target-input')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('scheduler-detail-target-input')).toHaveTextContent('{"key":"value"}');
  });

  it('blocks the update when the target payload is not valid JSON', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));

    fireEvent.change(screen.getByTestId('scheduler-detail-edit-payload'), {
      target: { value: 'not-json' },
    });
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    expect(screen.getByTestId('scheduler-detail-save-error')).toHaveTextContent(
      'must be valid JSON',
    );
    expect(updateScheduleMock).not.toHaveBeenCalled();
  });

  it('saves the edited target payload and timezone after confirmation', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));

    expect(screen.getByTestId('scheduler-detail-edit-timezone')).toHaveValue('UTC');
    fireEvent.change(screen.getByTestId('scheduler-detail-edit-timezone'), {
      target: { value: 'Europe/Dublin' },
    });
    fireEvent.change(screen.getByTestId('scheduler-detail-edit-payload'), {
      target: { value: '{"payload":1}' },
    });
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-save-status')).toBeInTheDocument(),
    );
    expect(updateScheduleMock).toHaveBeenCalledWith(
      'nightly',
      'default',
      expect.objectContaining({
        scheduleExpressionTimezone: 'Europe/Dublin',
        targetInput: '{"payload":1}',
      }),
    );
  });

  it('saves a null timezone when the timezone field is cleared', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));

    fireEvent.change(screen.getByTestId('scheduler-detail-edit-timezone'), {
      target: { value: '   ' },
    });
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-save-status')).toBeInTheDocument(),
    );
    expect(updateScheduleMock).toHaveBeenCalledWith(
      'nightly',
      'default',
      expect.objectContaining({ scheduleExpressionTimezone: null }),
    );
  });

  it('prefills an empty timezone field when the schedule has no timezone', async () => {
    getScheduleMock.mockResolvedValue({
      ...detailResult,
      scheduleExpressionTimezone: null,
    });
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('scheduler-detail-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('scheduler-detail-edit-toggle'));

    expect(screen.getByTestId('scheduler-detail-edit-timezone')).toHaveValue('');
  });
});
