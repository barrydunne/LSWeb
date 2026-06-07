import { useRef, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import {
  exportWorkspaceSnapshot,
  importWorkspaceSnapshot,
  type SnapshotExportResult,
  type SnapshotImportResult,
  type WorkspaceSnapshot,
} from '../api/client';
import { NotificationCenter } from './NotificationCenter';

type ExportState =
  | { kind: 'idle' }
  | { kind: 'exporting' }
  | { kind: 'error'; error: string };

type ImportState =
  | { kind: 'idle' }
  | { kind: 'importing' }
  | { kind: 'error'; error: string };

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const buttonStyle: CSSProperties = {
  padding: '8px 16px',
  borderRadius: 6,
  border: '1px solid #2ea043',
  background: '#238636',
  color: '#ffffff',
  fontSize: 14,
  cursor: 'pointer',
  fontWeight: 500,
};

const buttonDisabledStyle: CSSProperties = {
  ...buttonStyle,
  opacity: 0.6,
  cursor: 'not-allowed',
};

const buttonDangerStyle: CSSProperties = {
  ...buttonStyle,
  border: '1px solid #da3633',
  background: '#da3633',
};

const messageStyle: CSSProperties = {
  fontSize: 13,
  opacity: 0.8,
};

function snapshotFilename(): string {
  const now = new Date();
  return `workspace-snapshot-${now.toISOString().split('T')[0]}-${now.getTime()}.json`;
}

export function SnapshotPanel() {
  const [exportState, setExportState] = useState<ExportState>({ kind: 'idle' });
  const [importState, setImportState] = useState<ImportState>({ kind: 'idle' });
  const [lastExport, setLastExport] = useState<SnapshotExportResult | null>(null);
  const [lastImport, setLastImport] = useState<SnapshotImportResult | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleExport = () => {
    setExportState({ kind: 'exporting' });
    exportWorkspaceSnapshot()
      .then((result) => {
        setLastExport(result);
        setExportState({ kind: 'idle' });
        // TODO: Trigger the actual snapshot download after retrieving full snapshot data
        NotificationCenter.show({
          title: 'Snapshot Exported',
          message: `${result.totalResources} resource(s) captured`,
          type: 'success',
        });
      })
      .catch((err) => {
        const error = err instanceof Error ? err.message : 'Failed to export snapshot';
        setExportState({ kind: 'error', error });
        NotificationCenter.show({
          title: 'Export Failed',
          message: error,
          type: 'error',
        });
      });
  };

  const handleImportClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) {
      return;
    }

    if (!window.confirm(`Import resources from "${file.name}"? Existing resources will not be affected.`)) {
      return;
    }

    setImportState({ kind: 'importing' });
    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const snapshot = JSON.parse(event.target?.result as string) as WorkspaceSnapshot;
        importWorkspaceSnapshot(snapshot)
          .then((result) => {
            setLastImport(result);
            setImportState({ kind: 'idle' });
            NotificationCenter.show({
              title: 'Snapshot Imported',
              message: `${result.successCount} of ${result.totalResources} resource(s) created`,
              type: result.failureCount === 0 ? 'success' : 'warning',
            });
          })
          .catch((err) => {
            const error = err instanceof Error ? err.message : 'Failed to import snapshot';
            setImportState({ kind: 'error', error });
            NotificationCenter.show({
              title: 'Import Failed',
              message: error,
              type: 'error',
            });
          });
      } catch (err) {
        const error = err instanceof Error ? err.message : 'Invalid snapshot file format';
        setImportState({ kind: 'error', error });
        NotificationCenter.show({
          title: 'Import Failed',
          message: error,
          type: 'error',
        });
      }
    };
    reader.readAsText(file);
    // Reset input so the same file can be selected again
    e.target.value = '';
  };

  const isExporting = exportState.kind === 'exporting';
  const isImporting = importState.kind === 'importing';

  return (
    <div style={sectionStyle}>
      <Heading as="h3" sx={{ fontSize: 14, fontWeight: 600 }}>
        Workspace Snapshot
      </Heading>

      <div style={sectionStyle}>
        <Text as="p" sx={{ fontSize: 13, opacity: 0.85 }}>
          Export your current workspace configuration as a snapshot file, or import a previously saved snapshot to
          restore resources.
        </Text>

        <div style={{ display: 'flex', gap: 12 }}>
          <button
            style={isExporting ? buttonDisabledStyle : buttonStyle}
            onClick={handleExport}
            disabled={isExporting || isImporting}
            title="Download a snapshot of all current resources"
          >
            {isExporting ? 'Exporting...' : 'Export Snapshot'}
          </button>

          <button
            style={isImporting ? buttonDisabledStyle : buttonStyle}
            onClick={handleImportClick}
            disabled={isExporting || isImporting}
            title="Upload a snapshot file to restore resources"
          >
            {isImporting ? 'Importing...' : 'Import Snapshot'}
          </button>

          <input
            ref={fileInputRef}
            type="file"
            accept=".json"
            onChange={handleFileChange}
            style={{ display: 'none' }}
          />
        </div>

        {exportState.kind === 'error' && (
          <Text as="p" sx={messageStyle}>
            Export error: {exportState.error}
          </Text>
        )}

        {importState.kind === 'error' && (
          <Text as="p" sx={messageStyle}>
            Import error: {importState.error}
          </Text>
        )}

        {lastExport && (
          <Text as="p" sx={messageStyle}>
            Last export: {lastExport.totalResources} resource(s) from {lastExport.services.length} service(s)
          </Text>
        )}

        {lastImport && (
          <Text as="p" sx={messageStyle}>
            Last import: {lastImport.successCount} of {lastImport.totalResources} resource(s)
            {lastImport.failureCount > 0 && ` ({lastImport.failureCount} failed)`}
          </Text>
        )}
      </div>
    </div>
  );
}
