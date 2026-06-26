import { useEffect, useState } from 'react';
import type { CSSProperties, ReactElement } from 'react';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ResourceLink } from '../../components/ResourceLink';
import { RawJsonViewer } from '../../components/RawJsonViewer';
import {
  getEventBridgeRules,
  getEventBridgeTargets,
  putEventBridgeEvent,
  getScheduledRules,
  getScheduledRule,
  putScheduledRule,
  updateScheduledRule,
  deleteScheduledRule,
  setScheduledRuleState,
  putScheduledRuleTargets,
  removeScheduledRuleTargets,
} from '../../api/client';
import type {
  EventBridgeRuleItem,
  EventBridgeTargetItem,
  PutEventBridgeEventResult,
  ScheduledRuleDetail,
} from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';
import { EventBridgePatternBuilder } from './EventBridgePatternBuilder';
import { EventBridgeBusesManager } from './EventBridgeBusesManager';
import { EventBridgeTargetsManager } from './EventBridgeTargetsManager';

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

const scheduledColumns: DataListColumn[] = [
  { key: 'name', label: 'Rule' },
  { key: 'state', label: 'State' },
  { key: 'schedule', label: 'Schedule' },
  { key: 'detail', label: '' },
];

type ScheduledListState =
  | { kind: 'loading' }
  | { kind: 'ready'; rules: EventBridgeRuleItem[] }
  | { kind: 'error' };

type ScheduledDetailState =
  | { kind: 'idle' }
  | { kind: 'loading'; name: string }
  | { kind: 'ready'; detail: ScheduledRuleDetail }
  | { kind: 'error'; name: string };

type ActionState = 'idle' | 'busy' | 'error';

function CreateScheduledRuleForm({ onCreated }: { onCreated: () => void }) {
  const [name, setName] = useState('');
  const [schedule, setSchedule] = useState('');
  const [ruleState, setRuleState] = useState('ENABLED');
  const [description, setDescription] = useState('');
  const [bus, setBus] = useState('');
  const [status, setStatus] = useState<ActionState>('idle');

  const canSubmit = name.trim() !== '' && schedule.trim() !== '';

  const handleCreate = () => {
    setStatus('busy');
    const trimmedBus = bus.trim();
    const trimmedDescription = description.trim();
    putScheduledRule({
      name: name.trim(),
      scheduleExpression: schedule.trim(),
      state: ruleState,
      description: trimmedDescription === '' ? null : trimmedDescription,
      eventBusName: trimmedBus === '' ? null : trimmedBus,
    })
      .then(() => {
        setName('');
        setSchedule('');
        setRuleState('ENABLED');
        setDescription('');
        setBus('');
        setStatus('idle');
        onCreated();
      })
      .catch(() => setStatus('error'));
  };

  return (
    <div data-testid="eventbridge-scheduled-create" style={sectionStyle}>
      <h3 data-testid="eventbridge-scheduled-create-heading" style={sectionHeadingStyle}>
        Create scheduled rule
      </h3>
      <div style={formStyle}>
        <label style={labelStyle} htmlFor="eventbridge-scheduled-create-name">
          Name
        </label>
        <input
          id="eventbridge-scheduled-create-name"
          data-testid="eventbridge-scheduled-create-name"
          style={inputStyle}
          value={name}
          onChange={(event) => setName(event.target.value)}
        />
        <label style={labelStyle} htmlFor="eventbridge-scheduled-create-schedule">
          Schedule expression
        </label>
        <input
          id="eventbridge-scheduled-create-schedule"
          data-testid="eventbridge-scheduled-create-schedule"
          style={inputStyle}
          value={schedule}
          onChange={(event) => setSchedule(event.target.value)}
        />
        <label style={labelStyle} htmlFor="eventbridge-scheduled-create-state">
          State
        </label>
        <select
          id="eventbridge-scheduled-create-state"
          data-testid="eventbridge-scheduled-create-state"
          style={inputStyle}
          value={ruleState}
          onChange={(event) => setRuleState(event.target.value)}
        >
          <option value="ENABLED">ENABLED</option>
          <option value="DISABLED">DISABLED</option>
        </select>
        <label style={labelStyle} htmlFor="eventbridge-scheduled-create-description">
          Description (optional)
        </label>
        <input
          id="eventbridge-scheduled-create-description"
          data-testid="eventbridge-scheduled-create-description"
          style={inputStyle}
          value={description}
          onChange={(event) => setDescription(event.target.value)}
        />
        <label style={labelStyle} htmlFor="eventbridge-scheduled-create-bus">
          Event bus name (optional)
        </label>
        <input
          id="eventbridge-scheduled-create-bus"
          data-testid="eventbridge-scheduled-create-bus"
          style={inputStyle}
          value={bus}
          onChange={(event) => setBus(event.target.value)}
        />
        <button
          type="button"
          data-testid="eventbridge-scheduled-create-submit"
          style={buttonStyle}
          disabled={status === 'busy' || !canSubmit}
          onClick={handleCreate}
        >
          Create rule
        </button>
        {status === 'error' && (
          <p data-testid="eventbridge-scheduled-create-error" style={messageStyle}>
            Unable to create the scheduled rule.
          </p>
        )}
      </div>
    </div>
  );
}

