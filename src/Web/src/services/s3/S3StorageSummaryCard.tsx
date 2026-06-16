import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { getS3BucketStorageSummary } from '../../api/client';
import type { S3BucketStorageSummaryResult } from '../../api/client';
import { formatBytes } from './formatBytes';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'row',
  gap: 24,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  flexWrap: 'wrap',
};

const messageStyle: CSSProperties = { fontSize: 14 };

const metricStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
};

const metricLabelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const metricValueStyle: CSSProperties = { fontSize: 18, fontWeight: 600 };

type SummaryState =
  | { kind: 'loading' }
  | { kind: 'ready'; summary: S3BucketStorageSummaryResult }
  | { kind: 'error' };

export function S3StorageSummaryCard({
  bucketName,
  reloadToken = 0,
}: {
  bucketName: string;
  reloadToken?: number;
}) {
  const [state, setState] = useState<SummaryState>({ kind: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getS3BucketStorageSummary(bucketName, controller.signal)
      .then((summary) => setState({ kind: 'ready', summary }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [bucketName, reloadToken]);

  if (state.kind === 'loading') {
    return (
      <p data-testid="s3-storage-summary-loading" style={messageStyle}>
        Loading storage summary&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="s3-storage-summary-error" style={messageStyle}>
        Unable to load the storage summary.
      </p>
    );
  }

  const { summary } = state;

  return (
    <div data-testid="s3-storage-summary" style={containerStyle}>
      <div style={metricStyle}>
        <span style={metricLabelStyle}>Objects</span>
        <span data-testid="s3-storage-summary-object-count" style={metricValueStyle}>
          {summary.objectCount.toLocaleString()}
        </span>
      </div>
      <div style={metricStyle}>
        <span style={metricLabelStyle}>Total size</span>
        <span data-testid="s3-storage-summary-total-size" style={metricValueStyle}>
          {formatBytes(summary.totalSizeBytes)}
        </span>
      </div>
    </div>
  );
}
