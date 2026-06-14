import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import {
  deleteRoute53Record,
  getRoute53Records,
  upsertRoute53Record,
} from '../../api/client';
import type { Route53RecordItem } from '../../api/client';
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
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 13, fontFamily: 'monospace', wordBreak: 'break-word' };

const listStyle: CSSProperties = {
  listStyle: 'none',
  margin: 0,
  padding: 0,
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const itemRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const recordTypes = ['A', 'AAAA', 'CNAME', 'TXT', 'MX'];

type LoadState = 'loading' | 'ready' | 'error';

/**
 * Hosted zone detail view that lists DNS records and supports creating, editing and deleting
 * records across the core record types.
 */
export function Route53DetailView({ resourceId }: ServiceDetailViewProps) {
  const [loadState, setLoadState] = useState<LoadState>('loading');
  const [records, setRecords] = useState<Route53RecordItem[]>([]);
  const [name, setName] = useState('');
  const [type, setType] = useState('A');
  const [ttl, setTtl] = useState('300');
  const [values, setValues] = useState('');
  const [formError, setFormError] = useState<string | null>(null);
  const [saveState, setSaveState] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle');

  const load = useCallback(
    (signal?: AbortSignal) => {
      setLoadState('loading');
      return getRoute53Records(resourceId, signal)
        .then((result) => {
          setRecords(result.records);
          setLoadState('ready');
        })
        .catch(() => setLoadState('error'));
    },
    [resourceId],
  );

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const reload = useCallback(() => load(), [load]);

  const handleEdit = (record: Route53RecordItem) => {
    setName(record.name);
    setType(record.type);
    setTtl(String(record.ttl));
    setValues(record.values.join('\n'));
    setFormError(null);
  };

  const handleSave = () => {
    const trimmedName = name.trim();
    if (trimmedName === '') {
      setFormError('Enter a record name.');
      setSaveState('error');
      return;
    }
    const ttlValue = Number(ttl);
    if (!Number.isInteger(ttlValue) || ttlValue <= 0) {
      setFormError('TTL must be a positive whole number.');
      setSaveState('error');
      return;
    }
    const valueList = values
      .split('\n')
      .map((value) => value.trim())
      .filter((value) => value !== '');
    if (valueList.length === 0) {
      setFormError('Enter at least one record value.');
      setSaveState('error');
      return;
    }
    setFormError(null);
    setSaveState('saving');
    upsertRoute53Record(resourceId, { name: trimmedName, type, ttl: ttlValue, values: valueList })
      .then(() => {
        setSaveState('saved');
        setName('');
        setType('A');
        setTtl('300');
        setValues('');
        return reload();
      })
      .catch(() => setSaveState('error'));
  };

  const handleDelete = (record: Route53RecordItem) => {
    setSaveState('idle');
    deleteRoute53Record(resourceId, record)
      .then(() => reload())
      .catch(() => setSaveState('error'));
  };

  if (loadState === 'loading') {
    return (
      <p data-testid="route53-detail-loading" style={messageStyle}>
        Loading records&hellip;
      </p>
    );
  }

  if (loadState === 'error') {
    return (
      <p data-testid="route53-detail-error" style={messageStyle}>
        Unable to load the hosted zone records.
      </p>
    );
  }

  return (
    <div data-testid="route53-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="route53-detail-zone" style={{ fontSize: 16 }}>
        {resourceId}
      </Heading>

      {records.length === 0 ? (
        <Text data-testid="route53-detail-records-empty" style={messageStyle}>
          This hosted zone has no records yet.
        </Text>
      ) : (
        <ul data-testid="route53-detail-records" style={listStyle}>
          {records.map((record) => (
            <li
              key={`${record.name}|${record.type}`}
              data-testid="route53-record-row"
              style={itemRowStyle}
            >
              <div>
                <Text data-testid="route53-record-name" style={valueStyle}>
                  {record.name}
                </Text>{' '}
                <Text data-testid="route53-record-type" style={labelStyle}>
                  {record.type}
                </Text>
                <Text data-testid="route53-record-values" style={valueStyle}>
                  {record.values.join(', ')}
                </Text>
              </div>
              <div style={{ display: 'flex', gap: 8 }}>
                <button
                  type="button"
                  data-testid="route53-record-edit"
                  style={buttonStyle}
                  onClick={() => handleEdit(record)}
                >
                  Edit
                </button>
                <button
                  type="button"
                  data-testid="route53-record-delete"
                  style={buttonStyle}
                  onClick={() => handleDelete(record)}
                >
                  Delete
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}

      <div data-testid="route53-record-form" style={formStyle}>
        <Heading as="h4" style={{ fontSize: 13 }}>
          Create or replace a record
        </Heading>
        <label style={labelStyle} htmlFor="route53-record-form-name">
          Name
        </label>
        <input
          id="route53-record-form-name"
          type="text"
          data-testid="route53-record-form-name"
          style={inputStyle}
          placeholder="www.example.com."
          value={name}
          onChange={(event) => setName(event.target.value)}
        />
        <label style={labelStyle} htmlFor="route53-record-form-type">
          Type
        </label>
        <select
          id="route53-record-form-type"
          data-testid="route53-record-form-type"
          style={inputStyle}
          value={type}
          onChange={(event) => setType(event.target.value)}
        >
          {recordTypes.map((recordType) => (
            <option key={recordType} value={recordType}>
              {recordType}
            </option>
          ))}
        </select>
        <label style={labelStyle} htmlFor="route53-record-form-ttl">
          TTL (seconds)
        </label>
        <input
          id="route53-record-form-ttl"
          type="text"
          data-testid="route53-record-form-ttl"
          style={inputStyle}
          value={ttl}
          onChange={(event) => setTtl(event.target.value)}
        />
        <label style={labelStyle} htmlFor="route53-record-form-values">
          Values (one per line)
        </label>
        <textarea
          id="route53-record-form-values"
          data-testid="route53-record-form-values"
          style={{ ...inputStyle, minHeight: 60 }}
          value={values}
          onChange={(event) => setValues(event.target.value)}
        />
        <button
          type="button"
          data-testid="route53-record-form-submit"
          style={buttonStyle}
          disabled={saveState === 'saving'}
          onClick={handleSave}
        >
          {saveState === 'saving' ? 'Saving\u2026' : 'Save record'}
        </button>
        {saveState === 'error' ? (
          <p data-testid="route53-record-form-error" style={messageStyle}>
            {formError ?? 'Unable to save the record.'}
          </p>
        ) : null}
        {saveState === 'saved' ? (
          <p data-testid="route53-record-status" style={messageStyle}>
            Record saved.
          </p>
        ) : null}
      </div>
    </div>
  );
}

export default Route53DetailView;
