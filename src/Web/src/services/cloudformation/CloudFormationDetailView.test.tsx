import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { CloudFormationDetailView } from './CloudFormationDetailView';
import {
  getStack,
  getStackEvents,
  getStackResources,
  getStackTemplate,
  resolveReference,
  updateStack,
  getChangeSets,
  getExports,
} from '../../api/client';
import type {
  CloudFormationStackDetailResult,
  CloudFormationStackEventListResult,
  CloudFormationStackResourceListResult,
  CloudFormationStackTemplateResult,
} from '../../api/client';

vi.mock('../../api/client');

const getStackMock = vi.mocked(getStack);
const getStackTemplateMock = vi.mocked(getStackTemplate);
const getStackResourcesMock = vi.mocked(getStackResources);
const getStackEventsMock = vi.mocked(getStackEvents);
const resolveReferenceMock = vi.mocked(resolveReference);
const updateStackMock = vi.mocked(updateStack);
const getChangeSetsMock = vi.mocked(getChangeSets);
const getExportsMock = vi.mocked(getExports);

const jsonTemplate: CloudFormationStackTemplateResult = {
  templateBody: '{"Resources":{"Q":{"Type":"AWS::SQS::Queue"}}}',
  format: 'json',
};

const stackResources: CloudFormationStackResourceListResult = {
  resources: [
    {
      logicalResourceId: 'OrdersQueue',
      physicalResourceId: 'orders-queue',
      resourceType: 'AWS::SQS::Queue',
      resourceStatus: 'CREATE_COMPLETE',
      resourceStatusReason: null,
      lastUpdatedTime: '2024-01-01T00:00:00+00:00',
    },
    {
      logicalResourceId: 'CustomThing',
      physicalResourceId: 'custom-thing',
      resourceType: 'AWS::Custom::Thing',
      resourceStatus: 'CREATE_COMPLETE',
      resourceStatusReason: null,
      lastUpdatedTime: '2024-01-01T00:00:00+00:00',
    },
    {
      logicalResourceId: 'Pending',
      physicalResourceId: null,
      resourceType: 'AWS::Custom::Thing',
      resourceStatus: 'CREATE_IN_PROGRESS',
      resourceStatusReason: null,
      lastUpdatedTime: '2024-01-01T00:00:00+00:00',
    },
  ],
};

const stackEvents: CloudFormationStackEventListResult = {
  events: [
    {
      eventId: 'event-3',
      timestamp: '2024-01-01T00:02:00+00:00',
      logicalResourceId: 'OrdersQueue',
      physicalResourceId: 'orders-queue',
      resourceType: 'AWS::SQS::Queue',
      resourceStatus: 'CREATE_COMPLETE',
      resourceStatusReason: null,
    },
    {
      eventId: 'event-2',
      timestamp: '2024-01-01T00:01:00+00:00',
      logicalResourceId: 'OrdersQueue',
      physicalResourceId: 'orders-queue',
      resourceType: 'AWS::SQS::Queue',
      resourceStatus: 'CREATE_FAILED',
      resourceStatusReason: 'Insufficient permissions',
    },
    {
      eventId: 'event-1',
      timestamp: '2024-01-01T00:00:00+00:00',
      logicalResourceId: 'orders-stack',
      physicalResourceId: 'orders-stack',
      resourceType: 'AWS::CloudFormation::Stack',
      resourceStatus: 'CREATE_IN_PROGRESS',
      resourceStatusReason: null,
    },
  ],
};

const fullStack: CloudFormationStackDetailResult = {
  stackName: 'orders-stack',
  stackId: 'arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc',
  stackStatus: 'CREATE_COMPLETE',
  stackStatusReason: 'User initiated',
  description: 'Orders processing stack',
  creationTime: '2024-01-01T00:00:00+00:00',
  lastUpdatedTime: '2024-02-01T00:00:00+00:00',
  parameters: [{ parameterKey: 'Environment', parameterValue: 'Production' }],
  outputs: [
    {
      outputKey: 'QueueUrl',
      outputValue: 'https://sqs/orders',
      description: 'The orders queue',
      exportName: 'OrdersQueueUrl',
    },
  ],
  tags: [{ key: 'team', value: 'platform' }],
  capabilities: ['CAPABILITY_IAM'],
};

const minimalStack: CloudFormationStackDetailResult = {
  stackName: 'billing-stack',
  stackId: 'arn:aws:cloudformation:eu-west-1:000000000000:stack/billing-stack/def',
  stackStatus: 'CREATE_COMPLETE',
  stackStatusReason: null,
  description: null,
  creationTime: '2024-03-01T00:00:00+00:00',
  lastUpdatedTime: null,
  parameters: [],
  outputs: [
    {
      outputKey: 'Topic',
      outputValue: 'arn:aws:sns:topic',
      description: null,
      exportName: null,
    },
  ],
  tags: [],
  capabilities: [],
};

function renderView() {
  return render(
    <MemoryRouter>
      <CloudFormationDetailView serviceKey="cloudformation" resourceId="orders-stack" />
    </MemoryRouter>,
  );
}

