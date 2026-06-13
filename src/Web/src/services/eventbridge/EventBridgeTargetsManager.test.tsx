import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { EventBridgeTargetsManager } from './EventBridgeTargetsManager';
import {
  getEventBridgeTargets,
  putEventBridgeRuleTargets,
  removeEventBridgeRuleTargets,
} from '../../api/client';

vi.mock('../../api/client');

const getEventBridgeTargetsMock = vi.mocked(getEventBridgeTargets);
const putEventBridgeRuleTargetsMock = vi.mocked(putEventBridgeRuleTargets);
const removeEventBridgeRuleTargetsMock = vi.mocked(removeEventBridgeRuleTargets);

function renderManager() {
  render(
    <ThemeProvider colorMode="night">
      <EventBridgeTargetsManager />
    </ThemeProvider>,
  );
}

describe('EventBridgeTargetsManager', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows an error when loading without a rule name', () => {
    renderManager();

    fireEvent.click(screen.getByTestId('eventbridge-targets-load'));

    expect(screen.getByTestId('eventbridge-targets-error')).toBeInTheDocument();
    expect(getEventBridgeTargetsMock).not.toHaveBeenCalled();
  });

  it('loads and lists existing targets for a rule', async () => {
    getEventBridgeTargetsMock.mockResolvedValue({
      targets: [{ id: 't1', arn: 'arn:aws:lambda:eu-west-1:000000000000:function:fn' }],
    });
    renderManager();

    fireEvent.change(screen.getByTestId('eventbridge-targets-rule'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-targets-load'));

    await waitFor(() => expect(screen.getByTestId('eventbridge-target-t1')).toBeInTheDocument());
    expect(getEventBridgeTargetsMock).toHaveBeenCalledWith('orders-rule');
  });

  it('shows an empty message when a rule has no targets', async () => {
    getEventBridgeTargetsMock.mockResolvedValue({ targets: [] });
    renderManager();

    fireEvent.change(screen.getByTestId('eventbridge-targets-rule'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-targets-load'));

    await waitFor(() => expect(screen.getByTestId('eventbridge-targets-empty')).toBeInTheDocument());
  });

  it('shows an error when loading targets fails', async () => {
    getEventBridgeTargetsMock.mockRejectedValue(new Error('boom'));
    renderManager();

    fireEvent.change(screen.getByTestId('eventbridge-targets-rule'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-targets-load'));

    await waitFor(() => expect(screen.getByTestId('eventbridge-targets-error')).toBeInTheDocument());
  });

  it('requires the rule, id and ARN before adding', () => {
    renderManager();

    fireEvent.click(screen.getByTestId('eventbridge-target-add'));

    expect(screen.getByTestId('eventbridge-target-form-error')).toHaveTextContent('are all required');
    expect(putEventBridgeRuleTargetsMock).not.toHaveBeenCalled();
  });

  it('validates the ARN against the selected target type', () => {
    renderManager();

    fireEvent.change(screen.getByTestId('eventbridge-targets-rule'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-target-id'), { target: { value: 't1' } });
    fireEvent.change(screen.getByTestId('eventbridge-target-arn'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:queue' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-target-add'));

    expect(screen.getByTestId('eventbridge-target-form-error')).toHaveTextContent(
      'does not look like a Lambda function ARN',
    );
    expect(putEventBridgeRuleTargetsMock).not.toHaveBeenCalled();
  });

  it('adds a target and reflects the change immediately', async () => {
    putEventBridgeRuleTargetsMock.mockResolvedValue();
    getEventBridgeTargetsMock.mockResolvedValue({
      targets: [{ id: 't1', arn: 'arn:aws:sqs:eu-west-1:000000000000:queue' }],
    });
    renderManager();

    fireEvent.change(screen.getByTestId('eventbridge-targets-rule'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-target-id'), { target: { value: 't1' } });
    fireEvent.change(screen.getByTestId('eventbridge-target-type'), { target: { value: 'sqs' } });
    fireEvent.change(screen.getByTestId('eventbridge-target-arn'), {
      target: { value: 'arn:aws:sqs:eu-west-1:000000000000:queue' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-target-add'));

    await waitFor(() =>
      expect(putEventBridgeRuleTargetsMock).toHaveBeenCalledWith('orders-rule', [
        { id: 't1', arn: 'arn:aws:sqs:eu-west-1:000000000000:queue', roleArn: null, input: null },
      ]),
    );
    await waitFor(() => expect(getEventBridgeTargetsMock).toHaveBeenCalled());
  });

  it('shows an error when adding a target fails', async () => {
    putEventBridgeRuleTargetsMock.mockRejectedValue(new Error('boom'));
    renderManager();

    fireEvent.change(screen.getByTestId('eventbridge-targets-rule'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-target-id'), { target: { value: 't1' } });
    fireEvent.change(screen.getByTestId('eventbridge-target-type'), { target: { value: 'other' } });
    fireEvent.change(screen.getByTestId('eventbridge-target-arn'), {
      target: { value: 'arn:custom:thing' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-target-add'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-target-form-error')).toHaveTextContent(
        'could not be saved',
      ),
    );
  });

  it('removes a target after confirmation', async () => {
    getEventBridgeTargetsMock.mockResolvedValue({
      targets: [{ id: 't1', arn: 'arn:aws:lambda:eu-west-1:000000000000:function:fn' }],
    });
    removeEventBridgeRuleTargetsMock.mockResolvedValue();
    renderManager();

    fireEvent.change(screen.getByTestId('eventbridge-targets-rule'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-targets-load'));
    await waitFor(() => expect(screen.getByTestId('eventbridge-target-t1')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(removeEventBridgeRuleTargetsMock).toHaveBeenCalledWith('orders-rule', ['t1']),
    );
  });

  it('shows an error when removing a target fails', async () => {
    getEventBridgeTargetsMock.mockResolvedValue({
      targets: [{ id: 't1', arn: 'arn:aws:lambda:eu-west-1:000000000000:function:fn' }],
    });
    removeEventBridgeRuleTargetsMock.mockRejectedValue(new Error('boom'));
    renderManager();

    fireEvent.change(screen.getByTestId('eventbridge-targets-rule'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-targets-load'));
    await waitFor(() => expect(screen.getByTestId('eventbridge-target-t1')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-target-form-error')).toHaveTextContent(
        'could not be removed',
      ),
    );
  });
});