function ScheduledRuleDetailPanel({ detail }: { detail: ScheduledRuleDetail }) {
  return (
    <div data-testid="eventbridge-scheduled-detail" style={formStyle}>
      <h3 style={sectionHeadingStyle}>{detail.name}</h3>
      <span style={labelStyle}>ARN</span>
      <span data-testid="eventbridge-scheduled-detail-arn" style={arnCellStyle}>
        {detail.arn}
      </span>
      <span style={labelStyle}>Event bus</span>
      <span data-testid="eventbridge-scheduled-detail-bus" style={arnCellStyle}>
        {detail.eventBusName}
      </span>
      <span style={labelStyle}>State</span>
      <span data-testid="eventbridge-scheduled-detail-state" style={stateBadgeStyle}>
        {detail.state}
      </span>
      <span style={labelStyle}>Schedule</span>
      <span data-testid="eventbridge-scheduled-detail-schedule" style={arnCellStyle}>
        {detail.scheduleExpression ?? '—'}
      </span>
      <span style={labelStyle}>Description</span>
      <span data-testid="eventbridge-scheduled-detail-description" style={messageStyle}>
        {detail.description ?? '—'}
      </span>
      <span style={labelStyle}>Role ARN</span>
      <span data-testid="eventbridge-scheduled-detail-role" style={arnCellStyle}>
        {detail.roleArn ?? '—'}
      </span>
      <span style={labelStyle}>Managed by</span>
      <span data-testid="eventbridge-scheduled-detail-managed" style={arnCellStyle}>
        {detail.managedBy ?? '—'}
      </span>
    </div>
  );
}