describe('CloudFormationDetailView', () => {
  beforeEach(() => {
    getStackMock.mockResolvedValue(fullStack);
    getStackTemplateMock.mockResolvedValue(jsonTemplate);
    getStackResourcesMock.mockResolvedValue({ resources: [] });
    getStackEventsMock.mockResolvedValue({ events: [] });
    resolveReferenceMock.mockResolvedValue(null as never);
    updateStackMock.mockResolvedValue({ stackId: 'arn:stack/updated' });
    getChangeSetsMock.mockResolvedValue({ changeSets: [] });
    getExportsMock.mockResolvedValue({ exports: [] });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the stack arrives', () => {
    getStackMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('cloudformation-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getStackMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-error')).toBeInTheDocument(),
    );
  });

  it('requests the stack by resource id', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-view')).toBeInTheDocument(),
    );

    expect(getStackMock).toHaveBeenCalledWith('orders-stack', expect.any(AbortSignal));
  });

  it('renders the full stack detail', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('cloudformation-detail-name')).toHaveTextContent('orders-stack');
    expect(screen.getByTestId('cloudformation-detail-status')).toHaveTextContent(
      'CREATE_COMPLETE',
    );
    expect(screen.getByTestId('cloudformation-detail-status-reason')).toHaveTextContent(
      'User initiated',
    );
    expect(screen.getByTestId('cloudformation-detail-description')).toHaveTextContent(
      'Orders processing stack',
    );
    expect(screen.getByTestId('cloudformation-detail-updated')).toHaveTextContent(
      '2024-02-01T00:00:00+00:00',
    );
    expect(screen.getByTestId('cloudformation-detail-parameters')).toHaveTextContent(
      'Environment',
    );
    expect(screen.getByTestId('cloudformation-detail-outputs')).toHaveTextContent('OrdersQueueUrl');
    expect(screen.getByTestId('cloudformation-detail-tags')).toHaveTextContent('platform');
    expect(screen.getByTestId('cloudformation-detail-capabilities')).toHaveTextContent(
      'CAPABILITY_IAM',
    );
  });

  it('renders empty states and dashes when optional data is missing', async () => {
    getStackMock.mockResolvedValue(minimalStack);

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-view')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('cloudformation-detail-status-reason')).not.toBeInTheDocument();
    expect(screen.queryByTestId('cloudformation-detail-description')).not.toBeInTheDocument();
    expect(screen.getByTestId('cloudformation-detail-updated')).toHaveTextContent('\u2014');
    expect(screen.getByTestId('cloudformation-detail-parameters-empty')).toBeInTheDocument();
    expect(screen.getByTestId('cloudformation-detail-tags-empty')).toBeInTheDocument();
    expect(screen.getByTestId('cloudformation-detail-capabilities-empty')).toBeInTheDocument();
    const outputCells = screen.getAllByText('\u2014');
    expect(outputCells.length).toBeGreaterThanOrEqual(2);
    await screen.findByTestId('cloudformation-changesets-empty');
  });

  it('shows an empty outputs state when there are no outputs', async () => {
    getStackMock.mockResolvedValue({ ...minimalStack, outputs: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('cloudformation-detail-outputs-empty')).toBeInTheDocument();
  });

  it('requests and renders the formatted json template', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-template')).toBeInTheDocument(),
    );

    expect(getStackTemplateMock).toHaveBeenCalledWith('orders-stack', expect.any(AbortSignal));
    const template = screen.getByTestId('cloudformation-detail-template');
    expect(template).toHaveTextContent('"AWS::SQS::Queue"');
    expect(template.textContent).toContain('\n');
  });

  it('renders a yaml template body verbatim', async () => {
    getStackTemplateMock.mockResolvedValue({
      templateBody: 'Resources:\n  Q:\n    Type: AWS::SQS::Queue',
      format: 'yaml',
    });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-template')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('cloudformation-detail-template')).toHaveTextContent(
      'Type: AWS::SQS::Queue',
    );
  });

  it('renders an invalid json template body verbatim', async () => {
    getStackTemplateMock.mockResolvedValue({ templateBody: '{not json', format: 'json' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-template')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('cloudformation-detail-template')).toHaveTextContent('{not json');
  });

  it('shows a loading state before the template arrives', async () => {
    getStackTemplateMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-template-loading')).toBeInTheDocument(),
    );
  });

  it('shows an error state when the template request fails', async () => {
    getStackTemplateMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-template-error')).toBeInTheDocument(),
    );
  });

  it('copies the raw template body to the clipboard', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    vi.stubGlobal('navigator', { clipboard: { writeText } });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-template-copy')).toBeInTheDocument(),
    );
    screen.getByTestId('cloudformation-detail-template-copy').click();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-template-copy')).toHaveTextContent('Copied'),
    );
    expect(writeText).toHaveBeenCalledWith(jsonTemplate.templateBody);
    vi.unstubAllGlobals();
  });

  it('keeps the copy label when the clipboard write fails', async () => {
    const writeText = vi.fn().mockRejectedValue(new Error('denied'));
    vi.stubGlobal('navigator', { clipboard: { writeText } });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-template-copy')).toBeInTheDocument(),
    );
    screen.getByTestId('cloudformation-detail-template-copy').click();

    await waitFor(() => expect(writeText).toHaveBeenCalled());
    expect(screen.getByTestId('cloudformation-detail-template-copy')).toHaveTextContent('Copy');
    vi.unstubAllGlobals();
  });

  it('shows a loading state before the resources arrive', async () => {
    getStackResourcesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-resources-loading')).toBeInTheDocument(),
    );
  });

  it('shows an error state when the resources request fails', async () => {
    getStackResourcesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-resources-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty resources state when there are no resources', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-resources-empty')).toBeInTheDocument(),
    );

    expect(getStackResourcesMock).toHaveBeenCalledWith('orders-stack', expect.any(AbortSignal));
  });

  it('renders the resources table with cross-links for managed types', async () => {
    getStackResourcesMock.mockResolvedValue(stackResources);
    resolveReferenceMock.mockResolvedValue({
      serviceKey: 'sqs',
      resourceId: 'orders-queue',
      route: '/services/sqs/orders-queue',
    });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-resources')).toBeInTheDocument(),
    );

    const table = screen.getByTestId('cloudformation-detail-resources');
    expect(table).toHaveTextContent('OrdersQueue');
    expect(table).toHaveTextContent('AWS::SQS::Queue');
    await waitFor(() =>
      expect(
        screen.getByRole('link', { name: 'orders-queue' }),
      ).toHaveAttribute('href', '/services/sqs/orders-queue'),
    );
    expect(resolveReferenceMock).toHaveBeenCalledWith(
      'orders-queue',
      'sqs',
      expect.any(AbortSignal),
    );
    expect(table).toHaveTextContent('custom-thing');
    expect(table).toHaveTextContent('\u2014');
  });

  it('shows a loading state before the events arrive', async () => {
    getStackEventsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-events-loading')).toBeInTheDocument(),
    );
  });

  it('shows an error state when the events request fails', async () => {
    getStackEventsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-events-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty events state when there are no events', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-events-empty')).toBeInTheDocument(),
    );

    expect(getStackEventsMock).toHaveBeenCalledWith('orders-stack', expect.any(AbortSignal));
  });

  it('renders the events timeline with statuses and reasons', async () => {
    getStackEventsMock.mockResolvedValue(stackEvents);

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-events')).toBeInTheDocument(),
    );

    const table = screen.getByTestId('cloudformation-detail-events');
    expect(table).toHaveTextContent('CREATE_COMPLETE');
    expect(table).toHaveTextContent('CREATE_FAILED');
    expect(table).toHaveTextContent('CREATE_IN_PROGRESS');
    expect(table).toHaveTextContent('Insufficient permissions');
    expect(table).toHaveTextContent('\u2014');
  });

  it('refreshes the events timeline when the refresh button is clicked', async () => {
    getStackEventsMock.mockResolvedValue({ events: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-events-empty')).toBeInTheDocument(),
    );
    expect(getStackEventsMock).toHaveBeenCalledTimes(1);

    getStackEventsMock.mockResolvedValue(stackEvents);
    fireEvent.click(screen.getByTestId('cloudformation-detail-events-refresh'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-events')).toBeInTheDocument(),
    );
    expect(getStackEventsMock).toHaveBeenCalledTimes(2);
  });

  it('toggles the update form seeded from the loaded stack', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-view')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('cloudformation-update-form')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('cloudformation-detail-update-toggle'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-update-form')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('cloudformation-update-templateBody')).toHaveValue(
      jsonTemplate.templateBody,
    );
    expect(screen.getByTestId('cloudformation-update-parameter-key-0')).toHaveValue(
      'Environment',
    );
    expect(screen.getByTestId('cloudformation-update-capability-CAPABILITY_IAM')).toBeChecked();

    fireEvent.click(screen.getByTestId('cloudformation-detail-update-toggle'));
    expect(screen.queryByTestId('cloudformation-update-form')).not.toBeInTheDocument();
  });

  it('updates the stack and refreshes the detail', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-view')).toBeInTheDocument(),
    );
    expect(getStackMock).toHaveBeenCalledTimes(1);

    fireEvent.click(screen.getByTestId('cloudformation-detail-update-toggle'));
    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-update-form')).toBeInTheDocument(),
    );
    fireEvent.change(screen.getByTestId('cloudformation-update-templateBody'), {
      target: { value: '{"Resources":{"New":{}}}' },
    });
    fireEvent.click(screen.getByTestId('cloudformation-update-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-update-status')).toBeInTheDocument(),
    );
    expect(updateStackMock).toHaveBeenCalledWith(
      'orders-stack',
      '{"Resources":{"New":{}}}',
      [{ parameterKey: 'Environment', parameterValue: 'Production' }],
      ['CAPABILITY_IAM'],
    );
    await waitFor(() => expect(getStackMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('cloudformation-update-form')).not.toBeInTheDocument();
  });

  it('shows an error when the update fails', async () => {
    updateStackMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getByTestId('cloudformation-detail-update-toggle'));
    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-update-form')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('cloudformation-update-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('cloudformation-detail-update-error')).toBeInTheDocument(),
    );
  });
});
