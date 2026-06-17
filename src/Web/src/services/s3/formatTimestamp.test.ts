import { describe, expect, it } from 'vitest';
import { formatTimestamp } from './formatTimestamp';

describe('formatTimestamp', () => {
  it('formats an ISO string with 7-digit fractional seconds into a human-readable UTC value', () => {
    expect(formatTimestamp('2026-12-25T11:22:33.0000000Z')).toBe('25 Dec 2026, 11:22:33 UTC');
  });

  it('formats an ISO string without fractional seconds', () => {
    expect(formatTimestamp('2026-01-05T09:07:03Z')).toBe('05 Jan 2026, 09:07:03 UTC');
  });

  it('returns the original value when it is empty', () => {
    expect(formatTimestamp('')).toBe('');
  });

  it('returns the original value when it cannot be parsed', () => {
    expect(formatTimestamp('not-a-date')).toBe('not-a-date');
  });
});
