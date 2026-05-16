import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, within } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import {
  DataListShell,
  type DataListColumn,
  type DataListRow,
} from './DataListShell';
import { executeBulkAction, type BulkActionResult } from '../api/client';

vi.mock('../api/client', () => ({
  executeBulkAction: vi.fn(),
}));

const executeBulkActionMock = vi.mocked(executeBulkAction);

function renderShell(props: { onRefresh?: () => void; actions?: React.ReactNode } = {}) {
  return render(
    <ThemeProvider colorMode="night">
      <DataListShell title="Queues" onRefresh={props.onRefresh ?? (() => {})} actions={props.actions}>
        <div data-testid="list-body-content">rows</div>
      </DataListShell>
    </ThemeProvider>,
  );
}

describe('DataListShell', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('renders the title, body content and an auto-refresh toggle', () => {
    renderShell();

    expect(screen.getByTestId('data-list-shell-title')).toHaveTextContent('Queues');
    expect(screen.getByTestId('data-list-shell-body')).toContainElement(
      screen.getByTestId('list-body-content'),
    );
    expect(screen.getByTestId('auto-refresh-toggle')).toBeInTheDocument();
  });

  it('renders optional header actions', () => {
    renderShell({ actions: <button data-testid="custom-action">Action</button> });

    expect(screen.getByTestId('custom-action')).toBeInTheDocument();
  });

  it('drives the auto-refresh toggle with the provided onRefresh callback', () => {
    vi.useFakeTimers();
    const onRefresh = vi.fn();
    renderShell({ onRefresh });

    fireEvent.click(screen.getByTestId('auto-refresh-switch'));
    vi.advanceTimersByTime(5_000);

    expect(onRefresh).toHaveBeenCalledTimes(1);
  });

  it('renders the children when the list has items', () => {
    render(
      <ThemeProvider colorMode="night">
        <DataListShell title="Queues" onRefresh={() => {}} itemCount={2}>
          <div data-testid="rows">rows</div>
        </DataListShell>
      </ThemeProvider>,
    );

    expect(screen.getByTestId('rows')).toBeInTheDocument();
    expect(screen.queryByTestId('empty-state')).not.toBeInTheDocument();
  });

  it('renders the "no resources yet" empty state when there are zero items and no filter', () => {
    render(
      <ThemeProvider colorMode="night">
        <DataListShell title="Queues" onRefresh={() => {}} itemCount={0}>
          <div data-testid="rows">rows</div>
        </DataListShell>
      </ThemeProvider>,
    );

    expect(screen.getByTestId('empty-state')).toHaveAttribute('data-variant', 'no-resources');
    expect(screen.queryByTestId('rows')).not.toBeInTheDocument();
  });

  it('renders the "no matches" empty state when a filter is active', () => {
    render(
      <ThemeProvider colorMode="night">
        <DataListShell
          title="Queues"
          onRefresh={() => {}}
          itemCount={0}
          hasActiveFilter
          emptyState={{ message: 'Nothing matches "demo".' }}
        >
          <div data-testid="rows">rows</div>
        </DataListShell>
      </ThemeProvider>,
    );

    expect(screen.getByTestId('empty-state')).toHaveAttribute('data-variant', 'no-matches');
    expect(screen.getByTestId('empty-state-message')).toHaveTextContent('Nothing matches "demo".');
  });
});

const managedColumns: DataListColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'status', label: 'Status' },
];

const managedRows: DataListRow[] = [
  { id: 'q1', cells: { name: 'orders', status: 'Active' } },
  { id: 'q2', cells: { name: 'billing', status: 'Idle' } },
];

function renderManaged(
  overrides: Partial<React.ComponentProps<typeof DataListShell>> = {},
) {
  return render(
    <ThemeProvider colorMode="night">
      <DataListShell
        title="Queues"
        onRefresh={() => {}}
        columns={managedColumns}
        rows={managedRows}
        bulkActions={[{ key: 'delete', label: 'Delete' }]}
        {...overrides}
      />
    </ThemeProvider>,
  );
}

