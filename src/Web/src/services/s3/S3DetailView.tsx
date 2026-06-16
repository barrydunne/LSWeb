import { useCallback, useEffect, useMemo, useState } from 'react';
import type { CSSProperties, DragEvent } from 'react';
import {
  createS3Folder,
  deleteS3Object,
  getS3ObjectMetadata,
  getS3ObjectPreview,
  getS3Objects,
  getS3PresignedUrl,
  s3ObjectDownloadUrl,
  updateS3ObjectTags,
  uploadS3Object,
  copyS3Object,
  moveS3Object,
} from '../../api/client';
import type {
  S3ObjectListingResult,
  S3ObjectMetadataResult,
  S3ObjectPreviewResult,
  S3PresignedUrlResult,
} from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { RawJsonViewer } from '../../components/RawJsonViewer';
import { S3ConfigurationPanel } from './S3ConfigurationPanel';
import { S3StorageSummaryCard } from './S3StorageSummaryCard';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const messageStyle: CSSProperties = { fontSize: 14 };

const breadcrumbStyle: CSSProperties = {
  display: 'flex',
  flexWrap: 'wrap',
  gap: 4,
  alignItems: 'center',
  fontSize: 13,
};

const crumbButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '2px 6px',
  borderRadius: 6,
  border: '1px solid transparent',
  background: 'transparent',
  color: '#58a6ff',
  cursor: 'pointer',
};

const tableStyle: CSSProperties = {
  width: '100%',
  borderCollapse: 'collapse',
  fontSize: 13,
};

const cellStyle: CSSProperties = {
  textAlign: 'left',
  padding: '4px 8px',
  borderBottom: '1px solid #30363d',
};

const folderButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: 0,
  border: 'none',
  background: 'transparent',
  color: '#58a6ff',
  cursor: 'pointer',
};

const formStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  alignItems: 'flex-end',
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

const dropzoneStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 16,
  borderRadius: 6,
  border: '1px dashed #30363d',
  background: '#0d1117',
  fontSize: 13,
  textAlign: 'center',
};

const dropzoneActiveStyle: CSSProperties = {
  ...dropzoneStyle,
  borderColor: '#58a6ff',
  background: '#10243e',
};

const hiddenInputStyle: CSSProperties = { display: 'none' };

const actionsCellStyle: CSSProperties = {
  ...cellStyle,
  textAlign: 'right',
  whiteSpace: 'nowrap',
};

const actionsRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'flex-end',
  flexWrap: 'nowrap',
  gap: 12,
};

const downloadLinkStyle: CSSProperties = {
  fontSize: 13,
  color: '#58a6ff',
};

const previewButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: 0,
  border: 'none',
  background: 'transparent',
  color: '#58a6ff',
  cursor: 'pointer',
};

const previewPanelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const previewHeaderStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  fontSize: 13,
};

const previewTextStyle: CSSProperties = {
  margin: 0,
  padding: 12,
  borderRadius: 6,
  background: '#161b22',
  fontFamily: 'monospace',
  fontSize: 12,
  lineHeight: 1.5,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
  overflowX: 'auto',
  maxHeight: 320,
  overflowY: 'auto',
};

const previewImageStyle: CSSProperties = {
  maxWidth: '100%',
  maxHeight: 320,
  borderRadius: 6,
};

const presignButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: 0,
  border: 'none',
  background: 'transparent',
  color: '#58a6ff',
  cursor: 'pointer',
};

const presignUrlStyle: CSSProperties = {
  margin: 0,
  padding: 12,
  borderRadius: 6,
  background: '#161b22',
  fontFamily: 'monospace',
  fontSize: 12,
  lineHeight: 1.5,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-all',
};

const presignControlsStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  alignItems: 'flex-end',
  flexWrap: 'wrap',
};

const selectStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const tabStripStyle: CSSProperties = {
  display: 'flex',
  gap: 4,
  borderBottom: '1px solid #30363d',
};

const tabButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '6px 12px',
  border: 'none',
  borderBottomWidth: 2,
  borderBottomStyle: 'solid',
  borderBottomColor: 'transparent',
  background: 'transparent',
  color: 'inherit',
  cursor: 'pointer',
};

