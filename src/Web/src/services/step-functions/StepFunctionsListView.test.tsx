import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { StepFunctionsListView } from './StepFunctionsListView';
import { createStateMachine, deleteStateMachine, getStateMachines } from '../../api/client';
import type { StateMachineListResult } from '../../api/client';

vi.mock('../../api/client');

const getStateMachinesMock = vi.mocked(getStateMachines);
const createStateMachineMock = vi.mocked(createStateMachine);
const deleteStateMachineMock = vi.mocked(deleteStateMachine);

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
    createStateMachineMock.mockResolvedValue({
      stateMachineArn: 'arn:aws:states:eu-west-1:000000000000:stateMachine:new',
      creationDate: '2024-03-01T00:00:00+00:00',
    });
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('deletes a state machine after confirmation and refreshes the list', async () => {
    deleteStateMachineMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteStateMachineMock).toHaveBeenCalledWith(
        'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow',
      ),
    );
    await waitFor(() => expect(getStateMachinesMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when state machine deletion fails', async () => {
    deleteStateMachineMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-list-error')).toBeInTheDocument(),
    );
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

  const validDefinition = JSON.stringify({
    StartAt: 'A',
    States: { A: { Type: 'Pass', End: true } },
  });

  it('creates a state machine with a valid definition', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    fireEvent.change(screen.getByTestId('step-functions-create-name'), { target: { value: 'orders' } });
    fireEvent.change(screen.getByTestId('step-functions-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/sfn' },
    });
    fireEvent.change(screen.getByTestId('step-functions-create-type'), { target: { value: 'EXPRESS' } });
    fireEvent.change(screen.getByTestId('step-functions-create-definition'), {
      target: { value: validDefinition },
    });
    fireEvent.click(screen.getByTestId('step-functions-create-submit'));

    await waitFor(() =>
      expect(createStateMachineMock).toHaveBeenCalledWith({
        name: 'orders',
        definition: validDefinition,
        roleArn: 'arn:aws:iam::000000000000:role/sfn',
        type: 'EXPRESS',
      }),
    );
    await waitFor(() => expect(screen.getByTestId('step-functions-create-status')).toBeInTheDocument());
  });

  it('blocks creation when the name or role is missing', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    fireEvent.click(screen.getByTestId('step-functions-create-submit'));

    expect(screen.getByTestId('step-functions-create-invalid')).toBeInTheDocument();
    expect(createStateMachineMock).not.toHaveBeenCalled();
  });

  it('blocks creation when the definition is not valid ASL', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    fireEvent.change(screen.getByTestId('step-functions-create-name'), { target: { value: 'orders' } });
    fireEvent.change(screen.getByTestId('step-functions-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/sfn' },
    });
    fireEvent.change(screen.getByTestId('step-functions-create-definition'), {
      target: { value: '{"States":{}}' },
    });
    fireEvent.click(screen.getByTestId('step-functions-create-submit'));

    expect(screen.getByTestId('step-functions-create-invalid')).toBeInTheDocument();
    expect(createStateMachineMock).not.toHaveBeenCalled();
  });

  it('rejects a definition that is not a JSON object', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    fireEvent.change(screen.getByTestId('step-functions-create-name'), { target: { value: 'orders' } });
    fireEvent.change(screen.getByTestId('step-functions-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/sfn' },
    });
    fireEvent.change(screen.getByTestId('step-functions-create-definition'), {
      target: { value: '[1,2,3]' },
    });
    fireEvent.click(screen.getByTestId('step-functions-create-submit'));

    expect(screen.getByTestId('step-functions-create-invalid')).toBeInTheDocument();
    expect(createStateMachineMock).not.toHaveBeenCalled();
  });

  it('rejects malformed JSON in the definition', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    fireEvent.change(screen.getByTestId('step-functions-create-name'), { target: { value: 'orders' } });
    fireEvent.change(screen.getByTestId('step-functions-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/sfn' },
    });
    fireEvent.change(screen.getByTestId('step-functions-create-definition'), {
      target: { value: 'not valid json {' },
    });
    fireEvent.click(screen.getByTestId('step-functions-create-submit'));

    expect(screen.getByTestId('step-functions-create-invalid')).toBeInTheDocument();
    expect(createStateMachineMock).not.toHaveBeenCalled();
  });

  it('rejects a definition with a non-string StartAt', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    fireEvent.change(screen.getByTestId('step-functions-create-name'), { target: { value: 'orders' } });
    fireEvent.change(screen.getByTestId('step-functions-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/sfn' },
    });
    fireEvent.change(screen.getByTestId('step-functions-create-definition'), {
      target: { value: '{"StartAt":1,"States":{"A":{}}}' },
    });
    fireEvent.click(screen.getByTestId('step-functions-create-submit'));

    expect(screen.getByTestId('step-functions-create-invalid')).toBeInTheDocument();
    expect(createStateMachineMock).not.toHaveBeenCalled();
  });

  it('shows an error when the create request fails', async () => {
    createStateMachineMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    fireEvent.change(screen.getByTestId('step-functions-create-name'), { target: { value: 'orders' } });
    fireEvent.change(screen.getByTestId('step-functions-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/sfn' },
    });
    fireEvent.change(screen.getByTestId('step-functions-create-definition'), {
      target: { value: validDefinition },
    });
    fireEvent.click(screen.getByTestId('step-functions-create-submit'));

    await waitFor(() => expect(screen.getByTestId('step-functions-create-error')).toBeInTheDocument());
  });

  it('hides the create form when toggled off', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    expect(screen.getByTestId('step-functions-create-form')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('step-functions-create-toggle'));
    expect(screen.queryByTestId('step-functions-create-form')).not.toBeInTheDocument();
  });
});
