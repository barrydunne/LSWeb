import type { CSSProperties } from 'react';
import { parseStateMachineGraph } from './aslGraph';

const graphStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const nodeStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const nodeHeaderStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
  flexWrap: 'wrap',
};

const nameStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace', fontWeight: 600 };
const typeBadgeStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 8px',
  borderRadius: 10,
  border: '1px solid #30363d',
  opacity: 0.85,
};
const tagStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 8px',
  borderRadius: 10,
  border: '1px solid #2ea043',
  color: '#3fb950',
};
const terminalTagStyle: CSSProperties = {
  fontSize: 11,
  padding: '1px 8px',
  borderRadius: 10,
  border: '1px solid #6e7681',
  opacity: 0.85,
};
const transitionStyle: CSSProperties = { fontSize: 12, fontFamily: 'monospace', opacity: 0.85 };
const messageStyle: CSSProperties = { fontSize: 13, opacity: 0.7 };

export function StateMachineGraph({ definition }: { definition: string }) {
  const nodes = parseStateMachineGraph(definition);

  if (nodes === null || nodes.length === 0) {
    return (
      <p data-testid="step-functions-graph-empty" style={messageStyle}>
        The state machine definition could not be parsed into a graph.
      </p>
    );
  }

  return (
    <div data-testid="step-functions-graph" style={graphStyle}>
      {nodes.map((node) => (
        <div
          key={node.name}
          data-testid="step-functions-graph-node"
          data-state-name={node.name}
          style={nodeStyle}
        >
          <div style={nodeHeaderStyle}>
            <span style={nameStyle}>{node.name}</span>
            <span style={typeBadgeStyle}>{node.type}</span>
            {node.isStart ? <span style={tagStyle}>Start</span> : null}
            {node.isTerminal ? <span style={terminalTagStyle}>End</span> : null}
          </div>
          {node.transitions.length > 0 ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {node.transitions.map((transition, index) => (
                <span
                  key={`${transition.label}-${transition.target}-${index}`}
                  data-testid="step-functions-graph-transition"
                  style={transitionStyle}
                >
                  {transition.label} &rarr; {transition.target}
                </span>
              ))}
            </div>
          ) : null}
        </div>
      ))}
    </div>
  );
}

export default StateMachineGraph;
