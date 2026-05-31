import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { StateMachineGraph } from './StateMachineGraph';
import { parseStateMachineGraph } from './aslGraph';

const linearDefinition = JSON.stringify({
  StartAt: 'First',
  States: {
    First: { Type: 'Task', Next: 'Second' },
    Second: { Type: 'Pass', End: true },
  },
});

describe('parseStateMachineGraph', () => {
  it('returns null when the definition is not valid JSON', () => {
    expect(parseStateMachineGraph('{ not json')).toBeNull();
  });

  it('returns null when there is no States object', () => {
    expect(parseStateMachineGraph(JSON.stringify({ StartAt: 'First' }))).toBeNull();
  });

  it('orders states starting from StartAt', () => {
    const nodes = parseStateMachineGraph(linearDefinition);

    expect(nodes?.map((node) => node.name)).toEqual(['First', 'Second']);
    expect(nodes?.[0].isStart).toBe(true);
    expect(nodes?.[0].transitions).toEqual([{ target: 'Second', label: 'Next' }]);
    expect(nodes?.[1].isTerminal).toBe(true);
  });

  it('captures choice, default and catch transitions', () => {
    const definition = JSON.stringify({
      StartAt: 'Decide',
      States: {
        Decide: {
          Type: 'Choice',
          Choices: [{ Next: 'Left' }],
          Default: 'Right',
        },
        Left: { Type: 'Task', Catch: [{ Next: 'Handle' }], End: true },
        Right: { Type: 'Succeed' },
        Handle: { Type: 'Fail' },
      },
    });

    const nodes = parseStateMachineGraph(definition);
    const decide = nodes?.find((node) => node.name === 'Decide');
    const left = nodes?.find((node) => node.name === 'Left');
    const right = nodes?.find((node) => node.name === 'Right');

    expect(decide?.transitions).toEqual([
      { target: 'Left', label: 'Choice' },
      { target: 'Right', label: 'Default' },
    ]);
    expect(left?.transitions).toEqual([{ target: 'Handle', label: 'Catch' }]);
    expect(right?.isTerminal).toBe(true);
  });

  it('appends states that are not reachable from StartAt', () => {
    const definition = JSON.stringify({
      StartAt: 'Only',
      States: {
        Only: { Type: 'Pass', End: true },
        Orphan: { End: true },
      },
    });

    const nodes = parseStateMachineGraph(definition);

    expect(nodes?.map((node) => node.name)).toEqual(['Only', 'Orphan']);
    expect(nodes?.[1].type).toBe('Unknown');
  });

  it('visits a shared target only once', () => {
    const definition = JSON.stringify({
      StartAt: 'Decide',
      States: {
        Decide: {
          Type: 'Choice',
          Choices: [{ Next: 'Shared' }, { Next: 'Shared' }],
        },
        Shared: { Type: 'Pass', End: true },
      },
    });

    const nodes = parseStateMachineGraph(definition);

    expect(nodes?.map((node) => node.name)).toEqual(['Decide', 'Shared']);
  });
});

describe('StateMachineGraph', () => {
  it('renders a node per state with transitions', () => {
    render(<StateMachineGraph definition={linearDefinition} />);

    expect(screen.getAllByTestId('step-functions-graph-node')).toHaveLength(2);
    expect(screen.getByTestId('step-functions-graph-transition')).toHaveTextContent(
      'Next → Second',
    );
  });

  it('renders an empty message when the definition cannot be parsed', () => {
    render(<StateMachineGraph definition="not-json" />);

    expect(screen.getByTestId('step-functions-graph-empty')).toBeInTheDocument();
  });
});
