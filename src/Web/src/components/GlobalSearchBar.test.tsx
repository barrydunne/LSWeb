import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { GlobalSearchBar } from './GlobalSearchBar';
import {
  getSearch,
  getSearchState,
  resolveReference,
  type SearchMatchItem,
} from '../api/client';

vi.mock('../api/client');

const getSearchMock = vi.mocked(getSearch);
const getSearchStateMock = vi.mocked(getSearchState);
const resolveReferenceMock = vi.mocked(resolveReference);

function match(serviceKey: string, resourceId: string): SearchMatchItem {
  return {
    serviceKey,
    resourceId,
    displayName: resourceId,
    route: `/services/${serviceKey}/${resourceId}`,
  };
}

function renderBar() {
  return render(
    <ThemeProvider colorMode="night">
      <GlobalSearchBar />
    </ThemeProvider>,
  );
}

function type(value: string) {
  fireEvent.change(screen.getByTestId('global-search-input'), { target: { value } });
}

describe('GlobalSearchBar', () => {
  beforeEach(() => {
    getSearchStateMock.mockResolvedValue({
      builtAt: '2026-01-02T03:04:05Z',
      entryCount: 3,
      isBuilding: false,
    });
    getSearchMock.mockResolvedValue({ matches: [] });
    resolveReferenceMock.mockImplementation((reference, service) =>
      Promise.resolve({
        serviceKey: service ?? 'unknown',
        resourceId: reference,
        route: `/services/${service ?? 'unknown'}/${reference}`,
      }),
    );
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.resetAllMocks();
  });

  it('renders the search input without a results panel initially', () => {
    renderBar();

    expect(screen.getByTestId('global-search-input')).toBeInTheDocument();
    expect(screen.queryByTestId('global-search-panel')).not.toBeInTheDocument();
  });

  it('shows a loading indicator immediately while the debounce is pending', () => {
    vi.useFakeTimers();
    renderBar();

    type('ord');

    expect(screen.getByTestId('global-search-loading')).toBeInTheDocument();
    expect(getSearchMock).not.toHaveBeenCalled();
  });

  it('queries the search endpoint once after the debounce elapses', async () => {
    vi.useFakeTimers();
    getSearchMock.mockResolvedValue({ matches: [match('sqs', 'orders')] });

    renderBar();
    type('ord');

    await act(async () => {
      await vi.advanceTimersByTimeAsync(250);
    });

    expect(getSearchMock).toHaveBeenCalledTimes(1);
    expect(getSearchMock).toHaveBeenCalledWith('ord', expect.any(AbortSignal));
    expect(screen.getByTestId('search-results-group-name')).toHaveTextContent('sqs');
  });

  it('debounces rapid keystrokes into a single query for the latest term', async () => {
    vi.useFakeTimers();

    renderBar();
    type('o');
    await act(async () => {
      await vi.advanceTimersByTimeAsync(100);
    });
    type('or');
    await act(async () => {
      await vi.advanceTimersByTimeAsync(100);
    });
    type('ord');
    await act(async () => {
      await vi.advanceTimersByTimeAsync(250);
    });

    expect(getSearchMock).toHaveBeenCalledTimes(1);
    expect(getSearchMock).toHaveBeenCalledWith('ord', expect.any(AbortSignal));
  });

  it('trims the query before searching', async () => {
    vi.useFakeTimers();

    renderBar();
    type('  ord  ');

    await act(async () => {
      await vi.advanceTimersByTimeAsync(250);
    });

    expect(getSearchMock).toHaveBeenCalledWith('ord', expect.any(AbortSignal));
  });

  it('clears the panel when the query is emptied', async () => {
    vi.useFakeTimers();

    renderBar();
    type('ord');
    await act(async () => {
      await vi.advanceTimersByTimeAsync(250);
    });
    expect(screen.getByTestId('global-search-panel')).toBeInTheDocument();

    type('   ');

    expect(screen.queryByTestId('global-search-panel')).not.toBeInTheDocument();
    expect(screen.queryByTestId('global-search-loading')).not.toBeInTheDocument();
  });

  it('shows an error message when the search request fails', async () => {
    vi.useFakeTimers();
    getSearchMock.mockRejectedValue(new Error('boom'));

    renderBar();
    type('ord');

    await act(async () => {
      await vi.advanceTimersByTimeAsync(250);
    });

    expect(screen.getByTestId('global-search-error')).toBeInTheDocument();
  });

  it('passes the fetched index state hint into the results panel', async () => {
    getSearchMock.mockResolvedValue({ matches: [match('sqs', 'orders')] });

    renderBar();
    type('ord');

    await waitFor(() => expect(screen.getByTestId('search-results-state')).toBeInTheDocument());
    expect(screen.getByTestId('search-results-state')).toHaveTextContent('Search index: 3 resources.');
    await screen.findByTestId('resource-link');
  });

  it('tolerates a failed index-state request by omitting the hint', async () => {
    getSearchStateMock.mockRejectedValue(new Error('boom'));
    getSearchMock.mockResolvedValue({ matches: [match('sqs', 'orders')] });

    renderBar();
    type('ord');

    await waitFor(() => expect(screen.getByTestId('search-results-group')).toBeInTheDocument());
    expect(screen.queryByTestId('search-results-state')).not.toBeInTheDocument();
  });
});
