import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { DynamoDbListView } from './DynamoDbListView';
import { getDynamoDbTables, createDynamoDbTable, deleteDynamoDbTable } from '../../api/client';
import type { DynamoDbTableListResult } from '../../api/client';

vi.mock('../../api/client');

const getDynamoDbTablesMock = vi.mocked(getDynamoDbTables);
const createDynamoDbTableMock = vi.mocked(createDynamoDbTable);
const deleteDynamoDbTableMock = vi.mocked(deleteDynamoDbTable);

const listResult: DynamoDbTableListResult = {
  tables: [{ name: 'orders' }, { name: 'invoices' }],
};

function renderView() {
  return render(
    <MemoryRouter>
      <DynamoDbListView serviceKey="dynamodb" />
    </MemoryRouter>,
  );
}

describe('DynamoDbListView', () => {
  beforeEach(() => {
    getDynamoDbTablesMock.mockResolvedValue(listResult);
    createDynamoDbTableMock.mockResolvedValue();
    deleteDynamoDbTableMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before tables arrive', () => {
    getDynamoDbTablesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('dynamodb-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getDynamoDbTablesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('dynamodb-list-error')).toBeInTheDocument());
  });

  it('renders a row per table', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());

    const links = screen.getAllByTestId('dynamodb-list-link');
    expect(links[0]).toHaveTextContent('orders');
    expect(links[0]).toHaveAttribute('href', '/services/dynamodb/orders');
  });

  it('reloads the tables when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());
      expect(getDynamoDbTablesMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getDynamoDbTablesMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('toggles the create form', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());

    expect(screen.queryByTestId('dynamodb-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('dynamodb-create-toggle'));
    expect(screen.getByTestId('dynamodb-create-form')).toBeInTheDocument();
  });

  it('reveals the sort key type when a sort key name is entered', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('dynamodb-create-toggle'));

    expect(screen.queryByTestId('dynamodb-create-sort-type')).not.toBeInTheDocument();
    fireEvent.change(screen.getByTestId('dynamodb-create-sort-name'), {
      target: { value: 'sk' },
    });
    expect(screen.getByTestId('dynamodb-create-sort-type')).toBeInTheDocument();
  });

  it('reveals capacity fields when provisioned billing is selected', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('dynamodb-create-toggle'));

    expect(screen.queryByTestId('dynamodb-create-rcu')).not.toBeInTheDocument();
    fireEvent.change(screen.getByTestId('dynamodb-create-billing'), {
      target: { value: 'PROVISIONED' },
    });
    expect(screen.getByTestId('dynamodb-create-rcu')).toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-create-wcu')).toBeInTheDocument();
  });

  it('creates a pay-per-request table and shows a status message', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('dynamodb-create-toggle'));

    fireEvent.change(screen.getByTestId('dynamodb-create-name'), {
      target: { value: 'events' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-partition-name'), {
      target: { value: 'pk' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-create-status')).toBeInTheDocument(),
    );
    expect(createDynamoDbTableMock).toHaveBeenCalledWith({
      tableName: 'events',
      partitionKeyName: 'pk',
      partitionKeyType: 'S',
      sortKeyName: null,
      sortKeyType: null,
      billingMode: 'PAY_PER_REQUEST',
      readCapacityUnits: null,
      writeCapacityUnits: null,
    });
  });

  it('creates a provisioned table with a sort key and capacity units', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('dynamodb-create-toggle'));

    fireEvent.change(screen.getByTestId('dynamodb-create-name'), {
      target: { value: 'events' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-partition-name'), {
      target: { value: 'pk' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-partition-type'), {
      target: { value: 'N' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-sort-name'), {
      target: { value: 'sk' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-sort-type'), {
      target: { value: 'B' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-billing'), {
      target: { value: 'PROVISIONED' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-rcu'), {
      target: { value: '10' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-wcu'), {
      target: { value: '20' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-create-status')).toBeInTheDocument(),
    );
    expect(createDynamoDbTableMock).toHaveBeenCalledWith({
      tableName: 'events',
      partitionKeyName: 'pk',
      partitionKeyType: 'N',
      sortKeyName: 'sk',
      sortKeyType: 'B',
      billingMode: 'PROVISIONED',
      readCapacityUnits: 10,
      writeCapacityUnits: 20,
    });
  });

  it('shows an error when table creation fails', async () => {
    createDynamoDbTableMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('dynamodb-create-toggle'));

    fireEvent.change(screen.getByTestId('dynamodb-create-name'), {
      target: { value: 'events' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-create-partition-name'), {
      target: { value: 'pk' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-create-error')).toBeInTheDocument(),
    );
  });

  it('deletes a table after confirmation', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteDynamoDbTableMock).toHaveBeenCalledWith('orders'));
  });

  it('shows an error when table deletion fails', async () => {
    deleteDynamoDbTableMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('dynamodb-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-list-error')).toBeInTheDocument());
  });
});
