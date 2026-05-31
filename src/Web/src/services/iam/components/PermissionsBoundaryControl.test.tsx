import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { PermissionsBoundaryControl } from './PermissionsBoundaryControl';
import { resolveReference } from '../../../api/client';

vi.mock('../../../api/client');

const resolveReferenceMock = vi.mocked(resolveReference);

function renderControl(boundaryArn: string | null) {
  resolveReferenceMock.mockRejectedValue(new Error('unresolved'));
  const onSet = vi.fn();
  const onRemove = vi.fn();
  render(
    <MemoryRouter>
      <PermissionsBoundaryControl
        boundaryArn={boundaryArn}
        onSet={onSet}
        onRemove={onRemove}
        testId="boundary"
      />
    </MemoryRouter>,
  );
  return { onSet, onRemove };
}

describe('PermissionsBoundaryControl', () => {
  it('shows an empty message when no boundary is set', () => {
    renderControl(null);

    expect(screen.getByTestId('boundary-empty')).toHaveTextContent('No permissions boundary set.');
    expect(screen.queryByTestId('boundary-current')).not.toBeInTheDocument();
  });

  it('shows the current boundary when one is set', () => {
    renderControl('arn:aws:iam::aws:policy/Boundary');

    expect(screen.getByTestId('boundary-current')).toBeInTheDocument();
  });

  it('requires an ARN before setting a boundary', () => {
    const { onSet } = renderControl(null);

    fireEvent.click(screen.getByTestId('boundary-submit'));

    expect(screen.getByTestId('boundary-arn-error')).toBeInTheDocument();
    expect(onSet).not.toHaveBeenCalled();
  });

  it('sets a trimmed boundary ARN and clears the input', () => {
    const { onSet } = renderControl(null);

    fireEvent.change(screen.getByTestId('boundary-arn'), {
      target: { value: '  arn:aws:iam::aws:policy/Boundary  ' },
    });
    fireEvent.click(screen.getByTestId('boundary-submit'));

    expect(onSet).toHaveBeenCalledWith('arn:aws:iam::aws:policy/Boundary');
    expect((screen.getByTestId('boundary-arn') as HTMLInputElement).value).toBe('');
  });

  it('removes the boundary after confirmation', async () => {
    const { onRemove } = renderControl('arn:aws:iam::aws:policy/Boundary');
    await waitFor(() => expect(screen.getByTestId('boundary-current')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    expect(onRemove).toHaveBeenCalled();
  });
});
