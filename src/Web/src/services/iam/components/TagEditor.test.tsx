import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { TagEditor } from './TagEditor';
import type { IamTag } from '../../../api/client';

function renderEditor(tags: IamTag[]) {
  const onAdd = vi.fn();
  const onRemove = vi.fn();
  render(<TagEditor tags={tags} onAdd={onAdd} onRemove={onRemove} testId="tags" />);
  return { onAdd, onRemove };
}

describe('TagEditor', () => {
  it('shows an empty message when there are no tags', () => {
    renderEditor([]);

    expect(screen.getByTestId('tags-empty')).toHaveTextContent('No tags applied.');
    expect(screen.queryByTestId('tags-list')).not.toBeInTheDocument();
  });

  it('renders tags including the dash fallback for an empty value', () => {
    renderEditor([
      { key: 'env', value: 'prod' },
      { key: 'team', value: '' },
    ]);

    const items = screen.getAllByTestId('tags-item');
    expect(items).toHaveLength(2);
    expect(items[0]).toHaveTextContent('env');
    expect(items[0]).toHaveTextContent('prod');
    expect(items[1]).toHaveTextContent('\u2014');
  });

  it('requires a key before adding a tag', () => {
    const { onAdd } = renderEditor([]);

    fireEvent.click(screen.getByTestId('tags-submit'));

    expect(screen.getByTestId('tags-key-error')).toBeInTheDocument();
    expect(onAdd).not.toHaveBeenCalled();
  });

  it('adds a trimmed key with its value and clears the inputs', () => {
    const { onAdd } = renderEditor([]);

    fireEvent.change(screen.getByTestId('tags-key'), { target: { value: '  env  ' } });
    fireEvent.change(screen.getByTestId('tags-value'), { target: { value: 'prod' } });
    fireEvent.click(screen.getByTestId('tags-submit'));

    expect(onAdd).toHaveBeenCalledWith('env', 'prod');
    expect((screen.getByTestId('tags-key') as HTMLInputElement).value).toBe('');
    expect((screen.getByTestId('tags-value') as HTMLInputElement).value).toBe('');
  });

  it('removes a tag after confirmation', () => {
    const { onRemove } = renderEditor([{ key: 'env', value: 'prod' }]);

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    expect(onRemove).toHaveBeenCalledWith('env');
  });
});
