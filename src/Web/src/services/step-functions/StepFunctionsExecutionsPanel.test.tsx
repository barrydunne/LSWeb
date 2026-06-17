import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { StepFunctionsExecutionsPanel } from './StepFunctionsExecutionsPanel';
import { getExecutions, getExecutionHistory, startExecution } from '../../api/client';
import type { ExecutionListResult, StartExecutionResult } from '../../api/client';

vi.mock('../../api/client');

const getExecutionsMock = vi.mocked(getExecutions);
const getExecutionHistoryMock = vi.mocked(getExecutionHistory);
const startExecutionMock = vi.mocked(startExecution);

const stateMachineArn = 'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow';

const listResult: ExecutionListResult = {
  executions: [
    {
      executionArn:
        'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1',
      name: 'run-1',
      stateMachineArn,
      status: 'SUCCEEDED',
      startDate: '2024-01-01T00:00:00+00:00',
      stopDate: '2024-01-01T00:01:00+00:00',
    },
    {
      executionArn:
        'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-2',
      name: 'run-2',
      stateMachineArn,
      status: 'RUNNING',
      startDate: '2024-01-02T00:00:00+00:00',
      stopDate: null,
    },
  ],
};

const startResult: StartExecutionResult = {
  executionArn: 'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-3',
  startDate: '2024-01-03T00:00:00+00:00',
};

function renderPanel() {
  return render(<StepFunctionsExecutionsPanel stateMachineArn={stateMachineArn} />);
}

