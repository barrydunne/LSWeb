export interface StateTransition {
  target: string;
  label: string;
}

export interface StateMachineGraphNode {
  name: string;
  type: string;
  transitions: StateTransition[];
  isStart: boolean;
  isTerminal: boolean;
}

interface AslState {
  Type?: string;
  Next?: string;
  End?: boolean;
  Default?: string;
  Choices?: { Next?: string }[];
  Catch?: { Next?: string }[];
}

interface AslDefinition {
  StartAt?: string;
  States?: Record<string, AslState>;
}

function transitionsOf(state: AslState): StateTransition[] {
  const transitions: StateTransition[] = [];
  if (state.Next) {
    transitions.push({ target: state.Next, label: 'Next' });
  }
  if (Array.isArray(state.Choices)) {
    for (const choice of state.Choices) {
      if (choice?.Next) {
        transitions.push({ target: choice.Next, label: 'Choice' });
      }
    }
  }
  if (state.Default) {
    transitions.push({ target: state.Default, label: 'Default' });
  }
  if (Array.isArray(state.Catch)) {
    for (const handler of state.Catch) {
      if (handler?.Next) {
        transitions.push({ target: handler.Next, label: 'Catch' });
      }
    }
  }
  return transitions;
}

export function parseStateMachineGraph(definition: string): StateMachineGraphNode[] | null {
  let parsed: AslDefinition;
  try {
    parsed = JSON.parse(definition) as AslDefinition;
  } catch {
    return null;
  }

  const states = parsed.States;
  if (states === null || typeof states !== 'object') {
    return null;
  }

  const startAt = parsed.StartAt;
  const ordered: string[] = [];
  const seen = new Set<string>();
  const queue: string[] = [];

  if (startAt && states[startAt]) {
    queue.push(startAt);
  }

  while (queue.length > 0) {
    const name = queue.shift() as string;
    if (seen.has(name)) {
      continue;
    }
    seen.add(name);
    ordered.push(name);
    for (const transition of transitionsOf(states[name])) {
      if (states[transition.target] && !seen.has(transition.target)) {
        queue.push(transition.target);
      }
    }
  }

  for (const name of Object.keys(states)) {
    if (!seen.has(name)) {
      seen.add(name);
      ordered.push(name);
    }
  }

  return ordered.map((name) => {
    const state = states[name];
    return {
      name,
      type: state.Type ?? 'Unknown',
      transitions: transitionsOf(state),
      isStart: name === startAt,
      isTerminal: state.End === true || state.Type === 'Succeed' || state.Type === 'Fail',
    };
  });
}
