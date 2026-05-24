import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { DynamoDbItemsPanel } from './DynamoDbItemsPanel';
import { deleteDynamoDbItem, putDynamoDbItem, scanDynamoDbItems } from '../../api/client';
import type { DynamoDbItemListResult, DynamoDbKeyAttribute } from '../../api/client';

vi.mock('../../api/client');

const scanDynamoDbItemsMock = vi.mocked(scanDynamoDbItems);
const putDynamoDbItemMock = vi.mocked(putDynamoDbItem);
const deleteDynamoDbItemMock = vi.mocked(deleteDynamoDbItem);

const keySchema: DynamoDbKeyAttribute[] = [{ attributeName: 'id', keyType: 'HASH' }];

const twoItems: DynamoDbItemListResult = {
  items: [{ json: '{"id":"a"}' }, { json: '{"id":"b"}' }],
  truncated: false,
};

function renderPanel(schema: DynamoDbKeyAttribute[] = keySchema) {
  return render(<DynamoDbItemsPanel tableName="orders" keySchema={schema} />);
}

describe('DynamoDbItemsPanel', () => {
  beforeEach(() => {
    scanDynamoDbItemsMock.mockResolvedValue(twoItems);
    putDynamoDbItemMock.mockResolvedValue();
    deleteDynamoDbItemMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before items arrive', () => {
    scanDynamoDbItemsMock.mockReturnValue(new Promise(() => {}));

    renderPanel();

    expect(screen.getByTestId('dynamodb-items-loading')).toBeInTheDocument();
  });

  it('shows an error state when the scan fails', async () => {
    scanDynamoDbItemsMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('dynamodb-items-error')).toBeInTheDocument());
  });

  it('shows an empty state when there are no items', async () => {
    scanDynamoDbItemsMock.mockResolvedValue({ items: [], truncated: false });

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('dynamodb-items-empty')).toBeInTheDocument());
  });

  it('renders an entry for each item with edit and delete controls', async () => {
    renderPanel();

    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    expect(screen.getByTestId('dynamodb-item-json-0')).toHaveTextContent('{"id":"a"}');
    expect(screen.getByTestId('dynamodb-item-json-1')).toHaveTextContent('{"id":"b"}');
    expect(screen.getByTestId('dynamodb-item-edit-0')).toBeInTheDocument();
    expect(screen.getAllByTestId('confirm-trigger')).toHaveLength(2);
    expect(screen.queryByTestId('dynamodb-items-truncated')).not.toBeInTheDocument();
  });

  it('shows a truncation hint when more items exist', async () => {
    scanDynamoDbItemsMock.mockResolvedValue({ items: [{ json: '{"id":"a"}' }], truncated: true });

    renderPanel();

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-items-truncated')).toBeInTheDocument(),
    );
  });

  it('toggles the add-item editor open and closed', async () => {
    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    const toggle = screen.getByTestId('dynamodb-item-add-toggle');
    expect(screen.queryByTestId('dynamodb-item-editor')).not.toBeInTheDocument();
    expect(toggle).toHaveTextContent('Add item');

    fireEvent.click(toggle);
    expect(screen.getByTestId('dynamodb-item-editor')).toBeInTheDocument();
    expect(toggle).toHaveTextContent('Cancel');

    fireEvent.click(toggle);
    expect(screen.queryByTestId('dynamodb-item-editor')).not.toBeInTheDocument();
    expect(toggle).toHaveTextContent('Add item');
  });

  it('closes the editor with its cancel button', async () => {
    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('dynamodb-item-add-toggle'));
    fireEvent.click(screen.getByTestId('dynamodb-item-editor-cancel'));

    expect(screen.queryByTestId('dynamodb-item-editor')).not.toBeInTheDocument();
  });

  it('creates a new item and refreshes the list', async () => {
    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('dynamodb-item-add-toggle'));
    fireEvent.change(screen.getByTestId('dynamodb-item-editor-input'), {
      target: { value: '{"id":"new"}' },
    });
    fireEvent.click(screen.getByTestId('dynamodb-item-editor-save'));

    await waitFor(() =>
      expect(putDynamoDbItemMock).toHaveBeenCalledWith('orders', '{"id":"new"}'),
    );
    await waitFor(() =>
      expect(screen.queryByTestId('dynamodb-item-editor')).not.toBeInTheDocument(),
    );
    await waitFor(() => expect(scanDynamoDbItemsMock).toHaveBeenCalledTimes(2));
  });

  it('shows an editor error when saving fails', async () => {
    putDynamoDbItemMock.mockRejectedValue(new Error('boom'));

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('dynamodb-item-add-toggle'));
    fireEvent.click(screen.getByTestId('dynamodb-item-editor-save'));

    await waitFor(() =>
      expect(screen.getByTestId('dynamodb-item-editor-error')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('dynamodb-item-editor')).toBeInTheDocument();
  });

  it('disables the save button while a save is in flight', async () => {
    putDynamoDbItemMock.mockReturnValue(new Promise(() => {}));

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('dynamodb-item-add-toggle'));
    fireEvent.click(screen.getByTestId('dynamodb-item-editor-save'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-item-editor-save')).toBeDisabled());
    expect(screen.getByTestId('dynamodb-item-editor-save')).toHaveTextContent('Saving');
  });

  it('prefills the editor when editing an existing item', async () => {
    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('dynamodb-item-edit-0'));

    const input = screen.getByTestId('dynamodb-item-editor-input') as HTMLTextAreaElement;
    expect(input.value).toBe('{"id":"a"}');

    fireEvent.change(input, { target: { value: '{"id":"a","n":1}' } });
    fireEvent.click(screen.getByTestId('dynamodb-item-editor-save'));

    await waitFor(() =>
      expect(putDynamoDbItemMock).toHaveBeenCalledWith('orders', '{"id":"a","n":1}'),
    );
  });

  it('deletes an item using its extracted key and refreshes', async () => {
    scanDynamoDbItemsMock.mockResolvedValue({
      items: [{ json: '{"id":"abc","name":"x"}' }],
      truncated: false,
    });

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteDynamoDbItemMock).toHaveBeenCalledWith('orders', '{"id":"abc"}'),
    );
    await waitFor(() => expect(scanDynamoDbItemsMock).toHaveBeenCalledTimes(2));
  });

  it('shows an error state when a delete fails', async () => {
    scanDynamoDbItemsMock.mockResolvedValue({
      items: [{ json: '{"id":"abc"}' }],
      truncated: false,
    });
    deleteDynamoDbItemMock.mockRejectedValue(new Error('boom'));

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-items-error')).toBeInTheDocument());
  });

  it('shows an error and skips the request when the item key cannot be parsed', async () => {
    scanDynamoDbItemsMock.mockResolvedValue({
      items: [{ json: 'not-json' }],
      truncated: false,
    });

    renderPanel();
    await waitFor(() => expect(screen.getByTestId('dynamodb-item-0')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-items-error')).toBeInTheDocument());
    expect(deleteDynamoDbItemMock).not.toHaveBeenCalled();
  });
});
