import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { CloudFormationListView } from './CloudFormationListView';
import { createStack, deleteStack, getStacks, validateTemplate } from '../../api/client';
import type { CloudFormationStackListResult } from '../../api/client';

vi.mock('../../api/client');

const getStacksMock = vi.mocked(getStacks);
const createStackMock = vi.mocked(createStack);
const deleteStackMock = vi.mocked(deleteStack);
const validateTemplateMock = vi.mocked(validateTemplate);

const listResult: CloudFormationStackListResult = {
  stacks: [
    {
      stackName: 'orders-stack',
      stackId: 'arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc',
      stackStatus: 'CREATE_COMPLETE',
      description: 'Orders processing stack',
      creationTime: '2024-01-01T00:00:00+00:00',
      lastUpdatedTime: '2024-02-01T00:00:00+00:00',
    },
    {
      stackName: 'billing-stack',
      stackId: 'arn:aws:cloudformation:eu-west-1:000000000000:stack/billing-stack/def',
      stackStatus: 'UPDATE_COMPLETE',
      description: null,
      creationTime: '2024-03-01T00:00:00+00:00',
      lastUpdatedTime: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <CloudFormationListView serviceKey="cloudformation" />
    </MemoryRouter>,
  );
}

describe('CloudFormationListView', () => {
  beforeEach(() => {
    getStacksMock.mockResolvedValue(listResult);
    createStackMock.mockResolvedValue({ stackId: 'arn:stack/new' });
    deleteStackMock.mockResolvedValue();
    validateTemplateMock.mockResolvedValue({
      description: 'An example template',
      capabilitiesReason: 'Requires IAM',
      capabilities: ['CAPABILITY_IAM'],
      parameters: [
        { parameterKey: 'Env', defaultValue: 'dev', noEcho: false, description: 'Environment' },
      ],
    });
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before stacks arrive', () => {
    getStacksMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('cloudformation-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getStacksMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-error')).toBeInTheDocument(),
    );
  });

  it('renders a row per stack', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('data-list-row-orders-stack')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-billing-stack')).toBeInTheDocument();
  });

  it('shows the name and status for each stack and a dash for missing updated time', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    const names = screen.getAllByTestId('cloudformation-list-name');
    const statuses = screen.getAllByTestId('cloudformation-list-status');
    expect(names[0]).toHaveTextContent('orders-stack');
    expect(statuses[0]).toHaveTextContent('CREATE_COMPLETE');
    expect(screen.getByText('\u2014')).toBeInTheDocument();
  });

  it('links each stack name to its name-keyed detail view', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    const names = screen.getAllByTestId('cloudformation-list-name');
    expect(names[0]).toHaveAttribute('href', '/services/cloudformation/orders-stack');
  });

  it('reloads the stacks when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
      );
      expect(getStacksMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getStacksMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('toggles the create form open and closed', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('cloudformation-create-form')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    expect(screen.getByTestId('cloudformation-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    expect(screen.queryByTestId('cloudformation-create-form')).not.toBeInTheDocument();
  });

  it('creates a stack with the entered name, template, parameters and capabilities', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    fireEvent.change(screen.getByTestId('cloudformation-create-stackName'), {
      target: { value: 'new-stack' },
    });
    fireEvent.change(screen.getByTestId('cloudformation-create-templateBody'), {
      target: { value: '{"Resources":{}}' },
    });
    fireEvent.click(screen.getByTestId('cloudformation-create-parameter-add'));
    fireEvent.change(screen.getByTestId('cloudformation-create-parameter-key-0'), {
      target: { value: 'Env' },
    });
    fireEvent.change(screen.getByTestId('cloudformation-create-parameter-value-0'), {
      target: { value: 'dev' },
    });
    fireEvent.click(screen.getByTestId('cloudformation-create-capability-CAPABILITY_IAM'));

    fireEvent.click(screen.getByTestId('cloudformation-create-submit'));

    await waitFor(() => expect(createStackMock).toHaveBeenCalledTimes(1));
    expect(createStackMock).toHaveBeenCalledWith(
      'new-stack',
      '{"Resources":{}}',
      null,
      [{ parameterKey: 'Env', parameterValue: 'dev' }],
      ['CAPABILITY_IAM'],
    );
    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-create-status')).toBeInTheDocument(),
    );
    expect(getStacksMock).toHaveBeenCalledTimes(2);
  });

  it('drops blank parameter rows before submitting', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    fireEvent.change(screen.getByTestId('cloudformation-create-stackName'), {
      target: { value: 'new-stack' },
    });
    fireEvent.change(screen.getByTestId('cloudformation-create-templateBody'), {
      target: { value: '{}' },
    });
    fireEvent.click(screen.getByTestId('cloudformation-create-parameter-add'));

    fireEvent.click(screen.getByTestId('cloudformation-create-submit'));

    await waitFor(() => expect(createStackMock).toHaveBeenCalledTimes(1));
    expect(createStackMock).toHaveBeenCalledWith('new-stack', '{}', null, [], []);
    await waitFor(() => expect(getStacksMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when creating a stack fails', async () => {
    createStackMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    fireEvent.change(screen.getByTestId('cloudformation-create-stackName'), {
      target: { value: 'new-stack' },
    });
    fireEvent.change(screen.getByTestId('cloudformation-create-templateBody'), {
      target: { value: '{}' },
    });
    fireEvent.click(screen.getByTestId('cloudformation-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-create-error')).toBeInTheDocument(),
    );
  });

  it('creates a stack from an S3 template URL when that source is selected', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    fireEvent.change(screen.getByTestId('cloudformation-create-stackName'), {
      target: { value: 'new-stack' },
    });
    fireEvent.change(screen.getByTestId('cloudformation-create-template-source'), {
      target: { value: 'url' },
    });
    fireEvent.change(screen.getByTestId('cloudformation-create-templateUrl'), {
      target: { value: 'https://example.s3.amazonaws.com/t.json' },
    });

    fireEvent.click(screen.getByTestId('cloudformation-create-submit'));

    await waitFor(() => expect(createStackMock).toHaveBeenCalledTimes(1));
    expect(createStackMock).toHaveBeenCalledWith(
      'new-stack',
      null,
      'https://example.s3.amazonaws.com/t.json',
      [],
      [],
    );
  });

  it('validates an inline template and shows the result', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    fireEvent.change(screen.getByTestId('cloudformation-create-templateBody'), {
      target: { value: '{"Resources":{}}' },
    });

    fireEvent.click(screen.getByTestId('cloudformation-create-validate'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-validate-result')).toBeInTheDocument(),
    );
    expect(validateTemplateMock).toHaveBeenCalledWith('{"Resources":{}}', null);
    expect(screen.getByTestId('cloudformation-validate-description')).toHaveTextContent(
      'An example template',
    );
    expect(screen.getByTestId('cloudformation-validate-capabilities')).toHaveTextContent(
      'CAPABILITY_IAM',
    );
    expect(screen.getByTestId('cloudformation-validate-parameter')).toHaveTextContent('Env');
  });

  it('falls back to placeholder text when validation reports no description or capabilities', async () => {
    validateTemplateMock.mockResolvedValue({
      description: '',
      capabilitiesReason: '',
      capabilities: [],
      parameters: [],
    });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    fireEvent.change(screen.getByTestId('cloudformation-create-templateBody'), {
      target: { value: '{"Resources":{}}' },
    });

    fireEvent.click(screen.getByTestId('cloudformation-create-validate'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-validate-result')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('cloudformation-validate-description')).toHaveTextContent(
      'No description provided.',
    );
    expect(screen.getByTestId('cloudformation-validate-capabilities')).toHaveTextContent(
      'No additional capabilities required.',
    );
    expect(screen.queryByTestId('cloudformation-validate-parameter')).not.toBeInTheDocument();
  });

  it('shows an error when template validation fails', async () => {
    validateTemplateMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-create-toggle'));
    fireEvent.change(screen.getByTestId('cloudformation-create-templateBody'), {
      target: { value: '{"Resources":{}}' },
    });

    fireEvent.click(screen.getByTestId('cloudformation-create-validate'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-validate-error')).toBeInTheDocument(),
    );
  });

  it('deletes a stack after confirmation and reloads', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    const triggers = screen.getAllByTestId('confirm-trigger');
    fireEvent.click(triggers[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteStackMock).toHaveBeenCalledWith('orders-stack'));
    await waitFor(() => expect(getStacksMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error when deleting a stack fails', async () => {
    deleteStackMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-view')).toBeInTheDocument(),
    );

    const triggers = screen.getAllByTestId('confirm-trigger');
    fireEvent.click(triggers[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-list-error')).toBeInTheDocument(),
    );
  });
});
