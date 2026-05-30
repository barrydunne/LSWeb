import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LambdaEnvironmentTab } from './LambdaEnvironmentTab';
import { getLambdaEnvironment, updateLambdaEnvironment } from '../../api/client';
import type { LambdaEnvironmentResult } from '../../api/client';

vi.mock('../../api/client');

const getLambdaEnvironmentMock = vi.mocked(getLambdaEnvironment);
const updateLambdaEnvironmentMock = vi.mocked(updateLambdaEnvironment);

const environment: LambdaEnvironmentResult = {
  variables: [
    { name: 'API_KEY', value: '********', isSensitive: true },
    { name: 'REGION', value: 'eu-west-1', isSensitive: false },
  ],
  revealAllowed: true,
};

function renderTab() {
  return render(<LambdaEnvironmentTab functionName="process-orders" />);
}

describe('LambdaEnvironmentTab', () => {
  beforeEach(() => {
    getLambdaEnvironmentMock.mockResolvedValue(environment);
    updateLambdaEnvironmentMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the environment arrives', () => {
    getLambdaEnvironmentMock.mockReturnValue(new Promise(() => {}));

    renderTab();

    expect(screen.getByTestId('lambda-environment-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getLambdaEnvironmentMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-error')).toBeInTheDocument(),
    );
  });

  it('renders variables with sensitive badges and the reveal control', async () => {
    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument(),
    );

    expect(getLambdaEnvironmentMock).toHaveBeenCalledWith('process-orders', false, expect.anything());
    expect(screen.getByTestId('lambda-environment-name-0')).toHaveValue('API_KEY');
    expect(screen.getByTestId('lambda-environment-value-0')).toHaveValue('********');
    expect(screen.getByTestId('lambda-environment-sensitive-0')).toBeInTheDocument();
    expect(screen.getByTestId('lambda-environment-name-1')).toHaveValue('REGION');
    expect(screen.queryByTestId('lambda-environment-sensitive-1')).not.toBeInTheDocument();
    expect(screen.getByTestId('lambda-environment-reveal')).toHaveTextContent('Reveal values');
    expect(screen.getByTestId('raw-json-viewer')).toBeInTheDocument();
  });

  it('hides the reveal control when revealing is not allowed', async () => {
    getLambdaEnvironmentMock.mockResolvedValue({
      variables: [{ name: 'REGION', value: 'eu-west-1', isSensitive: false }],
      revealAllowed: false,
    });

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('lambda-environment-reveal')).not.toBeInTheDocument();
  });

  it('reveals and hides every sensitive value with the global control', async () => {
    const user = userEvent.setup();
    getLambdaEnvironmentMock.mockResolvedValueOnce(environment).mockResolvedValueOnce({
      variables: [
        { name: 'API_KEY', value: 'super-secret', isSensitive: true },
        { name: 'REGION', value: 'eu-west-1', isSensitive: false },
      ],
      revealAllowed: true,
    });

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('lambda-environment-reveal'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-value-0')).toHaveValue('super-secret'),
    );
    expect(getLambdaEnvironmentMock).toHaveBeenLastCalledWith('process-orders', true);
    expect(screen.getByTestId('lambda-environment-reveal')).toHaveTextContent('Hide values');

    await user.click(screen.getByTestId('lambda-environment-reveal'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-value-0')).toHaveValue('********'),
    );
    expect(screen.getByTestId('lambda-environment-reveal')).toHaveTextContent('Reveal values');
  });

  it('reveals only the clicked row when its sensitive badge is clicked', async () => {
    const user = userEvent.setup();
    getLambdaEnvironmentMock
      .mockResolvedValueOnce({
        variables: [
          { name: 'API_KEY', value: '********', isSensitive: true },
          { name: 'DB_PASSWORD', value: '********', isSensitive: true },
          { name: 'REGION', value: 'eu-west-1', isSensitive: false },
        ],
        revealAllowed: true,
      })
      .mockResolvedValueOnce({
        variables: [
          { name: 'API_KEY', value: 'super-secret', isSensitive: true },
          { name: 'DB_PASSWORD', value: 'hunter2', isSensitive: true },
          { name: 'REGION', value: 'eu-west-1', isSensitive: false },
        ],
        revealAllowed: true,
      });

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('lambda-environment-sensitive-0'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-value-0')).toHaveValue('super-secret'),
    );
    // The other sensitive row stays masked.
    expect(screen.getByTestId('lambda-environment-value-1')).toHaveValue('********');
    expect(getLambdaEnvironmentMock).toHaveBeenLastCalledWith('process-orders', true);
    expect(screen.getByTestId('lambda-environment-sensitive-0')).toHaveTextContent('Sensitive \u00b7 hide');
    expect(screen.getByTestId('lambda-environment-sensitive-1')).toHaveTextContent('Sensitive \u00b7 reveal');

    // Clicking again hides only that row, without another fetch.
    getLambdaEnvironmentMock.mockClear();
    await user.click(screen.getByTestId('lambda-environment-sensitive-0'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-value-0')).toHaveValue('********'),
    );
    expect(getLambdaEnvironmentMock).not.toHaveBeenCalled();

    // Revealing a different row reuses the cached real values (no extra fetch).
    await user.click(screen.getByTestId('lambda-environment-sensitive-1'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-value-1')).toHaveValue('hunter2'),
    );
    expect(getLambdaEnvironmentMock).not.toHaveBeenCalled();
  });

  it('renders the sensitive badge as a static indicator when revealing is not allowed', async () => {
    const user = userEvent.setup();
    getLambdaEnvironmentMock.mockResolvedValue({
      variables: [{ name: 'API_KEY', value: '********', isSensitive: true }],
      revealAllowed: false,
    });

    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument(),
    );

    const badge = screen.getByTestId('lambda-environment-sensitive-0');
    expect(badge.tagName).toBe('SPAN');

    getLambdaEnvironmentMock.mockClear();
    await user.click(badge);

    expect(getLambdaEnvironmentMock).not.toHaveBeenCalled();
  });

  it('adds, edits and removes variable rows', async () => {
    const user = userEvent.setup();
    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('lambda-environment-add'));
    await user.type(screen.getByTestId('lambda-environment-name-2'), 'NEW');
    await user.type(screen.getByTestId('lambda-environment-value-2'), 'value');

    expect(screen.getByTestId('lambda-environment-name-2')).toHaveValue('NEW');
    expect(screen.getByTestId('lambda-environment-value-2')).toHaveValue('value');

    await user.click(screen.getByTestId('lambda-environment-remove-1'));

    expect(screen.queryByTestId('lambda-environment-name-1')).not.toBeInTheDocument();
  });

  it('saves the variables after confirmation and shows a status message', async () => {
    const user = userEvent.setup();
    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument(),
    );

    await user.clear(screen.getByTestId('lambda-environment-value-1'));
    await user.type(screen.getByTestId('lambda-environment-value-1'), 'us-east-1');

    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-save-status')).toBeInTheDocument(),
    );
    expect(updateLambdaEnvironmentMock).toHaveBeenCalledWith('process-orders', [
      { name: 'API_KEY', value: '********' },
      { name: 'REGION', value: 'us-east-1' },
    ]);
  });

  it('shows a save error when the update fails', async () => {
    const user = userEvent.setup();
    updateLambdaEnvironmentMock.mockRejectedValue(new Error('write boom'));
    renderTab();

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-tab')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-environment-save-error')).toBeInTheDocument(),
    );
  });
});