function ScheduledRuleActions({
  detail,
  onMutated,
  onDeleted,
}: {
  detail: ScheduledRuleDetail;
  onMutated: () => void;
  onDeleted: () => void;
}) {
  const [schedule, setSchedule] = useState(detail.scheduleExpression ?? '');
  const [editState, setEditState] = useState(detail.state);
  const [description, setDescription] = useState(detail.description ?? '');
  const [confirmingDelete, setConfirmingDelete] = useState(false);
  const [targetId, setTargetId] = useState('');
  const [targetArn, setTargetArn] = useState('');
  const [targetRole, setTargetRole] = useState('');
  const [targetInput, setTargetInput] = useState('');
  const [removeTargetId, setRemoveTargetId] = useState('');
  const [status, setStatus] = useState<ActionState>('idle');

  const bus = detail.eventBusName;
  const busy = status === 'busy';

  const run = (operation: Promise<void>, onSuccess: () => void) => {
    setStatus('busy');
    operation
      .then(() => {
        setStatus('idle');
        onSuccess();
      })
      .catch(() => setStatus('error'));
  };

  const handleToggle = () => {
    const nextState = detail.state === 'ENABLED' ? 'DISABLED' : 'ENABLED';
    run(setScheduledRuleState(detail.name, nextState, bus), onMutated);
  };

  const handleSaveEdit = () => {
    const trimmedDescription = description.trim();
    run(
      updateScheduledRule(
        detail.name,
        {
          scheduleExpression: schedule.trim(),
          state: editState,
          description: trimmedDescription === '' ? null : trimmedDescription,
        },
        bus,
      ),
      onMutated,
    );
  };

  const handleDelete = () => {
    run(deleteScheduledRule(detail.name, bus), onDeleted);
  };

  const handleAddTarget = () => {
    const trimmedRole = targetRole.trim();
    const trimmedInput = targetInput.trim();
    run(
      putScheduledRuleTargets(
        detail.name,
        [
          {
            id: targetId.trim(),
            arn: targetArn.trim(),
            roleArn: trimmedRole === '' ? null : trimmedRole,
            input: trimmedInput === '' ? null : trimmedInput,
          },
        ],
        bus,
      ),
      () => {
        setTargetId('');
        setTargetArn('');
        setTargetRole('');
        setTargetInput('');
        onMutated();
      },
    );
  };

  const handleRemoveTarget = () => {
    run(removeScheduledRuleTargets(detail.name, [removeTargetId.trim()], bus), () => {
      setRemoveTargetId('');
      onMutated();
    });
  };

  const canSaveEdit = schedule.trim() !== '';
  const canAddTarget = targetId.trim() !== '' && targetArn.trim() !== '';
  const canRemoveTarget = removeTargetId.trim() !== '';

  return (
    <div data-testid="eventbridge-scheduled-actions" style={formStyle}>
      <button
        type="button"
        data-testid="eventbridge-scheduled-toggle-state"
        style={buttonStyle}
        disabled={busy}
        onClick={handleToggle}
      >
        {detail.state === 'ENABLED' ? 'Disable' : 'Enable'}
      </button>

      <h4 data-testid="eventbridge-scheduled-edit-heading" style={sectionHeadingStyle}>
        Edit rule
      </h4>
      <label style={labelStyle} htmlFor="eventbridge-scheduled-edit-schedule">
        Schedule expression
      </label>
      <input
        id="eventbridge-scheduled-edit-schedule"
        data-testid="eventbridge-scheduled-edit-schedule"
        style={inputStyle}
        value={schedule}
        onChange={(event) => setSchedule(event.target.value)}
      />
      <label style={labelStyle} htmlFor="eventbridge-scheduled-edit-state">
        State
      </label>
      <select
        id="eventbridge-scheduled-edit-state"
        data-testid="eventbridge-scheduled-edit-state"
        style={inputStyle}
        value={editState}
        onChange={(event) => setEditState(event.target.value)}
      >
        <option value="ENABLED">ENABLED</option>
        <option value="DISABLED">DISABLED</option>
      </select>
      <label style={labelStyle} htmlFor="eventbridge-scheduled-edit-description">
        Description (optional)
      </label>
      <input
        id="eventbridge-scheduled-edit-description"
        data-testid="eventbridge-scheduled-edit-description"
        style={inputStyle}
        value={description}
        onChange={(event) => setDescription(event.target.value)}
      />
      <button
        type="button"
        data-testid="eventbridge-scheduled-edit-save"
        style={buttonStyle}
        disabled={busy || !canSaveEdit}
        onClick={handleSaveEdit}
      >
        Save changes
      </button>

      <h4 data-testid="eventbridge-scheduled-targets-heading" style={sectionHeadingStyle}>
        Targets
      </h4>
      <label style={labelStyle} htmlFor="eventbridge-scheduled-target-id">
        Target id
      </label>
      <input
        id="eventbridge-scheduled-target-id"
        data-testid="eventbridge-scheduled-target-id"
        style={inputStyle}
        value={targetId}
        onChange={(event) => setTargetId(event.target.value)}
      />
      <label style={labelStyle} htmlFor="eventbridge-scheduled-target-arn">
        Target ARN
      </label>
      <input
        id="eventbridge-scheduled-target-arn"
        data-testid="eventbridge-scheduled-target-arn"
        style={inputStyle}
        value={targetArn}
        onChange={(event) => setTargetArn(event.target.value)}
      />
      <label style={labelStyle} htmlFor="eventbridge-scheduled-target-role">
        Role ARN (optional)
      </label>
      <input
        id="eventbridge-scheduled-target-role"
        data-testid="eventbridge-scheduled-target-role"
        style={inputStyle}
        value={targetRole}
        onChange={(event) => setTargetRole(event.target.value)}
      />
      <label style={labelStyle} htmlFor="eventbridge-scheduled-target-input">
        Input (optional)
      </label>
      <input
        id="eventbridge-scheduled-target-input"
        data-testid="eventbridge-scheduled-target-input"
        style={inputStyle}
        value={targetInput}
        onChange={(event) => setTargetInput(event.target.value)}
      />
      <button
        type="button"
        data-testid="eventbridge-scheduled-target-add"
        style={buttonStyle}
        disabled={busy || !canAddTarget}
        onClick={handleAddTarget}
      >
        Add target
      </button>
      <label style={labelStyle} htmlFor="eventbridge-scheduled-target-remove-id">
        Remove target id
      </label>
      <input
        id="eventbridge-scheduled-target-remove-id"
        data-testid="eventbridge-scheduled-target-remove-id"
        style={inputStyle}
        value={removeTargetId}
        onChange={(event) => setRemoveTargetId(event.target.value)}
      />
      <button
        type="button"
        data-testid="eventbridge-scheduled-target-remove"
        style={buttonStyle}
        disabled={busy || !canRemoveTarget}
        onClick={handleRemoveTarget}
      >
        Remove target
      </button>

      {!confirmingDelete && (
        <button
          type="button"
          data-testid="eventbridge-scheduled-delete"
          style={buttonStyle}
          disabled={busy}
          onClick={() => setConfirmingDelete(true)}
        >
          Delete rule
        </button>
      )}
      {confirmingDelete && (
        <div data-testid="eventbridge-scheduled-delete-confirm" style={sectionStyle}>
          <span style={messageStyle}>Delete {detail.name}?</span>
          <button
            type="button"
            data-testid="eventbridge-scheduled-delete-yes"
            style={buttonStyle}
            disabled={busy}
            onClick={handleDelete}
          >
            Confirm delete
          </button>
          <button
            type="button"
            data-testid="eventbridge-scheduled-delete-cancel"
            style={buttonStyle}
            disabled={busy}
            onClick={() => setConfirmingDelete(false)}
          >
            Cancel
          </button>
        </div>
      )}
      {status === 'error' && (
        <p data-testid="eventbridge-scheduled-action-error" style={messageStyle}>
          The action could not be completed.
        </p>
      )}
    </div>
  );
}

