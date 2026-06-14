import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { S3DetailView } from './S3DetailView';
import {
  createS3Folder,
  deleteS3Object,
  getS3BucketConfiguration,
  getS3BucketStorageSummary,
  getS3ObjectMetadata,
  getS3ObjectPreview,
  getS3ObjectVersions,
  getS3Objects,
  getS3PresignedUrl,
  resolveReference,
  s3ObjectDownloadUrl,
  updateS3ObjectTags,
  uploadS3Object,
  copyS3Object,
  moveS3Object,
} from '../../api/client';
import type { S3ObjectListingResult } from '../../api/client';

vi.mock('../../api/client');

const getS3ObjectsMock = vi.mocked(getS3Objects);
const getS3BucketConfigurationMock = vi.mocked(getS3BucketConfiguration);
const getS3BucketStorageSummaryMock = vi.mocked(getS3BucketStorageSummary);
const getS3ObjectVersionsMock = vi.mocked(getS3ObjectVersions);
const resolveReferenceMock = vi.mocked(resolveReference);
const createS3FolderMock = vi.mocked(createS3Folder);
const uploadS3ObjectMock = vi.mocked(uploadS3Object);
const deleteS3ObjectMock = vi.mocked(deleteS3Object);
const s3ObjectDownloadUrlMock = vi.mocked(s3ObjectDownloadUrl);
const getS3ObjectPreviewMock = vi.mocked(getS3ObjectPreview);
const getS3PresignedUrlMock = vi.mocked(getS3PresignedUrl);
const getS3ObjectMetadataMock = vi.mocked(getS3ObjectMetadata);
const updateS3ObjectTagsMock = vi.mocked(updateS3ObjectTags);
const copyS3ObjectMock = vi.mocked(copyS3Object);
const moveS3ObjectMock = vi.mocked(moveS3Object);

const rootListing: S3ObjectListingResult = {
  prefixes: ['orders/'],
  objects: [{ key: 'readme.txt', size: 12, lastModified: '2026-01-02T03:04:05.0000000Z' }],
};

const ordersListing: S3ObjectListingResult = {
  prefixes: ['orders/2026/'],
  objects: [{ key: 'orders/summary.csv', size: 99, lastModified: '2026-03-04T05:06:07.0000000Z' }],
};

function renderView() {
  return render(
    <MemoryRouter>
      <S3DetailView serviceKey="s3" resourceId="data" />
    </MemoryRouter>,
  );
}

