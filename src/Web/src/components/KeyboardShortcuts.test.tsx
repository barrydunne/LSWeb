import { afterEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { KeyboardShortcuts } from './KeyboardShortcuts';

function renderShortcuts(options?: { onNavigate?: (path: string) => void; withSearch?: boolean }) {
  return render(
    <ThemeProvider colorMode="night">
      {options?.withSearch ? <input data-testid="home-search-input" /> : null}
      <KeyboardShortcuts onNavigate={options?.onNavigate} />
    </ThemeProvider>,
  );
}

describe('KeyboardShortcuts', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.clearAllMocks();
  });

  it('does not render the help overlay until requested', () => {
    renderShortcuts();

    expect(screen.queryByTestId('shortcut-help')).toBeNull();
  });

  it('focuses the search box when "/" is pressed', () => {
    renderShortcuts({ withSearch: true });

    fireEvent.keyDown(document.body, { key: '/' });

    expect(document.activeElement).toBe(screen.getByTestId('home-search-input'));
  });

  it('ignores "/" when there is no search box to focus', () => {
    renderShortcuts();

    fireEvent.keyDown(document.body, { key: '/' });

    expect(screen.queryByTestId('shortcut-help')).toBeNull();
  });

  it('navigates home when "g" then "h" is pressed', () => {
    const onNavigate = vi.fn();
    renderShortcuts({ onNavigate });

    fireEvent.keyDown(document.body, { key: 'g' });
    fireEvent.keyDown(document.body, { key: 'h' });

    expect(onNavigate).toHaveBeenCalledWith('/');
  });

  it('does not navigate when "g" is followed by an unmapped key', () => {
    const onNavigate = vi.fn();
    renderShortcuts({ onNavigate });

    fireEvent.keyDown(document.body, { key: 'g' });
    fireEvent.keyDown(document.body, { key: 'x' });

    expect(onNavigate).not.toHaveBeenCalled();
  });

  it('toggles the help overlay with "?"', () => {
    renderShortcuts();

    fireEvent.keyDown(document.body, { key: '?' });
    expect(screen.getByTestId('shortcut-help')).toBeInTheDocument();
    expect(screen.getAllByTestId('shortcut-help-item').length).toBeGreaterThan(0);

    fireEvent.keyDown(document.body, { key: '?' });
    expect(screen.queryByTestId('shortcut-help')).toBeNull();
  });

  it('closes the help overlay with Escape', () => {
    renderShortcuts();

    fireEvent.keyDown(document.body, { key: '?' });
    expect(screen.getByTestId('shortcut-help')).toBeInTheDocument();

    fireEvent.keyDown(document.body, { key: 'Escape' });
    expect(screen.queryByTestId('shortcut-help')).toBeNull();
  });

  it('closes the help overlay with the close button', () => {
    renderShortcuts();

    fireEvent.keyDown(document.body, { key: '?' });
    fireEvent.click(screen.getByTestId('shortcut-help-close'));

    expect(screen.queryByTestId('shortcut-help')).toBeNull();
  });

  it('does not hijack keys while typing in an input', () => {
    renderShortcuts({ withSearch: true });

    fireEvent.keyDown(screen.getByTestId('home-search-input'), { key: '?' });

    expect(screen.queryByTestId('shortcut-help')).toBeNull();
  });

  it('does not hijack keys while typing in a textarea', () => {
    render(
      <ThemeProvider colorMode="night">
        <textarea data-testid="notes" />
        <KeyboardShortcuts />
      </ThemeProvider>,
    );

    fireEvent.keyDown(screen.getByTestId('notes'), { key: '?' });

    expect(screen.queryByTestId('shortcut-help')).toBeNull();
  });

  it('navigates via window.location by default', () => {
    const assign = vi.fn();
    vi.stubGlobal('location', { ...window.location, assign } as Location);
    renderShortcuts();

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'g' }));
    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'h' }));

    expect(assign).toHaveBeenCalledWith('/');
  });

  it('stops handling shortcuts after unmount', () => {
    const onNavigate = vi.fn();
    const { unmount } = renderShortcuts({ onNavigate });

    unmount();
    fireEvent.keyDown(document.body, { key: 'g' });
    fireEvent.keyDown(document.body, { key: 'h' });

    expect(onNavigate).not.toHaveBeenCalled();
  });
});
