import { afterEach, describe, expect, it } from 'vitest';
import { registerServiceViews } from './registerServiceViews';
import { clearServiceViews, getServiceView } from './serviceViewRegistry';

describe('registerServiceViews', () => {
  afterEach(() => {
    clearServiceViews();
  });

  it('registers the lambda list and detail views', () => {
    // The module self-invokes on import, so the lambda view is already registered.
    const view = getServiceView('lambda');

    expect(view?.list).toBeDefined();
    expect(view?.detail).toBeDefined();
  });

  it('is idempotent and does not re-register after the first call', () => {
    clearServiceViews();

    // Guard short-circuits because registration already ran during import.
    registerServiceViews();

    expect(getServiceView('lambda')).toBeUndefined();
  });
});
