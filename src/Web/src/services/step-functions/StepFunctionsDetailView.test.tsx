import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { StepFunctionsDetailView } from './StepFunctionsDetailView';
import { getExecutions, getStateMachine, resolveReference } from '../../api/client';
import type { StateMachineDetailResult } from '../../api/client';

vi.mock('../../api/client');

const getStateMachineMock = vi.mocked(getStateMachine);
const getExecutionsMock = vi.mocked(getExecutions);
const resolveReferenceMock = vi.mocked(resolveReference);

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
});