const activeTabButtonStyle: CSSProperties = {
  ...tabButtonStyle,
  borderBottomColor: '#58a6ff',
  color: '#58a6ff',
};

type ViewMode = 'objects' | 'configuration';

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; listing: S3ObjectListingResult }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

type UploadState = 'idle' | 'uploading' | 'uploaded' | 'error';

type PreviewState =
  | { kind: 'closed' }
  | { kind: 'loading'; key: string }
  | { kind: 'ready'; key: string; preview: S3ObjectPreviewResult }
  | { kind: 'error'; key: string };

interface ExpiryOption {
  label: string;
  seconds: number;
}

const expiryOptions: ExpiryOption[] = [
  { label: '5 minutes', seconds: 300 },
  { label: '1 hour', seconds: 3600 },
  { label: '24 hours', seconds: 86400 },
  { label: '7 days', seconds: 604800 },
];

const defaultExpirySeconds = 3600;

type PresignState =
  | { kind: 'closed' }
  | { kind: 'idle'; key: string; expirySeconds: number }
  | { kind: 'loading'; key: string; expirySeconds: number }
  | { kind: 'ready'; key: string; expirySeconds: number; result: S3PresignedUrlResult }
  | { kind: 'error'; key: string; expirySeconds: number };

interface TagDraft {
  key: string;
  value: string;
}

interface MetaReadyState {
  kind: 'ready';
  key: string;
  metadata: S3ObjectMetadataResult;
  draftTags: TagDraft[];
  saving: boolean;
  saveError: boolean;
  saved: boolean;
}

type MetaState =
  | { kind: 'closed' }
  | { kind: 'loading'; key: string }
  | MetaReadyState
  | { kind: 'error'; key: string };

type TransferStatus = 'idle' | 'working' | 'error';

interface TransferState {
  kind: 'closed' | 'open';
  key: string;
  destinationBucket: string;
  destinationKey: string;
  status: TransferStatus;
}

interface Crumb {
  label: string;
  prefix: string;
}

function buildCrumbs(prefix: string): Crumb[] {
  const crumbs: Crumb[] = [{ label: 'root', prefix: '' }];
  if (prefix.length === 0) {
    return crumbs;
  }
  const segments = prefix.split('/').filter((segment) => segment.length > 0);
  let accumulated = '';
  for (const segment of segments) {
    accumulated += `${segment}/`;
    crumbs.push({ label: segment, prefix: accumulated });
  }
  return crumbs;
}

function folderName(folderPrefix: string, parentPrefix: string): string {
  const relative = folderPrefix.slice(parentPrefix.length);
  return relative.replace(/\/$/, '');
}

function objectName(key: string, parentPrefix: string): string {
  return key.slice(parentPrefix.length);
}

function tryParseJson(text: string): { ok: true; value: unknown } | { ok: false } {
  try {
    return { ok: true, value: JSON.parse(text) as unknown };
  } catch {
    return { ok: false };
  }
}

