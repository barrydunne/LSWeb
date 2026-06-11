import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { getAcmCertificates, importAcmCertificate, requestAcmCertificate } from '../../api/client';
import type { AcmCertificateItem } from '../../api/client';
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

const mutedStyle: CSSProperties = { color: '#8b949e' };

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

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const textAreaStyle: CSSProperties = {
  fontSize: 12,
  fontFamily: 'monospace',
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  minHeight: 80,
};

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
  { key: 'domainName', label: 'Domain' },
  { key: 'status', label: 'Status' },
  { key: 'type', label: 'Type' },
  { key: 'arn', label: 'ARN' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; certificates: AcmCertificateItem[] }
  | { kind: 'error' };

type ImportState = 'idle' | 'saving' | 'imported' | 'error';

type RequestState = 'idle' | 'saving' | 'requested' | 'error';

export function AcmListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showImport, setShowImport] = useState(false);
  const [certificate, setCertificate] = useState('');
  const [privateKey, setPrivateKey] = useState('');
  const [certificateChain, setCertificateChain] = useState('');
  const [importState, setImportState] = useState<ImportState>('idle');
  const [showRequest, setShowRequest] = useState(false);
  const [domainName, setDomainName] = useState('');
  const [validationMethod, setValidationMethod] = useState('DNS');
  const [subjectAlternativeNames, setSubjectAlternativeNames] = useState('');
  const [requestState, setRequestState] = useState<RequestState>('idle');
  const [requestedArn, setRequestedArn] = useState('');

  useEffect(() => {
    const controller = new AbortController();
    getAcmCertificates(controller.signal)
      .then((result) => setState({ kind: 'ready', certificates: result.certificates }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  };

  const handleImport = () => {
    setImportState('saving');
    const trimmedChain = certificateChain.trim();
    importAcmCertificate({
      certificate,
      privateKey,
      certificateChain: trimmedChain === '' ? null : trimmedChain,
    })
      .then(() => {
        setImportState('imported');
        setCertificate('');
        setPrivateKey('');
        setCertificateChain('');
        setShowImport(false);
        refresh();
      })
      .catch(() => setImportState('error'));
  };

  const handleRequest = () => {
    setRequestState('saving');
    const sans = subjectAlternativeNames
      .split(',')
      .map((entry) => entry.trim())
      .filter((entry) => entry !== '');
    requestAcmCertificate({ domainName, validationMethod, subjectAlternativeNames: sans })
      .then((result) => {
        setRequestState('requested');
        setRequestedArn(result.arn);
        setDomainName('');
        setSubjectAlternativeNames('');
        setShowRequest(false);
        refresh();
      })
      .catch(() => setRequestState('error'));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="acm-list-loading" style={messageStyle}>
        Loading certificates&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="acm-list-error" style={messageStyle}>
        Unable to load ACM certificates.
      </p>
    );
  }

  const rows: DataListRow[] = state.certificates.map((certificateItem) => ({
    id: certificateItem.arn,
    filterText: `${certificateItem.domainName} ${certificateItem.status} ${certificateItem.type ?? ''} ${certificateItem.arn}`,
    cells: {
      domainName: (
        <span data-testid="acm-list-domain" style={monoCellStyle}>
          {certificateItem.domainName}
        </span>
      ),
      status: (
        <span data-testid="acm-list-status" style={badgeStyle}>
          {certificateItem.status}
        </span>
      ),
      type: certificateItem.type ? (
        <span data-testid="acm-list-type" style={badgeStyle}>
          {certificateItem.type}
        </span>
      ) : (
        <span data-testid="acm-list-type-empty" style={mutedStyle}>
          &mdash;
        </span>
      ),
      arn: (
        <span data-testid="acm-list-arn" style={monoCellStyle}>
          {certificateItem.arn}
        </span>
      ),
    },
  }));

  return (
    <div data-testid="acm-list-view">
      <button
        type="button"
        data-testid="acm-import-toggle"
        style={buttonStyle}
        onClick={() => setShowImport((current) => !current)}
      >
        {showImport ? 'Cancel' : 'Import certificate'}
      </button>
      {showImport ? (
        <div data-testid="acm-import-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="acm-import-certificate">
              Certificate (PEM)
            </label>
            <textarea
              id="acm-import-certificate"
              data-testid="acm-import-certificate"
              style={textAreaStyle}
              value={certificate}
              onChange={(event) => setCertificate(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="acm-import-private-key">
              Private key (PEM)
            </label>
            <textarea
              id="acm-import-private-key"
              data-testid="acm-import-private-key"
              style={textAreaStyle}
              value={privateKey}
              onChange={(event) => setPrivateKey(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="acm-import-chain">
              Certificate chain (PEM, optional)
            </label>
            <textarea
              id="acm-import-chain"
              data-testid="acm-import-chain"
              style={textAreaStyle}
              value={certificateChain}
              onChange={(event) => setCertificateChain(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="acm-import-submit"
            style={buttonStyle}
            disabled={importState === 'saving'}
            onClick={handleImport}
          >
            {importState === 'saving' ? 'Importing\u2026' : 'Import'}
          </button>
        </div>
      ) : null}
      {importState === 'imported' ? (
        <p data-testid="acm-import-status" style={messageStyle}>
          Certificate imported.
        </p>
      ) : null}
      {importState === 'error' ? (
        <p data-testid="acm-import-error" style={messageStyle}>
          Unable to import the certificate.
        </p>
      ) : null}
      <button
        type="button"
        data-testid="acm-request-toggle"
        style={buttonStyle}
        onClick={() => setShowRequest((current) => !current)}
      >
        {showRequest ? 'Cancel' : 'Request certificate'}
      </button>
      {showRequest ? (
        <div data-testid="acm-request-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="acm-request-domain">
              Domain name
            </label>
            <input
              id="acm-request-domain"
              data-testid="acm-request-domain"
              style={inputStyle}
              value={domainName}
              onChange={(event) => setDomainName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="acm-request-validation-method">
              Validation method
            </label>
            <select
              id="acm-request-validation-method"
              data-testid="acm-request-validation-method"
              style={inputStyle}
              value={validationMethod}
              onChange={(event) => setValidationMethod(event.target.value)}
            >
              <option value="DNS">DNS</option>
              <option value="EMAIL">EMAIL</option>
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="acm-request-sans">
              Subject alternative names (comma separated, optional)
            </label>
            <input
              id="acm-request-sans"
              data-testid="acm-request-sans"
              style={inputStyle}
              value={subjectAlternativeNames}
              onChange={(event) => setSubjectAlternativeNames(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="acm-request-submit"
            style={buttonStyle}
            disabled={requestState === 'saving'}
            onClick={handleRequest}
          >
            {requestState === 'saving' ? 'Requesting\u2026' : 'Request'}
          </button>
        </div>
      ) : null}
      {requestState === 'requested' ? (
        <p data-testid="acm-request-status" style={messageStyle}>
          Certificate requested: {requestedArn}
        </p>
      ) : null}
      {requestState === 'error' ? (
        <p data-testid="acm-request-error" style={messageStyle}>
          Unable to request the certificate.
        </p>
      ) : null}
      <DataListShell
        title="Certificates"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter certificates"
        columnPrefsKey={`${serviceKey}-certificates`}
        emptyState={{ message: 'No ACM certificates found on this backend.' }}
      />
    </div>
  );
}

export default AcmListView;
