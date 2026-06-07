import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { SnapshotPanel } from './SnapshotPanel';
import * as client from '../api/client';

vi.mock('../api/client');
vi.mock('./NotificationCenter', () => ({
  NotificationCenter: {
    show: vi.fn(),
  },
}));

describe('SnapshotPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('renders export and import buttons', () => {
    render(<SnapshotPanel />);

    expect(screen.getByRole('button', { name: /Export Snapshot/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Import Snapshot/i })).toBeInTheDocument();
  });

  it('exports workspace snapshot on button click', async () => {
    const mockExport = vi.fn().mockResolvedValue({
      snapshotId: 'snap-123',
      exportedAt: new Date().toISOString(),
      services: ['lambda', 'sqs'],
      totalResources: 10,
    });

    vi.mocked(client.exportWorkspaceSnapshot).mockImplementation(mockExport);

    render(<SnapshotPanel />);

    const exportButton = screen.getByRole('button', { name: /Export Snapshot/i });
    await userEvent.click(exportButton);

    await waitFor(() => {
      expect(mockExport).toHaveBeenCalledOnce();
      expect(screen.getByText(/10 resource\(s\) from 2 service\(s\)/)).toBeInTheDocument();
    });
  });

  it('disables buttons while exporting', async () => {
    const mockExport = vi.fn().mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve({} as any), 100)),
    );

    vi.mocked(client.exportWorkspaceSnapshot).mockImplementation(mockExport);

    render(<SnapshotPanel />);

    const exportButton = screen.getByRole('button', { name: /Export Snapshot/i });
    const importButton = screen.getByRole('button', { name: /Import Snapshot/i });

    fireEvent.click(exportButton);

    expect(screen.getByRole('button', { name: /Exporting/i })).toBeDisabled();
    expect(importButton).toBeDisabled();
  });

  it('imports snapshot from file', async () => {
    const mockImport = vi.fn().mockResolvedValue({
      operationId: 'imp-123',
      operationType: 'Import',
      completedAt: new Date().toISOString(),
      totalResources: 5,
      successCount: 5,
      failureCount: 0,
      failures: [],
    });

    vi.mocked(client.importWorkspaceSnapshot).mockImplementation(mockImport);

    render(<SnapshotPanel />);

    const importButton = screen.getByRole('button', { name: /Import Snapshot/i });
    await userEvent.click(importButton);

    const fileInput = screen.getByRole('button', { name: /Import Snapshot/i })
      .parentElement?.querySelector('input[type="file"]') as HTMLInputElement;

    const snapshotData = JSON.stringify({
      id: 'snap-test',
      exportedAt: new Date().toISOString(),
      resources: {},
    });

    const file = new File([snapshotData], 'snapshot.json', { type: 'application/json' });

    fireEvent.change(fileInput, { target: { files: [file] } });

    await waitFor(() => {
      expect(mockImport).toHaveBeenCalledOnce();
    });
  });

  it('shows error message on export failure', async () => {
    const mockExport = vi.fn().mockRejectedValue(new Error('Export failed'));

    vi.mocked(client.exportWorkspaceSnapshot).mockImplementation(mockExport);

    render(<SnapshotPanel />);

    const exportButton = screen.getByRole('button', { name: /Export Snapshot/i });
    await userEvent.click(exportButton);

    await waitFor(() => {
      expect(screen.getByText(/Export error: Export failed/)).toBeInTheDocument();
    });
  });
});

function cleanup() {
  // Cleanup RTL
}
