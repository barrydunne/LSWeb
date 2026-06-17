import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor, fireEvent, cleanup } from '@testing-library/react';
import { CloudFormationChangeSetPanel } from './CloudFormationChangeSetPanel';
import {
  createChangeSet,
  deleteChangeSet,
  executeChangeSet,
  getChangeSet,
  getChangeSets,
} from '../../api/client';
import type {
  CloudFormationChangeSetDetailResult,
  CloudFormationChangeSetListResult,
} from '../../api/client';

vi.mock('../../api/client');

const getChangeSetsMock = vi.mocked(getChangeSets);
const getChangeSetMock = vi.mocked(getChangeSet);
const createChangeSetMock = vi.mocked(createChangeSet);
const executeChangeSetMock = vi.mocked(executeChangeSet);
const deleteChangeSetMock = vi.mocked(deleteChangeSet);

const changeSetList: CloudFormationChangeSetListResult = {
  changeSets: [
    {
      changeSetId: 'arn:changeset/add-queue',
      changeSetName: 'add-queue',
      stackName: 'orders-stack',
      status: 'CREATE_COMPLETE',
      statusReason: null,
      executionStatus: 'AVAILABLE',
      description: 'Adds a queue',
      creationTime: '2024-01-01T00:00:00+00:00',
    },
  ],
};

const changeSetDetail: CloudFormationChangeSetDetailResult = {
  changeSetName: 'add-queue',
  changeSetId: 'arn:changeset/add-queue',
  stackName: 'orders-stack',
  stackId: 'arn:stack/orders-stack',
  status: 'CREATE_COMPLETE',
  statusReason: null,
  executionStatus: 'AVAILABLE',
  description: 'Adds a queue',
  creationTime: '2024-01-01T00:00:00+00:00',
  parameters: [{ parameterKey: 'Env', parameterValue: 'dev' }],
  capabilities: ['CAPABILITY_IAM'],
  changes: [
    {
      action: 'Add',
      logicalResourceId: 'OrdersQueue',
      physicalResourceId: null,
      resourceType: 'AWS::SQS::Queue',
      replacement: null,
    },
    {
      action: 'Modify',
      logicalResourceId: 'OrdersTopic',
      physicalResourceId: 'orders-topic',
      resourceType: 'AWS::SNS::Topic',
      replacement: 'True',
    },
  ],
};

function renderPanel() {
  return render(
    <CloudFormationChangeSetPanel
      stackName="orders-stack"
      initialTemplateBody='{"Resources":{}}'
      initialParameters={[{ parameterKey: 'Env', parameterValue: 'dev' }]}
      initialCapabilities={['CAPABILITY_IAM']}
    />,
  );
}

