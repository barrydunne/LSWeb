import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AcmListView } from './AcmListView';
import { getAcmCertificates } from '../../api/client';
import type { AcmCertificateListResult } from '../../api/client';

vi.mock('../../api/client');

const getAcmCertificatesMock = vi.mocked(getAcmCertificates);

const result: AcmCertificateListResult = {
  certificates: [
    {
      arn: 'arn:aws:acm:eu-west-1:000000000000:certificate/abc',
      domainName: 'example.com',
      status: 'ISSUED',
      type: 'AMAZON_ISSUED',
    },
    {
      arn: 'arn:aws:acm:eu-west-1:000000000000:certificate/def',
      domainName: 'internal.example.com',
      status: 'PENDING_VALIDATION',
      type: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <AcmListView serviceKey="acm" />
    </MemoryRouter>,
  );
}

describe('AcmListView', () => {
  beforeEach(() => {
    getAcmCertificatesMock.mockResolvedValue(result);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before certificates arrive', () => {
    getAcmCertificatesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('acm-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getAcmCertificatesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('acm-list-error')).toBeInTheDocument(),
    );
  });

  it('renders a row per certificate', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('acm-list-view')).toBeInTheDocument(),
    );

    expect(
      screen.getByTestId('data-list-row-arn:aws:acm:eu-west-1:000000000000:certificate/abc'),
    ).toBeInTheDocument();
    expect(
      screen.getByTestId('data-list-row-arn:aws:acm:eu-west-1:000000000000:certificate/def'),
    ).toBeInTheDocument();
  });

  it('shows the domain, status and type and falls back when the type is missing', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('acm-list-view')).toBeInTheDocument(),
    );

    const domains = screen.getAllByTestId('acm-list-domain');
    const statuses = screen.getAllByTestId('acm-list-status');
    expect(domains[0]).toHaveTextContent('example.com');
    expect(statuses[0]).toHaveTextContent('ISSUED');
    expect(screen.getByTestId('acm-list-type')).toHaveTextContent('AMAZON_ISSUED');
    expect(screen.getByTestId('acm-list-type-empty')).toBeInTheDocument();
  });

  it('reloads the certificates when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('acm-list-view')).toBeInTheDocument(),
      );
      expect(getAcmCertificatesMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getAcmCertificatesMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });
});
