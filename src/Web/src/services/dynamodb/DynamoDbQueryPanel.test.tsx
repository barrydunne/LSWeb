import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { DynamoDbQueryPanel } from './DynamoDbQueryPanel';
import { queryDynamoDbTable } from '../../api/client';

vi.mock('../../api/client');

const queryDynamoDbTableMock = vi.mocked(queryDynamoDbTable);

function renderPanel(indexNames: string[] = ['gsi-status']) {
  return render(<DynamoDbQueryPanel tableName="orders" indexNames={indexNames} />);
}

function fillPartition() {
  fireEvent.change(screen.getByTestId('dynamodb-query-partition-attr'), {
    target: { value: 'pk' },
  });
  fireEvent.change(screen.getByTestId('dynamodb-query-partition-value'), {
    target: { value: 'a' },
  });
}

const runButton = () => screen.getByTestId('dynamodb-query-run');

describe('DynamoDbQueryPanel', () => {
  beforeEach(() => {
    queryDynamoDbTableMock.mockResolvedValue({ items: [], nextToken: null });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders in query mode with partition fields and no sort fields by default', () => {
    renderPanel();

    expect(screen.getByTestId('dynamodb-query-panel')).toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-query-partition')).toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-query-sort-toggle')).toBeInTheDocument();
    expect(screen.queryByTestId('dynamodb-query-sort')).not.toBeInTheDocument();
  });

  it('hides key conditions in scan mode and restores them in query mode', () => {
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-query-mode-scan'));
    expect(screen.queryByTestId('dynamodb-query-partition')).not.toBeInTheDocument();
    expect(screen.queryByTestId('dynamodb-query-sort-toggle')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('dynamodb-query-mode-query'));
    expect(screen.getByTestId('dynamodb-query-partition')).toBeInTheDocument();
  });

  it('sends a query with the selected index and partition key', async () => {
    queryDynamoDbTableMock.mockResolvedValue({
      items: [{ json: '{"pk":"a"}' }],
      nextToken: null,
    });
    renderPanel(['gsi-status']);

    fireEvent.change(screen.getByTestId('dynamodb-query-index'), {
      target: { value: 'gsi-status' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-query-partition-type'), {
      target: { value: 'N' },
    });
    fillPartition();
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-result-0')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('dynamodb-query-result-json-0')).toHaveTextContent('{"pk":"a"}');
    expect(queryDynamoDbTableMock).toHaveBeenCalledWith('orders', {
      indexName: 'gsi-status',
      scan: false,
      partitionKey: { attributeName: 'pk', operator: '=', valueType: 'N', value: 'a', secondValue: null },
      sortKey: null,
      filters: [],
      limit: 25,
      startToken: null,
    });
    expect(screen.queryByTestId('dynamodb-query-load-more')).not.toBeInTheDocument();
  });

  it('shows an empty state when no items match', async () => {
    queryDynamoDbTableMock.mockResolvedValue({ items: [], nextToken: null });
    renderPanel();

    fillPartition();
    fireEvent.click(runButton());

    await waitFor(() => expect(screen.getByTestId('dynamodb-query-empty')).toBeInTheDocument());
  });

  it('shows an error state when the query fails', async () => {
    queryDynamoDbTableMock.mockRejectedValue(new Error('boom'));
    renderPanel();

    fillPartition();
    fireEvent.click(runButton());

    await waitFor(() => expect(screen.getByTestId('dynamodb-query-error')).toBeInTheDocument());
  });

  it('shows a running state while the query is in flight', () => {
    queryDynamoDbTableMock.mockReturnValue(new Promise(() => {}));
    renderPanel();

    fillPartition();
    fireEvent.click(runButton());

    expect(screen.getByTestId('dynamodb-query-loading')).toBeInTheDocument();
    expect(runButton()).toBeDisabled();
    expect(runButton()).toHaveTextContent('Running');
  });

  it('includes a between sort condition with a second value', async () => {
    queryDynamoDbTableMock.mockResolvedValue({ items: [{ json: '{}' }], nextToken: null });
    renderPanel();

    fillPartition();
    fireEvent.click(screen.getByTestId('dynamodb-query-sort-toggle'));
    expect(screen.getByTestId('dynamodb-query-sort')).toBeInTheDocument();

    fireEvent.change(screen.getByTestId('dynamodb-query-sort-attr'), { target: { value: 'sk' } });
    fireEvent.change(screen.getByTestId('dynamodb-query-sort-operator'), {
      target: { value: 'between' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-query-sort-type'), { target: { value: 'N' } });
    fireEvent.change(screen.getByTestId('dynamodb-query-sort-value'), { target: { value: '1' } });
    expect(screen.getByTestId('dynamodb-query-sort-second')).toBeInTheDocument();
    fireEvent.change(screen.getByTestId('dynamodb-query-sort-second'), { target: { value: '9' } });
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-result-0')).toBeInTheDocument(),
    );
    expect(queryDynamoDbTableMock).toHaveBeenCalledWith(
      'orders',
      expect.objectContaining({
        sortKey: {
          attributeName: 'sk',
          operator: 'between',
          valueType: 'N',
          value: '1',
          secondValue: '9',
        },
      }),
    );
  });

  it('omits the second value for a non-between sort condition and can be turned off', async () => {
    queryDynamoDbTableMock.mockResolvedValue({ items: [{ json: '{}' }], nextToken: null });
    renderPanel();

    fillPartition();
    fireEvent.click(screen.getByTestId('dynamodb-query-sort-toggle'));
    fireEvent.change(screen.getByTestId('dynamodb-query-sort-operator'), {
      target: { value: 'begins_with' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-query-sort-attr'), { target: { value: 'sk' } });
    fireEvent.change(screen.getByTestId('dynamodb-query-sort-value'), { target: { value: 'p' } });
    expect(screen.queryByTestId('dynamodb-query-sort-second')).not.toBeInTheDocument();
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-result-0')).toBeInTheDocument(),
    );
    expect(queryDynamoDbTableMock).toHaveBeenCalledWith(
      'orders',
      expect.objectContaining({
        sortKey: {
          attributeName: 'sk',
          operator: 'begins_with',
          valueType: 'S',
          value: 'p',
          secondValue: null,
        },
      }),
    );

    fireEvent.click(screen.getByTestId('dynamodb-query-sort-toggle'));
    expect(screen.queryByTestId('dynamodb-query-sort')).not.toBeInTheDocument();
  });

  it('adds, edits and removes filter rows before running', async () => {
    queryDynamoDbTableMock.mockResolvedValue({ items: [{ json: '{}' }], nextToken: null });
    renderPanel();

    fillPartition();
    fireEvent.click(screen.getByTestId('dynamodb-query-filter-add'));
    fireEvent.click(screen.getByTestId('dynamodb-query-filter-add'));

    fireEvent.change(screen.getByTestId('dynamodb-query-filter-attr-0'), {
      target: { value: 'name' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-query-filter-operator-0'), {
      target: { value: 'contains' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-query-filter-type-0'), {
      target: { value: 'S' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-query-filter-value-0'), {
      target: { value: 'abc' },
    });

    fireEvent.change(screen.getByTestId('dynamodb-query-filter-operator-1'), {
      target: { value: 'between' },
    });
    expect(screen.getByTestId('dynamodb-query-filter-second-1')).toBeInTheDocument();
    fireEvent.change(screen.getByTestId('dynamodb-query-filter-attr-1'), {
      target: { value: 'age' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-query-filter-value-1'), {
      target: { value: '1' },
    });
    fireEvent.change(screen.getByTestId('dynamodb-query-filter-second-1'), {
      target: { value: '9' },
    });

    fireEvent.click(screen.getByTestId('dynamodb-query-filter-remove-0'));
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-result-0')).toBeInTheDocument(),
    );
    expect(queryDynamoDbTableMock).toHaveBeenCalledWith(
      'orders',
      expect.objectContaining({
        filters: [
          { attributeName: 'age', operator: 'between', valueType: 'S', value: '1', secondValue: '9' },
        ],
      }),
    );
  });

  it('loads more items using the next token and hides the control when exhausted', async () => {
    queryDynamoDbTableMock
      .mockResolvedValueOnce({ items: [{ json: '{"n":1}' }], nextToken: 'tok' })
      .mockResolvedValueOnce({ items: [{ json: '{"n":2}' }], nextToken: null });
    renderPanel();

    fillPartition();
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-result-0')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('dynamodb-query-load-more')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('dynamodb-query-load-more'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-result-1')).toBeInTheDocument(),
    );
    expect(queryDynamoDbTableMock.mock.calls[1][1].startToken).toBe('tok');
    expect(queryDynamoDbTableMock.mock.calls[1][1].scan).toBe(false);
    expect(screen.queryByTestId('dynamodb-query-load-more')).not.toBeInTheDocument();
  });

  it('shows a load-more error when fetching the next page fails', async () => {
    queryDynamoDbTableMock
      .mockResolvedValueOnce({ items: [{ json: '{}' }], nextToken: 'tok' })
      .mockRejectedValueOnce(new Error('boom'));
    renderPanel();

    fillPartition();
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-load-more')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('dynamodb-query-load-more'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-load-more-error')).toBeInTheDocument(),
    );
  });

  it('disables the load-more control while the next page is loading', async () => {
    queryDynamoDbTableMock
      .mockResolvedValueOnce({ items: [{ json: '{}' }], nextToken: 'tok' })
      .mockReturnValueOnce(new Promise(() => {}));
    renderPanel();

    fillPartition();
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-load-more')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('dynamodb-query-load-more'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-load-more')).toHaveTextContent('Loading'),
    );
    expect(screen.getByTestId('dynamodb-query-load-more')).toBeDisabled();
  });

  it('sends a scan request without key conditions', async () => {
    queryDynamoDbTableMock.mockResolvedValue({ items: [{ json: '{}' }], nextToken: null });
    renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-query-mode-scan'));
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-query-result-0')).toBeInTheDocument(),
    );
    expect(queryDynamoDbTableMock).toHaveBeenCalledWith('orders', {
      indexName: null,
      scan: true,
      partitionKey: null,
      sortKey: null,
      filters: [],
      limit: 25,
      startToken: null,
    });
  });
});
