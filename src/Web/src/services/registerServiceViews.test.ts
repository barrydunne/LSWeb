import { afterEach, describe, expect, it } from 'vitest';
import { registerServiceViews } from './registerServiceViews';
import { clearServiceViews, getServiceView } from './serviceViewRegistry';

describe('registerServiceViews', () => {
  afterEach(() => {
    clearServiceViews();
  });

  it('registers the lambda and s3 views', () => {
    // The module self-invokes on import, so the views are already registered.
    const lambda = getServiceView('lambda');
    expect(lambda?.list).toBeDefined();
    expect(lambda?.detail).toBeDefined();

    const s3 = getServiceView('s3');
    expect(s3?.list).toBeDefined();
  });

  it('is idempotent and does not re-register after the first call', () => {
    clearServiceViews();

    // Guard short-circuits because registration already ran during import.
    registerServiceViews();

    expect(getServiceView('lambda')).toBeUndefined();
  });
});
