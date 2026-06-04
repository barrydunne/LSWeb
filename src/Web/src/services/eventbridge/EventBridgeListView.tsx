import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ResourceLink } from '../../components/ResourceLink';
import { RawJsonViewer } from '../../components/RawJsonViewer';
import {
  getEventBridgeRules,
  getEventBridgeTargets,
  putEventBridgeEvent,
} from '../../api/client';
import type {
  EventBridgeRuleItem,
  EventBridgeTargetItem,
  PutEventBridgeEventResult,
} from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const arnCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const sectionHeadingStyle: CSSProperties = { fontSize: 14, margin: 0 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  marginBottom: 12,
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
};

const textAreaStyle: CSSProperties = {
  ...inputStyle,
  fontFamily: 'monospace',
  minHeight: 80,
  resize: 'vertical',
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

const stateBadgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 6px',
  borderRadius: 10,
  border: '1px solid #30363d',
  background: '#21262d',
  fontFamily: 'monospace',
};

const targetListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const mutedStyle: CSSProperties = { color: '#8b949e' };

const columns: DataListColumn[] = [
  { key: 'name', label: 'Rule' },
  { key: 'state', label: 'State' },
  { key: 'schedule', label: 'Schedule' },
  { key: 'targets', label: 'Targets' },
];

interface RuleWithTargets {
  rule: EventBridgeRuleItem;
  targets: EventBridgeTargetItem[];
}

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; rules: RuleWithTargets[] }
  | { kind: 'error' };

type PutState = 'idle' | 'saving' | 'done' | 'error';

function parseJson(value: string): unknown {
  try {
    return JSON.parse(value) as unknown;
  } catch {
    return value;
  }
}

function SendTestEventForm() {
  const [source, setSource] = useState('');
  const [detailType, setDetailType] = useState('');
  const [detail, setDetail] = useState('');
  const [eventBusName, setEventBusName] = useState('');
  const [putState, setPutState] = useState<PutState>('idle');
  const [result, setResult] = useState<PutEventBridgeEventResult | null>(null);

  const trimmedDetail = detail.trim();
  const canSubmit =
    source.trim() !== '' && detailType.trim() !== '' && trimmedDetail !== '';

  const handleSend = () => {
    setPutState('saving');
    setResult(null);
    const trimmedBus = eventBusName.trim();
    putEventBridgeEvent({
      source: source.trim(),
      detailType: detailType.trim(),
      detail: trimmedDetail,
      eventBusName: trimmedBus === '' ? null : trimmedBus,
    })
      .then((outcome) => {
        setResult(outcome);
        setPutState('done');
      })
      .catch(() => setPutState('error'));
  };

  return (
    <div data-testid="eventbridge-send-event" style={sectionStyle}>
      <h3 data-testid="eventbridge-send-event-heading" style={sectionHeadingStyle}>
        Send test event
      </h3>
      <div style={formStyle}>
        <label style={labelStyle} htmlFor="eventbridge-send-source">
          Source
        </label>
        <input
          id="eventbridge-send-source"
          data-testid="eventbridge-send-source"
          style={inputStyle}
          value={source}
          onChange={(event) => setSource(event.target.value)}
        />
        <label style={labelStyle} htmlFor="eventbridge-send-detail-type">
          Detail type
        </label>
        <input
          id="eventbridge-send-detail-type"
          data-testid="eventbridge-send-detail-type"
          style={inputStyle}
          value={detailType}
          onChange={(event) => setDetailType(event.target.value)}
        />
        <label style={labelStyle} htmlFor="eventbridge-send-bus">
          Event bus name (optional)
        </label>
        <input
          id="eventbridge-send-bus"
          data-testid="eventbridge-send-bus"
          style={inputStyle}
          value={eventBusName}
          onChange={(event) => setEventBusName(event.target.value)}
        />
        <label style={labelStyle} htmlFor="eventbridge-send-detail">
          Detail JSON
        </label>
        <textarea
          id="eventbridge-send-detail"
          data-testid="eventbridge-send-detail"
          style={textAreaStyle}
          value={detail}
          onChange={(event) => setDetail(event.target.value)}
        />
        {trimmedDetail !== '' && (
          <RawJsonViewer value={parseJson(trimmedDetail)} title="Detail preview" />
        )}
        <button
          type="button"
          data-testid="eventbridge-send-submit"
          style={buttonStyle}
          disabled={putState === 'saving' || !canSubmit}
          onClick={handleSend}
        >
          Send event
        </button>
        {putState === 'done' && result?.accepted && (
          <p data-testid="eventbridge-send-accepted" style={messageStyle}>
            Event accepted{result.eventId ? ` (id ${result.eventId})` : ''}.
          </p>
        )}
        {putState === 'done' && result && !result.accepted && (
          <p data-testid="eventbridge-send-rejected" style={messageStyle}>
            Event rejected: {result.errorMessage ?? result.errorCode ?? 'unknown error'}.
          </p>
        )}
        {putState === 'error' && (
          <p data-testid="eventbridge-send-error" style={messageStyle}>
            Unable to send the event.
          </p>
        )}
      </div>
    </div>
  );
}

export function EventBridgeListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getEventBridgeRules(controller.signal)
      .then(async (result) => {
        const rules = await Promise.all(
          result.rules.map(async (rule) => {
            const targets = await getEventBridgeTargets(rule.name, controller.signal);
            return { rule, targets: targets.targets };
          }),
        );
        setState({ kind: 'ready', rules });
      })
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  };

  if (state.kind === 'loading') {
    return (
      <div>
        <SendTestEventForm />
        <p data-testid="eventbridge-list-loading" style={messageStyle}>
          Loading rules&hellip;
        </p>
      </div>
    );
  }

  if (state.kind === 'error') {
    return (
      <div>
        <SendTestEventForm />
        <p data-testid="eventbridge-list-error" style={messageStyle}>
          Unable to load EventBridge rules.
        </p>
      </div>
    );
  }

  const rows: DataListRow[] = state.rules.map(({ rule, targets }) => ({
    id: rule.name,
    filterText: `${rule.name} ${rule.state} ${rule.scheduleExpression ?? ''}`,
    cells: {
      name: (
        <span data-testid="eventbridge-list-name" style={arnCellStyle}>
          {rule.name}
        </span>
      ),
      state: (
        <span data-testid="eventbridge-list-state" style={stateBadgeStyle}>
          {rule.state}
        </span>
      ),
      schedule: rule.scheduleExpression ? (
        <span data-testid="eventbridge-list-schedule" style={arnCellStyle}>
          {rule.scheduleExpression}
        </span>
      ) : (
        <span data-testid="eventbridge-list-schedule-empty" style={mutedStyle}>
          &mdash;
        </span>
      ),
      targets:
        targets.length > 0 ? (
          <span data-testid="eventbridge-list-targets" style={targetListStyle}>
            {targets.map((target) => (
              <ResourceLink key={target.id} reference={target.arn} />
            ))}
          </span>
        ) : (
          <span data-testid="eventbridge-list-targets-empty" style={mutedStyle}>
            No targets
          </span>
        ),
    },
  }));

  return (
    <div data-testid="eventbridge-list-view">
      <SendTestEventForm />
      <DataListShell
        title="Rules"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter rules"
        columnPrefsKey={`${serviceKey}-rules`}
        emptyState={{ message: 'No EventBridge rules found on this backend.' }}
      />
    </div>
  );
}

export default EventBridgeListView;
