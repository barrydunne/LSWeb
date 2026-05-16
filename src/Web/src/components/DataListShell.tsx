import { type CSSProperties, type ReactNode, useEffect, useState } from 'react';
import { Heading, Text } from '@primer/react';
import { AutoRefreshToggle } from './AutoRefreshToggle';
import { EmptyState } from './EmptyState';
import { executeBulkAction, type BulkActionResult } from '../api/client';

export interface DataListShellEmptyState {
  message?: string;
  cliHint?: string;
  action?: ReactNode;
}

export interface DataListColumn {
  key: string;
  label: string;
}

export interface DataListRow {
  id: string;
  cells: Record<string, ReactNode>;
  filterText?: string;
}

export interface DataListBulkAction {
  key: string;
  label: string;
}

interface ColumnPref {
  key: string;
  visible: boolean;
}

export interface DataListShellProps {
  title: string;
  onRefresh: () => void;
  actions?: ReactNode;
  children?: ReactNode;
  itemCount?: number;
  hasActiveFilter?: boolean;
  emptyState?: DataListShellEmptyState;
  columns?: DataListColumn[];
  rows?: DataListRow[];
  bulkActions?: DataListBulkAction[];
  columnPrefsKey?: string;
  filterPlaceholder?: string;
  onBulkActionComplete?: (result: BulkActionResult) => void;
}

const shellStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
};

const controlsStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
};

const toolbarStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
  position: 'relative',
};

const filterInputStyle: CSSProperties = {
  padding: '6px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  fontSize: 14,
  flex: 1,
  minWidth: 160,
};

const secondaryButtonStyle: CSSProperties = {
  padding: '6px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: '#c9d1d9',
  fontSize: 13,
  cursor: 'pointer',
};

const columnMenuStyle: CSSProperties = {
  position: 'absolute',
  top: '100%',
  right: 0,
  marginTop: 6,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  zIndex: 20,
  minWidth: 220,
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
};

const columnRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 8,
};

const moveButtonStyle: CSSProperties = {
  padding: '2px 8px',
  borderRadius: 4,
  border: '1px solid #30363d',
  background: '#21262d',
  color: '#c9d1d9',
  fontSize: 12,
  cursor: 'pointer',
};

const tableStyle: CSSProperties = {
  width: '100%',
  borderCollapse: 'collapse',
  fontSize: 14,
};

const thStyle: CSSProperties = {
  textAlign: 'left',
  padding: '8px 10px',
  borderBottom: '1px solid #30363d',
  color: '#8b949e',
  fontWeight: 600,
};

const tdStyle: CSSProperties = {
  padding: '8px 10px',
  borderBottom: '1px solid #21262d',
  color: '#c9d1d9',
};

const bulkBarStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
  padding: '8px 12px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
};

const bulkActionButtonStyle: CSSProperties = {
  padding: '6px 12px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: '#c9d1d9',
  fontSize: 13,
  cursor: 'pointer',
};

