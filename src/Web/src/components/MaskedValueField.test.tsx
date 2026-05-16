import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MaskedValueField } from './MaskedValueField';

describe('MaskedValueField', () => {
  it('renders the name, source and value', () => {
    render(
      <MaskedValueField
        name="Access key"
        value="********"
        source="EnvironmentVariable"
        isSensitive
        revealed={false}
        revealAllowed={false}
        onToggleReveal={vi.fn()}
      />,
    );

    expect(screen.getByTestId('masked-value-name')).toHaveTextContent('Access key');
    expect(screen.getByTestId('masked-value-source')).toHaveTextContent('EnvironmentVariable');
    expect(screen.getByTestId('masked-value-value')).toHaveTextContent('********');
  });

  it('hides the toggle for non-sensitive values', () => {
    render(
      <MaskedValueField
        name="Region"
        value="eu-west-1"
        source="Default"
        isSensitive={false}
        revealed={false}
        revealAllowed
        onToggleReveal={vi.fn()}
      />,
    );

    expect(screen.queryByTestId('masked-value-toggle')).not.toBeInTheDocument();
  });

  it('hides the toggle when reveal is not allowed', () => {
    render(
      <MaskedValueField
        name="Access key"
        value="********"
        source="EnvironmentVariable"
        isSensitive
        revealed={false}
        revealAllowed={false}
        onToggleReveal={vi.fn()}
      />,
    );

    expect(screen.queryByTestId('masked-value-toggle')).not.toBeInTheDocument();
  });

  it('shows a reveal toggle that invokes the callback', async () => {
    const onToggleReveal = vi.fn();
    render(
      <MaskedValueField
        name="Access key"
        value="********"
        source="EnvironmentVariable"
        isSensitive
        revealed={false}
        revealAllowed
        onToggleReveal={onToggleReveal}
      />,
    );

    const toggle = screen.getByTestId('masked-value-toggle');
    expect(toggle).toHaveTextContent('Reveal');
    await userEvent.click(toggle);

    expect(onToggleReveal).toHaveBeenCalledTimes(1);
  });

  it('shows a hide toggle when already revealed', () => {
    render(
      <MaskedValueField
        name="Access key"
        value="AKIA"
        source="EnvironmentVariable"
        isSensitive
        revealed
        revealAllowed
        onToggleReveal={vi.fn()}
      />,
    );

    expect(screen.getByTestId('masked-value-toggle')).toHaveTextContent('Hide');
  });
});