describe('S3DetailView', () => {
  beforeEach(() => {
    getS3ObjectsMock.mockResolvedValue(rootListing);
    s3ObjectDownloadUrlMock.mockReturnValue(
      '/api/services/s3/buckets/data/objects/content?key=readme.txt',
    );
    getS3ObjectPreviewMock.mockResolvedValue({
      kind: 'Text',
      contentType: 'text/plain',
      truncated: false,
      totalSize: 11,
      text: 'hello world',
      dataUrl: null,
    });
    getS3PresignedUrlMock.mockResolvedValue({
      url: 'https://example.test/presigned',
      expirySeconds: 3600,
    });
    getS3ObjectMetadataMock.mockResolvedValue({
      contentType: 'text/plain',
      contentLength: 12,
      lastModified: '2026-01-02T03:04:05.0000000Z',
      eTag: '"abc123"',
      metadata: [{ key: 'owner', value: 'alice' }],
      tags: [{ key: 'stage', value: 'prod' }],
    });
    updateS3ObjectTagsMock.mockResolvedValue();
    copyS3ObjectMock.mockResolvedValue();
    moveS3ObjectMock.mockResolvedValue();
    getS3BucketConfigurationMock.mockResolvedValue({
      versioningStatus: 'Enabled',
      encryptionAlgorithm: '',
      encryptionKeyId: '',
      lifecycleRules: [],
      notifications: [],
      policy: '',
    });
    getS3BucketStorageSummaryMock.mockResolvedValue({ objectCount: 0, totalSizeBytes: 0 });
    getS3ObjectVersionsMock.mockResolvedValue({ versions: [] });
    resolveReferenceMock.mockRejectedValue(new Error('unresolved'));
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before objects arrive', () => {
    getS3ObjectsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('s3-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getS3ObjectsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-error')).toBeInTheDocument());
  });

  it('renders folders and objects for the root prefix', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    expect(screen.getByTestId('s3-detail-folder-link')).toHaveTextContent('orders/');
    expect(screen.getAllByTestId('s3-detail-object-row')).toHaveLength(1);
    expect(screen.getByText('readme.txt')).toBeInTheDocument();
    expect(getS3ObjectsMock).toHaveBeenCalledWith('data', '', expect.any(AbortSignal));
  });

  it('shows an empty row when the location has no folders or objects', async () => {
    getS3ObjectsMock.mockResolvedValue({ prefixes: [], objects: [] });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-empty-row')).toBeInTheDocument());
  });

  it('navigates into a folder when its link is clicked', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-folder-link')).toBeInTheDocument());

    getS3ObjectsMock.mockResolvedValue(ordersListing);
    fireEvent.click(screen.getByTestId('s3-detail-folder-link'));

    await waitFor(() =>
      expect(getS3ObjectsMock).toHaveBeenCalledWith('data', 'orders/', expect.any(AbortSignal)),
    );
    await waitFor(() => expect(screen.getByText('summary.csv')).toBeInTheDocument());
  });

  it('navigates back to a parent prefix via breadcrumb', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-folder-link')).toBeInTheDocument());

    getS3ObjectsMock.mockResolvedValue(ordersListing);
    fireEvent.click(screen.getByTestId('s3-detail-folder-link'));

    await waitFor(() => expect(screen.getAllByTestId('s3-detail-crumb')).toHaveLength(2));

    getS3ObjectsMock.mockResolvedValue(rootListing);
    fireEvent.click(screen.getAllByTestId('s3-detail-crumb')[0]);

    await waitFor(() =>
      expect(getS3ObjectsMock).toHaveBeenLastCalledWith('data', '', expect.any(AbortSignal)),
    );
  });

  it('creates a folder from the form and refreshes the listing', async () => {
    createS3FolderMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-create-toggle'));
    fireEvent.change(screen.getByTestId('s3-detail-folderName'), {
      target: { value: 'reports' },
    });
    fireEvent.click(screen.getByTestId('s3-detail-create-submit'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-create-status')).toBeInTheDocument());

    expect(createS3FolderMock).toHaveBeenCalledWith('data', 'reports/');
    await waitFor(() => expect(getS3ObjectsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('s3-detail-create-form')).not.toBeInTheDocument();
  });

  it('ignores creation when the folder name is blank', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-create-toggle'));
    fireEvent.change(screen.getByTestId('s3-detail-folderName'), {
      target: { value: '   ' },
    });
    fireEvent.click(screen.getByTestId('s3-detail-create-submit'));

    expect(createS3FolderMock).not.toHaveBeenCalled();
  });

  it('shows an error when folder creation fails', async () => {
    createS3FolderMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-create-toggle'));
    fireEvent.change(screen.getByTestId('s3-detail-folderName'), {
      target: { value: 'reports' },
    });
    fireEvent.click(screen.getByTestId('s3-detail-create-submit'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-create-error')).toBeInTheDocument());
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-create-toggle'));
    expect(screen.getByTestId('s3-detail-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('s3-detail-create-toggle'));
    expect(screen.queryByTestId('s3-detail-create-form')).not.toBeInTheDocument();
  });

  it('uploads a file selected from the input and refreshes the listing', async () => {
    uploadS3ObjectMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    const file = new File(['hello'], 'note.txt', { type: 'text/plain' });
    fireEvent.change(screen.getByTestId('s3-detail-upload-input'), {
      target: { files: [file] },
    });

    await waitFor(() =>
      expect(screen.getByTestId('s3-detail-upload-status')).toHaveTextContent('Upload complete.'),
    );
    expect(uploadS3ObjectMock).toHaveBeenCalledWith('data', '', file);
    await waitFor(() => expect(getS3ObjectsMock).toHaveBeenCalledTimes(2));
  });

  it('uploads a file dropped onto the dropzone', async () => {
    uploadS3ObjectMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    const file = new File(['hi'], 'dropped.txt', { type: 'text/plain' });
    fireEvent.drop(screen.getByTestId('s3-detail-dropzone'), {
      dataTransfer: { files: [file] },
    });

    await waitFor(() => expect(uploadS3ObjectMock).toHaveBeenCalledWith('data', '', file));
    await waitFor(() => expect(getS3ObjectsMock).toHaveBeenCalledTimes(2));
  });

  it('highlights the dropzone while dragging and clears on leave', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    const dropzone = screen.getByTestId('s3-detail-dropzone');
    fireEvent.dragOver(dropzone);
    expect(dropzone).toHaveStyle({ borderColor: '#58a6ff' });

    fireEvent.dragLeave(dropzone);
    expect(dropzone).not.toHaveStyle({ borderColor: '#58a6ff' });
  });

  it('ignores an upload when no file is provided', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-detail-upload-input'), {
      target: { files: [] },
    });
    fireEvent.drop(screen.getByTestId('s3-detail-dropzone'), {
      dataTransfer: { files: null },
    });

    expect(uploadS3ObjectMock).not.toHaveBeenCalled();
  });

  it('shows an error when the upload fails', async () => {
    uploadS3ObjectMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    const file = new File(['hello'], 'note.txt', { type: 'text/plain' });
    fireEvent.change(screen.getByTestId('s3-detail-upload-input'), {
      target: { files: [file] },
    });

    await waitFor(() => expect(screen.getByTestId('s3-detail-upload-error')).toBeInTheDocument());
  });

  it('renders a download link for each object', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    expect(screen.getByTestId('s3-detail-download-link')).toHaveAttribute(
      'href',
      '/api/services/s3/buckets/data/objects/content?key=readme.txt',
    );
    expect(s3ObjectDownloadUrlMock).toHaveBeenCalledWith('data', 'readme.txt');
  });

  it('deletes an object after confirmation and refreshes the listing', async () => {
    deleteS3ObjectMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    const row = screen.getByTestId('s3-detail-object-row');
    fireEvent.click(within(row).getByTestId('confirm-trigger'));
    fireEvent.click(within(row).getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteS3ObjectMock).toHaveBeenCalledWith('data', 'readme.txt'));
    await waitFor(() => expect(getS3ObjectsMock).toHaveBeenCalledTimes(2));
  });

  it('ignores a failed delete without throwing', async () => {
    deleteS3ObjectMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    const row = screen.getByTestId('s3-detail-object-row');
    fireEvent.click(within(row).getByTestId('confirm-trigger'));
    fireEvent.click(within(row).getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteS3ObjectMock).toHaveBeenCalledWith('data', 'readme.txt'));
  });

  it('anchors the row action buttons in a single right-aligned row', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    const row = screen.getByTestId('s3-detail-object-row');
    const previewButton = within(row).getByTestId('s3-detail-preview-button');
    const deleteTrigger = within(row).getByTestId('confirm-trigger');

    const actionsRow = previewButton.parentElement as HTMLElement;
    expect(actionsRow.style.display).toBe('flex');
    expect(actionsRow.style.justifyContent).toBe('flex-end');
    expect(actionsRow).toContainElement(deleteTrigger);
  });

  it('shows a loading state while the preview is fetched', async () => {
    getS3ObjectPreviewMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-button'));

    expect(screen.getByTestId('s3-detail-preview-loading')).toBeInTheDocument();
    expect(getS3ObjectPreviewMock).toHaveBeenCalledWith('data', 'readme.txt');
  });

  it('renders a text preview', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-preview-text')).toBeInTheDocument());
    expect(screen.getByTestId('s3-detail-preview-text')).toHaveTextContent('hello world');
    expect(screen.getByTestId('s3-detail-preview-title')).toHaveTextContent('readme.txt');
  });

  it('renders a json preview with the raw json viewer', async () => {
    getS3ObjectPreviewMock.mockResolvedValue({
      kind: 'Json',
      contentType: 'application/json',
      truncated: false,
      totalSize: 7,
      text: '{"a":1}',
      dataUrl: null,
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-preview-json')).toBeInTheDocument());
    expect(screen.getByTestId('raw-json-content')).toHaveTextContent('"a": 1');
  });

  it('falls back to text when json content cannot be parsed', async () => {
    getS3ObjectPreviewMock.mockResolvedValue({
      kind: 'Json',
      contentType: 'application/json',
      truncated: false,
      totalSize: 5,
      text: 'not-json',
      dataUrl: null,
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-preview-text')).toBeInTheDocument());
    expect(screen.getByTestId('s3-detail-preview-text')).toHaveTextContent('not-json');
  });

  it('renders an image preview and a truncated note', async () => {
    getS3ObjectPreviewMock.mockResolvedValue({
      kind: 'Image',
      contentType: 'image/png',
      truncated: true,
      totalSize: 2048,
      text: null,
      dataUrl: 'data:image/png;base64,AAAA',
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-preview-image')).toBeInTheDocument());
    expect(screen.getByTestId('s3-detail-preview-image')).toHaveAttribute(
      'src',
      'data:image/png;base64,AAAA',
    );
    expect(screen.getByTestId('s3-detail-preview-truncated')).toBeInTheDocument();
  });

  it('shows an unavailable message with a download link for binary content', async () => {
    getS3ObjectPreviewMock.mockResolvedValue({
      kind: 'Binary',
      contentType: 'application/octet-stream',
      truncated: false,
      totalSize: 10,
      text: null,
      dataUrl: null,
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-button'));

    await waitFor(() =>
      expect(screen.getByTestId('s3-detail-preview-unavailable')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('s3-detail-preview-download')).toBeInTheDocument();
  });

  it('shows an error when the preview fails to load', async () => {
    getS3ObjectPreviewMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-preview-error')).toBeInTheDocument());
  });

  it('closes the preview panel when the close button is clicked', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-preview-panel')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-preview-close'));

    expect(screen.queryByTestId('s3-detail-preview-panel')).not.toBeInTheDocument();
  });

  it('opens the presign panel with the default expiry when share link is clicked', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-button'));

    expect(screen.getByTestId('s3-detail-presign-panel')).toBeInTheDocument();
    expect(screen.getByTestId('s3-detail-presign-title')).toHaveTextContent('readme.txt');
    expect(screen.getByTestId('s3-detail-presign-expiry')).toHaveValue('3600');
  });

  it('generates a presigned url using the selected expiry', async () => {
    getS3PresignedUrlMock.mockResolvedValue({
      url: 'https://example.test/share',
      expirySeconds: 86400,
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-button'));
    fireEvent.change(screen.getByTestId('s3-detail-presign-expiry'), {
      target: { value: '86400' },
    });
    fireEvent.click(screen.getByTestId('s3-detail-presign-generate'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-presign-url')).toBeInTheDocument());
    expect(screen.getByTestId('s3-detail-presign-url')).toHaveTextContent(
      'https://example.test/share',
    );
    expect(getS3PresignedUrlMock).toHaveBeenCalledWith('data', 'readme.txt', 86400);
  });

  it('shows a loading state while the presigned url is generated', async () => {
    getS3PresignedUrlMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-button'));
    fireEvent.click(screen.getByTestId('s3-detail-presign-generate'));

    expect(screen.getByTestId('s3-detail-presign-generate')).toBeDisabled();
  });

  it('shows an error when the presigned url cannot be generated', async () => {
    getS3PresignedUrlMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-button'));
    fireEvent.click(screen.getByTestId('s3-detail-presign-generate'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-presign-error')).toBeInTheDocument());
  });

  it('copies the generated url to the clipboard', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.assign(navigator, { clipboard: { writeText } });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-button'));
    fireEvent.click(screen.getByTestId('s3-detail-presign-generate'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-presign-copy')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-copy'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-presign-copied')).toBeInTheDocument());
    expect(writeText).toHaveBeenCalledWith('https://example.test/presigned');
  });

  it('clears a copied confirmation when the expiry changes', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.assign(navigator, { clipboard: { writeText } });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-button'));
    fireEvent.click(screen.getByTestId('s3-detail-presign-generate'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-presign-copy')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('s3-detail-presign-copy'));
    await waitFor(() => expect(screen.getByTestId('s3-detail-presign-copied')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('s3-detail-presign-expiry'), {
      target: { value: '300' },
    });

    expect(screen.queryByTestId('s3-detail-presign-copied')).not.toBeInTheDocument();
    expect(screen.queryByTestId('s3-detail-presign-url')).not.toBeInTheDocument();
  });

  it('keeps the copied confirmation hidden when the clipboard write fails', async () => {
    const writeText = vi.fn().mockRejectedValue(new Error('denied'));
    Object.assign(navigator, { clipboard: { writeText } });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-button'));
    fireEvent.click(screen.getByTestId('s3-detail-presign-generate'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-presign-copy')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('s3-detail-presign-copy'));

    await waitFor(() => expect(writeText).toHaveBeenCalled());
    expect(screen.queryByTestId('s3-detail-presign-copied')).not.toBeInTheDocument();
  });

  it('closes the presign panel when the close button is clicked', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-presign-panel')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-presign-close'));

    expect(screen.queryByTestId('s3-detail-presign-panel')).not.toBeInTheDocument();
  });

  it('shows a loading state then object details when the metadata panel opens', async () => {
    let resolveMetadata: (value: Awaited<ReturnType<typeof getS3ObjectMetadata>>) => void = () => {};
    getS3ObjectMetadataMock.mockReturnValue(
      new Promise((resolve) => {
        resolveMetadata = resolve;
      }),
    );

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-meta-button'));

    expect(screen.getByTestId('s3-detail-meta-loading')).toBeInTheDocument();
    expect(getS3ObjectMetadataMock).toHaveBeenCalledWith('data', 'readme.txt');

    resolveMetadata({
      contentType: 'text/plain',
      contentLength: 12,
      lastModified: '2026-01-02T03:04:05.0000000Z',
      eTag: '"abc123"',
      metadata: [{ key: 'owner', value: 'alice' }],
      tags: [{ key: 'stage', value: 'prod' }],
    });

    await waitFor(() => expect(screen.getByTestId('s3-detail-meta-table')).toBeInTheDocument());
    expect(screen.getByText('text/plain')).toBeInTheDocument();
    expect(screen.getByText('"abc123"')).toBeInTheDocument();
    expect(screen.getByTestId('s3-detail-meta-user-row')).toHaveTextContent('owner');
    expect(screen.getAllByTestId('s3-detail-tag-row')).toHaveLength(1);
  });

  it('shows an error state when metadata cannot be loaded', async () => {
    getS3ObjectMetadataMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-meta-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-meta-error')).toBeInTheDocument());
  });

  it('closes the metadata panel when the close button is clicked', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-meta-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-meta-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-meta-close'));

    expect(screen.queryByTestId('s3-detail-meta-panel')).not.toBeInTheDocument();
  });

  it('edits, adds and removes tag rows and saves them', async () => {
    getS3ObjectMetadataMock
      .mockResolvedValueOnce({
        contentType: 'text/plain',
        contentLength: 12,
        lastModified: '2026-01-02T03:04:05.0000000Z',
        eTag: '"abc123"',
        metadata: [],
        tags: [{ key: 'stage', value: 'prod' }],
      })
      .mockResolvedValueOnce({
        contentType: 'text/plain',
        contentLength: 12,
        lastModified: '2026-01-02T03:04:05.0000000Z',
        eTag: '"abc123"',
        metadata: [],
        tags: [{ key: 'stage', value: 'staging' }],
      });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-meta-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-meta-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-tag-add'));
    expect(screen.getAllByTestId('s3-detail-tag-row')).toHaveLength(2);

    const secondRow = screen.getAllByTestId('s3-detail-tag-row')[1];
    fireEvent.change(within(secondRow).getByTestId('s3-detail-tag-key'), {
      target: { value: '   ' },
    });
    fireEvent.click(within(secondRow).getByTestId('s3-detail-tag-remove'));
    expect(screen.getAllByTestId('s3-detail-tag-row')).toHaveLength(1);

    const firstRow = screen.getAllByTestId('s3-detail-tag-row')[0];
    fireEvent.change(within(firstRow).getByTestId('s3-detail-tag-value'), {
      target: { value: 'staging' },
    });

    fireEvent.click(screen.getByTestId('s3-detail-tag-save'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-tag-saved')).toBeInTheDocument());
    expect(updateS3ObjectTagsMock).toHaveBeenCalledWith('data', 'readme.txt', {
      stage: 'staging',
    });
  });

  it('shows an error when saving tags fails', async () => {
    updateS3ObjectTagsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-meta-button'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-meta-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-tag-save'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-tag-error')).toBeInTheDocument());
  });

  it('shows the objects tab by default', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    expect(screen.queryByTestId('s3-config-view')).not.toBeInTheDocument();
    expect(getS3BucketConfigurationMock).not.toHaveBeenCalled();
  });

  it('switches to the configuration tab and loads the bucket configuration', async () => {
    getS3BucketConfigurationMock.mockResolvedValue({
      versioningStatus: 'Suspended',
      encryptionAlgorithm: '',
      encryptionKeyId: '',
      lifecycleRules: [],
      notifications: [],
      policy: '',
    });

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-tab-configuration'));

    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());
    expect(getS3BucketConfigurationMock).toHaveBeenCalledWith('data', expect.any(AbortSignal));
    expect(screen.queryByTestId('s3-detail-table')).not.toBeInTheDocument();
    expect(screen.getByTestId('s3-config-versioning-status')).toHaveTextContent('Suspended');
  });

  it('switches back to the objects tab from the configuration tab', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-tab-configuration'));
    await waitFor(() => expect(screen.getByTestId('s3-config-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-tab-objects'));

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());
    expect(screen.queryByTestId('s3-config-view')).not.toBeInTheDocument();
  });
});

describe('S3DetailView copy and move', () => {
  beforeEach(() => {
    getS3ObjectsMock.mockResolvedValue(rootListing);
    s3ObjectDownloadUrlMock.mockReturnValue(
      '/api/services/s3/buckets/data/objects/content?key=readme.txt',
    );
    copyS3ObjectMock.mockResolvedValue();
    moveS3ObjectMock.mockResolvedValue();
    getS3BucketStorageSummaryMock.mockResolvedValue({ objectCount: 0, totalSizeBytes: 0 });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('copies an object to a new destination and refreshes the listing', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-transfer-button'));

    expect(screen.getByTestId('s3-detail-transfer-bucket')).toHaveValue('data');
    expect(screen.getByTestId('s3-detail-transfer-key')).toHaveValue('readme.txt');

    fireEvent.change(screen.getByTestId('s3-detail-transfer-bucket'), {
      target: { value: 'archive' },
    });
    fireEvent.change(screen.getByTestId('s3-detail-transfer-key'), {
      target: { value: 'orders/2026/readme.txt' },
    });

    fireEvent.click(screen.getByTestId('s3-detail-transfer-copy'));

    await waitFor(() =>
      expect(copyS3ObjectMock).toHaveBeenCalledWith(
        'data',
        'readme.txt',
        'archive',
        'orders/2026/readme.txt',
      ),
    );
    await waitFor(() => expect(getS3ObjectsMock).toHaveBeenCalledTimes(2));
    expect(screen.queryByTestId('s3-detail-transfer-panel')).not.toBeInTheDocument();
  });

  it('moves an object to a new destination and refreshes the listing', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-transfer-button'));

    fireEvent.change(screen.getByTestId('s3-detail-transfer-key'), {
      target: { value: 'moved/readme.txt' },
    });

    fireEvent.click(screen.getByTestId('s3-detail-transfer-move'));

    await waitFor(() =>
      expect(moveS3ObjectMock).toHaveBeenCalledWith(
        'data',
        'readme.txt',
        'data',
        'moved/readme.txt',
      ),
    );
    await waitFor(() => expect(getS3ObjectsMock).toHaveBeenCalledTimes(2));
  });

  it('does nothing when the destination fields are empty', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-transfer-button'));

    fireEvent.change(screen.getByTestId('s3-detail-transfer-bucket'), {
      target: { value: '   ' },
    });

    fireEvent.click(screen.getByTestId('s3-detail-transfer-copy'));

    expect(copyS3ObjectMock).not.toHaveBeenCalled();
    expect(screen.getByTestId('s3-detail-transfer-panel')).toBeInTheDocument();
  });

  it('shows an error when the copy fails', async () => {
    copyS3ObjectMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-transfer-button'));
    fireEvent.change(screen.getByTestId('s3-detail-transfer-key'), {
      target: { value: 'copies/readme.txt' },
    });
    fireEvent.click(screen.getByTestId('s3-detail-transfer-copy'));

    await waitFor(() =>
      expect(screen.getByTestId('s3-detail-transfer-error')).toBeInTheDocument(),
    );
  });

  it('closes the transfer panel without copying', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('s3-detail-table')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('s3-detail-transfer-button'));
    fireEvent.click(screen.getByTestId('s3-detail-transfer-close'));

    expect(screen.queryByTestId('s3-detail-transfer-panel')).not.toBeInTheDocument();
    expect(copyS3ObjectMock).not.toHaveBeenCalled();
  });
});
