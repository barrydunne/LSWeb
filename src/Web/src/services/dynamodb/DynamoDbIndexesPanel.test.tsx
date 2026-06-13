import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { DynamoDbIndexesPanel } from './DynamoDbIndexesPanel';
import { createDynamoDbIndex, deleteDynamoDbIndex } from '../../api/client';
import type { DynamoDbSecondaryIndex } from '../../api/client';

vi.mock('../../api/client');

const createDynamoDbIndexMock = vi.mocked(createDynamoDbIndex);
const deleteDynamoDbIndexMock = vi.mocked(deleteDynamoDbIndex);

const gsi: DynamoDbSecondaryIndex = {
  name: 'gsi-1',
  status: 'ACTIVE',
  keySchema: [{ attributeName: 'gpk', keyType: 'HASH' }],
};

const lsi: DynamoDbSecondaryIndex = {
  name: 'lsi-1',
  status: null,
  keySchema: [{ attributeName: 'lpk', keyType: 'RANGE' }],
};

function renderPanel(
  globalSecondaryIndexes: DynamoDbSecondaryIndex[] = [],
  localSecondaryIndexes: DynamoDbSecondaryIndex[] = [],
  onChanged = vi.fn(),
) {
  render(
    <ThemeProvider colorMode="night">
      <DynamoDbIndexesPanel
        tableName="orders"
        globalSecondaryIndexes={globalSecondaryIndexes}
        localSecondaryIndexes={localSecondaryIndexes}
        onChanged={onChanged}
      />
    </ThemeProvider>,
  );
  return onChanged;
}

function fillForm() {
  fireEvent.change(screen.getByTestId('dynamodb-index-name'), { target: { value: 'gsi-new' } });
  fireEvent.change(screen.getByTestId('dynamodb-index-pk-name'), { target: { value: 'gpk' } });
}

describe('DynamoDbIndexesPanel', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows empty messages when there are no indexes', () => {
    renderPanel();

    expect(screen.getByTestId('dynamodb-indexes-gsi-empty')).toBeInTheDocument();
    expect(screen.getByTestId('dynamodb-indexes-lsi-empty')).toBeInTheDocument();
  });

  it('lists existing GSIs and LSIs', () => {
    renderPanel([gsi, { name: 'gsi-2', status: null, keySchema: [{ attributeName: 'g2', keyType: 'HASH' }] }], [lsi]);

    expect(screen.getByTestId('dynamodb-indexes-gsi-gsi-1')).toHaveTextContent('gsi-1');
    expect(screen.getByTestId('dynamodb-indexes-gsi-gsi-1')).toHaveTextContent('ACTIVE');
    expect(screen.getByTestId('dynamodb-indexes-gsi-gsi-2')).toHaveTextContent('gsi-2');
    expect(screen.getByTestId('dynamodb-indexes-lsi-lsi-1')).toHaveTextContent('lsi-1');
  });

  it('creates a hash-only GSI and notifies the parent', async () => {
    createDynamoDbIndexMock.mockResolvedValue();
    const onChanged = renderPanel();

    fillForm();
    fireEvent.click(screen.getByTestId('dynamodb-index-create-submit'));

    await waitFor(() => expect(onChanged).toHaveBeenCalledTimes(1));
    expect(createDynamoDbIndexMock).toHaveBeenCalledWith('orders', {
      indexName: 'gsi-new',
      partitionKeyName: 'gpk',
      partitionKeyType: 'S',
      sortKeyName: null,
      sortKeyType: null,
      projectionType: 'ALL',
    });
  });

  it('creates a GSI with a sort key and chosen types', async () => {
    createDynamoDbIndexMock.mockResolvedValue();
    const onChanged = renderPanel();

    fillForm();
    fireEvent.change(screen.getByTestId('dynamodb-index-pk-type'), { target: { value: 'N' } });
    fireEvent.change(screen.getByTestId('dynamodb-index-sk-name'), { target: { value: 'gsk' } });
    fireEvent.change(screen.getByTestId('dynamodb-index-sk-type'), { target: { value: 'B' } });
    fireEvent.change(screen.getByTestId('dynamodb-index-projection'), { target: { value: 'KEYS_ONLY' } });
    fireEvent.click(screen.getByTestId('dynamodb-index-create-submit'));

    await waitFor(() => expect(onChanged).toHaveBeenCalledTimes(1));
    expect(createDynamoDbIndexMock).toHaveBeenCalledWith('orders', {
      indexName: 'gsi-new',
      partitionKeyName: 'gpk',
      partitionKeyType: 'N',
      sortKeyName: 'gsk',
      sortKeyType: 'B',
      projectionType: 'KEYS_ONLY',
    });
  });

  it('shows an error and does not call the API when required fields are blank', () => {
    const onChanged = renderPanel();

    fireEvent.click(screen.getByTestId('dynamodb-index-create-submit'));

    expect(screen.getByTestId('dynamodb-indexes-error')).toBeInTheDocument();
    expect(createDynamoDbIndexMock).not.toHaveBeenCalled();
    expect(onChanged).not.toHaveBeenCalled();
  });

  it('shows an error when the create fails', async () => {
    createDynamoDbIndexMock.mockRejectedValue(new Error('boom'));
    const onChanged = renderPanel();

    fillForm();
    fireEvent.click(screen.getByTestId('dynamodb-index-create-submit'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-indexes-error')).toBeInTheDocument());
    expect(onChanged).not.toHaveBeenCalled();
  });

  it('deletes a GSI after confirmation and notifies the parent', async () => {
    deleteDynamoDbIndexMock.mockResolvedValue();
    const onChanged = renderPanel([gsi]);

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(onChanged).toHaveBeenCalledTimes(1));
    expect(deleteDynamoDbIndexMock).toHaveBeenCalledWith('orders', 'gsi-1');
  });

  it('shows an error when the delete fails', async () => {
    deleteDynamoDbIndexMock.mockRejectedValue(new Error('boom'));
    const onChanged = renderPanel([gsi]);

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('dynamodb-indexes-error')).toBeInTheDocument());
    expect(onChanged).not.toHaveBeenCalled();
  });
});
