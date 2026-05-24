import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { queryDynamoDbTable } from '../../api/client';
import type { DynamoDbItem, DynamoDbQueryCondition, DynamoDbQueryRequest } from '../../api/client';

const QUERY_LIMIT = 25;
const VALUE_TYPES = ['S', 'N', 'BOOL'];
const SORT_OPERATORS = ['=', '<', '<=', '>', '>=', 'begins_with', 'between'];
const FILTER_OPERATORS = ['=', '<>', '<', '<=', '>', '>=', 'begins_with', 'contains', 'between'];

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

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexWrap: 'wrap',
  alignItems: 'flex-end',
  gap: 8,
};

const fieldStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 6px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#010409',
  color: 'inherit',
};

const toggleGroupStyle: CSSProperties = { display: 'flex', gap: 8 };

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

const activeButtonStyle: CSSProperties = {
  ...buttonStyle,
  borderColor: '#1f6feb',
  background: '#1f6feb',
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

export interface DynamoDbQueryPanelProps {
  tableName: string;
  indexNames: string[];
}

type Mode = 'query' | 'scan';

interface ConditionForm {
  attributeName: string;
  operator: string;
  valueType: string;
  value: string;
  secondValue: string;
}

type QueryState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'ready'; items: DynamoDbItem[]; nextToken: string | null; request: DynamoDbQueryRequest }
  | { kind: 'error' };

function emptyCondition(operator: string): ConditionForm {
  return { attributeName: '', operator, valueType: 'S', value: '', secondValue: '' };
}

function toCondition(form: ConditionForm): DynamoDbQueryCondition {
  return {
    attributeName: form.attributeName,
    operator: form.operator,
    valueType: form.valueType,
    value: form.value,
    secondValue: form.operator === 'between' ? form.secondValue : null,
  };
}

function renderOptions(values: string[]) {
  return values.map((value) => (
    <option key={value} value={value}>
      {value}
    </option>
  ));
}

