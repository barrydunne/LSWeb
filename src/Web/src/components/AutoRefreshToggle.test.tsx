import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { AutoRefreshToggle } from './AutoRefreshToggle';

function renderToggle(onRefresh: () => void) {
  return render(
    <ThemeProvider colorMode="night">
      <AutoRefreshToggle onRefresh={onRefresh} />
    </ThemeProvider>,
  );
}

function setHidden(value: boolean) {
  Object.defineProperty(document, 'hidden', { configurable: true, value });
  document.dispatchEvent(new Event('visibilitychange'));
}

describe('AutoRefreshToggle', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    setHidden(false);
  });

  it('does not refresh while auto-refresh is off', () => {
    const onRefresh = vi.fn();
    renderToggle(onRefresh);

    expect(screen.getByTestId('auto-refresh-switch')).toHaveTextContent('Auto-refresh off');

    act(() => {
      vi.advanceTimersByTime(30_000);
    });

    expect(onRefresh).not.toHaveBeenCalled();
  });

  it('refreshes on the selected interval once enabled', () => {
    const onRefresh = vi.fn();
    renderToggle(onRefresh);

    act(() => {
      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
    });

    expect(screen.getByTestId('auto-refresh-switch')).toHaveTextContent('Auto-refresh on');

    act(() => {
      vi.advanceTimersByTime(5_000);
    });
    expect(onRefresh).toHaveBeenCalledTimes(1);

    act(() => {
      vi.advanceTimersByTime(5_000);
    });
    expect(onRefresh).toHaveBeenCalledTimes(2);
  });

  it('re-fetches on the chosen interval after the interval changes', () => {
    const onRefresh = vi.fn();
    renderToggle(onRefresh);

    act(() => {
      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
    });
    act(() => {
      fireEvent.change(screen.getByTestId('auto-refresh-interval'), { target: { value: '15' } });
    });

    act(() => {
      vi.advanceTimersByTime(5_000);
    });
    expect(onRefresh).not.toHaveBeenCalled();

    act(() => {
      vi.advanceTimersByTime(10_000);
    });
    expect(onRefresh).toHaveBeenCalledTimes(1);
  });

  it('stops refreshing when toggled back off', () => {
    const onRefresh = vi.fn();
    renderToggle(onRefresh);

    act(() => {
      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
    });
    act(() => {
      vi.advanceTimersByTime(5_000);
    });
    expect(onRefresh).toHaveBeenCalledTimes(1);

    act(() => {
      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
    });
    act(() => {
      vi.advanceTimersByTime(30_000);
    });
    expect(onRefresh).toHaveBeenCalledTimes(1);
  });

  it('pauses while the tab is hidden and resumes when visible again', () => {
    const onRefresh = vi.fn();
    renderToggle(onRefresh);

    act(() => {
      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
    });

    act(() => {
      setHidden(true);
    });
    act(() => {
      vi.advanceTimersByTime(30_000);
    });
    expect(onRefresh).not.toHaveBeenCalled();

    act(() => {
      setHidden(false);
    });
    act(() => {
      vi.advanceTimersByTime(5_000);
    });
    expect(onRefresh).toHaveBeenCalledTimes(1);
  });
});
