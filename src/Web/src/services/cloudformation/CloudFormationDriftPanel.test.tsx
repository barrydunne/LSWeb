import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, render, screen, waitFor, fireEvent } from '@testing-library/react';
import { CloudFormationDriftPanel } from './CloudFormationDriftPanel';
import { detectStackDrift, getDriftStatus, getResourceDrifts } from '../../api/client';
import type {
  CloudFormationDriftStatusResult,
  CloudFormationResourceDriftListResult,
} from '../../api/client';

vi.mock('../../api/client');

const detectStackDriftMock = vi.mocked(detectStackDrift);
const getDriftStatusMock = vi.mocked(getDriftStatus);
const getResourceDriftsMock = vi.mocked(getResourceDrifts);

const completeStatus: CloudFormationDriftStatusResult = {
  stackDriftDetectionId: 'drift-123',
  stackId: 'arn:stack/orders-stack',
  detectionStatus: 'DETECTION_COMPLETE',
  detectionStatusReason: null,
  stackDriftStatus: 'DRIFTED',
  driftedStackResourceCount: 1,
  timestamp: '2024-01-01T00:00:00+00:00',
};

const driftedResources: CloudFormationResourceDriftListResult = {
  drifts: [
    {
      logicalResourceId: 'OrdersQueue',
      physicalResourceId: 'orders-queue',
      resourceType: 'AWS::SQS::Queue',
      driftStatus: 'MODIFIED',
      expectedProperties: '{"DelaySeconds":"0"}',
      actualProperties: '{"DelaySeconds":"30"}',
      timestamp: '2024-01-01T00:00:00+00:00',
    },
  ],
};

function renderPanel() {
  return render(<CloudFormationDriftPanel stackName="orders-stack" />);
}

describe('CloudFormationDriftPanel', () => {
  beforeEach(() => {
    detectStackDriftMock.mockResolvedValue({ stackDriftDetectionId: 'drift-123' });
    getDriftStatusMock.mockResolvedValue(completeStatus);
    getResourceDriftsMock.mockResolvedValue(driftedResources);
  });

  afterEach(() => {
    vi.resetAllMocks();
    vi.useRealTimers();
  });

  it('renders the detect button in the idle state', () => {
    renderPanel();

    expect(screen.getByTestId('cloudformation-drift-detect')).toHaveTextContent('Detect drift');
    expect(screen.queryByTestId('cloudformation-drift-status')).toBeNull();
  });

  it('detects drift and shows the overall status and per-resource drift', async () => {
    renderPanel();

    fireEvent.click(screen.getByTestId('cloudformation-drift-detect'));

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-drift-status')).toHaveTextContent('DRIFTED');
    });
    expect(detectStackDriftMock).toHaveBeenCalledWith('orders-stack');
    expect(getDriftStatusMock).toHaveBeenCalledWith('drift-123');
    expect(getResourceDriftsMock).toHaveBeenCalledWith('orders-stack');
    expect(screen.getByTestId('cloudformation-drift-detection-status')).toHaveTextContent(
      'DETECTION_COMPLETE',
    );
    expect(screen.getByTestId('cloudformation-drift-count')).toHaveTextContent('1');
    const table = screen.getByTestId('cloudformation-drift-resources');
    expect(table).toHaveTextContent('OrdersQueue');
    expect(table).toHaveTextContent('orders-queue');
    expect(table).toHaveTextContent('MODIFIED');
    expect(table).toHaveTextContent('{"DelaySeconds":"0"}');
    expect(table).toHaveTextContent('{"DelaySeconds":"30"}');
  });

  it('shows the detection status reason when present', async () => {
    getDriftStatusMock.mockResolvedValue({
      ...completeStatus,
      detectionStatusReason: 'Completed successfully',
    });
    renderPanel();

    fireEvent.click(screen.getByTestId('cloudformation-drift-detect'));

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-drift-detection-reason')).toHaveTextContent(
        'Completed successfully',
      );
    });
  });

  it('shows an empty message when no resources have drifted', async () => {
    getDriftStatusMock.mockResolvedValue({
      ...completeStatus,
      stackDriftStatus: 'IN_SYNC',
      driftedStackResourceCount: 0,
    });
    getResourceDriftsMock.mockResolvedValue({ drifts: [] });
    renderPanel();

    fireEvent.click(screen.getByTestId('cloudformation-drift-detect'));

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-drift-resources-empty')).toBeInTheDocument();
    });
    expect(screen.getByTestId('cloudformation-drift-status')).toHaveTextContent('IN_SYNC');
  });

  it('renders an em dash when a resource has no physical id or properties', async () => {
    getResourceDriftsMock.mockResolvedValue({
      drifts: [
        {
          logicalResourceId: 'OrdersQueue',
          physicalResourceId: null,
          resourceType: 'AWS::SQS::Queue',
          driftStatus: 'NOT_CHECKED',
          expectedProperties: null,
          actualProperties: null,
          timestamp: '2024-01-01T00:00:00+00:00',
        },
      ],
    });
    renderPanel();

    fireEvent.click(screen.getByTestId('cloudformation-drift-detect'));

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-drift-resources')).toBeInTheDocument();
    });
    const table = screen.getByTestId('cloudformation-drift-resources');
    expect(table).toHaveTextContent('NOT_CHECKED');
    expect(table.textContent).toContain('\u2014');
  });

  it('polls until detection completes', async () => {
    vi.useFakeTimers();
    getDriftStatusMock
      .mockResolvedValueOnce({ ...completeStatus, detectionStatus: 'DETECTION_IN_PROGRESS' })
      .mockResolvedValue(completeStatus);
    renderPanel();

    await act(async () => {
      fireEvent.click(screen.getByTestId('cloudformation-drift-detect'));
      await vi.runAllTimersAsync();
    });

    expect(getDriftStatusMock).toHaveBeenCalledTimes(2);
    expect(screen.getByTestId('cloudformation-drift-status')).toHaveTextContent('DRIFTED');
  });

  it('shows a not-supported message when detection fails', async () => {
    detectStackDriftMock.mockRejectedValue(new Error('not supported'));
    renderPanel();

    fireEvent.click(screen.getByTestId('cloudformation-drift-detect'));

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-drift-unsupported')).toBeInTheDocument();
    });
    expect(screen.queryByTestId('cloudformation-drift-status')).toBeNull();
  });
});
