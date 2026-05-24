import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { executeDynamoDbStatement } from '../../api/client';
import type { DynamoDbItem } from '../../api/client';

const STATEMENT_LIMIT = 25;

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const headerStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 8,
};

const sectionHeadingStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const fieldStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const textAreaStyle: CSSProperties = {
  fontFamily: 'monospace',
  fontSize: 13,
  minHeight: 96,
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
  color: 'inherit',
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

const itemStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const preStyle: CSSProperties = {
  margin: 0,
  fontFamily: 'monospace',
  fontSize: 13,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
};

export interface DynamoDbStatementPanelProps {
  tableName: string;
}

type StatementState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'ready'; items: DynamoDbItem[]; nextToken: string | null; statement: string }
  | { kind: 'error' };

export function DynamoDbStatementPanel({ tableName }: DynamoDbStatementPanelProps) {
  const [statement, setStatement] = useState(`SELECT * FROM "${tableName}"`);
  const [state, setState] = useState<StatementState>({ kind: 'idle' });
  const [loadingMore, setLoadingMore] = useState(false);
  const [loadMoreFailed, setLoadMoreFailed] = useState(false);

  const runStatement = () => {
    const text = statement;
    setState({ kind: 'loading' });
    setLoadMoreFailed(false);
    executeDynamoDbStatement({ statement: text, limit: STATEMENT_LIMIT, nextToken: null })
      .then((result) =>
        setState({
          kind: 'ready',
          items: result.items,
          nextToken: result.nextToken,
          statement: text,
        }),
      )
      .catch(() => setState({ kind: 'error' }));
  };

  const loadMore = (items: DynamoDbItem[], token: string, text: string) => {
    setLoadingMore(true);
    setLoadMoreFailed(false);
    executeDynamoDbStatement({ statement: text, limit: STATEMENT_LIMIT, nextToken: token })
      .then((result) =>
        setState({
          kind: 'ready',
          items: [...items, ...result.items],
          nextToken: result.nextToken,
          statement: text,
        }),
      )
      .catch(() => setLoadMoreFailed(true))
      .finally(() => setLoadingMore(false));
  };

  return (
    <div data-testid="dynamodb-statement-panel" style={sectionStyle}>
      <div style={headerStyle}>
        <Heading as="h4" style={sectionHeadingStyle}>
          PartiQL editor
        </Heading>
      </div>

      <div style={formStyle}>
        <label style={fieldStyle}>
          <span style={labelStyle}>Statement</span>
          <textarea
            data-testid="dynamodb-statement-input"
            style={textAreaStyle}
            value={statement}
            onChange={(event) => setStatement(event.target.value)}
          />
        </label>

        <button
          type="button"
          data-testid="dynamodb-statement-run"
          style={buttonStyle}
          disabled={state.kind === 'loading' || statement.trim() === ''}
          onClick={runStatement}
        >
          {state.kind === 'loading' ? 'Running\u2026' : 'Run'}
        </button>
      </div>

      {state.kind === 'loading' ? (
        <p data-testid="dynamodb-statement-loading" style={messageStyle}>
          Running&hellip;
        </p>
      ) : null}
      {state.kind === 'error' ? (
        <p data-testid="dynamodb-statement-error" style={messageStyle}>
          Unable to run this statement.
        </p>
      ) : null}
      {state.kind === 'ready' && state.items.length === 0 ? (
        <p data-testid="dynamodb-statement-empty" style={messageStyle}>
          The statement returned no items.
        </p>
      ) : null}
      {state.kind === 'ready' && state.items.length > 0 ? (
        <>
          {state.items.map((item, index) => (
            <div key={index} data-testid={`dynamodb-statement-result-${index}`} style={itemStyle}>
              <pre data-testid={`dynamodb-statement-result-json-${index}`} style={preStyle}>
                {item.json}
              </pre>
            </div>
          ))}
          {state.nextToken !== null ? (
            <button
              type="button"
              data-testid="dynamodb-statement-load-more"
              style={buttonStyle}
              disabled={loadingMore}
              onClick={() => loadMore(state.items, state.nextToken!, state.statement)}
            >
              {loadingMore ? 'Loading\u2026' : 'Load more'}
            </button>
          ) : null}
          {loadMoreFailed ? (
            <p data-testid="dynamodb-statement-load-more-error" style={messageStyle}>
              Unable to load more items.
            </p>
          ) : null}
        </>
      ) : null}
    </div>
  );
}

export default DynamoDbStatementPanel;
