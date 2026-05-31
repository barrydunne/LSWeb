import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { StepFunctionsListView } from './StepFunctionsListView';
import { getStateMachines } from '../../api/client';
import type { StateMachineListResult } from '../../api/client';

vi.mock('../../api/client');

const getStateMachinesMock = vi.mocked(getStateMachines);

const listResult: StateMachineListResult = {
  stateMachines: [
    {
      name: 'orders-workflow',
      stateMachineArn: 'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
      type: 'STANDARD',
      creationDate: '2024-01-01T00:00:00+00:00',
    },
    {
      name: 'express-workflow',
      stateMachineArn: 'arn:aws:states:eu-west-1:000000000000:stateMachine:express-workflow',
      type: 'EXPRESS',
      creationDate: '2024-02-01T00:00:00+00:00',
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <StepFunctionsListView serviceKey="step-functions" />
    </MemoryRouter>,
  );
}

describe('StepFunctionsListView', () => {
  beforeEach(() => {
    getStateMachinesMock.mockResolvedValue(listResult);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before state machines arrive', () => {
    getStateMachinesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('step-functions-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getStateMachinesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-list-error')).toBeInTheDocument(),
    );
  });

  it('renders a row per state machine', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument(),
    );

    expect(
      screen.getByTestId(
        'data-list-row-arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByTestId(
        'data-list-row-arn:aws:states:eu-west-1:000000000000:stateMachine:express-workflow',
      ),
    ).toBeInTheDocument();
  });

  it('shows the name, type and arn for each state machine', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument(),
    );

    const names = screen.getAllByTestId('step-functions-list-name');
    const types = screen.getAllByTestId('step-functions-list-type');
    const arns = screen.getAllByTestId('step-functions-list-arn');
    expect(names[0]).toHaveTextContent('orders-workflow');
    expect(types[0]).toHaveTextContent('STANDARD');
    expect(arns[0]).toHaveTextContent(
      'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
    );
  });

  it('links each state machine name to its arn-keyed detail view', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument(),
    );

    const names = screen.getAllByTestId('step-functions-list-name');
    expect(names[0]).toHaveAttribute(
      'href',
      '/services/step-functions/arn%3Aaws%3Astates%3Aeu-west-1%3A000000000000%3AstateMachine%3Aorders-workflow',
    );
  });

  it('reloads the state machines when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument(),
      );
      expect(getStateMachinesMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getStateMachinesMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });
});