describe('CloudFormationChangeSetPanel', () => {
  beforeEach(() => {
    getChangeSetsMock.mockResolvedValue(changeSetList);
    getChangeSetMock.mockResolvedValue(changeSetDetail);
    createChangeSetMock.mockResolvedValue({ changeSetId: 'arn:changeset/new' });
    executeChangeSetMock.mockResolvedValue();
    deleteChangeSetMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before the change sets arrive', () => {
    getChangeSetsMock.mockReturnValue(new Promise(() => {}));

    renderPanel();

    expect(screen.getByTestId('cloudformation-changesets-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getChangeSetsMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty state when there are no change sets', async () => {
    getChangeSetsMock.mockResolvedValue({ changeSets: [] });

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-empty')).toBeInTheDocument(),
    );
  });

  it('requests and renders the change sets for the stack', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    expect(getChangeSetsMock).toHaveBeenCalledWith('orders-stack', expect.any(AbortSignal));
    expect(screen.getByTestId('cloudformation-changesets')).toHaveTextContent('add-queue');
    expect(screen.getByTestId('cloudformation-changesets')).toHaveTextContent('AVAILABLE');
  });

  it('refreshes the change sets when refresh is clicked', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-refresh'));

    await waitFor(() => expect(getChangeSetsMock).toHaveBeenCalledTimes(2));
  });

  it('toggles the create form', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('cloudformation-changesets-create')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('cloudformation-changesets-create-toggle'));
    expect(screen.getByTestId('cloudformation-changesets-create')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('cloudformation-changesets-create-toggle'));
    expect(screen.queryByTestId('cloudformation-changesets-create')).not.toBeInTheDocument();
  });

  it('creates a change set with the supplied name and type', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-create-toggle'));
    fireEvent.change(screen.getByTestId('cloudformation-changesets-name'), {
      target: { value: 'add-bucket' },
    });
    fireEvent.change(screen.getByTestId('cloudformation-changesets-type'), {
      target: { value: 'CREATE' },
    });
    fireEvent.click(screen.getByTestId('cloudformation-changeset-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-create-status')).toBeInTheDocument(),
    );

    expect(createChangeSetMock).toHaveBeenCalledWith(
      'orders-stack',
      'add-bucket',
      'CREATE',
      '{"Resources":{}}',
      [{ parameterKey: 'Env', parameterValue: 'dev' }],
      ['CAPABILITY_IAM'],
    );
    await waitFor(() => expect(getChangeSetsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('cloudformation-changesets-create')).not.toBeInTheDocument();
  });

  it('shows an error when creating a change set fails', async () => {
    createChangeSetMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-create-toggle'));
    fireEvent.click(screen.getByTestId('cloudformation-changeset-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-create-error')).toBeInTheDocument(),
    );
  });

  it('previews the resource changes for a change set', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-preview-add-queue'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-preview-table')).toBeInTheDocument(),
    );

    expect(getChangeSetMock).toHaveBeenCalledWith(
      'orders-stack',
      'add-queue',
      expect.any(AbortSignal),
    );
    const preview = screen.getByTestId('cloudformation-changesets-preview-table');
    expect(preview).toHaveTextContent('Add');
    expect(preview).toHaveTextContent('OrdersQueue');
    expect(preview).toHaveTextContent('Modify');
    expect(preview).toHaveTextContent('True');
  });

  it('hides the preview when toggled again', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-preview-add-queue'));
    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-preview-table')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-preview-add-queue'));
    expect(screen.queryByTestId('cloudformation-changesets-preview')).not.toBeInTheDocument();
  });

  it('shows an empty preview when the change set has no changes', async () => {
    getChangeSetMock.mockResolvedValue({ ...changeSetDetail, changes: [] });

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-preview-add-queue'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-preview-empty')).toBeInTheDocument(),
    );
  });

  it('shows an error when the preview fails to load', async () => {
    getChangeSetMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-preview-add-queue'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-preview-error')).toBeInTheDocument(),
    );
  });

  it('executes a change set after confirmation', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    // Open the preview first so we can assert it is cleared after execution.
    fireEvent.click(screen.getByTestId('cloudformation-changesets-preview-add-queue'));
    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-preview-table')).toBeInTheDocument(),
    );

    // The first confirmation host in the actions cell is the Execute action.
    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(executeChangeSetMock).toHaveBeenCalledWith('orders-stack', 'add-queue'));
    await waitFor(() => expect(getChangeSetsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('cloudformation-changesets-preview')).not.toBeInTheDocument();
  });

  it('shows an action error when execution fails', async () => {
    executeChangeSetMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-action-error')).toBeInTheDocument(),
    );
  });

  it('deletes a change set after confirmation', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-changesets-preview-add-queue'));
    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-preview-table')).toBeInTheDocument(),
    );

    // The second confirmation host in the actions cell is the Delete action.
    fireEvent.click(screen.getAllByTestId('confirm-trigger')[1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteChangeSetMock).toHaveBeenCalledWith('orders-stack', 'add-queue'));
    await waitFor(() => expect(getChangeSetsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('cloudformation-changesets-preview')).not.toBeInTheDocument();
  });

  it('deletes a change set without an open preview', async () => {
    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteChangeSetMock).toHaveBeenCalledWith('orders-stack', 'add-queue'));
    await waitFor(() => expect(getChangeSetsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('cloudformation-changesets-preview')).not.toBeInTheDocument();
  });

  it('shows an action error when deletion fails', async () => {
    deleteChangeSetMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[1]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-changesets-action-error')).toBeInTheDocument(),
    );
  });
});
