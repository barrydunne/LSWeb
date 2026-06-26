import { useEffect, useState, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { DataListShell } from '../../components/DataListShell';
import type { DataListColumn, DataListRow } from '../../components/DataListShell';
import { ConfirmationHost } from '../../components/ConfirmationHost';
import {
  createSchedule,
  createScheduleGroup,
  deleteSchedule,
  deleteScheduleGroup,
  getScheduleGroups,
  getSchedules,
} from '../../api/client';
import type { ScheduleGroupItem, ScheduleSummaryItem } from '../../api/client';
import type { ServiceListViewProps } from '../serviceViewRegistry';

const messageStyle: CSSProperties = { fontSize: 14 };

const arnCellStyle: CSSProperties = { fontFamily: 'monospace', fontSize: 12 };

const stateBadgeStyle: CSSProperties = {
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

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
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

const windowModes = ['OFF', 'FLEXIBLE'];
const scheduleStates = ['ENABLED', 'DISABLED'];

const columns: DataListColumn[] = [
  { key: 'name', label: 'Schedule' },
  { key: 'group', label: 'Group' },
  { key: 'state', label: 'State' },
  { key: 'target', label: 'Target' },
  { key: 'actions', label: 'Actions' },
];

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; schedules: ScheduleSummaryItem[] }
  | { kind: 'error' };

type CreateState = 'idle' | 'saving' | 'created' | 'error';

const scheduleTargetTypes: { value: string; label: string; token: string | null }[] = [
  { value: 'lambda', label: 'Lambda function', token: ':lambda:' },
  { value: 'sqs', label: 'SQS queue', token: ':sqs:' },
  { value: 'sns', label: 'SNS topic', token: ':sns:' },
  { value: 'other', label: 'Other / custom ARN', token: null },
];

export function SchedulerListView({ serviceKey }: ServiceListViewProps) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [groups, setGroups] = useState<ScheduleGroupItem[]>([]);
  const [groupFilter, setGroupFilter] = useState('');
  const [showGroups, setShowGroups] = useState(false);
  const [newGroupName, setNewGroupName] = useState('');
  const [groupCreateState, setGroupCreateState] = useState<CreateState>('idle');
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState('');
  const [groupName, setGroupName] = useState('default');
  const [scheduleExpression, setScheduleExpression] = useState('');
  const [scheduleExpressionTimezone, setScheduleExpressionTimezone] = useState('');
  const [targetArn, setTargetArn] = useState('');
  const [targetType, setTargetType] = useState('lambda');
  const [targetInput, setTargetInput] = useState('');
  const [roleArn, setRoleArn] = useState('');
  const [flexibleTimeWindowMode, setFlexibleTimeWindowMode] = useState('OFF');
  const [maximumWindowInMinutes, setMaximumWindowInMinutes] = useState('');
  const [scheduleState, setScheduleState] = useState('ENABLED');
  const [createState, setCreateState] = useState<CreateState>('idle');
  const [createError, setCreateError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    getSchedules(controller.signal)
      .then((result) => setState({ kind: 'ready', schedules: result.schedules }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [reloadToken]);

  useEffect(() => {
    const controller = new AbortController();
    getScheduleGroups(controller.signal)
      .then((result) => setGroups(result.groups))
      .catch(() => setGroups([]));
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    const trimmedArn = targetArn.trim();
    const trimmedInput = targetInput.trim();
    const selectedType = scheduleTargetTypes.find((candidate) => candidate.value === targetType);
    if (selectedType?.token && !trimmedArn.includes(selectedType.token)) {
      setCreateError(`The target ARN does not look like a ${selectedType.label} ARN (expected to contain "${selectedType.token}").`);
      setCreateState('error');
      return;
    }
    if (trimmedInput !== '') {
      try {
        JSON.parse(trimmedInput);
      } catch {
        setCreateError('The target payload must be valid JSON.');
        setCreateState('error');
        return;
      }
    }
    setCreateError(null);
    setCreateState('saving');
    const trimmedWindow = maximumWindowInMinutes.trim();
    createSchedule({
      name,
      groupName,
      scheduleExpression,
      scheduleExpressionTimezone:
        scheduleExpressionTimezone.trim() === '' ? null : scheduleExpressionTimezone.trim(),
      description: null,
      startDate: null,
      endDate: null,
      targetArn,
      roleArn,
      flexibleTimeWindowMode,
      maximumWindowInMinutes: trimmedWindow === '' ? null : Number(trimmedWindow),
      state: scheduleState,
      targetInput: trimmedInput === '' ? null : trimmedInput,
    })
      .then(() => {
        setCreateState('created');
        setName('');
        setGroupName('default');
        setScheduleExpression('');
        setScheduleExpressionTimezone('');
        setTargetArn('');
        setTargetInput('');
        setRoleArn('');
        setFlexibleTimeWindowMode('OFF');
        setMaximumWindowInMinutes('');
        setScheduleState('ENABLED');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleDelete = useCallback(
    (scheduleName: string, group: string) => {
      deleteSchedule(scheduleName, group)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  const handleCreateGroup = () => {
    setGroupCreateState('saving');
    createScheduleGroup({ name: newGroupName })
      .then(() => {
        setGroupCreateState('created');
        setNewGroupName('');
        refresh();
      })
      .catch(() => setGroupCreateState('error'));
  };

  const handleDeleteGroup = useCallback(
    (name: string) => {
      deleteScheduleGroup(name)
        .then(() => refresh())
        .catch(() => setState({ kind: 'error' }));
    },
    [refresh],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="scheduler-list-loading" style={messageStyle}>
        Loading schedules&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="scheduler-list-error" style={messageStyle}>
        Unable to load EventBridge Scheduler schedules.
      </p>
    );
  }

  const visibleSchedules =
    groupFilter === ''
      ? state.schedules
      : state.schedules.filter((schedule) => schedule.groupName === groupFilter);

  const rows: DataListRow[] = visibleSchedules.map((schedule) => ({
    id: schedule.arn,
    filterText: `${schedule.name} ${schedule.groupName} ${schedule.state} ${schedule.targetArn}`,
    cells: {
      name: (
        <Link
          data-testid="scheduler-list-name"
          to={`/services/${serviceKey}/${encodeURIComponent(`${schedule.groupName}/${schedule.name}`)}`}
        >
          {schedule.name}
        </Link>
      ),
      group: (
        <span data-testid="scheduler-list-group" style={arnCellStyle}>
          {schedule.groupName}
        </span>
      ),
      state: (
        <span data-testid="scheduler-list-state" style={stateBadgeStyle}>
          {schedule.state}
        </span>
      ),
      target: (
        <span data-testid="scheduler-list-target" style={arnCellStyle}>
          {schedule.targetArn}
        </span>
      ),
      actions: (
        <ConfirmationHost
          actionLabel="Delete"
          prompt={`Delete ${schedule.groupName}/${schedule.name}?`}
          confirmLabel="Confirm"
          onConfirm={() => handleDelete(schedule.name, schedule.groupName)}
        />
      ),
    },
  }));

  return (
    <div data-testid="scheduler-list-view">
      <button
        type="button"
        data-testid="scheduler-groups-toggle"
        style={buttonStyle}
        onClick={() => setShowGroups((current) => !current)}
      >
        {showGroups ? 'Hide groups' : 'Manage groups'}
      </button>
      {showGroups ? (
        <div data-testid="scheduler-groups-panel" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-group-name">
              New group name
            </label>
            <input
              id="scheduler-group-name"
              type="text"
              data-testid="scheduler-group-name"
              style={inputStyle}
              value={newGroupName}
              onChange={(event) => setNewGroupName(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="scheduler-group-submit"
            style={buttonStyle}
            disabled={groupCreateState === 'saving'}
            onClick={handleCreateGroup}
          >
            {groupCreateState === 'saving' ? 'Creating\u2026' : 'Create group'}
          </button>
          {groupCreateState === 'created' ? (
            <p data-testid="scheduler-group-status" style={messageStyle}>
              Schedule group created.
            </p>
          ) : null}
          {groupCreateState === 'error' ? (
            <p data-testid="scheduler-group-error" style={messageStyle}>
              Unable to create the schedule group.
            </p>
          ) : null}
          <ul data-testid="scheduler-groups-list" style={{ margin: 0, paddingLeft: 16 }}>
            {groups.map((group) => (
              <li key={group.arn} data-testid="scheduler-group-item">
                <span data-testid="scheduler-group-item-name">{group.name}</span>
                <span style={stateBadgeStyle}>{group.state}</span>
                {group.name === 'default' ? null : (
                  <ConfirmationHost
                    actionLabel="Delete"
                    prompt={`Delete schedule group ${group.name}?`}
                    confirmLabel="Confirm"
                    onConfirm={() => handleDeleteGroup(group.name)}
                  />
                )}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
      <div style={fieldRowStyle}>
        <label style={labelStyle} htmlFor="scheduler-group-filter">
          Group filter
        </label>
        <select
          id="scheduler-group-filter"
          data-testid="scheduler-group-filter"
          style={inputStyle}
          value={groupFilter}
          onChange={(event) => setGroupFilter(event.target.value)}
        >
          <option value="">All groups</option>
          {groups.map((group) => (
            <option key={group.arn} value={group.name}>
              {group.name}
            </option>
          ))}
        </select>
      </div>
      <button
        type="button"
        data-testid="scheduler-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create schedule'}
      </button>
      {showCreate ? (
        <div data-testid="scheduler-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-name">
              Schedule name
            </label>
            <input
              id="scheduler-create-name"
              type="text"
              data-testid="scheduler-create-name"
              style={inputStyle}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-group">
              Group name
            </label>
            <input
              id="scheduler-create-group"
              type="text"
              data-testid="scheduler-create-group"
              style={inputStyle}
              value={groupName}
              onChange={(event) => setGroupName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-expression">
              Schedule expression
            </label>
            <input
              id="scheduler-create-expression"
              type="text"
              data-testid="scheduler-create-expression"
              style={inputStyle}
              value={scheduleExpression}
              onChange={(event) => setScheduleExpression(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-timezone">
              Timezone (optional)
            </label>
            <input
              id="scheduler-create-timezone"
              type="text"
              data-testid="scheduler-create-timezone"
              style={inputStyle}
              placeholder="e.g. Europe/Dublin"
              value={scheduleExpressionTimezone}
              onChange={(event) => setScheduleExpressionTimezone(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-target-type">
              Target type
            </label>
            <select
              id="scheduler-create-target-type"
              data-testid="scheduler-create-target-type"
              style={inputStyle}
              value={targetType}
              onChange={(event) => setTargetType(event.target.value)}
            >
              {scheduleTargetTypes.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-target">
              Target ARN
            </label>
            <input
              id="scheduler-create-target"
              type="text"
              data-testid="scheduler-create-target"
              style={inputStyle}
              value={targetArn}
              onChange={(event) => setTargetArn(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-payload">
              Target payload (optional JSON)
            </label>
            <input
              id="scheduler-create-payload"
              type="text"
              data-testid="scheduler-create-payload"
              style={inputStyle}
              placeholder='e.g. {"key":"value"}'
              value={targetInput}
              onChange={(event) => setTargetInput(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-role">
              Role ARN
            </label>
            <input
              id="scheduler-create-role"
              type="text"
              data-testid="scheduler-create-role"
              style={inputStyle}
              value={roleArn}
              onChange={(event) => setRoleArn(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-mode">
              Flexible time window
            </label>
            <select
              id="scheduler-create-mode"
              data-testid="scheduler-create-mode"
              style={inputStyle}
              value={flexibleTimeWindowMode}
              onChange={(event) => setFlexibleTimeWindowMode(event.target.value)}
            >
              {windowModes.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>
          {flexibleTimeWindowMode === 'FLEXIBLE' ? (
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="scheduler-create-window">
                Maximum window (minutes)
              </label>
              <input
                id="scheduler-create-window"
                type="number"
                data-testid="scheduler-create-window"
                style={inputStyle}
                value={maximumWindowInMinutes}
                onChange={(event) => setMaximumWindowInMinutes(event.target.value)}
              />
            </div>
          ) : null}
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="scheduler-create-state">
              State
            </label>
            <select
              id="scheduler-create-state"
              data-testid="scheduler-create-state"
              style={inputStyle}
              value={scheduleState}
              onChange={(event) => setScheduleState(event.target.value)}
            >
              {scheduleStates.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>
          <button
            type="button"
            data-testid="scheduler-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
        </div>
      ) : null}
      {createState === 'created' ? (
        <p data-testid="scheduler-create-status" style={messageStyle}>
          Schedule created.
        </p>
      ) : null}
      {createState === 'error' ? (
        <p data-testid="scheduler-create-error" style={messageStyle}>
          {createError ?? 'Unable to create the schedule.'}
        </p>
      ) : null}
      <DataListShell
        title="Schedules"
        onRefresh={refresh}
        columns={columns}
        rows={rows}
        itemCount={rows.length}
        filterPlaceholder="Filter schedules"
        columnPrefsKey={`${serviceKey}-schedules`}
        emptyState={{ message: 'No EventBridge Scheduler schedules found on this backend.' }}
      />
    </div>
  );
}

export default SchedulerListView;
