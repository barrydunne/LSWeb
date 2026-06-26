import { useEffect, useState, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { createDynamoDbTable, deleteDynamoDbTable, getDynamoDbTables } from '../../api/client';
import type { DynamoDbTableItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  marginBottom: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const columns: DataListColumn[] = [
  { key: 'name', label: 'Table' },
  { key: 'actions', label: 'Actions' },
];

const scalarTypes = ['S', 'N', 'B'];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; tables: DynamoDbTableItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

export function DynamoDbListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [tableName, setTableName] = useState('');
  const [partitionKeyName, setPartitionKeyName] = useState('');
  const [partitionKeyType, setPartitionKeyType] = useState('S');
  const [sortKeyName, setSortKeyName] = useState('');
  const [sortKeyType, setSortKeyType] = useState('S');
  const [billingMode, setBillingMode] = useState('PAY_PER_REQUEST');
  const [readCapacityUnits, setReadCapacityUnits] = useState('5');
  const [writeCapacityUnits, setWriteCapacityUnits] = useState('5');
  const [createState, setCreateState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getDynamoDbTables(controller.signal)
      .then((result) => setState({ kind: 'ready', tables: result.tables }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const isProvisioned = billingMode === 'PROVISIONED';

  const handleCreate = () => {
    setCreateState('saving');
    const trimmedSortKey = sortKeyName.trim();
    createDynamoDbTable({
      tableName,
      partitionKeyName,
      partitionKeyType,
      sortKeyName: trimmedSortKey === '' ? null : trimmedSortKey,
      sortKeyType: trimmedSortKey === '' ? null : sortKeyType,
      billingMode,
      readCapacityUnits: isProvisioned ? Number(readCapacityUnits) : null,
      writeCapacityUnits: isProvisioned ? Number(writeCapacityUnits) : null,
    })
      .then(() => {
        setCreateState('created');
        setTableName('');
        setPartitionKeyName('');
        setSortKeyName('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (name: string) => {
      deleteDynamoDbTable(name)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="dynamodb-list-loading" style={messageStyle}>
        Loading tables&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="dynamodb-list-error" style={messageStyle}>
        Unable to load DynamoDB tables.
      </p>
    );
  }

  const rows: DataListRow[] = state.tables.map((table) => ({
    id: table.name,
    filterText: table.name,
    cells: {
      name: (
        <Link
          data-testid="dynamodb-list-link"
          to={`/services/${serviceKey}/${encodeURIComponent(table.name)}`}
        >
          {table.name}
        </Link>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${table.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(table.name)}
        />
      ),
    },
  }));

  return (
    <div data-testid="dynamodb-list-view">
      <button
        type="button"
        data-testid="dynamodb-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create table'}
      </button>
      {showCreate ? (
        <div data-testid="dynamodb-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="dynamodb-create-name">
              Table name
            </label>
            <input
              id="dynamodb-create-name"
              type="text"
              data-testid="dynamodb-create-name"
              style={inputStyle}
              value={tableName}
              onChange={(event) => setTableName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="dynamodb-create-partition-name">
              Partition key name
            </label>
            <input
              id="dynamodb-create-partition-name"
              type="text"
              data-testid="dynamodb-create-partition-name"
              style={inputStyle}
              value={partitionKeyName}
              onChange={(event) => setPartitionKeyName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="dynamodb-create-partition-type">
              Partition key type
            </label>
            <select
              id="dynamodb-create-partition-type"
              data-testid="dynamodb-create-partition-type"
              style={inputStyle}
              value={partitionKeyType}
              onChange={(event) => setPartitionKeyType(event.target.value)}
            >
              {scalarTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="dynamodb-create-sort-name">
              Sort key name (optional)
            </label>
            <input
              id="dynamodb-create-sort-name"
              type="text"
              data-testid="dynamodb-create-sort-name"
              style={inputStyle}
              value={sortKeyName}
              onChange={(event) => setSortKeyName(event.target.value)}
            />
          </div>
          {sortKeyName.trim() !== '' ? (
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="dynamodb-create-sort-type">
                Sort key type
              </label>
              <select
                id="dynamodb-create-sort-type"
                data-testid="dynamodb-create-sort-type"
                style={inputStyle}
                value={sortKeyType}
                onChange={(event) => setSortKeyType(event.target.value)}
              >
                {scalarTypes.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </div>
          ) : null}
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="dynamodb-create-billing">
              Billing mode
            </label>
            <select
              id="dynamodb-create-billing"
              data-testid="dynamodb-create-billing"
              style={inputStyle}
              value={billingMode}
              onChange={(event) => setBillingMode(event.target.value)}
            >
              <option value="PAY_PER_REQUEST">PAY_PER_REQUEST</option>
              <option value="PROVISIONED">PROVISIONED</option>
            </select>
          </div>
          {isProvisioned ? (
            <>
              <div style={fieldRowStyle}>
                <label style={labelStyle} htmlFor="dynamodb-create-rcu">
                  Read capacity units
                </label>
                <input
                  id="dynamodb-create-rcu"
                  type="number"
                  min={1}
                  data-testid="dynamodb-create-rcu"
                  style={inputStyle}
                  value={readCapacityUnits}
                  onChange={(event) => setReadCapacityUnits(event.target.value)}
                />
              </div>
              <div style={fieldRowStyle}>
                <label style={labelStyle} htmlFor="dynamodb-create-wcu">
                  Write capacity units
                </label>
                <input
                  id="dynamodb-create-wcu"
                  type="number"
                  min={1}
                  data-testid="dynamodb-create-wcu"
                  style={inputStyle}
                  value={writeCapacityUnits}
                  onChange={(event) => setWriteCapacityUnits(event.target.value)}
                />
              </div>
            </>
          ) : null}
          <button
            type="button"
            data-testid="dynamodb-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="dynamodb-create-status" style={messageStyle}>
          Table created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="dynamodb-create-error" style={messageStyle}>
          Unable to create the table.
        </p>
      ) : null}
      <DataListShell
        title="Tables"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter tables"
        columnPrefsKey="dynamodb-tables"
        emptyState={{ message: 'No DynamoDB tables found on this backend.' }}
      />
    </div>
  );
}

export default DynamoDbListView;
