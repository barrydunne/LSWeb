import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { EventBridgePatternBuilder } from './EventBridgePatternBuilder';
import { createEventBridgeRule } from '../../api/client';

vi.mock('../../api/client');

const createEventBridgeRuleMock = vi.mocked(createEventBridgeRule);

function renderBuilder(onCreated = vi.fn()) {
  render(
    <ThemeProvider colorMode="night">
      <EventBridgePatternBuilder onCreated={onCreated} />
    </ThemeProvider>,
  );
  return onCreated;
}

describe('EventBridgePatternBuilder', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows the empty state until a field has a value', () => {
    renderBuilder();

    expect(screen.getByTestId('eventbridge-pattern-empty')).toBeInTheDocument();
    expect(screen.getByTestId('eventbridge-pattern-preview')).toHaveTextContent('{}');
  });

  it('builds a JSON preview from field values', () => {
    renderBuilder();

    fireEvent.change(screen.getByTestId('eventbridge-pattern-field-name-0'), {
      target: { value: 'region' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-pattern-field-values-0'), {
      target: { value: 'eu-west-1, us-east-1' },
    });

    expect(screen.getByTestId('eventbridge-pattern-valid')).toBeInTheDocument();
    const preview = screen.getByTestId('eventbridge-pattern-preview').textContent ?? '';
    expect(JSON.parse(preview)).toEqual({ region: ['eu-west-1', 'us-east-1'] });
  });

  it('adds and removes pattern fields', () => {
    renderBuilder();

    fireEvent.click(screen.getByTestId('eventbridge-pattern-add-field'));
    expect(screen.getByTestId('eventbridge-pattern-field-2')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('eventbridge-pattern-field-remove-2'));
    expect(screen.queryByTestId('eventbridge-pattern-field-2')).not.toBeInTheDocument();
  });

  it('shows an actionable error when the rule name is missing', () => {
    renderBuilder();

    fireEvent.change(screen.getByTestId('eventbridge-pattern-field-values-0'), {
      target: { value: 'my.app' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-pattern-save'));

    expect(screen.getByTestId('eventbridge-pattern-error')).toHaveTextContent('Enter a rule name');
    expect(createEventBridgeRuleMock).not.toHaveBeenCalled();
  });

  it('shows an actionable error when no field has a value', () => {
    renderBuilder();

    fireEvent.change(screen.getByTestId('eventbridge-pattern-name'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-pattern-save'));

    expect(screen.getByTestId('eventbridge-pattern-error')).toHaveTextContent(
      'Add at least one field',
    );
    expect(createEventBridgeRuleMock).not.toHaveBeenCalled();
  });

  it('saves the generated pattern and notifies the parent', async () => {
    createEventBridgeRuleMock.mockResolvedValue();
    const onCreated = renderBuilder();

    fireEvent.change(screen.getByTestId('eventbridge-pattern-name'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-pattern-state'), {
      target: { value: 'DISABLED' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-pattern-bus'), {
      target: { value: 'custom-bus' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-pattern-field-values-0'), {
      target: { value: 'my.app' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-pattern-save'));

    await waitFor(() => expect(screen.getByTestId('eventbridge-pattern-done')).toBeInTheDocument());
    expect(onCreated).toHaveBeenCalledTimes(1);
    expect(createEventBridgeRuleMock).toHaveBeenCalledWith({
      name: 'orders-rule',
      eventPattern: JSON.stringify({ source: ['my.app'] }),
      state: 'DISABLED',
      description: null,
      eventBusName: 'custom-bus',
    });
  });

  it('shows an error when saving fails', async () => {
    createEventBridgeRuleMock.mockRejectedValue(new Error('boom'));
    renderBuilder();

    fireEvent.change(screen.getByTestId('eventbridge-pattern-name'), {
      target: { value: 'orders-rule' },
    });
    fireEvent.change(screen.getByTestId('eventbridge-pattern-field-values-0'), {
      target: { value: 'my.app' },
    });
    fireEvent.click(screen.getByTestId('eventbridge-pattern-save'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-pattern-error')).toHaveTextContent(
        'could not be saved',
      ),
    );
  });
});
