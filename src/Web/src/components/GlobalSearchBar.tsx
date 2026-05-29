import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import { useLocation } from 'react-router-dom';
import {
  getSearch,
  getSearchState,
  type SearchMatchItem,
  type SearchStateResult,
} from '../api/client';
import { SearchResultsPage } from '../pages/SearchResultsPage';

const debounceMs = 250;

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  flex: 1,
  minWidth: 220,
  maxWidth: 420,
  position: 'relative',
};

const inputStyle: CSSProperties = {
  padding: '8px 12px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  fontSize: 14,
  width: '100%',
};

const panelStyle: CSSProperties = {
  position: 'absolute',
  top: '100%',
  left: 0,
  right: 0,
  marginTop: 8,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  zIndex: 20,
  maxHeight: '70vh',
  overflowY: 'auto',
};

type ResultsState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'ready'; matches: SearchMatchItem[] }
  | { kind: 'error' };

export function GlobalSearchBar() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<ResultsState>({ kind: 'idle' });
  const [indexState, setIndexState] = useState<SearchStateResult | null>(null);
  const location = useLocation();

  useEffect(() => {
    const controller = new AbortController();
    getSearchState(controller.signal)
      .then(setIndexState)
      .catch(() => setIndexState(null));
    return () => controller.abort();
  }, []);

  useEffect(() => {
    const term = query.trim();
    if (term.length === 0) {
      setResults({ kind: 'idle' });
      return;
    }

    const controller = new AbortController();
    setResults({ kind: 'loading' });
    const handle = setTimeout(() => {
      getSearch(term, controller.signal)
        .then((result) => setResults({ kind: 'ready', matches: result.matches }))
        .catch(() => setResults({ kind: 'error' }));
    }, debounceMs);

    return () => {
      clearTimeout(handle);
      controller.abort();
    };
  }, [query]);

  useEffect(() => {
    setQuery('');
    setResults({ kind: 'idle' });
  }, [location.key]);

  return (
    <div data-testid="global-search" style={containerStyle}>
      <input
        data-testid="global-search-input"
        type="search"
        aria-label="Search resources"
        placeholder={'Search resources\u2026'}
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        style={inputStyle}
      />

      {results.kind !== 'idle' ? (
        <div data-testid="global-search-panel" style={panelStyle}>
          {results.kind === 'loading' ? (
            <Text data-testid="global-search-loading" style={{ fontSize: 14 }}>
              Searching&hellip;
            </Text>
          ) : null}

          {results.kind === 'error' ? (
            <Text data-testid="global-search-error" style={{ fontSize: 14 }}>
              Unable to search right now.
            </Text>
          ) : null}

          {results.kind === 'ready' ? (
            <SearchResultsPage query={query.trim()} matches={results.matches} state={indexState} />
          ) : null}
        </div>
      ) : null}
    </div>
  );
}
