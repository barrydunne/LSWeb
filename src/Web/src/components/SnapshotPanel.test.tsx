import { cleanup, render, screen, fireEvent, waitFor } from '@testing-library/react';
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
      () =>
        new Promise((resolve) =>
          setTimeout(
            () => resolve({} as unknown as Awaited<ReturnType<typeof client.exportWorkspaceSnapshot>>),
            100,
          ),
        ),
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
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
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

    confirmSpy.mockRestore();
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

  it('shows a generic export error when the failure is not an Error', async () => {
    const mockExport = vi.fn().mockRejectedValue('boom');
    vi.mocked(client.exportWorkspaceSnapshot).mockImplementation(mockExport);

    render(<SnapshotPanel />);

    await userEvent.click(screen.getByRole('button', { name: /Export Snapshot/i }));

    await waitFor(() => {
      expect(screen.getByText(/Export error: Failed to export snapshot/)).toBeInTheDocument();
    });
  });

  function getFileInput(): HTMLInputElement {
    return screen
      .getByRole('button', { name: /Import Snapshot/i })
      .parentElement?.querySelector('input[type="file"]') as HTMLInputElement;
  }

  it('ignores a file change when no file is selected', () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);

    render(<SnapshotPanel />);

    fireEvent.change(getFileInput(), { target: { files: [] } });

    expect(confirmSpy).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('does not import when the confirmation is cancelled', () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const mockImport = vi.fn();
    vi.mocked(client.importWorkspaceSnapshot).mockImplementation(mockImport);

    render(<SnapshotPanel />);

    const file = new File(['{}'], 'snapshot.json', { type: 'application/json' });
    fireEvent.change(getFileInput(), { target: { files: [file] } });

    expect(mockImport).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('shows the last import summary including a failure count', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const mockImport = vi.fn().mockResolvedValue({
      operationId: 'imp-9',
      operationType: 'Import',
      completedAt: new Date().toISOString(),
      totalResources: 5,
      successCount: 3,
      failureCount: 2,
      failures: [],
    });
    vi.mocked(client.importWorkspaceSnapshot).mockImplementation(mockImport);

    render(<SnapshotPanel />);

    const snapshotData = JSON.stringify({ id: 's', exportedAt: '', resources: {} });
    const file = new File([snapshotData], 'snapshot.json', { type: 'application/json' });
    fireEvent.change(getFileInput(), { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/Last import: 3 of 5 resource\(s\)/)).toBeInTheDocument();
    });
    expect(screen.getByText(/\(2 failed\)/)).toBeInTheDocument();
    confirmSpy.mockRestore();
  });

  it('shows an error message when the import request fails', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const mockImport = vi.fn().mockRejectedValue(new Error('Import boom'));
    vi.mocked(client.importWorkspaceSnapshot).mockImplementation(mockImport);

    render(<SnapshotPanel />);

    const snapshotData = JSON.stringify({ id: 's', exportedAt: '', resources: {} });
    const file = new File([snapshotData], 'snapshot.json', { type: 'application/json' });
    fireEvent.change(getFileInput(), { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/Import error: Import boom/)).toBeInTheDocument();
    });
    confirmSpy.mockRestore();
  });

  it('shows a generic import error when the failure is not an Error', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const mockImport = vi.fn().mockRejectedValue('boom');
    vi.mocked(client.importWorkspaceSnapshot).mockImplementation(mockImport);

    render(<SnapshotPanel />);

    const snapshotData = JSON.stringify({ id: 's', exportedAt: '', resources: {} });
    const file = new File([snapshotData], 'snapshot.json', { type: 'application/json' });
    fireEvent.change(getFileInput(), { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/Import error: Failed to import snapshot/)).toBeInTheDocument();
    });
    confirmSpy.mockRestore();
  });

  it('shows an error message when the file is not valid JSON', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);

    render(<SnapshotPanel />);

    const file = new File(['not-json'], 'snapshot.json', { type: 'application/json' });
    fireEvent.change(getFileInput(), { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/Import error:/)).toBeInTheDocument();
    });
    confirmSpy.mockRestore();
  });
});
