import { useCallback, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { detectStackDrift, getDriftStatus, getResourceDrifts } from '../../api/client';
import type {
  CloudFormationDriftStatusResult,
  CloudFormationResourceDrift,
} from '../../api/client';

const POLL_INTERVAL_MS = 1500;
const MAX_POLL_ATTEMPTS = 20;

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};
const sectionHeadingStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };
const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};
const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const tableStyle: CSSProperties = {
  borderCollapse: 'collapse',
  fontSize: 13,
  width: '100%',
};
const cellStyle: CSSProperties = {
  textAlign: 'left',
  padding: '4px 8px',
  border: '1px solid #30363d',
  fontFamily: 'monospace',
  verticalAlign: 'top',
};
const headerCellStyle: CSSProperties = {
  ...cellStyle,
  fontFamily: 'inherit',
  opacity: 0.7,
};
const propertiesStyle: CSSProperties = {
  margin: 0,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
  maxWidth: 320,
};

/**
 * Maps a drift status to a colour, so a drifted resource or stack stands out at a glance.
 */
function driftColor(status: string): string {
  if (status === 'IN_SYNC') {
    return '#3fb950';
  }
  if (status === 'NOT_CHECKED') {
    return '#d29922';
  }
  return '#f85149';
}

function delay(ms: number): Promise<void> {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

type DriftState =
  | { kind: 'idle' }
  | { kind: 'detecting' }
  | {
      kind: 'ready';
      status: CloudFormationDriftStatusResult;
      drifts: CloudFormationResourceDrift[];
    }
  | { kind: 'unsupported' };

interface CloudFormationDriftPanelProps {
  stackName: string;
}

export function CloudFormationDriftPanel({ stackName }: CloudFormationDriftPanelProps) {
  const [state, setState] = useState<DriftState>({ kind: 'idle' });

  const runDetection = useCallback(async () => {
    setState({ kind: 'detecting' });
    try {
      const detection = await detectStackDrift(stackName);
      let status = await getDriftStatus(detection.stackDriftDetectionId);
      let attempts = 0;
      while (status.detectionStatus === 'DETECTION_IN_PROGRESS' && attempts < MAX_POLL_ATTEMPTS) {
        await delay(POLL_INTERVAL_MS);
        status = await getDriftStatus(detection.stackDriftDetectionId);
        attempts += 1;
      }
      const resources = await getResourceDrifts(stackName);
      setState({ kind: 'ready', status, drifts: resources.drifts });
    } catch {
      setState({ kind: 'unsupported' });
    }
  }, [stackName]);

  return (
    <div style={sectionStyle}>
      <Heading as="h3" data-testid="cloudformation-drift-heading" style={sectionHeadingStyle}>
        Drift detection
      </Heading>
      <button
        type="button"
        data-testid="cloudformation-drift-detect"
        style={buttonStyle}
        disabled={state.kind === 'detecting'}
        onClick={() => {
          void runDetection();
        }}
      >
        {state.kind === 'detecting' ? 'Detecting\u2026' : 'Detect drift'}
      </button>
      {state.kind === 'detecting' ? (
        <p data-testid="cloudformation-drift-detecting" style={messageStyle}>
          Detecting drift&hellip;
        </p>
      ) : null}
      {state.kind === 'unsupported' ? (
        <p data-testid="cloudformation-drift-unsupported" style={messageStyle}>
          Drift detection is not available for this stack.
        </p>
      ) : null}
      {state.kind === 'ready' ? (
        <div style={sectionStyle}>
          <div style={rowStyle}>
            <span style={labelStyle}>Stack drift status</span>
            <span
              data-testid="cloudformation-drift-status"
              style={{ ...valueStyle, color: driftColor(state.status.stackDriftStatus) }}
            >
              {state.status.stackDriftStatus}
            </span>
          </div>
          <div style={rowStyle}>
            <span style={labelStyle}>Detection status</span>
            <span data-testid="cloudformation-drift-detection-status" style={valueStyle}>
              {state.status.detectionStatus}
            </span>
          </div>
          {state.status.detectionStatusReason !== null ? (
            <div style={rowStyle}>
              <span style={labelStyle}>Detection status reason</span>
              <span data-testid="cloudformation-drift-detection-reason" style={valueStyle}>
                {state.status.detectionStatusReason}
              </span>
            </div>
          ) : null}
          <div style={rowStyle}>
            <span style={labelStyle}>Drifted resources</span>
            <span data-testid="cloudformation-drift-count" style={valueStyle}>
              {state.status.driftedStackResourceCount}
            </span>
          </div>
          {state.drifts.length === 0 ? (
            <p data-testid="cloudformation-drift-resources-empty" style={messageStyle}>
              No resource drift detected.
            </p>
          ) : (
            <table data-testid="cloudformation-drift-resources" style={tableStyle}>
              <thead>
                <tr>
                  <th style={headerCellStyle}>Logical ID</th>
                  <th style={headerCellStyle}>Physical ID</th>
                  <th style={headerCellStyle}>Type</th>
                  <th style={headerCellStyle}>Drift status</th>
                  <th style={headerCellStyle}>Expected</th>
                  <th style={headerCellStyle}>Actual</th>
                </tr>
              </thead>
              <tbody>
                {state.drifts.map((drift) => (
                  <tr key={drift.logicalResourceId}>
                    <td style={cellStyle}>{drift.logicalResourceId}</td>
                    <td style={cellStyle}>{drift.physicalResourceId ?? '\u2014'}</td>
                    <td style={cellStyle}>{drift.resourceType}</td>
                    <td style={{ ...cellStyle, color: driftColor(drift.driftStatus) }}>
                      {drift.driftStatus}
                    </td>
                    <td style={cellStyle}>
                      <pre style={propertiesStyle}>{drift.expectedProperties ?? '\u2014'}</pre>
                    </td>
                    <td style={cellStyle}>
                      <pre style={propertiesStyle}>{drift.actualProperties ?? '\u2014'}</pre>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      ) : null}
    </div>
  );
}

export default CloudFormationDriftPanel;