function ScheduledRulesSection({ serviceKey }: { serviceKey: string }) {
  const [state, setState] = useState<ScheduledListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [detailState, setDetailState] = useState<ScheduledDetailState>({ kind: 'idle' });

  useEffect(() => {
    const controller = new AbortController();
    getScheduledRules(controller.signal)
      .then((result) => setState({ kind: 'ready', rules: result.rules }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = () => {
    setDetailState({ kind: 'idle' });
    setReloadToken((token) => token + 1);
  };

  const showDetailByName = (name: string, eventBusName: string) => {
    setDetailState({ kind: 'loading', name });
    getScheduledRule(name, eventBusName)
      .then((detail) => setDetailState({ kind: 'ready', detail }))
      .catch(() => setDetailState({ kind: 'error', name }));
  };

  const showDetail = (rule: EventBridgeRuleItem) => showDetailByName(rule.name, rule.eventBusName);

  const reloadAfterMutation = (name: string, eventBusName: string) => {
    setReloadToken((token) => token + 1);
    showDetailByName(name, eventBusName);
  };

  let detailPanel: ReactElement | null = null;
  if (detailState.kind === 'loading') {
    detailPanel = (
      <p data-testid="eventbridge-scheduled-detail-loading" style={messageStyle}>
        Loading {detailState.name}&hellip;
      </p>
    );
  } else if (detailState.kind === 'error') {
    detailPanel = (
      <p data-testid="eventbridge-scheduled-detail-error" style={messageStyle}>
        Unable to load {detailState.name}.
      </p>
    );
  } else if (detailState.kind === 'ready') {
    const current = detailState.detail;
    detailPanel = (
      <div data-testid="eventbridge-scheduled-detail-wrapper">
        <ScheduledRuleDetailPanel detail={current} />
        <ScheduledRuleActions
          key={`${current.name}:${current.state}:${current.scheduleExpression ?? ''}`}
          detail={current}
          onMutated={() => reloadAfterMutation(current.name, current.eventBusName)}
          onDeleted={refresh}
        />
      </div>
    );
  }

  let listContent: ReactElement;
  if (state.kind === 'loading') {
    listContent = (
      <p data-testid="eventbridge-scheduled-loading" style={messageStyle}>
        Loading scheduled rules&hellip;
      </p>
    );
  } else if (state.kind === 'error') {
    listContent = (
      <p data-testid="eventbridge-scheduled-error" style={messageStyle}>
        Unable to load scheduled rules.
      </p>
    );
  } else {
    const rows: DataListRow[] = state.rules.map((rule) => ({
      id: rule.name,
      filterText: `${rule.name} ${rule.state} ${rule.scheduleExpression ?? ''}`,
      cells: {
        name: (
          <span data-testid="eventbridge-scheduled-name" style={arnCellStyle}>
            {rule.name}
          </span>
        ),
        state: (
          <span data-testid="eventbridge-scheduled-state" style={stateBadgeStyle}>
            {rule.state}
          </span>
        ),
        schedule: rule.scheduleExpression ? (
          <span data-testid="eventbridge-scheduled-schedule" style={arnCellStyle}>
            {rule.scheduleExpression}
          </span>
        ) : (
          <span data-testid="eventbridge-scheduled-schedule-empty" style={mutedStyle}>
            &mdash;
          </span>
        ),
        detail: (
          <button
            type="button"
            data-testid="eventbridge-scheduled-view"
            style={buttonStyle}
            onClick={() => showDetail(rule)}
          >
            View
          </button>
        ),
      },
    }));

    listContent = (
      <DataListShell
        title="Scheduled rules"
        onRefresh={refresh}
        columns={scheduledColumns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter scheduled rules"
        columnPrefsKey={`${serviceKey}-scheduled-rules`}
        emptyState={{ message: 'No scheduled EventBridge rules found on this backend.' }}
      />
    );
  }

  return (
    <div data-testid="eventbridge-scheduled-section" style={sectionStyle}>
      <h3 style={sectionHeadingStyle}>Scheduled rules</h3>
      <CreateScheduledRuleForm onCreated={refresh} />
      {listContent}
      {detailPanel}
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
    setReloadToken((token) => token + 1);
  };

  let rulesContent: ReactElement;
  if (state.kind === 'loading') {
    rulesContent = (
      <p data-testid="eventbridge-list-loading" style={messageStyle}>
        Loading rules&hellip;
      </p>
    );
  } else if (state.kind === 'error') {
    rulesContent = (
      <p data-testid="eventbridge-list-error" style={messageStyle}>
        Unable to load EventBridge rules.
      </p>
    );
  } else {
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

    rulesContent = (
      <div data-testid="eventbridge-list-view">
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

  return (
    <div>
      <EventBridgeBusesManager />
      <EventBridgePatternBuilder onCreated={refresh} />
      <EventBridgeTargetsManager />
      <SendTestEventForm />
      {rulesContent}
      <ScheduledRulesSection serviceKey={serviceKey} />
    </div>
  );
}

export default EventBridgeListView;
