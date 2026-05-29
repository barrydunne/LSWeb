import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@primer/react';
import { ConfirmationHost } from './ConfirmationHost';

function renderHost(onConfirm = vi.fn()) {
  render(
    <ThemeProvider colorMode="night">
      <ConfirmationHost
        actionLabel="Delete"
        prompt="Delete it?"
        confirmLabel="Yes, delete"
        onConfirm={onConfirm}
      />
    </ThemeProvider>,
  );
  return onConfirm;
}

describe('ConfirmationHost', () => {
  it('shows only the trigger button before it is armed', () => {
    renderHost();

    expect(screen.getByTestId('confirm-trigger')).toHaveTextContent('Delete');
    expect(screen.queryByTestId('confirm-accept')).not.toBeInTheDocument();
  });

  it('confirms the action in a single click after arming', async () => {
    const user = userEvent.setup();
    const onConfirm = renderHost();

    await user.click(screen.getByTestId('confirm-trigger'));
    expect(screen.getByTestId('confirm-prompt')).toHaveTextContent('Delete it?');
    await user.click(screen.getByTestId('confirm-accept'));

    expect(onConfirm).toHaveBeenCalledTimes(1);
    expect(screen.getByTestId('confirm-trigger')).toBeInTheDocument();
  });

  it('makes no change when the confirmation is cancelled', async () => {
    const user = userEvent.setup();
    const onConfirm = renderHost();

    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-cancel'));

    expect(onConfirm).not.toHaveBeenCalled();
    expect(screen.queryByTestId('confirm-accept')).not.toBeInTheDocument();
    expect(screen.getByTestId('confirm-trigger')).toBeInTheDocument();
  });

  it('dismisses the dialog when the backdrop is clicked', async () => {
    const user = userEvent.setup();
    const onConfirm = renderHost();

    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-overlay'));

    expect(onConfirm).not.toHaveBeenCalled();
    expect(screen.queryByTestId('confirm-accept')).not.toBeInTheDocument();
    expect(screen.getByTestId('confirm-trigger')).toBeInTheDocument();
  });

  it('keeps the dialog open when its surface is clicked', async () => {
    const user = userEvent.setup();
    const onConfirm = renderHost();

    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-dialog'));

    expect(onConfirm).not.toHaveBeenCalled();
    expect(screen.getByTestId('confirm-accept')).toBeInTheDocument();
  });
});
