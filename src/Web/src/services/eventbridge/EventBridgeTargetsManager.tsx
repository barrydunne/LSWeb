import { useCallback, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import {
  getEventBridgeTargets,
  putEventBridgeRuleTargets,
  removeEventBridgeRuleTargets,
} from '../../api/client';
import type { EventBridgeTargetItem } from '../../api/client';

const panelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 10,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  marginBottom: 12,
};

const headingStyle: CSSProperties = { fontSize: 14, margin: 0 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const messageStyle: CSSProperties = { fontSize: 13 };
const rowStyle: CSSProperties = { display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' };
const columnStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 8 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
  flex: 1,
  minWidth: 120,
};

const targetRowStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  alignItems: 'center',
  justifyContent: 'space-between',
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
};

const arnStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12, wordBreak: 'break-all' };

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const targetTypes: { value: string; label: string; token: string | null }[] = [
  { value: 'lambda', label: 'Lambda function', token: ':lambda:' },
  { value: 'sqs', label: 'SQS queue', token: ':sqs:' },
  { value: 'sns', label: 'SNS topic', token: ':sns:' },
  { value: 'other', label: 'Other / custom ARN', token: null },
];

type LoadState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'ready'; targets: EventBridgeTargetItem[] }
  | { kind: 'error' };

type SaveState = 'idle' | 'saving' | 'error';

export function EventBridgeTargetsManager() {
  const [ruleName, setRuleName] = useState('');
  const [loadState, setLoadState] = useState<LoadState>({ kind: 'idle' });

  const [targetId, setTargetId] = useState('');
  const [targetType, setTargetType] = useState('lambda');
  const [targetArn, setTargetArn] = useState('');
  const [saveState, setSaveState] = useState<SaveState>('idle');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const reload = useCallback((name: string) => {
    setLoadState({ kind: 'loading' });
    getEventBridgeTargets(name)
      .then((result) => setLoadState({ kind: 'ready', targets: result.targets }))
      .catch(() => setLoadState({ kind: 'error' }));
  }, []);

  const loadTargets = () => {
    const name = ruleName.trim();
    if (name.length === 0) {
      setLoadState({ kind: 'error' });
      return;
    }
    reload(name);
  };

  const addTarget = () => {
    const name = ruleName.trim();
    const id = targetId.trim();
    const arn = targetArn.trim();
    if (name.length === 0 || id.length === 0 || arn.length === 0) {
      setErrorMessage('Rule name, target id and ARN are all required.');
      setSaveState('error');
      return;
    }
    const type = targetTypes.find((candidate) => candidate.value === targetType);
    if (type?.token && !arn.includes(type.token)) {
      setErrorMessage(`The ARN does not look like a ${type.label} ARN (expected to contain "${type.token}").`);
      setSaveState('error');
      return;
    }
    setErrorMessage(null);
    setSaveState('saving');
    putEventBridgeRuleTargets(name, [{ id, arn, roleArn: null, input: null }])
      .then(() => {
        setTargetId('');
        setTargetArn('');
        setSaveState('idle');
        reload(name);
      })
      .catch(() => {
        setErrorMessage('The target could not be saved. Check the rule and ARN, then try again.');
        setSaveState('error');
      });
  };

  const removeTarget = (id: string) => {
    const name = ruleName.trim();
    removeEventBridgeRuleTargets(name, [id])
      .then(() => reload(name))
      .catch(() => {
        setErrorMessage('The target could not be removed.');
        setSaveState('error');
      });
  };

  return (
    <div data-testid="eventbridge-targets-manager" style={panelStyle}>
      <Heading as="h3" style={headingStyle}>
        Configure rule targets
      </Heading>

      <div style={rowStyle}>
        <input
          data-testid="eventbridge-targets-rule"
          style={inputStyle}
          placeholder="Rule name"
          value={ruleName}
          onChange={(event) => setRuleName(event.target.value)}
        />
        <button
          type="button"
          data-testid="eventbridge-targets-load"
          style={buttonStyle}
          onClick={loadTargets}
        >
          Load targets
        </button>
      </div>

      {loadState.kind === 'loading' ? (
        <Text data-testid="eventbridge-targets-loading" style={messageStyle}>
          Loading targets&hellip;
        </Text>
      ) : null}
      {loadState.kind === 'error' ? (
        <Text data-testid="eventbridge-targets-error" style={messageStyle}>
          Enter a rule name and try again.
        </Text>
      ) : null}
      {loadState.kind === 'ready' && loadState.targets.length === 0 ? (
        <Text data-testid="eventbridge-targets-empty" style={messageStyle}>
          This rule has no targets yet.
        </Text>
      ) : null}
      {loadState.kind === 'ready' && loadState.targets.length > 0 ? (
        <div style={columnStyle}>
          {loadState.targets.map((target) => (
            <div key={target.id} data-testid={`eventbridge-target-${target.id}`} style={targetRowStyle}>
              <span style={arnStyle}>
                {target.id}: {target.arn}
              </span>
              <ConfirmationHost
                actionLabel="Remove"
                prompt={`Remove target ${target.id}?`}
                confirmLabel="Confirm"
                onConfirm={() => removeTarget(target.id)}
              />
            </div>
          ))}
        </div>
      ) : null}

      <div style={columnStyle}>
        <Text style={labelStyle}>Add target</Text>
        <div style={rowStyle}>
          <input
            data-testid="eventbridge-target-id"
            style={inputStyle}
            placeholder="Target id"
            value={targetId}
            disabled={saveState === 'saving'}
            onChange={(event) => setTargetId(event.target.value)}
          />
          <select
            data-testid="eventbridge-target-type"
            style={inputStyle}
            value={targetType}
            disabled={saveState === 'saving'}
            onChange={(event) => setTargetType(event.target.value)}
          >
            {targetTypes.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>
        <input
          data-testid="eventbridge-target-arn"
          style={inputStyle}
          placeholder="Target ARN"
          value={targetArn}
          disabled={saveState === 'saving'}
          onChange={(event) => setTargetArn(event.target.value)}
        />
        <button
          type="button"
          data-testid="eventbridge-target-add"
          style={buttonStyle}
          disabled={saveState === 'saving'}
          onClick={addTarget}
        >
          {saveState === 'saving' ? 'Saving\u2026' : 'Add target'}
        </button>
        {saveState === 'error' && errorMessage !== null ? (
          <Text data-testid="eventbridge-target-form-error" style={messageStyle}>
            {errorMessage}
          </Text>
        ) : null}
      </div>
    </div>
  );
}

export default EventBridgeTargetsManager;
