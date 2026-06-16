import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { StepFunctionsDetailView } from './StepFunctionsDetailView';
import {
  deleteStateMachine,
  getExecutions,
  getStateMachine,
  resolveReference,
  updateStateMachineDefinition,
} from '../../api/client';
import type { StateMachineDetailResult } from '../../api/client';

const navigateMock = vi.fn();

vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>();
  return { ...actual, useNavigate: () => navigateMock };
});

vi.mock('../../api/client');

const getStateMachineMock = vi.mocked(getStateMachine);
const getExecutionsMock = vi.mocked(getExecutions);
const resolveReferenceMock = vi.mocked(resolveReference);
const updateStateMachineDefinitionMock = vi.mocked(updateStateMachineDefinition);
const deleteStateMachineMock = vi.mocked(deleteStateMachine);

const stateMachineArn = 'arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow';

const detailResult: StateMachineDetailResult = {
  name: 'orders-workflow',
  stateMachineArn,
  type: 'STANDARD',
  status: 'ACTIVE',
  roleArn: 'arn:aws:iam::000000000000:role/service-role/states',
  definition: '{"StartAt":"Done","States":{"Done":{"Type":"Pass","End":true}}}',
  creationDate: '2024-01-01T00:00:00+00:00',
};

function renderView() {
  return render(
    <MemoryRouter>
      <StepFunctionsDetailView serviceKey="step-functions" resourceId={stateMachineArn} />
    </MemoryRouter>,
  );
}

describe('StepFunctionsDetailView', () => {
  beforeEach(() => {
    getStateMachineMock.mockResolvedValue(detailResult);
    getExecutionsMock.mockResolvedValue({ executions: [] });
    resolveReferenceMock.mockResolvedValue(null as never);
    updateStateMachineDefinitionMock.mockResolvedValue();
    deleteStateMachineMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before the state machine arrives', () => {
    getStateMachineMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('step-functions-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getStateMachineMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-detail-error')).toBeInTheDocument(),
    );
  });

  it('renders the state machine metadata', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('step-functions-detail-name')).toHaveTextContent('orders-workflow');
    expect(screen.getByTestId('step-functions-detail-arn')).toHaveTextContent(stateMachineArn);
    expect(screen.getByTestId('step-functions-detail-type')).toHaveTextContent('STANDARD');
    expect(screen.getByTestId('step-functions-detail-status')).toHaveTextContent('ACTIVE');
    expect(screen.getByTestId('step-functions-detail-created')).toHaveTextContent(
      '2024-01-01T00:00:00+00:00',
    );
  });

  it('renders the execution role as a resource link reference', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-detail-role')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('step-functions-detail-role')).toHaveTextContent(
      'arn:aws:iam::000000000000:role/service-role/states',
    );
  });

  it('requests the state machine identified by the resource id', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument(),
    );

    expect(getStateMachineMock).toHaveBeenCalledWith(stateMachineArn, expect.anything());
  });

  it('renders the workflow graph parsed from the ASL definition', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-detail-graph-heading')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('step-functions-graph')).toBeInTheDocument();
    expect(screen.getByTestId('step-functions-graph-node')).toHaveAttribute(
      'data-state-name',
      'Done',
    );
  });

  it('renders the raw ASL definition viewer', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-detail-definition')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('raw-json-title')).toHaveTextContent('ASL definition');
  });

  const newDefinition = '{"StartAt":"Next","States":{"Next":{"Type":"Pass","End":true}}}';

  it('edits and saves the definition', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-definition-edit'));
    fireEvent.change(screen.getByTestId('step-functions-definition-editor'), {
      target: { value: newDefinition },
    });
    fireEvent.click(screen.getByTestId('step-functions-definition-save'));

    await waitFor(() =>
      expect(updateStateMachineDefinitionMock).toHaveBeenCalledWith(stateMachineArn, newDefinition),
    );
    await waitFor(() => expect(getStateMachineMock).toHaveBeenCalledTimes(2));
  });

  it('blocks saving an invalid definition', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-definition-edit'));
    fireEvent.change(screen.getByTestId('step-functions-definition-editor'), {
      target: { value: '{"States":{}}' },
    });
    fireEvent.click(screen.getByTestId('step-functions-definition-save'));

    expect(screen.getByTestId('step-functions-definition-invalid')).toBeInTheDocument();
    expect(updateStateMachineDefinitionMock).not.toHaveBeenCalled();
  });

  it('blocks saving a definition that is not a JSON object', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-definition-edit'));
    fireEvent.change(screen.getByTestId('step-functions-definition-editor'), {
      target: { value: '[1,2,3]' },
    });
    fireEvent.click(screen.getByTestId('step-functions-definition-save'));

    expect(screen.getByTestId('step-functions-definition-invalid')).toBeInTheDocument();
    expect(updateStateMachineDefinitionMock).not.toHaveBeenCalled();
  });

  it('blocks saving malformed JSON', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-definition-edit'));
    fireEvent.change(screen.getByTestId('step-functions-definition-editor'), {
      target: { value: 'not valid json {' },
    });
    fireEvent.click(screen.getByTestId('step-functions-definition-save'));

    expect(screen.getByTestId('step-functions-definition-invalid')).toBeInTheDocument();
    expect(updateStateMachineDefinitionMock).not.toHaveBeenCalled();
  });

  it('shows an error when saving the definition fails', async () => {
    updateStateMachineDefinitionMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-definition-edit'));
    fireEvent.change(screen.getByTestId('step-functions-definition-editor'), {
      target: { value: newDefinition },
    });
    fireEvent.click(screen.getByTestId('step-functions-definition-save'));

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-definition-error')).toBeInTheDocument(),
    );
  });

  it('cancels editing without saving', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('step-functions-definition-edit'));
    expect(screen.getByTestId('step-functions-definition-editor')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('step-functions-definition-cancel'));

    expect(screen.queryByTestId('step-functions-definition-editor')).not.toBeInTheDocument();
    expect(updateStateMachineDefinitionMock).not.toHaveBeenCalled();
  });

  it('deletes the state machine after confirmation and navigates back to the list', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteStateMachineMock).toHaveBeenCalledWith(stateMachineArn));
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/services/step-functions'));
  });

  it('shows an error when deleting the state machine fails', async () => {
    deleteStateMachineMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('step-functions-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('step-functions-detail-delete-error')).toBeInTheDocument(),
    );
    expect(navigateMock).not.toHaveBeenCalled();
  });
});