describe('StepFunctionsExecutionsPanel', () => {
  beforeEach(() => {
    getExecutionsMock.mockResolvedValue(listResult);
    startExecutionMock.mockResolvedValue(startResult);
    getExecutionHistoryMock.mockResolvedValue({ events: [] });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('polls while there are non-terminal executions', async () => {
    const timeoutSpy = vi.spyOn(globalThis, 'setTimeout');
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    expect(getExecutionsMock).toHaveBeenCalledTimes(1);
    const pollCall = timeoutSpy.mock.calls.find((call) => call[1] === 1500);
    expect(pollCall).toBeDefined();

    const tick = pollCall![0] as () => void;
    await act(async () => {
      tick();
    });
    await waitFor(() => expect(getExecutionsMock).toHaveBeenCalledTimes(2));

    const pollSchedules = timeoutSpy.mock.calls.filter((call) => call[1] === 1500);
    expect(pollSchedules.length).toBeGreaterThanOrEqual(2);

    timeoutSpy.mockRestore();
  });

  it('does not poll when all executions are terminal', async () => {
    const timeoutSpy = vi.spyOn(globalThis, 'setTimeout');
    getExecutionsMock.mockResolvedValue({ executions: [listResult.executions[0]] });
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    expect(getExecutionsMock).toHaveBeenCalledTimes(1);
    const pollCall = timeoutSpy.mock.calls.find((call) => call[1] === 1500);
    expect(pollCall).toBeUndefined();
    timeoutSpy.mockRestore();
  });

  it('shows a loading state before executions arrive', () => {
    getExecutionsMock.mockReturnValue(new Promise(() => {}));

    renderPanel();

    expect(screen.getByTestId('step-functions-executions-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getExecutionsMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-error')).toBeInTheDocument(),
    );
  });

  it('ignores an aborted request without surfacing an error state', async () => {
    getExecutionsMock.mockRejectedValue(
      new DOMException('The operation was aborted.', 'AbortError'),
    );

    renderPanel();

    await waitFor(() => expect(getExecutionsMock).toHaveBeenCalled());
    expect(screen.queryByTestId('step-functions-executions-error')).not.toBeInTheDocument();
    expect(screen.getByTestId('step-functions-executions-loading')).toBeInTheDocument();
  });

  it('shows an empty state when there are no executions', async () => {
    getExecutionsMock.mockResolvedValue({ executions: [] });

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-empty')).toBeInTheDocument(),
    );
  });

  it('renders executions with status and stop date fallback', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    const rows = screen.getAllByTestId('step-functions-execution-row');
    expect(rows).toHaveLength(2);
    const statuses = screen.getAllByTestId('step-functions-execution-status');
    expect(statuses[0]).toHaveTextContent('SUCCEEDED');
    expect(statuses[1]).toHaveTextContent('RUNNING');
    expect(rows[0]).toHaveTextContent('2024-01-01T00:01:00+00:00');
    expect(rows[1]).toHaveTextContent('—');
    expect(getExecutionsMock).toHaveBeenCalledWith(stateMachineArn, expect.any(AbortSignal));
  });

  it('starts an execution with the trimmed name and input then refreshes', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('step-functions-execution-name'), {
      target: { value: '  run-3  ' },
    });
    fireEvent.change(screen.getByTestId('step-functions-execution-input'), {
      target: { value: '  {"key":"value"}  ' },
    });
    fireEvent.click(screen.getByTestId('step-functions-execution-start'));

    await waitFor(() =>
      expect(startExecutionMock).toHaveBeenCalledWith({
        stateMachineArn,
        name: 'run-3',
        input: '{"key":"value"}',
      }),
    );
    await waitFor(() => expect(getExecutionsMock).toHaveBeenCalledTimes(2));
  });

  it('sends null name and input when the fields are blank', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('step-functions-execution-start'));

    await waitFor(() =>
      expect(startExecutionMock).toHaveBeenCalledWith({
        stateMachineArn,
        name: null,
        input: null,
      }),
    );
    await waitFor(() => expect(getExecutionsMock).toHaveBeenCalledTimes(2));
  });

  it('blocks starting when the input is not valid JSON', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('step-functions-execution-input'), {
      target: { value: 'not json' },
    });
    fireEvent.click(screen.getByTestId('step-functions-execution-start'));

    expect(screen.getByTestId('step-functions-execution-input-error')).toBeInTheDocument();
    expect(startExecutionMock).not.toHaveBeenCalled();
  });

  it('shows an error message when starting fails', async () => {
    startExecutionMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('step-functions-execution-start'));

    await waitFor(() =>
      expect(
        screen.getByTestId('step-functions-execution-start-error'),
      ).toBeInTheDocument(),
    );
  });

  it('toggles the history panel for the selected execution', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    const toggles = screen.getAllByTestId('step-functions-execution-history-toggle');
    expect(toggles[0]).toHaveTextContent('View history');

    fireEvent.click(toggles[0]);

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-execution-history-row')).toBeInTheDocument(),
    );
    expect(getExecutionHistoryMock).toHaveBeenCalledWith(
      'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1',
      expect.any(AbortSignal),
    );
    expect(screen.getAllByTestId('step-functions-execution-history-toggle')[0]).toHaveTextContent(
      'Hide history',
    );

    fireEvent.click(screen.getAllByTestId('step-functions-execution-history-toggle')[0]);

    await waitFor(() =>
      expect(
        screen.queryByTestId('step-functions-execution-history-row'),
      ).not.toBeInTheDocument(),
    );
  });

  it('reconciles a stale row status when its history reveals a terminal status', async () => {
    const reconciled: ExecutionListResult = {
      executions: [
        listResult.executions[0],
        { ...listResult.executions[1], status: 'SUCCEEDED', stopDate: '2024-01-02T00:00:05+00:00' },
      ],
    };
    getExecutionsMock.mockResolvedValueOnce(listResult).mockResolvedValueOnce(reconciled);
    getExecutionHistoryMock.mockResolvedValue({
      events: [
        {
          id: 1,
          previousEventId: null,
          type: 'ExecutionSucceeded',
          timestamp: '2024-01-02T00:00:05+00:00',
          name: null,
          input: null,
          output: null,
          error: null,
          cause: null,
        },
      ],
    });

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );
    expect(screen.getAllByTestId('step-functions-execution-status')[1]).toHaveTextContent('RUNNING');

    fireEvent.click(screen.getAllByTestId('step-functions-execution-history-toggle')[1]);

    await waitFor(() => expect(getExecutionsMock).toHaveBeenCalledTimes(2));
    await waitFor(() =>
      expect(screen.getAllByTestId('step-functions-execution-status')[1]).toHaveTextContent(
        'SUCCEEDED',
      ),
    );
  });

  it('does not reconcile when the history status matches the row status', async () => {
    getExecutionHistoryMock.mockResolvedValue({
      events: [
        {
          id: 1,
          previousEventId: null,
          type: 'ExecutionSucceeded',
          timestamp: '2024-01-01T00:01:00+00:00',
          name: null,
          input: null,
          output: null,
          error: null,
          cause: null,
        },
      ],
    });

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('step-functions-execution-history-toggle')[0]);

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-execution-history-row')).toBeInTheDocument(),
    );
    expect(getExecutionsMock).toHaveBeenCalledTimes(1);
  });

  const withFailure: ExecutionListResult = {
    executions: [
      ...listResult.executions,
      {
        executionArn: 'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-3',
        name: 'run-3',
        stateMachineArn,
        status: 'FAILED',
        startDate: '2024-01-03T00:00:00+00:00',
        stopDate: '2024-01-03T00:01:00+00:00',
      },
    ],
  };

  it('filters executions by status', async () => {
    getExecutionsMock.mockResolvedValue(withFailure);
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );
    expect(screen.getAllByTestId('step-functions-execution-row')).toHaveLength(3);

    fireEvent.change(screen.getByTestId('step-functions-executions-filter'), {
      target: { value: 'FAILED' },
    });

    const rows = screen.getAllByTestId('step-functions-execution-row');
    expect(rows).toHaveLength(1);
    expect(rows[0]).toHaveTextContent('run-3');
  });

  it('highlights failed executions', async () => {
    getExecutionsMock.mockResolvedValue(withFailure);
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    const statuses = screen.getAllByTestId('step-functions-execution-status');
    const failed = statuses.find((cell) => cell.textContent === 'FAILED');
    expect(failed).toHaveStyle({ color: '#f85149' });
  });

  it('shows an empty message when no executions match the filter', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-executions-table')).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByTestId('step-functions-executions-filter'), {
      target: { value: 'ABORTED' },
    });

    expect(screen.getByTestId('step-functions-executions-filter-empty')).toBeInTheDocument();
    expect(screen.queryByTestId('step-functions-executions-table')).not.toBeInTheDocument();
  });
});