export function DynamoDbQueryPanel({ tableName, indexNames }: DynamoDbQueryPanelProps) {
  const [mode, setMode] = useState<Mode>('query');
  const [indexName, setIndexName] = useState('');
  const [partition, setPartition] = useState<ConditionForm>(() => emptyCondition('='));
  const [includeSort, setIncludeSort] = useState(false);
  const [sort, setSort] = useState<ConditionForm>(() => emptyCondition('='));
  const [filters, setFilters] = useState<ConditionForm[]>([]);
  const [state, setState] = useState<QueryState>({ kind: 'idle' });
  const [loadingMore, setLoadingMore] = useState(false);
  const [loadMoreFailed, setLoadMoreFailed] = useState(false);

  const buildRequest = (): DynamoDbQueryRequest => ({
    indexName: indexName === '' ? null : indexName,
    scan: mode === 'scan',
    partitionKey: mode === 'query' ? toCondition(partition) : null,
    sortKey: mode === 'query' && includeSort ? toCondition(sort) : null,
    filters: filters.map(toCondition),
    limit: QUERY_LIMIT,
    startToken: null,
  });

  const runQuery = () => {
    const request = buildRequest();
    setState({ kind: 'loading' });
    setLoadMoreFailed(false);
    queryDynamoDbTable(tableName, request)
      .then((result) =>
        setState({ kind: 'ready', items: result.items, nextToken: result.nextToken, request }),
      )
      .catch(() => setState({ kind: 'error' }));
  };

  const loadMore = (items: DynamoDbItem[], token: string, request: DynamoDbQueryRequest) => {
    setLoadingMore(true);
    setLoadMoreFailed(false);
    queryDynamoDbTable(tableName, { ...request, startToken: token })
      .then((result) =>
        setState({
          kind: 'ready',
          items: [...items, ...result.items],
          nextToken: result.nextToken,
          request,
        }),
      )
      .catch(() => setLoadMoreFailed(true))
      .finally(() => setLoadingMore(false));
  };

  const addFilter = () => setFilters((current) => [...current, emptyCondition('=')]);
  const removeFilter = (index: number) =>
    setFilters((current) => current.filter((_, position) => position !== index));
  const updateFilter = (index: number, patch: Partial<ConditionForm>) =>
    setFilters((current) =>
      current.map((filter, position) => (position === index ? { ...filter, ...patch } : filter)),
    );

  return (
    <div data-testid="dynamodb-query-panel" style={sectionStyle}>
      <div style={headerStyle}>
        <Heading as="h4" style={sectionHeadingStyle}>
          Query &amp; scan
        </Heading>
      </div>

      <div style={formStyle}>
        <div style={toggleGroupStyle}>
          <button
            type="button"
            data-testid="dynamodb-query-mode-query"
            style={mode === 'query' ? activeButtonStyle : buttonStyle}
            onClick={() => setMode('query')}
          >
            Query
          </button>
          <button
            type="button"
            data-testid="dynamodb-query-mode-scan"
            style={mode === 'scan' ? activeButtonStyle : buttonStyle}
            onClick={() => setMode('scan')}
          >
            Scan
          </button>
        </div>

        <label style={fieldStyle}>
          <span style={labelStyle}>Index</span>
          <select
            data-testid="dynamodb-query-index"
            style={inputStyle}
            value={indexName}
            onChange={(event) => setIndexName(event.target.value)}
          >
            <option value="">Table (primary)</option>
            {renderOptions(indexNames)}
          </select>
        </label>

        {mode === 'query' ? (
          <div data-testid="dynamodb-query-partition" style={fieldRowStyle}>
            <label style={fieldStyle}>
              <span style={labelStyle}>Partition key</span>
              <input
                type="text"
                data-testid="dynamodb-query-partition-attr"
                style={inputStyle}
                value={partition.attributeName}
                onChange={(event) =>
                  setPartition({ ...partition, attributeName: event.target.value })
                }
              />
            </label>
            <label style={fieldStyle}>
              <span style={labelStyle}>Type</span>
              <select
                data-testid="dynamodb-query-partition-type"
                style={inputStyle}
                value={partition.valueType}
                onChange={(event) => setPartition({ ...partition, valueType: event.target.value })}
              >
                {renderOptions(VALUE_TYPES)}
              </select>
            </label>
            <label style={fieldStyle}>
              <span style={labelStyle}>Value</span>
              <input
                type="text"
                data-testid="dynamodb-query-partition-value"
                style={inputStyle}
                value={partition.value}
                onChange={(event) => setPartition({ ...partition, value: event.target.value })}
              />
            </label>
          </div>
        ) : null}

        {mode === 'query' ? (
          <label style={{ ...fieldStyle, flexDirection: 'row', gap: 6 }}>
            <input
              type="checkbox"
              data-testid="dynamodb-query-sort-toggle"
              checked={includeSort}
              onChange={(event) => setIncludeSort(event.target.checked)}
            />
            <span style={messageStyle}>Add sort key condition</span>
          </label>
        ) : null}

        {mode === 'query' && includeSort ? (
          <div data-testid="dynamodb-query-sort" style={fieldRowStyle}>
            <label style={fieldStyle}>
              <span style={labelStyle}>Sort key</span>
              <input
                type="text"
                data-testid="dynamodb-query-sort-attr"
                style={inputStyle}
                value={sort.attributeName}
                onChange={(event) => setSort({ ...sort, attributeName: event.target.value })}
              />
            </label>
            <label style={fieldStyle}>
              <span style={labelStyle}>Operator</span>
              <select
                data-testid="dynamodb-query-sort-operator"
                style={inputStyle}
                value={sort.operator}
                onChange={(event) => setSort({ ...sort, operator: event.target.value })}
              >
                {renderOptions(SORT_OPERATORS)}
              </select>
            </label>
            <label style={fieldStyle}>
              <span style={labelStyle}>Type</span>
              <select
                data-testid="dynamodb-query-sort-type"
                style={inputStyle}
                value={sort.valueType}
                onChange={(event) => setSort({ ...sort, valueType: event.target.value })}
              >
                {renderOptions(VALUE_TYPES)}
              </select>
            </label>
            <label style={fieldStyle}>
              <span style={labelStyle}>Value</span>
              <input
                type="text"
                data-testid="dynamodb-query-sort-value"
                style={inputStyle}
                value={sort.value}
                onChange={(event) => setSort({ ...sort, value: event.target.value })}
              />
            </label>
            {sort.operator === 'between' ? (
              <label style={fieldStyle}>
                <span style={labelStyle}>Second value</span>
                <input
                  type="text"
                  data-testid="dynamodb-query-sort-second"
                  style={inputStyle}
                  value={sort.secondValue}
                  onChange={(event) => setSort({ ...sort, secondValue: event.target.value })}
                />
              </label>
            ) : null}
          </div>
        ) : null}

        <div style={sectionStyle}>
          <div style={headerStyle}>
            <span style={labelStyle}>Filters</span>
            <button
              type="button"
              data-testid="dynamodb-query-filter-add"
              style={buttonStyle}
              onClick={addFilter}
            >
              Add filter
            </button>
          </div>
          {filters.map((filter, index) => (
            <div key={index} data-testid={`dynamodb-query-filter-${index}`} style={fieldRowStyle}>
              <label style={fieldStyle}>
                <span style={labelStyle}>Attribute</span>
                <input
                  type="text"
                  data-testid={`dynamodb-query-filter-attr-${index}`}
                  style={inputStyle}
                  value={filter.attributeName}
                  onChange={(event) => updateFilter(index, { attributeName: event.target.value })}
                />
              </label>
              <label style={fieldStyle}>
                <span style={labelStyle}>Operator</span>
                <select
                  data-testid={`dynamodb-query-filter-operator-${index}`}
                  style={inputStyle}
                  value={filter.operator}
                  onChange={(event) => updateFilter(index, { operator: event.target.value })}
                >
                  {renderOptions(FILTER_OPERATORS)}
                </select>
              </label>
              <label style={fieldStyle}>
                <span style={labelStyle}>Type</span>
                <select
                  data-testid={`dynamodb-query-filter-type-${index}`}
                  style={inputStyle}
                  value={filter.valueType}
                  onChange={(event) => updateFilter(index, { valueType: event.target.value })}
                >
                  {renderOptions(VALUE_TYPES)}
                </select>
              </label>
              <label style={fieldStyle}>
                <span style={labelStyle}>Value</span>
                <input
                  type="text"
                  data-testid={`dynamodb-query-filter-value-${index}`}
                  style={inputStyle}
                  value={filter.value}
                  onChange={(event) => updateFilter(index, { value: event.target.value })}
                />
              </label>
              {filter.operator === 'between' ? (
                <label style={fieldStyle}>
                  <span style={labelStyle}>Second value</span>
                  <input
                    type="text"
                    data-testid={`dynamodb-query-filter-second-${index}`}
                    style={inputStyle}
                    value={filter.secondValue}
                    onChange={(event) => updateFilter(index, { secondValue: event.target.value })}
                  />
                </label>
              ) : null}
              <button
                type="button"
                data-testid={`dynamodb-query-filter-remove-${index}`}
                style={buttonStyle}
                onClick={() => removeFilter(index)}
              >
                Remove
              </button>
            </div>
          ))}
        </div>

        <button
          type="button"
          data-testid="dynamodb-query-run"
          style={buttonStyle}
          disabled={state.kind === 'loading'}
          onClick={runQuery}
        >
          {state.kind === 'loading' ? 'Running\u2026' : 'Run'}
        </button>
      </div>

      {state.kind === 'loading' ? (
        <p data-testid="dynamodb-query-loading" style={messageStyle}>
          Running&hellip;
        </p>
      ) : null}
      {state.kind === 'error' ? (
        <p data-testid="dynamodb-query-error" style={messageStyle}>
          Unable to run this query.
        </p>
      ) : null}
      {state.kind === 'ready' && state.items.length === 0 ? (
        <p data-testid="dynamodb-query-empty" style={messageStyle}>
          No items matched this query.
        </p>
      ) : null}
      {state.kind === 'ready' && state.items.length > 0 ? (
        <>
          {state.items.map((item, index) => (
            <div key={index} data-testid={`dynamodb-query-result-${index}`} style={itemStyle}>
              <pre data-testid={`dynamodb-query-result-json-${index}`} style={preStyle}>
                {item.json}
              </pre>
            </div>
          ))}
          {state.nextToken !== null ? (
            <button
              type="button"
              data-testid="dynamodb-query-load-more"
              style={buttonStyle}
              disabled={loadingMore}
              onClick={() => loadMore(state.items, state.nextToken!, state.request)}
            >
              {loadingMore ? 'Loading\u2026' : 'Load more'}
            </button>
          ) : null}
          {loadMoreFailed ? (
            <p data-testid="dynamodb-query-load-more-error" style={messageStyle}>
              Unable to load more items.
            </p>
          ) : null}
        </>
      ) : null}
    </div>
  );
}

export default DynamoDbQueryPanel;
