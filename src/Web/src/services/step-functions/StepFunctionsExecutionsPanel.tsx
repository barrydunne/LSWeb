import { Fragment, useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { getExecutions, startExecution } from '../../api/client';
import type { ExecutionSummary } from '../../api/client';
import { StepFunctionsExecutionHistoryPanel } from './StepFunctionsExecutionHistoryPanel';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const sectionHeadingStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

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

const tableStyle: CSSProperties = {
  width: '100%',
  borderCollapse: 'collapse',
  fontSize: 13,
};

const cellStyle: CSSProperties = {
  textAlign: 'left',
  padding: '4px 8px',
  borderBottom: '1px solid #30363d',
  fontFamily: 'monospace',
};

const headerCellStyle: CSSProperties = {
  ...cellStyle,
  fontFamily: 'inherit',
  opacity: 0.7,
};

const historyButtonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

type ListState =
  | { kind: 'loading' }
  | { kind: 'ready'; executions: ExecutionSummary[] }
  | { kind: 'error' };

type StartState = 'idle' | 'saving' | 'started' | 'error';

export function StepFunctionsExecutionsPanel({ stateMachineArn }: { stateMachineArn: string }) {
  const [state, setState] = useState<ListState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);
  const [name, setName] = useState('');
  const [input, setInput] = useState('');
  const [startState, setStartState] = useState<StartState>('idle');
  const [historyArn, setHistoryArn] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    getExecutions(stateMachineArn, controller.signal)
      .then((result) => setState({ kind: 'ready', executions: result.executions }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [stateMachineArn, reloadToken]);

  const refresh = useCallback(() => {
    setState({ kind: 'loading' });
    setReloadToken((token) => token + 1);
  }, []);

  const handleStart = () => {
    setStartState('saving');
    const trimmedName = name.trim();
    const trimmedInput = input.trim();
    startExecution({
      stateMachineArn,
      name: trimmedName === '' ? null : trimmedName,
      input: trimmedInput === '' ? null : trimmedInput,
    })
      .then(() => {
        setStartState('started');
        setName('');
        setInput('');
        refresh();
      })
      .catch(() => setStartState('error'));
  };

  return (
    <div data-testid="step-functions-executions-panel" style={sectionStyle}>
      <Heading
        as="h3"
        data-testid="step-functions-executions-heading"
        style={sectionHeadingStyle}
      >
        Executions
      </Heading>
      <div style={formStyle}>
        <label style={labelStyle} htmlFor="step-functions-execution-name">
          Name (optional)
        </label>
        <input
          id="step-functions-execution-name"
          data-testid="step-functions-execution-name"
          style={inputStyle}
          value={name}
          onChange={(event) => setName(event.target.value)}
        />
        <label style={labelStyle} htmlFor="step-functions-execution-input">
          Input JSON (optional)
        </label>
        <textarea
          id="step-functions-execution-input"
          data-testid="step-functions-execution-input"
          style={textAreaStyle}
          value={input}
          onChange={(event) => setInput(event.target.value)}
        />
        <button
          type="button"
          data-testid="step-functions-execution-start"
          style={buttonStyle}
          disabled={startState === 'saving'}
          onClick={handleStart}
        >
          Start execution
        </button>
        {startState === 'error' && (
          <p data-testid="step-functions-execution-start-error" style={messageStyle}>
            Unable to start the execution.
          </p>
        )}
      </div>
      {state.kind === 'loading' && (
        <p data-testid="step-functions-executions-loading" style={messageStyle}>
          Loading executions&hellip;
        </p>
      )}
      {state.kind === 'error' && (
        <p data-testid="step-functions-executions-error" style={messageStyle}>
          Unable to load executions.
        </p>
      )}
      {state.kind === 'ready' && state.executions.length === 0 && (
        <p data-testid="step-functions-executions-empty" style={messageStyle}>
          No executions found.
        </p>
      )}
      {state.kind === 'ready' && state.executions.length > 0 && (
        <table data-testid="step-functions-executions-table" style={tableStyle}>
          <thead>
            <tr>
              <th style={headerCellStyle}>Name</th>
              <th style={headerCellStyle}>Status</th>
              <th style={headerCellStyle}>Started</th>
              <th style={headerCellStyle}>Stopped</th>
              <th style={headerCellStyle}>History</th>
            </tr>
          </thead>
          <tbody>
            {state.executions.map((execution) => (
              <Fragment key={execution.executionArn}>
                <tr data-testid="step-functions-execution-row">
                  <td style={cellStyle}>{execution.name}</td>
                  <td style={cellStyle} data-testid="step-functions-execution-status">
                    {execution.status}
                  </td>
                  <td style={cellStyle}>{execution.startDate}</td>
                  <td style={cellStyle}>{execution.stopDate ?? '—'}</td>
                  <td style={cellStyle}>
                    <button
                      type="button"
                      data-testid="step-functions-execution-history-toggle"
                      style={historyButtonStyle}
                      onClick={() =>
                        setHistoryArn((current) =>
                          current === execution.executionArn ? null : execution.executionArn,
                        )
                      }
                    >
                      {historyArn === execution.executionArn ? 'Hide history' : 'View history'}
                    </button>
                  </td>
                </tr>
                {historyArn === execution.executionArn && (
                  <tr data-testid="step-functions-execution-history-row">
                    <td style={cellStyle} colSpan={5}>
                      <StepFunctionsExecutionHistoryPanel executionArn={execution.executionArn} />
                    </td>
                  </tr>
                )}
              </Fragment>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

export default StepFunctionsExecutionsPanel;
