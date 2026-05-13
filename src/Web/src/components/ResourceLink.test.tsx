import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { ResourceLink } from './ResourceLink';
import { resolveReference, type ResolvedReferenceResult } from '../api/client';

vi.mock('../api/client');

const resolveReferenceMock = vi.mocked(resolveReference);

const resolved: ResolvedReferenceResult = {
  serviceKey: 'sqs',
  resourceId: 'orders',
  route: '/services/sqs/orders',
};

function renderLink(props: { reference: string; service?: string; label?: string }) {
  return render(
    <ThemeProvider colorMode="night">
      <ResourceLink {...props} />
    </ThemeProvider>,
  );
}

describe('ResourceLink', () => {
  beforeEach(() => {
    resolveReferenceMock.mockResolvedValue(resolved);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders a link to the resolved route using the resource id as text', async () => {
    renderLink({ reference: 'arn:aws:sqs:eu-west-1:000000000000:orders' });

    await waitFor(() =>
      expect(screen.getByTestId('resource-link')).toHaveAttribute('href', '/services/sqs/orders'),
    );
    expect(screen.getByTestId('resource-link')).toHaveTextContent('orders');
    expect(resolveReferenceMock).toHaveBeenCalledWith(
      'arn:aws:sqs:eu-west-1:000000000000:orders',
      undefined,
      expect.any(AbortSignal),
    );
  });

  it('prefers an explicit label over the resolved resource id', async () => {
    renderLink({ reference: 'arn:aws:sqs:eu-west-1:000000000000:orders', label: 'Orders queue' });

    await waitFor(() =>
      expect(screen.getByTestId('resource-link')).toHaveAttribute('href', '/services/sqs/orders'),
    );
    expect(screen.getByTestId('resource-link')).toHaveTextContent('Orders queue');
  });

  it('passes the service hint when resolving a bare identifier', async () => {
    renderLink({ reference: 'orders', service: 'sqs' });

    await waitFor(() => expect(screen.getByTestId('resource-link')).toHaveAttribute('href'));
    expect(resolveReferenceMock).toHaveBeenCalledWith('orders', 'sqs', expect.any(AbortSignal));
  });

  it('falls back to plain text when the reference cannot be resolved', async () => {
    resolveReferenceMock.mockRejectedValue(new Error('boom'));

    renderLink({ reference: 'arn:aws:kinesis:eu-west-1:000000000000:stream/foo' });

    await waitFor(() => expect(resolveReferenceMock).toHaveBeenCalled());
    await act(async () => {});

    const link = screen.getByTestId('resource-link');
    expect(link).not.toHaveAttribute('href');
    expect(link).toHaveTextContent('arn:aws:kinesis:eu-west-1:000000000000:stream/foo');
  });

  it('aborts the in-flight request on unmount', () => {
    resolveReferenceMock.mockReturnValue(new Promise<ResolvedReferenceResult>(() => {}));

    const { unmount } = renderLink({ reference: 'arn:aws:sqs:eu-west-1:000000000000:orders' });

    expect(() => unmount()).not.toThrow();
  });
});
