import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import { deleteSesIdentity, getSesIdentities, verifySesDomainIdentity, verifySesEmailIdentity } from '../../api/client';
import type { SesIdentityItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const monoCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const badgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
  fontFamily: 'monospace',
};

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
  { key: 'identity', label: 'Identity' },
  { key: 'identityType', label: 'Type' },
  { key: 'verificationStatus', label: 'Verification' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; identities: SesIdentityItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'error';

export function SesListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showVerify, setShowVerify] = useState(false);
  const [email, setEmail] = useState('');
  const [createState, setCreateState] = useState<CreateState>('idle');
  const [showVerifyDomain, setShowVerifyDomain] = useState(false);
  const [domain, setDomain] = useState('');
  const [domainState, setDomainState] = useState<CreateState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getSesIdentities(controller.signal)
      .then((result) => setState({ kind: 'ready', identities: result.identities }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleVerify = () => {
    const trimmed = email.trim();
    if (!trimmed.includes('@')) {
      setCreateState('error');
      return;
    }
    setCreateState('saving');
    verifySesEmailIdentity(trimmed)
      .then(() => {
        setCreateState('idle');
        setEmail('');
        setShowVerify(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleVerifyDomain = () => {
    const trimmed = domain.trim();
    if (!trimmed.includes('.') || trimmed.includes('@')) {
      setDomainState('error');
      return;
    }
    setDomainState('saving');
    verifySesDomainIdentity(trimmed)
      .then(() => {
        setDomainState('idle');
        setDomain('');
        setShowVerifyDomain(false);
        refresh();
      })
      .catch(() => setDomainState('error'));
  };

  const handleDelete = useCallback(
    (identity: string) => {
      deleteSesIdentity(identity)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="ses-list-loading" style={messageStyle}>
        Loading identities&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="ses-list-error" style={messageStyle}>
        Unable to load SES identities.
      </p>
    );
  }

  const rows: DataListRow[] = state.identities.map((identity) => ({
    id: identity.identity,
    filterText: `${identity.identity} ${identity.identityType} ${identity.verificationStatus}`,
    cells: {
      identity: (
        <Link
          data-testid="ses-list-identity"
          to={`/services/${serviceKey}/${encodeURIComponent(identity.identity)}`}
          style={monoCellStyle}
        >
          {identity.identity}
        </Link>
      ),
      identityType: (
        <span data-testid="ses-list-type" style={badgeStyle}>
          {identity.identityType}
        </span>
      ),
      verificationStatus: (
        <span data-testid="ses-list-verification" style={badgeStyle}>
          {identity.verificationStatus}
        </span>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete the SES identity ${identity.identity}?`}
          confirmLabel="Delete"
          onConfirm={() => handleDelete(identity.identity)}
        />
      ),
    },
  }));

  return (
    <div data-testid="ses-list-view">
      <button
        type="button"
        data-testid="ses-verify-toggle"
        style={buttonStyle}
        onClick={() => setShowVerify((value) => !value)}
      >
        {showVerify ? 'Cancel' : 'Verify email identity'}
      </button>
      {showVerify ? (
        <div data-testid="ses-verify-form" style={formStyle}>
          <label style={labelStyle} htmlFor="ses-verify-email">
            Email address
          </label>
          <input
            id="ses-verify-email"
            type="text"
            data-testid="ses-verify-email"
            style={inputStyle}
            placeholder="sender@example.com"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
          <button
            type="button"
            data-testid="ses-verify-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleVerify}
          >
            {createState === 'saving' ? 'Requesting\u2026' : 'Request verification'}
          </button>
          {createState === 'error' ? (
            <p data-testid="ses-verify-error" style={messageStyle}>
              Enter a valid email address and try again.
            </p>
          ) : null}
        </div>
      ) : null}
      <button
        type="button"
        data-testid="ses-verify-domain-toggle"
        style={buttonStyle}
        onClick={() => setShowVerifyDomain((value) => !value)}
      >
        {showVerifyDomain ? 'Cancel' : 'Verify domain'}
      </button>
      {showVerifyDomain ? (
        <div data-testid="ses-verify-domain-form" style={formStyle}>
          <label style={labelStyle} htmlFor="ses-verify-domain">
            Domain name
          </label>
          <input
            id="ses-verify-domain"
            type="text"
            data-testid="ses-verify-domain"
            style={inputStyle}
            placeholder="example.com"
            value={domain}
            onChange={(event) => setDomain(event.target.value)}
          />
          <button
            type="button"
            data-testid="ses-verify-domain-submit"
            style={buttonStyle}
            disabled={domainState === 'saving'}
            onClick={handleVerifyDomain}
          >
            {domainState === 'saving' ? 'Requesting\u2026' : 'Initiate verification'}
          </button>
          {domainState === 'error' ? (
            <p data-testid="ses-verify-domain-error" style={messageStyle}>
              Enter a valid domain name and try again.
            </p>
          ) : null}
        </div>
      ) : null}
      <DataListShell
        title="Identities"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter identities"
        columnPrefsKey={`${serviceKey}-identities`}
        emptyState={{ message: 'No SES identities found on this backend.' }}
      />
    </div>
  );
}

export default SesListView;