const bulkResultStyle: CSSProperties = {
  marginTop: 10,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const bulkErrorStyle: CSSProperties = {
  marginTop: 10,
  color: '#f85149',
  fontSize: 13,
};

function storageKey(key: string): string {
  return `localstackweb:columns:${key}`;
}

function loadColumnState(columns: DataListColumn[], prefsKey?: string): ColumnPref[] {
  const defaults = columns.map((column) => ({ key: column.key, visible: true }));
  if (!prefsKey) {
    return defaults;
  }
  try {
    const raw = window.localStorage.getItem(storageKey(prefsKey));
    if (!raw) {
      return defaults;
    }
    const parsed = JSON.parse(raw) as ColumnPref[];
    const stored = parsed.filter((pref) => columns.some((column) => column.key === pref.key));
    if (stored.length !== columns.length) {
      return defaults;
    }
    return stored;
  } catch {
    return defaults;
  }
}

function rowMatchText(row: DataListRow): string {
  if (row.filterText !== undefined) {
    return row.filterText;
  }
  return Object.values(row.cells)
    .filter((value): value is string => typeof value === 'string')
    .join(' ');
}

export function DataListShell({
  title,
  onRefresh,
  actions,
  children,
  itemCount,
  hasActiveFilter,
  emptyState,
  columns,
  rows = [],
  bulkActions = [],
  columnPrefsKey,
  filterPlaceholder = 'Filter\u2026',
  onBulkActionComplete,
}: DataListShellProps) {
  const [filter, setFilter] = useState('');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [columnState, setColumnState] = useState<ColumnPref[]>(() =>
    loadColumnState(columns ?? [], columnPrefsKey),
  );
  const [showColumnMenu, setShowColumnMenu] = useState(false);
  const [bulkBusy, setBulkBusy] = useState(false);
  const [bulkResult, setBulkResult] = useState<BulkActionResult | null>(null);
  const [bulkError, setBulkError] = useState<string | null>(null);

  const emptyStateConfig = emptyState ?? {};

  useEffect(() => {
    if (!columnPrefsKey) {
      return;
    }
    try {
      window.localStorage.setItem(storageKey(columnPrefsKey), JSON.stringify(columnState));
    } catch {
      // Persisting column preferences is best-effort.
    }
  }, [columnPrefsKey, columnState]);

  const trimmedFilter = filter.trim().toLowerCase();
  const filteredRows =
    trimmedFilter.length === 0
      ? rows
      : rows.filter((row) => rowMatchText(row).toLowerCase().includes(trimmedFilter));
  const filteredIds = filteredRows.map((row) => row.id);
  const allSelected = filteredIds.length > 0 && filteredIds.every((id) => selectedIds.has(id));

  function toggleRow(id: string) {
    setSelectedIds((previous) => {
      const next = new Set(previous);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  function toggleSelectAll() {
    setSelectedIds((previous) => {
      const next = new Set(previous);
      if (allSelected) {
        filteredIds.forEach((id) => next.delete(id));
      } else {
        filteredIds.forEach((id) => next.add(id));
      }
      return next;
    });
  }

  function clearSelection() {
    setSelectedIds(new Set());
  }

  function toggleColumnVisibility(key: string) {
    setColumnState((previous) =>
      previous.map((pref) => (pref.key === key ? { ...pref, visible: !pref.visible } : pref)),
    );
  }

  function moveColumn(index: number, direction: number) {
    setColumnState((previous) => {
      const target = index + direction;
      if (target < 0 || target >= previous.length) {
        return previous;
      }
      const next = [...previous];
      const moved = next[index];
      next[index] = next[target];
      next[target] = moved;
      return next;
    });
  }

  async function runBulkAction(action: DataListBulkAction) {
    setBulkBusy(true);
    setBulkError(null);
    try {
      const result = await executeBulkAction(action.key, [...selectedIds]);
      setBulkResult(result);
      onBulkActionComplete?.(result);
    } catch {
      setBulkError(`Could not run "${action.label}".`);
    } finally {
      setBulkBusy(false);
    }
  }

  function renderManagedList(activeColumns: DataListColumn[]) {
    if (rows.length === 0) {
      return (
        <EmptyState
          variant="no-resources"
          message={emptyStateConfig.message}
          cliHint={emptyStateConfig.cliHint}
          action={emptyStateConfig.action}
        />
      );
    }

    const orderedColumns = columnState
      .map((pref) => ({ pref, column: activeColumns.find((column) => column.key === pref.key) }))
      .filter(
        (entry): entry is { pref: ColumnPref; column: DataListColumn } =>
          entry.column !== undefined,
      );
    const visibleColumns = orderedColumns
      .filter((entry) => entry.pref.visible)
      .map((entry) => entry.column);

    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
        <div style={toolbarStyle}>
          <input
            data-testid="data-list-filter"
            type="search"
            aria-label="Filter list"
            placeholder={filterPlaceholder}
            value={filter}
            onChange={(event) => setFilter(event.target.value)}
            style={filterInputStyle}
          />
          <button
            type="button"
            data-testid="data-list-columns-toggle"
            onClick={() => setShowColumnMenu((open) => !open)}
            style={secondaryButtonStyle}
          >
            Columns
          </button>
          {showColumnMenu ? (
            <div data-testid="data-list-columns-menu" style={columnMenuStyle}>
              {orderedColumns.map((entry, index) => (
                <div
                  key={entry.column.key}
                  data-testid={`data-list-column-option-${entry.column.key}`}
                  style={columnRowStyle}
                >
                  <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                    <input
                      type="checkbox"
                      data-testid={`data-list-column-visible-${entry.column.key}`}
                      checked={entry.pref.visible}
                      onChange={() => toggleColumnVisibility(entry.column.key)}
                    />
                    {entry.column.label}
                  </label>
                  <span style={{ display: 'flex', gap: 4 }}>
                    <button
                      type="button"
                      data-testid={`data-list-column-up-${entry.column.key}`}
                      aria-label={`Move ${entry.column.label} up`}
                      onClick={() => moveColumn(index, -1)}
                      style={moveButtonStyle}
                    >
                      {'\u2191'}
                    </button>
                    <button
                      type="button"
                      data-testid={`data-list-column-down-${entry.column.key}`}
                      aria-label={`Move ${entry.column.label} down`}
                      onClick={() => moveColumn(index, 1)}
                      style={moveButtonStyle}
                    >
                      {'\u2193'}
                    </button>
                  </span>
                </div>
              ))}
            </div>
          ) : null}
        </div>

        {selectedIds.size > 0 ? (
          <div data-testid="data-list-bulk-bar" style={bulkBarStyle}>
            <Text data-testid="data-list-bulk-count" style={{ fontSize: 13 }}>
              {`${selectedIds.size} selected`}
            </Text>
            {bulkActions.map((action) => (
              <button
                type="button"
                key={action.key}
                data-testid={`data-list-bulk-action-${action.key}`}
                disabled={bulkBusy}
                onClick={() => runBulkAction(action)}
                style={bulkActionButtonStyle}
              >
                {action.label}
              </button>
            ))}
            <button
              type="button"
              data-testid="data-list-bulk-clear"
              onClick={clearSelection}
              style={secondaryButtonStyle}
            >
              Clear
            </button>
          </div>
        ) : null}

        {bulkError ? (
          <Text data-testid="data-list-bulk-error" style={bulkErrorStyle}>
            {bulkError}
          </Text>
        ) : null}

        {filteredRows.length === 0 ? (
          <EmptyState variant="no-matches" message={emptyStateConfig.message} />
        ) : (
          <table data-testid="data-list-table" style={tableStyle}>
            <thead>
              <tr>
                <th style={thStyle}>
                  <input
                    type="checkbox"
                    data-testid="data-list-select-all"
                    aria-label="Select all rows"
                    checked={allSelected}
                    onChange={toggleSelectAll}
                  />
                </th>
                {visibleColumns.map((column) => (
                  <th key={column.key} style={thStyle}>
                    {column.label}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {filteredRows.map((row) => (
                <tr key={row.id} data-testid={`data-list-row-${row.id}`}>
                  <td style={tdStyle}>
                    <input
                      type="checkbox"
                      data-testid={`data-list-select-${row.id}`}
                      aria-label={`Select ${row.id}`}
                      checked={selectedIds.has(row.id)}
                      onChange={() => toggleRow(row.id)}
                    />
                  </td>
                  {visibleColumns.map((column) => (
                    <td key={column.key} style={tdStyle}>
                      {row.cells[column.key]}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {bulkResult ? (
          <div data-testid="data-list-bulk-result" style={bulkResultStyle}>
            <Text style={{ fontSize: 13, fontWeight: 600 }}>
              {`"${bulkResult.action}" \u2014 ${bulkResult.succeededCount} succeeded, ${bulkResult.failedCount} failed`}
            </Text>
            <ul style={{ margin: '8px 0 0', paddingLeft: 18 }}>
              {bulkResult.items.map((item, index) => (
                <li
                  key={`${item.resourceId}-${index}`}
                  data-testid={`data-list-bulk-result-item-${index}`}
                >
                  <Text style={{ fontSize: 13, color: item.succeeded ? '#c9d1d9' : '#f85149' }}>
                    {`${item.resourceId}: ${item.succeeded ? 'Succeeded' : 'Failed'}`}
                  </Text>
                  {item.error ? (
                    <Text style={{ fontSize: 12, color: '#8b949e' }}>{` (${item.error})`}</Text>
                  ) : null}
                </li>
              ))}
            </ul>
          </div>
        ) : null}
      </div>
    );
  }

  function renderBody() {
    if (columns) {
      return renderManagedList(columns);
    }
    if (itemCount === 0) {
      return (
        <EmptyState
          variant={hasActiveFilter ? 'no-matches' : 'no-resources'}
          message={emptyStateConfig.message}
          cliHint={emptyStateConfig.cliHint}
          action={emptyStateConfig.action}
        />
      );
    }
    return children;
  }

  return (
    <section data-testid="data-list-shell" style={shellStyle}>
      <header style={headerStyle}>
        <Heading as="h2" data-testid="data-list-shell-title" style={{ fontSize: 18 }}>
          {title}
        </Heading>
        <div style={controlsStyle}>
          {actions}
          <AutoRefreshToggle onRefresh={onRefresh} />
        </div>
      </header>
      <div data-testid="data-list-shell-body">{renderBody()}</div>
    </section>
  );
}
