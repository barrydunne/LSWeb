import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { useNavigate } from 'react-router-dom';
import { Heading } from '@primer/react';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { deleteSesIdentity, getSesIdentityDetail } from '../../api/client';
import type { SesIdentityDetailResult } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { SesDomainSetupPanel } from './SesDomainSetupPanel';

const containerStyle: CSSProperties = {
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

const rowStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 2 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };

const guidanceStyle: CSSProperties = {
  fontSize: 13,
  padding: '8px 12px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const lifecycleGuidance: Record<string, string> = {
  Pending:
    'A verification request is in progress. For an email identity, AWS sends a confirmation link to the address; the identity stays Pending until that link is followed.',
  Success: 'This identity is verified and can be used as a sender.',
  Failed: 'Verification failed. Delete the identity and request verification again.',
  NotStarted: 'No verification is on record for this identity yet.',
};

type LoadState = 'loading' | 'ready' | 'error';

export function SesDetailView({ resourceId }: ServiceDetailViewProps) {
  const navigate = useNavigate();
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [detail, setDetail] = useState<SesIdentityDetailResult | null>(null);
  const [deleteError, setDeleteError] = useState(false);

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getSesIdentityDetail(resourceId, signal)
        .then((data) => {
          setDetail(data);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [resourceId],
  );

  useEffect(() => {
    const controller = new AbortController();
    load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const handleDelete = () => {
    setDeleteError(false);
    deleteSesIdentity(resourceId)
      .then(() => navigate('/services/ses'))
      .catch(() => setDeleteError(true));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="ses-detail-loading" style={messageStyle}>
        Loading identity&hellip;
      </p>
    );
  }

  if (loadState === 'error' || detail === null) {
    return (
      <p data-testid="ses-detail-error" style={messageStyle}>
        Unable to load the SES identity.
      </p>
    );
  }

  const guidance = lifecycleGuidance[detail.verificationStatus] ?? 'Verification status reported by the backend.';

  return (
    <div data-testid="ses-detail-view" style={containerStyle}>
      <div style={headerStyle}>
        <Heading as="h2" style={{ fontSize: 18 }} data-testid="ses-detail-identity">
          {detail.identity}
        </Heading>
        <button type="button" data-testid="ses-detail-refresh" style={buttonStyle} onClick={() => load()}>
          Refresh
        </button>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Type</span>
        <span style={valueStyle} data-testid="ses-detail-type">
          {detail.identityType}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Verification status</span>
        <span style={valueStyle} data-testid="ses-detail-status">
          {detail.verificationStatus}
        </span>
      </div>
      <p style={guidanceStyle} data-testid="ses-detail-guidance">
        {guidance}
      </p>
      {detail.identityType === 'Domain' ? <SesDomainSetupPanel domain={detail.identity} /> : null}
      <ConfirmationHost
        actionLabel="Delete identity"
        prompt={`Delete the SES identity ${detail.identity}?`}
        confirmLabel="Delete"
        onConfirm={handleDelete}
      />
      {deleteError ? (
        <p data-testid="ses-detail-delete-error" style={messageStyle}>
          Unable to delete the identity.
        </p>
      ) : null}
    </div>
  );
}

export default SesDetailView;
