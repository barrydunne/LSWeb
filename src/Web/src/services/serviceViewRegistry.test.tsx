import { afterEach, describe, expect, it } from 'vitest';
import {
  clearServiceViews,
  getServiceView,
  registerServiceView,
} from './serviceViewRegistry';

function ListView() {
  return null;
}

function DetailView() {
  return null;
}

describe('serviceViewRegistry', () => {
  afterEach(() => {
    clearServiceViews();
  });

  it('returns undefined for an unregistered service', () => {
    expect(getServiceView('lambda')).toBeUndefined();
  });

  it('registers and retrieves a list view', () => {
    registerServiceView('lambda', { list: ListView });

    expect(getServiceView('lambda')?.list).toBe(ListView);
  });

  it('merges later registrations into the same entry', () => {
    registerServiceView('lambda', { list: ListView });
    registerServiceView('lambda', { detail: DetailView });

    const entry = getServiceView('lambda');
    expect(entry?.list).toBe(ListView);
    expect(entry?.detail).toBe(DetailView);
  });

  it('clears all registrations', () => {
    registerServiceView('lambda', { list: ListView });
    clearServiceViews();

    expect(getServiceView('lambda')).toBeUndefined();
  });
});