function bulkResult(): BulkActionResult {
  return {
    operationId: 'op-1',
    action: 'delete',
    totalCount: 2,
    succeededCount: 1,
    failedCount: 1,
    overallState: 'Failed',
    items: [
      { resourceId: 'q1', succeeded: true, error: null },
      { resourceId: 'q2', succeeded: false, error: 'in use' },
    ],
  };
}

describe('DataListShell managed list', () => {
  beforeEach(() => {
    executeBulkActionMock.mockReset();
    window.localStorage.clear();
  });

  it('renders a table with rows and the configured columns', () => {
    renderManaged();

    expect(screen.getByTestId('data-list-table')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-q1')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-q2')).toBeInTheDocument();
    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
  });

  it('filters rows client-side using cell values', () => {
    renderManaged();

    fireEvent.change(screen.getByTestId('data-list-filter'), { target: { value: 'orders' } });

    expect(screen.getByTestId('data-list-row-q1')).toBeInTheDocument();
    expect(screen.queryByTestId('data-list-row-q2')).not.toBeInTheDocument();
  });

  it('filters rows using explicit filterText when provided', () => {
    const rows: DataListRow[] = [
      { id: 'r1', cells: { name: <span>alpha</span> }, filterText: 'searchable-alpha' },
      { id: 'r2', cells: { name: <span>beta</span> }, filterText: 'other' },
    ];
    renderManaged({ rows, columns: [{ key: 'name', label: 'Name' }] });

    fireEvent.change(screen.getByTestId('data-list-filter'), { target: { value: 'searchable' } });

    expect(screen.getByTestId('data-list-row-r1')).toBeInTheDocument();
    expect(screen.queryByTestId('data-list-row-r2')).not.toBeInTheDocument();
  });

  it('shows the no-matches empty state when the filter excludes everything', () => {
    renderManaged({ emptyState: { message: 'No matches here.' } });

    fireEvent.change(screen.getByTestId('data-list-filter'), { target: { value: 'zzz' } });

    expect(screen.queryByTestId('data-list-table')).not.toBeInTheDocument();
    expect(screen.getByTestId('empty-state-message')).toHaveTextContent('No matches here.');
  });

  it('shows the no-resources empty state when there are no rows', () => {
    renderManaged({ rows: [] });

    expect(screen.getByTestId('empty-state')).toBeInTheDocument();
    expect(screen.queryByTestId('data-list-filter')).not.toBeInTheDocument();
  });

  it('selects rows and toggles the bulk-action bar', () => {
    renderManaged();

    expect(screen.queryByTestId('data-list-bulk-bar')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('data-list-select-q1'));
    expect(screen.getByTestId('data-list-bulk-count')).toHaveTextContent('1 selected');

    fireEvent.click(screen.getByTestId('data-list-select-q1'));
    expect(screen.queryByTestId('data-list-bulk-bar')).not.toBeInTheDocument();
  });

  it('selects and deselects all rows with the header checkbox', () => {
    renderManaged();

    fireEvent.click(screen.getByTestId('data-list-select-all'));
    expect(screen.getByTestId('data-list-bulk-count')).toHaveTextContent('2 selected');

    fireEvent.click(screen.getByTestId('data-list-select-all'));
    expect(screen.queryByTestId('data-list-bulk-bar')).not.toBeInTheDocument();
  });

  it('clears the selection with the clear button', () => {
    renderManaged();

    fireEvent.click(screen.getByTestId('data-list-select-q1'));
    fireEvent.click(screen.getByTestId('data-list-bulk-clear'));

    expect(screen.queryByTestId('data-list-bulk-bar')).not.toBeInTheDocument();
  });

  it('runs a bulk action and renders the per-item results', async () => {
    executeBulkActionMock.mockResolvedValue(bulkResult());
    const onComplete = vi.fn();
    renderManaged({ onBulkActionComplete: onComplete });

    fireEvent.click(screen.getByTestId('data-list-select-all'));
    fireEvent.click(screen.getByTestId('data-list-bulk-action-delete'));

    await screen.findByTestId('data-list-bulk-result');

    expect(executeBulkActionMock).toHaveBeenCalledWith('delete', ['q1', 'q2']);
    expect(screen.getByTestId('data-list-bulk-result-item-0')).toHaveTextContent('q1: Succeeded');
    expect(screen.getByTestId('data-list-bulk-result-item-1')).toHaveTextContent('q2: Failed');
    expect(screen.getByTestId('data-list-bulk-result-item-1')).toHaveTextContent('in use');
    expect(onComplete).toHaveBeenCalledWith(expect.objectContaining({ failedCount: 1 }));
  });

  it('runs a bulk action without an onBulkActionComplete callback', async () => {
    executeBulkActionMock.mockResolvedValue(bulkResult());
    renderManaged();

    fireEvent.click(screen.getByTestId('data-list-select-q1'));
    fireEvent.click(screen.getByTestId('data-list-bulk-action-delete'));

    await screen.findByTestId('data-list-bulk-result');
    expect(executeBulkActionMock).toHaveBeenCalledWith('delete', ['q1']);
  });

  it('shows an error when the bulk action fails', async () => {
    executeBulkActionMock.mockRejectedValue(new Error('boom'));
    renderManaged();

    fireEvent.click(screen.getByTestId('data-list-select-q1'));
    fireEvent.click(screen.getByTestId('data-list-bulk-action-delete'));

    const error = await screen.findByTestId('data-list-bulk-error');
    expect(error).toHaveTextContent('Could not run "Delete".');
  });

  it('toggles column visibility from the columns menu', () => {
    renderManaged();

    expect(screen.queryByTestId('data-list-columns-menu')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('data-list-columns-toggle'));
    expect(screen.getByTestId('data-list-columns-menu')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('data-list-column-visible-status'));

    const table = screen.getByTestId('data-list-table');
    expect(within(table).queryByText('Status')).not.toBeInTheDocument();
    expect(within(table).getByText('Name')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('data-list-columns-toggle'));
    expect(screen.queryByTestId('data-list-columns-menu')).not.toBeInTheDocument();
  });

  it('reorders columns and persists preferences to localStorage', () => {
    renderManaged({ columnPrefsKey: 'queues' });

    fireEvent.click(screen.getByTestId('data-list-columns-toggle'));

    // No-op boundaries: cannot move the first column up or the last column down.
    fireEvent.click(screen.getByTestId('data-list-column-up-name'));
    fireEvent.click(screen.getByTestId('data-list-column-down-status'));

    // Valid move: push the first column down.
    fireEvent.click(screen.getByTestId('data-list-column-down-name'));

    const stored = window.localStorage.getItem('localstackweb:columns:queues');
    expect(stored).not.toBeNull();
    const parsed = JSON.parse(stored as string) as { key: string }[];
    expect(parsed.map((pref) => pref.key)).toEqual(['status', 'name']);
  });

  it('restores saved column preferences from localStorage', () => {
    window.localStorage.setItem(
      'localstackweb:columns:queues',
      JSON.stringify([
        { key: 'status', visible: true },
        { key: 'name', visible: false },
      ]),
    );

    renderManaged({ columnPrefsKey: 'queues' });

    // name is hidden by the stored preference, status remains visible.
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.queryByText('Name')).not.toBeInTheDocument();
  });

  it('ignores stored preferences when the column set has changed', () => {
    window.localStorage.setItem(
      'localstackweb:columns:queues',
      JSON.stringify([{ key: 'name', visible: false }]),
    );

    renderManaged({ columnPrefsKey: 'queues' });

    // Mismatched length falls back to defaults, so both columns are visible.
    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
  });

  it('falls back to defaults when stored preferences are invalid JSON', () => {
    window.localStorage.setItem('localstackweb:columns:queues', 'not-json');

    renderManaged({ columnPrefsKey: 'queues' });

    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
  });

  it('keeps rendering when persisting preferences throws', () => {
    const setItemSpy = vi
      .spyOn(Storage.prototype, 'setItem')
      .mockImplementation(() => {
        throw new Error('quota exceeded');
      });

    renderManaged({ columnPrefsKey: 'queues' });

    expect(screen.getByTestId('data-list-table')).toBeInTheDocument();
    setItemSpy.mockRestore();
  });
});
