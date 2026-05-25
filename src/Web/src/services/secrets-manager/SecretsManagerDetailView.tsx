import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { getSecretValue, getSecretVersions, putSecretValue } from '../../api/client';
import type { SecretValueResult, SecretVersionListResult } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { ConfirmationHost } from '../../components/ConfirmationHost';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };

const textareaStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  fontFamily: 'monospace',
  minHeight: 64,
  resize: 'vertical',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const versionListStyle: CSSProperties = {
  listStyle: 'none',
  margin: 0,
  padding: 0,
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const versionRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
};

const stageContainerStyle: CSSProperties = {
  display: 'flex',
  flexWrap: 'wrap',
  gap: 4,
};

const stageBadgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
};

type LoadState = 'loading' | 'ready' | 'error';
type SaveState = 'idle' | 'saved' | 'error';

export function SecretsManagerDetailView({ resourceId }: ServiceDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [result, setResult] = useState<SecretValueResult | null>(null);
  const [reveal, setReveal] = useState(false);
  const [editValue, setEditValue] = useState('');
  const [saveState, setSaveState] = useState<SaveState>('idle');
  const [versions, setVersions] = useState<SecretVersionListResult | null>(null);
  const [versionsError, setVersionsError] = useState(false);

  const load = useCallback(
    (revealValue: boolean, signal?: AbortSignal) => {
      setLoadState('loading');
      return getSecretValue(resourceId, revealValue, signal)
        .then((data) => {
          setResult(data);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [resourceId],
  );

  const loadVersions = useCallback(
    (signal?: AbortSignal) => {
      setVersionsError(false);
      setVersions(null);
      return getSecretVersions(resourceId, signal)
        .then((data) => setVersions(data))
        .catch(() => setVersionsError(true));
    },
    [resourceId],
  );

  useEffect(() => {
    const controller = new AbortController();
    setReveal(false);
    setSaveState('idle');
    setEditValue('');
    void load(false, controller.signal);
    void loadVersions(controller.signal);
    return () => controller.abort();
  }, [load, loadVersions]);

  const handleToggleReveal = () => {
    const next = !reveal;
    setReveal(next);
    void load(next);
  };

  const handleSave = () => {
    setSaveState('idle');
    putSecretValue(resourceId, { secretString: editValue })
      .then(() => {
        setSaveState('saved');
        setEditValue('');
        void loadVersions();
        return load(reveal);
      })
      .catch(() => setSaveState('error'));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="secrets-manager-detail-loading" style={messageStyle}>
        Loading secret&hellip;
      </p>
    );
  }

  if (loadState === 'error' || result === null) {
    return (
      <p data-testid="secrets-manager-detail-error" style={messageStyle}>
        Unable to load this secret.
      </p>
    );
  }

  return (
    <div data-testid="secrets-manager-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="secrets-manager-detail-name" style={{ fontSize: 16 }}>
        {result.name}
      </Heading>
      <div data-testid="secrets-manager-detail-arn" style={rowStyle}>
        <Text style={labelStyle}>ARN</Text>
        <Text style={valueStyle}>{result.arn}</Text>
      </div>
      <div data-testid="secrets-manager-detail-versionId" style={rowStyle}>
        <Text style={labelStyle}>Version</Text>
        <Text style={valueStyle}>{result.versionId ?? '\u2014'}</Text>
      </div>
      <div style={rowStyle}>
        <div style={headerStyle}>
          <Text style={labelStyle}>Value</Text>
          {result.revealAllowed ? (
            <button
              type="button"
              data-testid="secrets-manager-detail-reveal"
              style={buttonStyle}
              onClick={handleToggleReveal}
            >
              {reveal ? 'Hide value' : 'Reveal value'}
            </button>
          ) : null}
        </div>
        <Text data-testid="secrets-manager-detail-value" style={valueStyle}>
          {result.value}
        </Text>
      </div>

      <div data-testid="secrets-manager-detail-versions" style={rowStyle}>
        <Text style={labelStyle}>Version stages</Text>
        {versionsError ? (
          <Text data-testid="secrets-manager-detail-versions-error" style={messageStyle}>
            Unable to load version stages.
          </Text>
        ) : versions === null ? (
          <Text data-testid="secrets-manager-detail-versions-loading" style={messageStyle}>
            Loading version stages&hellip;
          </Text>
        ) : versions.versions.length === 0 ? (
          <Text data-testid="secrets-manager-detail-versions-empty" style={messageStyle}>
            No staged versions.
          </Text>
        ) : (
          <ul data-testid="secrets-manager-detail-versions-list" style={versionListStyle}>
            {versions.versions.map((version) => (
              <li
                key={version.versionId}
                data-testid="secrets-manager-detail-version"
                style={versionRowStyle}
              >
                <Text style={valueStyle}>{version.versionId}</Text>
                <div style={stageContainerStyle}>
                  {version.stages.map((stage) => (
                    <span
                      key={stage}
                      data-testid="secrets-manager-detail-version-stage"
                      style={stageBadgeStyle}
                    >
                      {stage}
                    </span>
                  ))}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div style={rowStyle}>
        <label style={labelStyle} htmlFor="secrets-manager-detail-edit">
          New value
        </label>
        <textarea
          id="secrets-manager-detail-edit"
          data-testid="secrets-manager-detail-edit"
          style={textareaStyle}
          value={editValue}
          onChange={(event) => setEditValue(event.target.value)}
        />
      </div>

      <ConfirmationHost
        actionLabel="Save value"
        prompt={`Update the value for ${result.name}?`}
        confirmLabel="Confirm save"
        onConfirm={handleSave}
      />

      {saveState === 'saved' ? (
        <Text data-testid="secrets-manager-detail-save-status" style={messageStyle}>
          Secret value updated.
        </Text>
      ) : null}
      {saveState === 'error' ? (
        <Text data-testid="secrets-manager-detail-save-error" style={messageStyle}>
          Unable to update the secret value.
        </Text>
      ) : null}
    </div>
  );
}

export default SecretsManagerDetailView;
