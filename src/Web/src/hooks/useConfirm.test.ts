import { describe, expect, it, vi } from 'vitest';
import { act, renderHook } from '@testing-library/react';
import { useConfirm } from './useConfirm';

describe('useConfirm', () => {
  it('starts disarmed', () => {
    const { result } = renderHook(() => useConfirm(vi.fn()));

    expect(result.current.isArmed).toBe(false);
  });

  it('arms when arm is called', () => {
    const { result } = renderHook(() => useConfirm(vi.fn()));

    act(() => result.current.arm());

    expect(result.current.isArmed).toBe(true);
  });

  it('disarms without confirming when cancel is called', () => {
    const onConfirm = vi.fn();
    const { result } = renderHook(() => useConfirm(onConfirm));

    act(() => result.current.arm());
    act(() => result.current.cancel());

    expect(result.current.isArmed).toBe(false);
    expect(onConfirm).not.toHaveBeenCalled();
  });

  it('disarms and confirms when confirm is called', () => {
    const onConfirm = vi.fn();
    const { result } = renderHook(() => useConfirm(onConfirm));

    act(() => result.current.arm());
    act(() => result.current.confirm());

    expect(result.current.isArmed).toBe(false);
    expect(onConfirm).toHaveBeenCalledTimes(1);
  });
});
