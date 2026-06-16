import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { StepFunctionsExecutionHistoryPanel } from './StepFunctionsExecutionHistoryPanel';
import { getExecutionHistory } from '../../api/client';
import type { ExecutionHistoryEvent, ExecutionHistoryResult } from '../../api/client';

vi.mock('../../api/client');

const getExecutionHistoryMock = vi.mocked(getExecutionHistory);

const executionArn = 'arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1';

const events: ExecutionHistoryEvent[] = [
  {
    id: 1,
    previousEventId: null,
    type: 'ExecutionStarted',
    timestamp: '2024-01-01T00:00:00+00:00',
    name: null,
    input: '{"key":"value"}',
    output: null,
    error: null,
    cause: null,
  },
  {
    id: 2,
    previousEventId: 1,
    type: 'TaskStateExited',
    timestamp: '2024-01-01T00:00:01+00:00',
    name: 'DoWork',
    input: null,
    output: 'not-json',
    error: null,
    cause: null,
  },
  {
    id: 3,
    previousEventId: 2,
    type: 'ExecutionFailed',
    timestamp: '2024-01-01T00:00:02+00:00',
    name: null,
    input: null,
    output: null,
    error: 'States.Timeout',
    cause: 'It timed out',
  },
];

const historyResult: ExecutionHistoryResult = { events };

function renderPanel() {
  return render(<StepFunctionsExecutionHistoryPanel executionArn={executionArn} />);
}

describe('StepFunctionsExecutionHistoryPanel', () => {
  beforeEach(() => {
    getExecutionHistoryMock.mockResolvedValue(historyResult);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before history arrives', () => {
    getExecutionHistoryMock.mockReturnValue(new Promise(() => {}));

    renderPanel();

    expect(screen.getByTestId('step-functions-history-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getExecutionHistoryMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-history-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty state when there are no events', async () => {
    getExecutionHistoryMock.mockResolvedValue({ events: [] });

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-history-empty')).toBeInTheDocument(),
    );
  });

  it('renders events with type, name and error/cause', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getAllByTestId('step-functions-history-event')).toHaveLength(3),
    );

    const types = screen.getAllByTestId('step-functions-history-event-type');
    expect(types[0]).toHaveTextContent('ExecutionStarted');
    expect(types[1]).toHaveTextContent('TaskStateExited');
    expect(types[2]).toHaveTextContent('ExecutionFailed');

    expect(screen.getByTestId('step-functions-history-event-name')).toHaveTextContent('DoWork');
    expect(screen.getByTestId('step-functions-history-event-error')).toHaveTextContent(
      'States.Timeout: It timed out',
    );
    expect(getExecutionHistoryMock).toHaveBeenCalledWith(executionArn, expect.any(AbortSignal));
  });

  it('renders the error without a cause when none is provided', async () => {
    getExecutionHistoryMock.mockResolvedValue({
      events: [
        {
          id: 1,
          previousEventId: null,
          type: 'ExecutionFailed',
          timestamp: '2024-01-01T00:00:00+00:00',
          name: null,
          input: null,
          output: null,
          error: 'States.Timeout',
          cause: null,
        },
      ],
    });

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-history-event-error')).toHaveTextContent(
        'States.Timeout',
      ),
    );
    expect(screen.getByTestId('step-functions-history-event-error')).not.toHaveTextContent(':');
  });

  it('reports the resolved terminal status from the history events', async () => {
    const onResolvedStatus = vi.fn();

    render(
      <StepFunctionsExecutionHistoryPanel
        executionArn={executionArn}
        onResolvedStatus={onResolvedStatus}
      />,
    );

    await waitFor(() => expect(onResolvedStatus).toHaveBeenCalledWith('FAILED'));
  });

  it('does not report a status when no terminal event is present', async () => {
    getExecutionHistoryMock.mockResolvedValue({
      events: [
        {
          id: 1,
          previousEventId: null,
          type: 'ExecutionStarted',
          timestamp: '2024-01-01T00:00:00+00:00',
          name: null,
          input: null,
          output: null,
          error: null,
          cause: null,
        },
      ],
    });
    const onResolvedStatus = vi.fn();

    render(
      <StepFunctionsExecutionHistoryPanel
        executionArn={executionArn}
        onResolvedStatus={onResolvedStatus}
      />,
    );

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-history-event')).toBeInTheDocument(),
    );
    expect(onResolvedStatus).not.toHaveBeenCalled();
  });
});
