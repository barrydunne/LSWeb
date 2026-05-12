import { useCallback, useState } from 'react';

export interface UseConfirmResult {
  isArmed: boolean;
  arm: () => void;
  cancel: () => void;
  confirm: () => void;
}

export function useConfirm(onConfirm: () => void): UseConfirmResult {
  const [isArmed, setIsArmed] = useState(false);

  const arm = useCallback(() => setIsArmed(true), []);
  const cancel = useCallback(() => setIsArmed(false), []);
  const confirm = useCallback(() => {
    setIsArmed(false);
    onConfirm();
  }, [onConfirm]);

  return { isArmed, arm, cancel, confirm };
}