export function S3DetailView({ resourceId }: ServiceDetailViewProps) {
  const bucketName = resourceId;
  const [viewMode, setViewMode] = useState<ViewMode>('objects');
  const [prefix, setPrefix] = useState('');
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [folderName_, setFolderName] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');
  const [uploadState, setUploadState] = useState<UploadState>('idle');
  const [dragActive, setDragActive] = useState(false);
  const [preview, setPreview] = useState<PreviewState>({ kind: 'closed' });
  const [presign, setPresign] = useState<PresignState>({ kind: 'closed' });
  const [meta, setMeta] = useState<MetaState>({ kind: 'closed' });
  const [copied, setCopied] = useState(false);
  const [transfer, setTransfer] = useState<TransferState>({
    kind: 'closed',
    key: '',
    destinationBucket: '',
    destinationKey: '',
    status: 'idle',
  });

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getS3Objects(bucketName, prefix, controller.signal)
      .then((listing) => setState({ kind: 'ready', listing }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [bucketName, prefix, reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleUpload = useCallback(
    (file: File) => {
      setUploadState('uploading');
      uploadS3Object(bucketName, prefix, file)
        .then(() => {
          setUploadState('uploaded');
          refresh();
        })
        .catch(() => setUploadState('error'));
    },
    [bucketName, prefix, refresh],
  );

  const handleFiles = useCallback(
    (files: FileList | null) => {
      if (files === null || files.length === 0) {
        return;
      }
      handleUpload(files[0]);
    },
    [handleUpload],
  );

  const handleDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setDragActive(false);
    handleFiles(event.dataTransfer.files);
  };

  const handleDragOver = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setDragActive(true);
  };

  const handleDragLeave = () => {
    setDragActive(false);
  };

  const handleDelete = useCallback(
    (key: string) => {
      deleteS3Object(bucketName, key)
        .then(() => refresh())
        .catch(() => undefined);
    },
    [bucketName, refresh],
  );

  const handlePreview = useCallback(
    (key: string) => {
      setPreview({ kind: 'loading', key });
      getS3ObjectPreview(bucketName, key)
        .then((result) => setPreview({ kind: 'ready', key, preview: result }))
        .catch(() => setPreview({ kind: 'error', key }));
    },
    [bucketName],
  );

  const closePreview = useCallback(() => {
    setPreview({ kind: 'closed' });
  }, []);

  const openPresign = useCallback((key: string) => {
    setCopied(false);
    setPresign({ kind: 'idle', key, expirySeconds: defaultExpirySeconds });
  }, []);

  const closePresign = useCallback(() => {
    setCopied(false);
    setPresign({ kind: 'closed' });
  }, []);

  const changePresignExpiry = useCallback((key: string, expirySeconds: number) => {
    setCopied(false);
    setPresign({ kind: 'idle', key, expirySeconds });
  }, []);

  const generatePresign = useCallback(
    (key: string, expirySeconds: number) => {
      setCopied(false);
      setPresign({ kind: 'loading', key, expirySeconds });
      getS3PresignedUrl(bucketName, key, expirySeconds)
        .then((result) => setPresign({ kind: 'ready', key, expirySeconds, result }))
        .catch(() => setPresign({ kind: 'error', key, expirySeconds }));
    },
    [bucketName],
  );

  const copyPresignUrl = useCallback((url: string) => {
    void navigator.clipboard.writeText(url).then(
      () => setCopied(true),
      () => setCopied(false),
    );
  }, []);

  const openMeta = useCallback(
    (key: string) => {
      setMeta({ kind: 'loading', key });
      getS3ObjectMetadata(bucketName, key)
        .then((metadata) =>
          setMeta({
            kind: 'ready',
            key,
            metadata,
            draftTags: metadata.tags.map((tag) => ({ key: tag.key, value: tag.value })),
            saving: false,
            saveError: false,
            saved: false,
          }),
        )
        .catch(() => setMeta({ kind: 'error', key }));
    },
    [bucketName],
  );

  const closeMeta = useCallback(() => {
    setMeta({ kind: 'closed' });
  }, []);

  const openTransfer = useCallback(
    (key: string) => {
      setTransfer({
        kind: 'open',
        key,
        destinationBucket: bucketName,
        destinationKey: key,
        status: 'idle',
      });
    },
    [bucketName],
  );

  const closeTransfer = useCallback(() => {
    setTransfer((current) => ({ ...current, kind: 'closed', status: 'idle' }));
  }, []);

  const changeTransferBucket = useCallback((destinationBucket: string) => {
    setTransfer((current) => ({ ...current, destinationBucket, status: 'idle' }));
  }, []);

  const changeTransferKey = useCallback((destinationKey: string) => {
    setTransfer((current) => ({ ...current, destinationKey, status: 'idle' }));
  }, []);

  const performTransfer = useCallback(
    (mode: 'copy' | 'move', current: TransferState) => {
      const destinationBucket = current.destinationBucket.trim();
      const destinationKey = current.destinationKey.trim();
      if (destinationBucket.length === 0 || destinationKey.length === 0) {
        return;
      }
      setTransfer({ ...current, status: 'working' });
      const action =
        mode === 'copy'
          ? copyS3Object(bucketName, current.key, destinationBucket, destinationKey)
          : moveS3Object(bucketName, current.key, destinationBucket, destinationKey);
      action
        .then(() => {
          setTransfer((latest) => ({ ...latest, kind: 'closed', status: 'idle' }));
          refresh();
        })
        .catch(() => setTransfer((latest) => ({ ...latest, status: 'error' })));
    },
    [bucketName, refresh],
  );

  const updateDraftTag = useCallback(
    (readyMeta: MetaReadyState, index: number, field: 'key' | 'value', value: string) => {
      const draftTags = readyMeta.draftTags.map((tag, tagIndex) =>
        tagIndex === index ? { ...tag, [field]: value } : tag,
      );
      setMeta({ ...readyMeta, draftTags, saved: false, saveError: false });
    },
    [],
  );

  const addDraftTag = useCallback((readyMeta: MetaReadyState) => {
    setMeta({
      ...readyMeta,
      draftTags: [...readyMeta.draftTags, { key: '', value: '' }],
      saved: false,
      saveError: false,
    });
  }, []);

  const removeDraftTag = useCallback((readyMeta: MetaReadyState, index: number) => {
    setMeta({
      ...readyMeta,
      draftTags: readyMeta.draftTags.filter((_, tagIndex) => tagIndex !== index),
      saved: false,
      saveError: false,
    });
  }, []);

  const saveTags = useCallback(
    (readyMeta: MetaReadyState) => {
      const { key } = readyMeta;
      const tags: Record<string, string> = {};
      for (const tag of readyMeta.draftTags) {
        const trimmedKey = tag.key.trim();
        if (trimmedKey.length > 0) {
          tags[trimmedKey] = tag.value;
        }
      }
      setMeta({ ...readyMeta, saving: true, saveError: false, saved: false });
      updateS3ObjectTags(bucketName, key, tags)
        .then(() => getS3ObjectMetadata(bucketName, key))
        .then((metadata) =>
          setMeta({
            kind: 'ready',
            key,
            metadata,
            draftTags: metadata.tags.map((tag) => ({ key: tag.key, value: tag.value })),
            saving: false,
            saveError: false,
            saved: true,
          }),
        )
        .catch(() => setMeta({ ...readyMeta, saving: false, saveError: true, saved: false }));
    },
    [bucketName],
  );

  const crumbs = useMemo(() => buildCrumbs(prefix), [prefix]);

  const handleCreateFolder = () => {
    const trimmed = folderName_.trim();
    if (trimmed.length === 0) {
      return;
    }
    const folderKey = `${prefix}${trimmed}/`;
    setCreateState('saving');
    createS3Folder(bucketName, folderKey)
      .then(() => {
        setCreateState('created');
        setFolderName('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  return (
    <div data-testid="s3-detail-view" style={containerStyle}>
      <div data-testid="s3-detail-tabs" style={tabStripStyle}>
        <button
          type="button"
          data-testid="s3-detail-tab-objects"
          style={viewMode === 'objects' ? activeTabButtonStyle : tabButtonStyle}
          aria-pressed={viewMode === 'objects'}
          onClick={() => setViewMode('objects')}
        >
          Objects
        </button>
        <button
          type="button"
          data-testid="s3-detail-tab-configuration"
          style={viewMode === 'configuration' ? activeTabButtonStyle : tabButtonStyle}
          aria-pressed={viewMode === 'configuration'}
          onClick={() => setViewMode('configuration')}
        >
          Configuration
        </button>
      </div>

      <S3StorageSummaryCard bucketName={bucketName} reloadToken={reloadToken} />

      {viewMode === 'configuration' ? <S3ConfigurationPanel bucketName={bucketName} /> : null}

      {viewMode === 'objects' ? (
        <>
          <nav data-testid="s3-detail-breadcrumb" style={breadcrumbStyle}>
        {crumbs.map((crumb, index) => (
          <span key={crumb.prefix}>
            {index > 0 ? <span> / </span> : null}
            <button
              type="button"
              data-testid="s3-detail-crumb"
              style={crumbButtonStyle}
              onClick={() => setPrefix(crumb.prefix)}
            >
              {crumb.label}
            </button>
          </span>
        ))}
      </nav>

      <div>
        <button
          type="button"
          data-testid="s3-detail-create-toggle"
          style={buttonStyle}
          onClick={() => setShowCreate((current) => !current)}
        >
          {showCreate ? 'Cancel' : 'New folder'}
        </button>
        {showCreate ? (
          <div data-testid="s3-detail-create-form" style={formStyle}>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="s3-detail-folderName">
                Folder name
              </label>
              <input
                id="s3-detail-folderName"
                type="text"
                data-testid="s3-detail-folderName"
                style={inputStyle}
                value={folderName_}
                onChange={(event) => setFolderName(event.target.value)}
              />
            </div>
            <button
              type="button"
              data-testid="s3-detail-create-submit"
              style={buttonStyle}
              disabled={createState === 'saving'}
              onClick={handleCreateFolder}
            >
              {createState === 'saving' ? 'Creating\u2026' : 'Create'}
            </button>
          </div>
        ) : null}
        {createState === 'created' ? (
          <p data-testid="s3-detail-create-status" style={messageStyle}>
            Folder created.
          </p>
        ) : null}
        {createState === 'error' ? (
          <p data-testid="s3-detail-create-error" style={messageStyle}>
            Unable to create the folder.
          </p>
        ) : null}
      </div>

      <div
        data-testid="s3-detail-dropzone"
        style={dragActive ? dropzoneActiveStyle : dropzoneStyle}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
      >
        <span>Drag and drop a file here to upload, or</span>
        <label data-testid="s3-detail-upload-browse" style={buttonStyle}>
          Choose file
          <input
            type="file"
            data-testid="s3-detail-upload-input"
            style={hiddenInputStyle}
            onChange={(event) => handleFiles(event.target.files)}
          />
        </label>
        {uploadState === 'uploading' ? (
          <p data-testid="s3-detail-upload-status" style={messageStyle}>
            Uploading&hellip;
          </p>
        ) : null}
        {uploadState === 'uploaded' ? (
          <p data-testid="s3-detail-upload-status" style={messageStyle}>
            Upload complete.
          </p>
        ) : null}
        {uploadState === 'error' ? (
          <p data-testid="s3-detail-upload-error" style={messageStyle}>
            Unable to upload the file.
          </p>
        ) : null}
      </div>

      {state.kind === 'loading' ? (
        <p data-testid="s3-detail-loading" style={messageStyle}>
          Loading objects&hellip;
        </p>
      ) : null}

      {state.kind === 'error' ? (
        <p data-testid="s3-detail-error" style={messageStyle}>
          Unable to load objects for this bucket.
        </p>
      ) : null}

      {state.kind === 'ready' ? (
        <table data-testid="s3-detail-table" style={tableStyle}>
          <thead>
            <tr>
              <th style={cellStyle}>Name</th>
              <th style={cellStyle}>Type</th>
              <th style={cellStyle}>Size</th>
              <th style={cellStyle}>Last modified</th>
              <th style={actionsCellStyle}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {state.listing.prefixes.map((folderPrefix) => (
              <tr key={`folder:${folderPrefix}`} data-testid="s3-detail-folder-row">
                <td style={cellStyle}>
                  <button
                    type="button"
                    data-testid="s3-detail-folder-link"
                    style={folderButtonStyle}
                    onClick={() => setPrefix(folderPrefix)}
                  >
                    {folderName(folderPrefix, prefix)}/
                  </button>
                </td>
                <td style={cellStyle}>Folder</td>
                <td style={cellStyle}>&mdash;</td>
                <td style={cellStyle}>&mdash;</td>
                <td style={cellStyle}>&mdash;</td>
              </tr>
            ))}
            {state.listing.objects.map((object) => (
              <tr key={`object:${object.key}`} data-testid="s3-detail-object-row">
                <td style={cellStyle}>{objectName(object.key, prefix)}</td>
                <td style={cellStyle}>Object</td>
                <td style={cellStyle}>{object.size}</td>
                <td style={cellStyle}>{object.lastModified}</td>
                <td style={actionsCellStyle}>
                  <div style={actionsRowStyle}>
                    <button
                      type="button"
                      data-testid="s3-detail-preview-button"
                      style={previewButtonStyle}
                      onClick={() => handlePreview(object.key)}
                    >
                      Preview
                    </button>
                    <button
                      type="button"
                      data-testid="s3-detail-presign-button"
                      style={presignButtonStyle}
                      onClick={() => openPresign(object.key)}
                    >
                      Share link
                    </button>
                    <button
                      type="button"
                      data-testid="s3-detail-meta-button"
                      style={presignButtonStyle}
                      onClick={() => openMeta(object.key)}
                    >
                      Details
                    </button>
                    <button
                      type="button"
                      data-testid="s3-detail-transfer-button"
                      style={presignButtonStyle}
                      onClick={() => openTransfer(object.key)}
                    >
                      Copy/Move
                    </button>
                    <a
                      data-testid="s3-detail-download-link"
                      style={downloadLinkStyle}
                      href={s3ObjectDownloadUrl(bucketName, object.key)}
                    >
                      Download
                    </a>
                    <ConfirmationHost
                      actionLabel="Delete"
                      prompt={`Delete ${objectName(object.key, prefix)}?`}
                      confirmLabel="Confirm"
                      onConfirm={() => handleDelete(object.key)}
                    />
                  </div>
                </td>
              </tr>
            ))}
            {state.listing.prefixes.length === 0 && state.listing.objects.length === 0 ? (
              <tr data-testid="s3-detail-empty-row">
                <td style={cellStyle} colSpan={5}>
                  This location is empty.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      ) : null}

      {preview.kind !== 'closed' ? (
        <div data-testid="s3-detail-preview-panel" style={previewPanelStyle}>
          <div style={previewHeaderStyle}>
            <strong data-testid="s3-detail-preview-title">{preview.key}</strong>
            <button
              type="button"
              data-testid="s3-detail-preview-close"
              style={buttonStyle}
              onClick={closePreview}
            >
              Close
            </button>
          </div>
          {preview.kind === 'loading' ? (
            <p data-testid="s3-detail-preview-loading" style={messageStyle}>
              Loading preview&hellip;
            </p>
          ) : null}
          {preview.kind === 'error' ? (
            <p data-testid="s3-detail-preview-error" style={messageStyle}>
              Unable to load the preview.
            </p>
          ) : null}
          {preview.kind === 'ready' ? renderPreviewContent(preview.preview, bucketName, preview.key) : null}
        </div>
      ) : null}

      {presign.kind !== 'closed' ? (
        <div data-testid="s3-detail-presign-panel" style={previewPanelStyle}>
          <div style={previewHeaderStyle}>
            <strong data-testid="s3-detail-presign-title">{presign.key}</strong>
            <button
              type="button"
              data-testid="s3-detail-presign-close"
              style={buttonStyle}
              onClick={closePresign}
            >
              Close
            </button>
          </div>
          <div style={presignControlsStyle}>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="s3-detail-presign-expiry">
                Link expires in
              </label>
              <select
                id="s3-detail-presign-expiry"
                data-testid="s3-detail-presign-expiry"
                style={selectStyle}
                value={presign.expirySeconds}
                onChange={(event) => changePresignExpiry(presign.key, Number(event.target.value))}
              >
                {expiryOptions.map((option) => (
                  <option key={option.seconds} value={option.seconds}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
            <button
              type="button"
              data-testid="s3-detail-presign-generate"
              style={buttonStyle}
              disabled={presign.kind === 'loading'}
              onClick={() => generatePresign(presign.key, presign.expirySeconds)}
            >
              {presign.kind === 'loading' ? 'Generating\u2026' : 'Generate link'}
            </button>
          </div>
          {presign.kind === 'error' ? (
            <p data-testid="s3-detail-presign-error" style={messageStyle}>
              Unable to generate a presigned URL.
            </p>
          ) : null}
          {presign.kind === 'ready' ? (
            <>
              <p data-testid="s3-detail-presign-url" style={presignUrlStyle}>
                {presign.result.url}
              </p>
              <div style={presignControlsStyle}>
                <button
                  type="button"
                  data-testid="s3-detail-presign-copy"
                  style={buttonStyle}
                  onClick={() => copyPresignUrl(presign.result.url)}
                >
                  Copy link
                </button>
                {copied ? (
                  <span data-testid="s3-detail-presign-copied" style={messageStyle}>
                    Copied to clipboard.
                  </span>
                ) : null}
              </div>
            </>
          ) : null}
        </div>
      ) : null}

      {meta.kind !== 'closed' ? (
        <div data-testid="s3-detail-meta-panel" style={previewPanelStyle}>
          <div style={previewHeaderStyle}>
            <strong data-testid="s3-detail-meta-title">{meta.key}</strong>
            <button
              type="button"
              data-testid="s3-detail-meta-close"
              style={buttonStyle}
              onClick={closeMeta}
            >
              Close
            </button>
          </div>
          {meta.kind === 'loading' ? (
            <p data-testid="s3-detail-meta-loading" style={messageStyle}>
              Loading details&hellip;
            </p>
          ) : null}
          {meta.kind === 'error' ? (
            <p data-testid="s3-detail-meta-error" style={messageStyle}>
              Unable to load object details.
            </p>
          ) : null}
          {meta.kind === 'ready' ? (
            <>
              <table data-testid="s3-detail-meta-table" style={tableStyle}>
                <tbody>
                  <tr>
                    <td style={cellStyle}>Content type</td>
                    <td style={cellStyle}>{meta.metadata.contentType}</td>
                  </tr>
                  <tr>
                    <td style={cellStyle}>Content length</td>
                    <td style={cellStyle}>{meta.metadata.contentLength}</td>
                  </tr>
                  <tr>
                    <td style={cellStyle}>Last modified</td>
                    <td style={cellStyle}>{meta.metadata.lastModified}</td>
                  </tr>
                  <tr>
                    <td style={cellStyle}>ETag</td>
                    <td style={cellStyle}>{meta.metadata.eTag}</td>
                  </tr>
                  {meta.metadata.metadata.map((entry) => (
                    <tr key={`meta:${entry.key}`} data-testid="s3-detail-meta-user-row">
                      <td style={cellStyle}>{entry.key}</td>
                      <td style={cellStyle}>{entry.value}</td>
                    </tr>
                  ))}
                </tbody>
              </table>

              <strong style={messageStyle}>Tags</strong>
              {meta.draftTags.map((tag, index) => (
                <div key={`tag:${index}`} data-testid="s3-detail-tag-row" style={presignControlsStyle}>
                  <input
                    type="text"
                    aria-label="Tag key"
                    data-testid="s3-detail-tag-key"
                    style={inputStyle}
                    value={tag.key}
                    onChange={(event) => updateDraftTag(meta, index, 'key', event.target.value)}
                  />
                  <input
                    type="text"
                    aria-label="Tag value"
                    data-testid="s3-detail-tag-value"
                    style={inputStyle}
                    value={tag.value}
                    onChange={(event) => updateDraftTag(meta, index, 'value', event.target.value)}
                  />
                  <button
                    type="button"
                    data-testid="s3-detail-tag-remove"
                    style={buttonStyle}
                    onClick={() => removeDraftTag(meta, index)}
                  >
                    Remove
                  </button>
                </div>
              ))}
              <div style={presignControlsStyle}>
                <button
                  type="button"
                  data-testid="s3-detail-tag-add"
                  style={buttonStyle}
                  onClick={() => addDraftTag(meta)}
                >
                  Add tag
                </button>
                <button
                  type="button"
                  data-testid="s3-detail-tag-save"
                  style={buttonStyle}
                  disabled={meta.saving}
                  onClick={() => saveTags(meta)}
                >
                  {meta.saving ? 'Saving\u2026' : 'Save tags'}
                </button>
                {meta.saved ? (
                  <span data-testid="s3-detail-tag-saved" style={messageStyle}>
                    Tags saved.
                  </span>
                ) : null}
                {meta.saveError ? (
                  <span data-testid="s3-detail-tag-error" style={messageStyle}>
                    Unable to save tags.
                  </span>
                ) : null}
              </div>
            </>
          ) : null}
        </div>
      ) : null}

      {transfer.kind === 'open' ? (
        <div data-testid="s3-detail-transfer-panel" style={previewPanelStyle}>
          <div style={previewHeaderStyle}>
            <strong data-testid="s3-detail-transfer-title">{transfer.key}</strong>
            <button
              type="button"
              data-testid="s3-detail-transfer-close"
              style={buttonStyle}
              onClick={closeTransfer}
            >
              Close
            </button>
          </div>
          <div style={presignControlsStyle}>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="s3-detail-transfer-bucket">
                Destination bucket
              </label>
              <input
                id="s3-detail-transfer-bucket"
                type="text"
                data-testid="s3-detail-transfer-bucket"
                style={inputStyle}
                value={transfer.destinationBucket}
                onChange={(event) => changeTransferBucket(event.target.value)}
              />
            </div>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="s3-detail-transfer-key">
                Destination key
              </label>
              <input
                id="s3-detail-transfer-key"
                type="text"
                data-testid="s3-detail-transfer-key"
                style={inputStyle}
                value={transfer.destinationKey}
                onChange={(event) => changeTransferKey(event.target.value)}
              />
            </div>
          </div>
          <div style={presignControlsStyle}>
            <button
              type="button"
              data-testid="s3-detail-transfer-copy"
              style={buttonStyle}
              disabled={transfer.status === 'working'}
              onClick={() => performTransfer('copy', transfer)}
            >
              {transfer.status === 'working' ? 'Working\u2026' : 'Copy'}
            </button>
            <button
              type="button"
              data-testid="s3-detail-transfer-move"
              style={buttonStyle}
              disabled={transfer.status === 'working'}
              onClick={() => performTransfer('move', transfer)}
            >
              {transfer.status === 'working' ? 'Working\u2026' : 'Move'}
            </button>
          </div>
          {transfer.status === 'error' ? (
            <p data-testid="s3-detail-transfer-error" style={messageStyle}>
              Unable to copy or move the object.
            </p>
          ) : null}
        </div>
      ) : null}
        </>
      ) : null}
    </div>
  );
}

function renderPreviewContent(
  preview: S3ObjectPreviewResult,
  bucketName: string,
  key: string,
) {
  return (
    <>
      {preview.truncated ? (
        <p data-testid="s3-detail-preview-truncated" style={messageStyle}>
          Showing the first part of this object ({preview.totalSize} bytes total).
        </p>
      ) : null}
      {renderPreviewBody(preview, bucketName, key)}
    </>
  );
}

function renderPreviewBody(
  preview: S3ObjectPreviewResult,
  bucketName: string,
  key: string,
) {
  if (preview.kind === 'Json' && preview.text !== null) {
    const parsed = tryParseJson(preview.text);
    if (parsed.ok) {
      return (
        <div data-testid="s3-detail-preview-json">
          <RawJsonViewer value={parsed.value} title="Preview" initiallyExpanded />
        </div>
      );
    }
    return (
      <pre data-testid="s3-detail-preview-text" style={previewTextStyle}>
        {preview.text}
      </pre>
    );
  }
  if (preview.kind === 'Text' && preview.text !== null) {
    return (
      <pre data-testid="s3-detail-preview-text" style={previewTextStyle}>
        {preview.text}
      </pre>
    );
  }
  if (preview.kind === 'Image' && preview.dataUrl !== null) {
    return (
      <img
        data-testid="s3-detail-preview-image"
        style={previewImageStyle}
        src={preview.dataUrl}
        alt={key}
      />
    );
  }
  return (
    <div data-testid="s3-detail-preview-unavailable" style={messageStyle}>
      <span>Preview is not available for this object. </span>
      <a
        data-testid="s3-detail-preview-download"
        style={downloadLinkStyle}
        href={s3ObjectDownloadUrl(bucketName, key)}
      >
        Download
      </a>
    </div>
  );
}

export default S3DetailView;
