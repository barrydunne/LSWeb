import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { DynamoDbBatchPanel } from './DynamoDbBatchPanel';
import { executeDynamoDbBatchGet, executeDynamoDbBatchWrite } from '../../api/client';

vi.mock('../../api/client');

const executeDynamoDbBatchWriteMock = vi.mocked(executeDynamoDbBatchWrite);
const executeDynamoDbBatchGetMock = vi.mocked(executeDynamoDbBatchGet);

function renderPanel(tableName = 'orders') {
  render(
    <ThemeProvider colorMode="night">
      <DynamoDbBatchPanel tableName={tableName} />
    </ThemeProvider>,
  );
}

describe('DynamoDbBatchPanel', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('starts in batch write mode with one request defaulting to the current table', () => {
    renderPanel();

    expect(screen.getByTestId('dynamodb-batch-write')).toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-batch-write-table-0')).toHaveValue('orders');
    expect(screen.getByTestId('dynamodb-batch-write-remove-0')).toBeDisabled();
  });

  it('adds and removes batch write rows', () => {
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-batch-write-add'));
    expect(screen.getByTestId('dynamodb-batch-write-row-1')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('dynamodb-batch-write-remove-1'));
    expect(screen.queryByTestId('dynamodb-batch-write-row-1')).not.toBeInTheDocument();
  });

  it('runs a batch write and shows the outcome including unprocessed items', async () => {
    executeDynamoDbBatchWriteMock.mockResolvedValue({
      requested: 1,
      unprocessedItems: ['{"pk":{"S":"a"}}'],
    });
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-batch-write-add'));
    fireEvent.change(screen.getByTestId('dynamodb-batch-write-operation-0'), {
      target: { value: 'Delete' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-batch-write-table-0'), {
      target: { value: 'renamed' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-batch-write-json-0'), {
      target: { value: '{"pk":{"S":"a"}}' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-batch-write-json-1'), {
      target: { value: '{"pk":{"S":"b"}}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-batch-write-submit'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-batch-write-result')).toBeInTheDocument());
    expect(screen.getByTestId('dynamodb-batch-write-unprocessed-0')).toBeInTheDocument();
    expect(executeDynamoDbBatchWriteMock).toHaveBeenCalledWith([
      { operation: 'Delete', tableName: 'renamed', json: '{"pk":{"S":"a"}}' },
      { operation: 'Put', tableName: 'orders', json: '{"pk":{"S":"b"}}' },
    ]);
  });

  it('shows an error and does not call the API when a write payload is blank', () => {
    renderPanel();

    fireEvent.change(screen.getByTestId('dynamodb-batch-write-json-0'), { target: { value: '  ' } });
    fireEvent.click(screen.getByTestId('dynamodb-batch-write-submit'));

    expect(screen.getByTestId('dynamodb-batch-error')).toBeInTheDocument();
    expect(executeDynamoDbBatchWriteMock).not.toHaveBeenCalled();
  });

  it('shows an error when the batch write fails', async () => {
    executeDynamoDbBatchWriteMock.mockRejectedValue(new Error('boom'));
    renderPanel();

    fireEvent.change(screen.getByTestId('dynamodb-batch-write-json-0'), {
      target: { value: '{"pk":{"S":"a"}}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-batch-write-submit'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-batch-error')).toBeInTheDocument());
  });

  it('switches to batch get mode and runs a batch get with results', async () => {
    executeDynamoDbBatchGetMock.mockResolvedValue({
      requested: 1,
      items: [{ json: '{"pk":{"S":"a"}}' }],
    });
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-batch-mode-get'));
    fireEvent.click(screen.getByTestId('dynamodb-batch-get-add'));
    fireEvent.change(screen.getByTestId('dynamodb-batch-get-table-0'), {
      target: { value: 'renamed' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-batch-get-json-0'), {
      target: { value: '{"pk":{"S":"a"}}' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-batch-get-json-1'), {
      target: { value: '{"pk":{"S":"b"}}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-batch-get-submit'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-batch-get-result')).toBeInTheDocument());
    expect(screen.getByTestId('dynamodb-batch-get-item-0')).toBeInTheDocument();
    expect(executeDynamoDbBatchGetMock).toHaveBeenCalledWith([
      { tableName: 'renamed', json: '{"pk":{"S":"a"}}' },
      { tableName: 'orders', json: '{"pk":{"S":"b"}}' },
    ]);
  });

  it('adds and removes batch get rows', () => {
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-batch-mode-get'));
    fireEvent.click(screen.getByTestId('dynamodb-batch-get-add'));
    expect(screen.getByTestId('dynamodb-batch-get-row-1')).toBeInTheDocument();

    fireEvent.change(screen.getByTestId('dynamodb-batch-get-table-1'), { target: { value: 'other' } });
    expect(screen.getByTestId('dynamodb-batch-get-table-1')).toHaveValue('other');

    fireEvent.click(screen.getByTestId('dynamodb-batch-get-remove-1'));
    expect(screen.queryByTestId('dynamodb-batch-get-row-1')).not.toBeInTheDocument();
  });

  it('shows an empty message when a batch get finds no items', async () => {
    executeDynamoDbBatchGetMock.mockResolvedValue({ requested: 1, items: [] });
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-batch-mode-get'));
    fireEvent.change(screen.getByTestId('dynamodb-batch-get-json-0'), {
      target: { value: '{"pk":{"S":"missing"}}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-batch-get-submit'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-batch-get-empty')).toBeInTheDocument());
  });

  it('shows an error and does not call the API when a get key is blank', () => {
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-batch-mode-get'));
    fireEvent.change(screen.getByTestId('dynamodb-batch-get-json-0'), { target: { value: '  ' } });
    fireEvent.click(screen.getByTestId('dynamodb-batch-get-submit'));

    expect(screen.getByTestId('dynamodb-batch-error')).toBeInTheDocument();
    expect(executeDynamoDbBatchGetMock).not.toHaveBeenCalled();
  });

  it('shows an error when the batch get fails', async () => {
    executeDynamoDbBatchGetMock.mockRejectedValue(new Error('boom'));
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-batch-mode-get'));
    fireEvent.change(screen.getByTestId('dynamodb-batch-get-json-0'), {
      target: { value: '{"pk":{"S":"a"}}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-batch-get-submit'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-batch-error')).toBeInTheDocument());
  });

  it('switches back to batch write mode from batch get', () => {
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-batch-mode-get'));
    expect(screen.getByTestId('dynamodb-batch-get')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('dynamodb-batch-mode-write'));
    expect(screen.getByTestId('dynamodb-batch-write')).toBeInTheDocument();
  });
});
