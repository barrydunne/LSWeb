import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { EmptyState, type EmptyStateProps } from './EmptyState';

function renderEmptyState(props: EmptyStateProps) {
  return render(
    <ThemeProvider colorMode="night">
      <EmptyState {...props} />
    </ThemeProvider>,
  );
}

describe('EmptyState', () => {
  it('shows the default "no resources yet" guidance', () => {
    renderEmptyState({ variant: 'no-resources' });

    expect(screen.getByTestId('empty-state')).toHaveAttribute('data-variant', 'no-resources');
    expect(screen.getByTestId('empty-state-message')).toHaveTextContent('No resources yet');
    expect(screen.queryByTestId('empty-state-cli-hint')).not.toBeInTheDocument();
    expect(screen.queryByTestId('empty-state-action')).not.toBeInTheDocument();
  });

  it('shows the default "no matches for filter" guidance', () => {
    renderEmptyState({ variant: 'no-matches' });

    expect(screen.getByTestId('empty-state')).toHaveAttribute('data-variant', 'no-matches');
    expect(screen.getByTestId('empty-state-message')).toHaveTextContent('No matches');
  });

  it('renders a custom message when provided', () => {
    renderEmptyState({ variant: 'no-resources', message: 'No queues created.' });

    expect(screen.getByTestId('empty-state-message')).toHaveTextContent('No queues created.');
  });

  it('renders a CLI hint and a primary action when provided', () => {
    renderEmptyState({
      variant: 'no-resources',
      cliHint: 'awslocal sqs create-queue --queue-name demo',
      action: <button data-testid="create-button">Create</button>,
    });

    expect(screen.getByTestId('empty-state-cli-hint')).toHaveTextContent('awslocal sqs create-queue');
    expect(screen.getByTestId('empty-state-action')).toContainElement(
      screen.getByTestId('create-button'),
    );
  });
});
