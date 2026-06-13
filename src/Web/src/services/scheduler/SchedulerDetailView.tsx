import { useEffect, useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { getSchedule, updateSchedule } from '../../api/client';
import type { ScheduleDetailResult } from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { ResourceLink } from '../../components/ResourceLink';
import { RawJsonViewer } from '../../components/RawJsonViewer';
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

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };
const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
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

const windowModes = ['OFF', 'FLEXIBLE'];
const scheduleStates = ['ENABLED', 'DISABLED'];

function splitResourceId(resourceId: string): { group: string; name: string } {
  const separator = resourceId.indexOf('/');
  if (separator < 0) {
    return { group: 'default', name: resourceId };
  }
  return {
    group: resourceId.slice(0, separator),
    name: resourceId.slice(separator + 1),
  };
}

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; schedule: ScheduleDetailResult }
  | { kind: 'error' };

type SaveState = 'idle' | 'saving' | 'saved' | 'error';

export function SchedulerDetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [showEdit, setShowEdit] = useState(false);
  const [editExpression, setEditExpression] = useState('');
  const [editTimezone, setEditTimezone] = useState('');
  const [editTargetArn, setEditTargetArn] = useState('');
  const [editTargetInput, setEditTargetInput] = useState('');
  const [editRoleArn, setEditRoleArn] = useState('');
  const [editMode, setEditMode] = useState('OFF');
  const [editWindow, setEditWindow] = useState('');
  const [editState, setEditState] = useState('ENABLED');
  const [saveState, setSaveState] = useState<SaveState>('idle');
  const [saveError, setSaveError] = useState<string | null>(null);

  const { group, name } = useMemo(() => splitResourceId(resourceId), [resourceId]);

  useEffect(() => {
    const controller = new AbortController();
    getSchedule(name, group, controller.signal)
      .then((schedule) => setState({ kind: 'ready', schedule }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [name, group, reloadToken]);

  const handleToggleEdit = () => {
    setShowEdit((current) => {
      const next = !current;
      if (next && state.kind === 'ready') {
        setSaveState('idle');
        setEditExpression(state.schedule.scheduleExpression);
        setEditTimezone(state.schedule.scheduleExpressionTimezone ?? '');
        setEditTargetArn(state.schedule.targetArn);
        setEditTargetInput(state.schedule.targetInput ?? '');
        setEditRoleArn(state.schedule.roleArn);
        setEditMode(state.schedule.flexibleTimeWindowMode);
        setEditWindow(
          state.schedule.maximumWindowInMinutes === null
            ? ''
            : String(state.schedule.maximumWindowInMinutes),
        );
        setEditState(state.schedule.state);
      }
      return next;
    });
  };

  const handleSave = () => {
    const trimmedInput = editTargetInput.trim();
    if (trimmedInput !== '') {
      try {
        JSON.parse(trimmedInput);
      } catch {
        setSaveError('The target payload must be valid JSON.');
        setSaveState('error');
        return;
      }
    }
    setSaveError(null);
    setSaveState('saving');
    const trimmedWindow = editWindow.trim();
    updateSchedule(name, group, {
      scheduleExpression: editExpression,
      scheduleExpressionTimezone: editTimezone.trim() === '' ? null : editTimezone.trim(),
      description: null,
      startDate: null,
      endDate: null,
      targetArn: editTargetArn,
      roleArn: editRoleArn,
      flexibleTimeWindowMode: editMode,
      maximumWindowInMinutes: trimmedWindow === '' ? null : Number(trimmedWindow),
      state: editState,
      targetInput: trimmedInput === '' ? null : trimmedInput,
    })
      .then(() => {
        setSaveState('saved');
        setShowEdit(false);
        setReloadToken((token) => token + 1);
      })
      .catch(() => setSaveState('error'));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="scheduler-detail-loading" style={messageStyle}>
        Loading schedule&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="scheduler-detail-error" style={messageStyle}>
        Unable to load the schedule.
      </p>
    );
  }

  const schedule = state.schedule;

  return (
    <div data-testid="scheduler-detail-view" style={containerStyle}>
      <Heading as="h2" data-testid="scheduler-detail-name" style={{ fontSize: 18 }}>
        {schedule.name}
      </Heading>
      <div style={rowStyle}>
        <span style={labelStyle}>Group</span>
        <span data-testid="scheduler-detail-group" style={valueStyle}>
          {schedule.groupName}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>State</span>
        <span data-testid="scheduler-detail-state" style={valueStyle}>
          {schedule.state}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Schedule expression</span>
        <span data-testid="scheduler-detail-expression" style={valueStyle}>
          {schedule.scheduleExpression}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Timezone</span>
        <span data-testid="scheduler-detail-timezone" style={valueStyle}>
          {schedule.scheduleExpressionTimezone ?? '—'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Description</span>
        <span data-testid="scheduler-detail-description" style={valueStyle}>
          {schedule.description ?? '—'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Start date</span>
        <span data-testid="scheduler-detail-start" style={valueStyle}>
          {schedule.startDate ?? '—'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>End date</span>
        <span data-testid="scheduler-detail-end" style={valueStyle}>
          {schedule.endDate ?? '—'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Target</span>
        <span data-testid="scheduler-detail-target" style={valueStyle}>
          {schedule.targetArn}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Target payload</span>
        <span data-testid="scheduler-detail-target-input" style={valueStyle}>
          {schedule.targetInput ?? '\u2014'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Role</span>
        <span data-testid="scheduler-detail-role" style={valueStyle}>
          <ResourceLink reference={schedule.roleArn} service="iam" />
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Flexible time window</span>
        <span data-testid="scheduler-detail-flexible" style={valueStyle}>
          {schedule.flexibleTimeWindowMode}
          {schedule.maximumWindowInMinutes !== null
            ? ` (${schedule.maximumWindowInMinutes} min)`
            : ''}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>ARN</span>
        <span data-testid="scheduler-detail-arn" style={valueStyle}>
          {schedule.arn}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Created</span>
        <span data-testid="scheduler-detail-created" style={valueStyle}>
          {schedule.creationDate ?? '—'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Last modified</span>
        <span data-testid="scheduler-detail-modified" style={valueStyle}>
          {schedule.lastModificationDate ?? '—'}
        </span>
      </div>
      <div data-testid="scheduler-detail-raw" style={sectionStyle}>
        <RawJsonViewer value={schedule} title="Raw schedule" />
      </div>
      <button
        type="button"
        data-testid="scheduler-detail-edit-toggle"
        style={buttonStyle}
        onClick={handleToggleEdit}
      >
        {showEdit ? 'Cancel' : 'Edit schedule'}
      </button>
      {showEdit ? (
        <div data-testid="scheduler-detail-edit-form" style={sectionStyle}>
          <div style={rowStyle}>
            <label style={labelStyle} htmlFor="scheduler-detail-edit-expression">
              Schedule expression
            </label>
            <input
              id="scheduler-detail-edit-expression"
              type="text"
              data-testid="scheduler-detail-edit-expression"
              style={inputStyle}
              value={editExpression}
              onChange={(event) => setEditExpression(event.target.value)}
            />
          </div>
          <div style={rowStyle}>
            <label style={labelStyle} htmlFor="scheduler-detail-edit-timezone">
              Timezone
            </label>
            <input
              id="scheduler-detail-edit-timezone"
              type="text"
              data-testid="scheduler-detail-edit-timezone"
              style={inputStyle}
              value={editTimezone}
              onChange={(event) => setEditTimezone(event.target.value)}
            />
          </div>
          <div style={rowStyle}>
            <label style={labelStyle} htmlFor="scheduler-detail-edit-target">
              Target ARN
            </label>
            <input
              id="scheduler-detail-edit-target"
              type="text"
              data-testid="scheduler-detail-edit-target"
              style={inputStyle}
              value={editTargetArn}
              onChange={(event) => setEditTargetArn(event.target.value)}
            />
          </div>
          <div style={rowStyle}>
            <label style={labelStyle} htmlFor="scheduler-detail-edit-payload">
              Target payload (optional JSON)
            </label>
            <input
              id="scheduler-detail-edit-payload"
              type="text"
              data-testid="scheduler-detail-edit-payload"
              style={inputStyle}
              value={editTargetInput}
              onChange={(event) => setEditTargetInput(event.target.value)}
            />
          </div>
          <div style={rowStyle}>
            <label style={labelStyle} htmlFor="scheduler-detail-edit-role">
              Role ARN
            </label>
            <input
              id="scheduler-detail-edit-role"
              type="text"
              data-testid="scheduler-detail-edit-role"
              style={inputStyle}
              value={editRoleArn}
              onChange={(event) => setEditRoleArn(event.target.value)}
            />
          </div>
          <div style={rowStyle}>
            <label style={labelStyle} htmlFor="scheduler-detail-edit-mode">
              Flexible time window
            </label>
            <select
              id="scheduler-detail-edit-mode"
              data-testid="scheduler-detail-edit-mode"
              style={inputStyle}
              value={editMode}
              onChange={(event) => setEditMode(event.target.value)}
            >
              {windowModes.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>
          {editMode === 'FLEXIBLE' ? (
            <div style={rowStyle}>
              <label style={labelStyle} htmlFor="scheduler-detail-edit-window">
                Maximum window (minutes)
              </label>
              <input
                id="scheduler-detail-edit-window"
                type="number"
                data-testid="scheduler-detail-edit-window"
                style={inputStyle}
                value={editWindow}
                onChange={(event) => setEditWindow(event.target.value)}
              />
            </div>
          ) : null}
          <div style={rowStyle}>
            <label style={labelStyle} htmlFor="scheduler-detail-edit-state">
              State
            </label>
            <select
              id="scheduler-detail-edit-state"
              data-testid="scheduler-detail-edit-state"
              style={inputStyle}
              value={editState}
              onChange={(event) => setEditState(event.target.value)}
            >
              {scheduleStates.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>
          <ConfirmationHost
            actionLabel="Save schedule"
            prompt={`Update ${schedule.groupName}/${schedule.name}?`}
            confirmLabel="Confirm save"
            onConfirm={handleSave}
          />
        </div>
      ) : null}
      {saveState === 'saved' ? (
        <p data-testid="scheduler-detail-save-status" style={messageStyle}>
          Schedule updated.
        </p>
      ) : null}
      {saveState === 'error' ? (
        <p data-testid="scheduler-detail-save-error" style={messageStyle}>
          {saveError ?? 'Unable to update the schedule.'}
        </p>
      ) : null}
    </div>
  );
}

export default SchedulerDetailView;
