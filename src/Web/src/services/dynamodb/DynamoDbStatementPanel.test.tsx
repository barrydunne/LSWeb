import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { DynamoDbStatementPanel } from './DynamoDbStatementPanel';
import { executeDynamoDbStatement } from '../../api/client';

vi.mock('../../api/client');

const executeDynamoDbStatementMock = vi.mocked(executeDynamoDbStatement);

function renderPanel(tableName = 'orders') {
  return render(<DynamoDbStatementPanel tableName={tableName} />);
}

const runButton = () => screen.getByTestId('dynamodb-statement-run');
const input = () => screen.getByTestId('dynamodb-statement-input');

describe('DynamoDbStatementPanel', () => {
  beforeEach(() => {
    executeDynamoDbStatementMock.mockResolvedValue({ items: [], nextToken: null });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders with a default statement seeded from the table name', () => {
    renderPanel();

    expect(screen.getByTestId('dynamodb-statement-panel')).toBeInTheDocument();
    expect(input()).toHaveValue('SELECT * FROM "orders"');
  });

  it('disables the run control when the statement is blank', () => {
    renderPanel();

    fireEvent.change(input(), { target: { value: '   ' } });

    expect(runButton()).toBeDisabled();
  });

  it('runs the statement and renders the returned items', async () => {
    executeDynamoDbStatementMock.mockResolvedValue({
      items: [{ json: '{"pk":"a"}' }],
      nextToken: null,
    });
    renderPanel();

    fireEvent.change(input(), { target: { value: 'SELECT * FROM "widgets"' } });
    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-statement-result-0')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('dynamodb-statement-result-json-0')).toHaveTextContent('{"pk":"a"}');
    expect(executeDynamoDbStatementMock).toHaveBeenCalledWith({
      statement: 'SELECT * FROM "widgets"',
      limit: 25,
      nextToken: null,
    });
    expect(screen.queryByTestId('dynamodb-statement-load-more')).not.toBeInTheDocument();
  });

  it('shows an empty state when the statement returns no items', async () => {
    executeDynamoDbStatementMock.mockResolvedValue({ items: [], nextToken: null });
    renderPanel();

    fireEvent.click(runButton());

    await waitFor(() => expect(screen.getByTestId('dynamodb-statement-empty')).toBeInTheDocument());
  });

  it('shows an error state when the statement fails', async () => {
    executeDynamoDbStatementMock.mockRejectedValue(new Error('boom'));
    renderPanel();

    fireEvent.click(runButton());

    await waitFor(() => expect(screen.getByTestId('dynamodb-statement-error')).toBeInTheDocument());
  });

  it('shows a running state while the statement is in flight', () => {
    executeDynamoDbStatementMock.mockReturnValue(new Promise(() => {}));
    renderPanel();

    fireEvent.click(runButton());

    expect(screen.getByTestId('dynamodb-statement-loading')).toBeInTheDocument();
    expect(runButton()).toBeDisabled();
    expect(runButton()).toHaveTextContent('Running');
  });

  it('loads more items using the next token and hides the control when exhausted', async () => {
    executeDynamoDbStatementMock
      .mockResolvedValueOnce({ items: [{ json: '{"n":1}' }], nextToken: 'tok' })
      .mockResolvedValueOnce({ items: [{ json: '{"n":2}' }], nextToken: null });
    renderPanel();

    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-statement-result-0')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('dynamodb-statement-load-more')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('dynamodb-statement-load-more'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-statement-result-1')).toBeInTheDocument(),
    );
    expect(executeDynamoDbStatementMock.mock.calls[1][0].nextToken).toBe('tok');
    expect(screen.queryByTestId('dynamodb-statement-load-more')).not.toBeInTheDocument();
  });

  it('shows a load-more error when fetching the next page fails', async () => {
    executeDynamoDbStatementMock
      .mockResolvedValueOnce({ items: [{ json: '{}' }], nextToken: 'tok' })
      .mockRejectedValueOnce(new Error('boom'));
    renderPanel();

    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-statement-load-more')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('dynamodb-statement-load-more'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-statement-load-more-error')).toBeInTheDocument(),
    );
  });

  it('disables the load-more control while the next page is loading', async () => {
    executeDynamoDbStatementMock
      .mockResolvedValueOnce({ items: [{ json: '{}' }], nextToken: 'tok' })
      .mockReturnValueOnce(new Promise(() => {}));
    renderPanel();

    fireEvent.click(runButton());

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-statement-load-more')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('dynamodb-statement-load-more'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-statement-load-more')).toHaveTextContent('Loading'),
    );
    expect(screen.getByTestId('dynamodb-statement-load-more')).toBeDisabled();
  });
});
