import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { DynamoDbTransactionPanel } from './DynamoDbTransactionPanel';
import { executeDynamoDbTransaction } from '../../api/client';

vi.mock('../../api/client');

const executeDynamoDbTransactionMock = vi.mocked(executeDynamoDbTransaction);

function renderPanel(tableName = 'orders') {
  render(
    <ThemeProvider colorMode="night">
      <DynamoDbTransactionPanel tableName={tableName} />
    </ThemeProvider>,
  );
}

describe('DynamoDbTransactionPanel', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('starts with a single Put action defaulting to the current table', () => {
    renderPanel();

    expect(screen.getByTestId('dynamodb-transaction-action-0')).toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-transaction-table-0')).toHaveValue('orders');
    expect(screen.getByTestId('dynamodb-transaction-operation-0')).toHaveValue('Put');
    expect(screen.getByTestId('dynamodb-transaction-remove-0')).toBeDisabled();
  });

  it('adds and removes action rows', () => {
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-transaction-add'));
    expect(screen.getByTestId('dynamodb-transaction-action-1')).toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-transaction-remove-0')).toBeEnabled();

    fireEvent.click(screen.getByTestId('dynamodb-transaction-remove-1'));
    expect(screen.queryByTestId('dynamodb-transaction-action-1')).not.toBeInTheDocument();
  });

  it('submits all actions atomically and reports success', async () => {
    executeDynamoDbTransactionMock.mockResolvedValue();
    renderPanel();

    fireEvent.change(screen.getByTestId('dynamodb-transaction-json-0'), {
      target: { value: '{"pk":{"S":"a"}}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-transaction-add'));
    fireEvent.change(screen.getByTestId('dynamodb-transaction-operation-1'), {
      target: { value: 'Delete' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-transaction-table-1'), {
      target: { value: 'other' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-transaction-json-1'), {
      target: { value: '{"pk":{"S":"b"}}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-transaction-submit'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-transaction-success')).toBeInTheDocument());
    expect(executeDynamoDbTransactionMock).toHaveBeenCalledWith([
      { operation: 'Put', tableName: 'orders', json: '{"pk":{"S":"a"}}' },
      { operation: 'Delete', tableName: 'other', json: '{"pk":{"S":"b"}}' },
    ]);
  });

  it('shows an error and does not call the API when a payload is blank', () => {
    renderPanel();

    fireEvent.change(screen.getByTestId('dynamodb-transaction-json-0'), { target: { value: '   ' } });
    fireEvent.click(screen.getByTestId('dynamodb-transaction-submit'));

    expect(screen.getByTestId('dynamodb-transaction-error')).toBeInTheDocument();
    expect(executeDynamoDbTransactionMock).not.toHaveBeenCalled();
  });

  it('shows an error and does not call the API when a table name is blank', () => {
    renderPanel();

    fireEvent.change(screen.getByTestId('dynamodb-transaction-json-0'), {
      target: { value: '{"pk":{"S":"a"}}' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-transaction-table-0'), { target: { value: '' } });
    fireEvent.click(screen.getByTestId('dynamodb-transaction-submit'));

    expect(screen.getByTestId('dynamodb-transaction-error')).toBeInTheDocument();
    expect(executeDynamoDbTransactionMock).not.toHaveBeenCalled();
  });

  it('shows an error when the transaction fails', async () => {
    executeDynamoDbTransactionMock.mockRejectedValue(new Error('cancelled'));
    renderPanel();

    fireEvent.change(screen.getByTestId('dynamodb-transaction-json-0'), {
      target: { value: '{"pk":{"S":"a"}}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-transaction-submit'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-transaction-error')).toBeInTheDocument());
  });
});
