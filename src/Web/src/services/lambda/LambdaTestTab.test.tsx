import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LambdaTestTab } from './LambdaTestTab';
import {
  deleteLambdaTestEvent,
  getLambdaTestEvents,
  invokeLambdaFunction,
  saveLambdaTestEvent,
} from '../../api/client';
import type { LambdaInvocationResult, LambdaTestEventListResult } from '../../api/client';

vi.mock('../../api/client');

const invokeLambdaFunctionMock = vi.mocked(invokeLambdaFunction);
const getLambdaTestEventsMock = vi.mocked(getLambdaTestEvents);
const saveLambdaTestEventMock = vi.mocked(saveLambdaTestEvent);
const deleteLambdaTestEventMock = vi.mocked(deleteLambdaTestEvent);

const invocation: LambdaInvocationResult = {
  statusCode: 200,
  payload: '{"ok":true}',
  logTail: 'REPORT Billed Duration: 12 ms',
  functionError: '',
  durationMs: 18,
};

const testEvents: LambdaTestEventListResult = {
  events: [{ name: 'my-event', payload: '{"saved":true}' }],
  templates: [{ name: 'Empty', payload: '{}' }],
};

function renderTab() {
  return render(<LambdaTestTab functionName="process-orders" />);
}

describe('LambdaTestTab', () => {
  beforeEach(() => {
    invokeLambdaFunctionMock.mockResolvedValue(invocation);
    getLambdaTestEventsMock.mockResolvedValue(testEvents);
    saveLambdaTestEventMock.mockResolvedValue();
    deleteLambdaTestEventMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('renders the payload editor with a default payload', async () => {
    renderTab();

    expect(screen.getByTestId('lambda-test-payload')).toHaveValue('{}');
    await waitFor(() => expect(getLambdaTestEventsMock).toHaveBeenCalled());
  });

  it('invokes the function with the edited payload and shows the result', async () => {
    const user = userEvent.setup();
    renderTab();

    const editor = screen.getByTestId('lambda-test-payload');
    await user.clear(editor);
    await user.type(editor, '{{"id":1}');

    await user.click(screen.getByTestId('lambda-test-invoke'));

    await waitFor(() => expect(screen.getByTestId('lambda-test-result-0')).toBeInTheDocument());
    expect(invokeLambdaFunctionMock).toHaveBeenCalledWith('process-orders', '{"id":1}');
    expect(screen.getByTestId('lambda-test-status-0')).toHaveTextContent('200');
    expect(screen.getByTestId('lambda-test-duration-0')).toHaveTextContent('18');
    expect(screen.getByTestId('lambda-test-logs-0')).toHaveTextContent('Billed Duration');
    expect(screen.queryByTestId('lambda-test-function-error-0')).not.toBeInTheDocument();
  });

  it('shows a function error badge when the invocation reports one', async () => {
    invokeLambdaFunctionMock.mockResolvedValue({ ...invocation, functionError: 'Unhandled' });
    const user = userEvent.setup();
    renderTab();

    await user.click(screen.getByTestId('lambda-test-invoke'));

    await waitFor(() =>
      expect(screen.getByTestId('lambda-test-function-error-0')).toHaveTextContent('Unhandled'),
    );
  });

  it('keeps a history of each invocation with the most recent first', async () => {
    const user = userEvent.setup();
    renderTab();

    await user.click(screen.getByTestId('lambda-test-invoke'));
    await waitFor(() => expect(screen.getByTestId('lambda-test-result-0')).toBeInTheDocument());

    await user.click(screen.getByTestId('lambda-test-invoke'));
    await waitFor(() => expect(screen.getByTestId('lambda-test-result-1')).toBeInTheDocument());

    const history = within(screen.getByTestId('lambda-test-history'));
    const results = history.getAllByTestId(/lambda-test-result-/);
    expect(results).toHaveLength(2);
    expect(results[0]).toHaveAttribute('data-testid', 'lambda-test-result-1');
  });

  it('shows an error message when the invocation fails', async () => {
    invokeLambdaFunctionMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderTab();

    await user.click(screen.getByTestId('lambda-test-invoke'));

    await waitFor(() => expect(screen.getByTestId('lambda-test-error')).toBeInTheDocument());
  });

  it('omits the log panel when no logs are returned', async () => {
    invokeLambdaFunctionMock.mockResolvedValue({ ...invocation, logTail: '' });
    const user = userEvent.setup();
    renderTab();

    await user.click(screen.getByTestId('lambda-test-invoke'));

    await waitFor(() => expect(screen.getByTestId('lambda-test-result-0')).toBeInTheDocument());
    expect(screen.queryByTestId('lambda-test-logs-0')).not.toBeInTheDocument();
  });

  it('loads saved events and templates on mount', async () => {
    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-test-saved-my-event')).toBeInTheDocument());
    expect(getLambdaTestEventsMock).toHaveBeenCalledWith('process-orders');
  });

  it('populates the payload and name when a saved event is selected', async () => {
    const user = userEvent.setup();
    renderTab();
    await waitFor(() => expect(screen.getByTestId('lambda-test-saved-my-event')).toBeInTheDocument());

    await user.selectOptions(screen.getByTestId('lambda-test-selector'), 'saved:my-event');

    expect(screen.getByTestId('lambda-test-payload')).toHaveValue('{"saved":true}');
    expect(screen.getByTestId('lambda-test-name')).toHaveValue('my-event');
  });

  it('populates the payload when a template is selected', async () => {
    const user = userEvent.setup();
    renderTab();
    await waitFor(() => expect(screen.getByTestId('lambda-test-selector')).toBeInTheDocument());

    const editor = screen.getByTestId('lambda-test-payload');
    await user.clear(editor);
    await user.type(editor, 'changed');
    await user.selectOptions(screen.getByTestId('lambda-test-selector'), 'template:Empty');

    expect(editor).toHaveValue('{}');
    expect(screen.getByTestId('lambda-test-name')).toHaveValue('Empty');
  });

  it('ignores the placeholder option in the selector', async () => {
    const user = userEvent.setup();
    renderTab();
    await waitFor(() => expect(screen.getByTestId('lambda-test-selector')).toBeInTheDocument());

    await user.selectOptions(screen.getByTestId('lambda-test-selector'), '');

    expect(screen.getByTestId('lambda-test-payload')).toHaveValue('{}');
    expect(screen.getByTestId('lambda-test-name')).toHaveValue('');
  });

  it('saves the current payload under the entered name and reloads the list', async () => {
    const user = userEvent.setup();
    renderTab();
    await waitFor(() => expect(screen.getByTestId('lambda-test-selector')).toBeInTheDocument());

    await user.type(screen.getByTestId('lambda-test-name'), 'new-event');
    await user.click(screen.getByTestId('lambda-test-save'));

    await waitFor(() =>
      expect(saveLambdaTestEventMock).toHaveBeenCalledWith('process-orders', 'new-event', '{}'),
    );
    expect(getLambdaTestEventsMock).toHaveBeenCalledTimes(2);
  });

  it('disables the save button when the name is blank', async () => {
    renderTab();
    await waitFor(() => expect(screen.getByTestId('lambda-test-selector')).toBeInTheDocument());

    expect(screen.getByTestId('lambda-test-save')).toBeDisabled();
  });

  it('shows an error message when saving fails', async () => {
    saveLambdaTestEventMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderTab();
    await waitFor(() => expect(screen.getByTestId('lambda-test-selector')).toBeInTheDocument());

    await user.type(screen.getByTestId('lambda-test-name'), 'new-event');
    await user.click(screen.getByTestId('lambda-test-save'));

    await waitFor(() => expect(screen.getByTestId('lambda-test-save-error')).toBeInTheDocument());
  });

  it('deletes a saved event after confirmation and reloads the list', async () => {
    const user = userEvent.setup();
    renderTab();
    await waitFor(() => expect(screen.getByTestId('lambda-test-saved-my-event')).toBeInTheDocument());

    const row = within(screen.getByTestId('lambda-test-saved-my-event'));
    await user.click(row.getByTestId('confirm-trigger'));
    await user.click(row.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteLambdaTestEventMock).toHaveBeenCalledWith('process-orders', 'my-event'),
    );
    expect(getLambdaTestEventsMock).toHaveBeenCalledTimes(2);
  });

  it('shows an error message when deleting fails', async () => {
    deleteLambdaTestEventMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderTab();
    await waitFor(() => expect(screen.getByTestId('lambda-test-saved-my-event')).toBeInTheDocument());

    const row = within(screen.getByTestId('lambda-test-saved-my-event'));
    await user.click(row.getByTestId('confirm-trigger'));
    await user.click(row.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('lambda-test-save-error')).toBeInTheDocument());
  });

  it('clears the saved list when loading events fails', async () => {
    getLambdaTestEventsMock.mockRejectedValue(new Error('boom'));
    renderTab();

    await waitFor(() => expect(getLambdaTestEventsMock).toHaveBeenCalled());
    expect(screen.queryByTestId('lambda-test-saved-list')).not.toBeInTheDocument();
  });
});
