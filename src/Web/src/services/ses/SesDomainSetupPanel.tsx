import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { enableSesDomainDkim, getSesDomainSetup } from '../../api/client';
import type { SesDomainSetupResult } from '../../api/client';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const titleStyle: CSSProperties = { fontSize: 14, fontWeight: 600 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 13, fontFamily: 'monospace', wordBreak: 'break-all' };
const messageStyle: CSSProperties = { fontSize: 14 };

const recordRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
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

type LoadState = 'loading' | 'ready' | 'error';

export interface SesDomainSetupPanelProps {
  domain: string;
}

export function SesDomainSetupPanel({ domain }: SesDomainSetupPanelProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [setup, setSetup] = useState<SesDomainSetupResult | null>(null);
  const [dkimError, setDkimError] = useState(false);

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getSesDomainSetup(domain, signal)
        .then((data) => {
          setSetup(data);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [domain],
  );

  useEffect(() => {
    const controller = new AbortController();
    load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const handleEnableDkim = () => {
    setDkimError(false);
    enableSesDomainDkim(domain)
      .then(() => load())
      .catch(() => setDkimError(true));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="ses-domain-loading" style={messageStyle}>
        Loading domain setup&hellip;
      </p>
    );
  }

  if (loadState === 'error' || setup === null) {
    return (
      <p data-testid="ses-domain-error" style={messageStyle}>
        Unable to load the domain setup.
      </p>
    );
  }

  return (
    <div data-testid="ses-domain-setup" style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
      <div style={sectionStyle}>
        <span style={titleStyle}>Domain verification</span>
        <div>
          <span style={labelStyle}>Status: </span>
          <span style={valueStyle} data-testid="ses-domain-verification-status">
            {setup.verificationStatus}
          </span>
        </div>
        {setup.verificationToken.length > 0 ? (
          <div style={recordRowStyle} data-testid="ses-domain-txt-record">
            <span style={labelStyle}>Add this TXT record to your DNS</span>
            <span style={valueStyle}>Name: _amazonses.{setup.domain}</span>
            <span style={valueStyle}>Type: TXT</span>
            <span style={valueStyle} data-testid="ses-domain-txt-value">
              Value: {setup.verificationToken}
            </span>
          </div>
        ) : (
          <p style={messageStyle} data-testid="ses-domain-txt-empty">
            Initiate domain verification to generate the TXT record.
          </p>
        )}
      </div>
      <div style={sectionStyle}>
        <span style={titleStyle}>DKIM</span>
        <div>
          <span style={labelStyle}>Status: </span>
          <span style={valueStyle} data-testid="ses-domain-dkim-status">
            {setup.dkimVerificationStatus}
          </span>
        </div>
        <button type="button" data-testid="ses-domain-enable-dkim" style={buttonStyle} onClick={handleEnableDkim}>
          Enable DKIM
        </button>
        {dkimError ? (
          <p style={messageStyle} data-testid="ses-domain-dkim-error">
            Unable to enable DKIM.
          </p>
        ) : null}
        {setup.dkimTokens.length > 0 ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <span style={labelStyle}>Add these CNAME records to your DNS</span>
            {setup.dkimTokens.map((token) => (
              <div key={token} style={recordRowStyle} data-testid="ses-domain-dkim-record">
                <span style={valueStyle}>Name: {token}._domainkey.{setup.domain}</span>
                <span style={valueStyle}>Type: CNAME</span>
                <span style={valueStyle}>Value: {token}.dkim.amazonses.com</span>
              </div>
            ))}
          </div>
        ) : (
          <p style={messageStyle} data-testid="ses-domain-dkim-empty">
            Enable DKIM to generate the CNAME records.
          </p>
        )}
      </div>
    </div>
  );
}

export default SesDomainSetupPanel;
