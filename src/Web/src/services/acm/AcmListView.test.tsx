import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AcmListView } from './AcmListView';
import { getAcmCertificates, importAcmCertificate, requestAcmCertificate } from '../../api/client';
import type { AcmCertificateListResult } from '../../api/client';

vi.mock('../../api/client');

const getAcmCertificatesMock = vi.mocked(getAcmCertificates);
const importAcmCertificateMock = vi.mocked(importAcmCertificate);
const requestAcmCertificateMock = vi.mocked(requestAcmCertificate);

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
    importAcmCertificateMock.mockResolvedValue({
      arn: 'arn:aws:acm:eu-west-1:000000000000:certificate/new',
    });
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

describe('AcmListView import', () => {
  beforeEach(() => {
    getAcmCertificatesMock.mockResolvedValue(result);
    importAcmCertificateMock.mockResolvedValue({
      arn: 'arn:aws:acm:eu-west-1:000000000000:certificate/new',
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  async function renderReady() {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('acm-list-view')).toBeInTheDocument(),
    );
  }

  it('toggles the import form open and closed', async () => {
    await renderReady();

    expect(screen.queryByTestId('acm-import-form')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('acm-import-toggle'));
    expect(screen.getByTestId('acm-import-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('acm-import-toggle'));
    expect(screen.queryByTestId('acm-import-form')).not.toBeInTheDocument();
  });

  it('imports a certificate without a chain and reports success', async () => {
    await renderReady();

    fireEvent.click(screen.getByTestId('acm-import-toggle'));
    fireEvent.change(screen.getByTestId('acm-import-certificate'), {
      target: { value: 'CERT-PEM' },
    });
    fireEvent.change(screen.getByTestId('acm-import-private-key'), {
      target: { value: 'KEY-PEM' },
    });
    fireEvent.click(screen.getByTestId('acm-import-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('acm-import-status')).toBeInTheDocument(),
    );
    expect(importAcmCertificateMock).toHaveBeenCalledWith({
      certificate: 'CERT-PEM',
      privateKey: 'KEY-PEM',
      certificateChain: null,
    });
    expect(getAcmCertificatesMock).toHaveBeenCalledTimes(2);
    expect(screen.queryByTestId('acm-import-form')).not.toBeInTheDocument();
  });

  it('passes the certificate chain when one is supplied', async () => {
    await renderReady();

    fireEvent.click(screen.getByTestId('acm-import-toggle'));
    fireEvent.change(screen.getByTestId('acm-import-certificate'), {
      target: { value: 'CERT-PEM' },
    });
    fireEvent.change(screen.getByTestId('acm-import-private-key'), {
      target: { value: 'KEY-PEM' },
    });
    fireEvent.change(screen.getByTestId('acm-import-chain'), {
      target: { value: '  CHAIN-PEM  ' },
    });
    fireEvent.click(screen.getByTestId('acm-import-submit'));

    await waitFor(() =>
      expect(importAcmCertificateMock).toHaveBeenCalledWith({
        certificate: 'CERT-PEM',
        privateKey: 'KEY-PEM',
        certificateChain: 'CHAIN-PEM',
      }),
    );
  });

  it('shows an error message when the import fails', async () => {
    importAcmCertificateMock.mockRejectedValue(new Error('boom'));
    await renderReady();

    fireEvent.click(screen.getByTestId('acm-import-toggle'));
    fireEvent.change(screen.getByTestId('acm-import-certificate'), {
      target: { value: 'CERT-PEM' },
    });
    fireEvent.change(screen.getByTestId('acm-import-private-key'), {
      target: { value: 'KEY-PEM' },
    });
    fireEvent.click(screen.getByTestId('acm-import-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('acm-import-error')).toBeInTheDocument(),
    );
  });
});

describe('AcmListView request', () => {
  beforeEach(() => {
    getAcmCertificatesMock.mockResolvedValue(result);
    requestAcmCertificateMock.mockResolvedValue({
      arn: 'arn:aws:acm:eu-west-1:000000000000:certificate/requested',
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  async function renderReady() {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('acm-list-view')).toBeInTheDocument(),
    );
  }

  it('toggles the request form open and closed', async () => {
    await renderReady();

    expect(screen.queryByTestId('acm-request-form')).not.toBeInTheDocument();

    fireEvent.click(screen.getByTestId('acm-request-toggle'));
    expect(screen.getByTestId('acm-request-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('acm-request-toggle'));
    expect(screen.queryByTestId('acm-request-form')).not.toBeInTheDocument();
  });

  it('requests a certificate with parsed subject alternative names and reports success', async () => {
    await renderReady();

    fireEvent.click(screen.getByTestId('acm-request-toggle'));
    fireEvent.change(screen.getByTestId('acm-request-domain'), {
      target: { value: 'example.test' },
    });
    fireEvent.change(screen.getByTestId('acm-request-validation-method'), {
      target: { value: 'EMAIL' },
    });
    fireEvent.change(screen.getByTestId('acm-request-sans'), {
      target: { value: ' www.example.test , , api.example.test ' },
    });
    fireEvent.click(screen.getByTestId('acm-request-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('acm-request-status')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('acm-request-status')).toHaveTextContent(
      'arn:aws:acm:eu-west-1:000000000000:certificate/requested',
    );
    expect(requestAcmCertificateMock).toHaveBeenCalledWith({
      domainName: 'example.test',
      validationMethod: 'EMAIL',
      subjectAlternativeNames: ['www.example.test', 'api.example.test'],
    });
    expect(getAcmCertificatesMock).toHaveBeenCalledTimes(2);
    expect(screen.queryByTestId('acm-request-form')).not.toBeInTheDocument();
  });

  it('requests a certificate without subject alternative names', async () => {
    await renderReady();

    fireEvent.click(screen.getByTestId('acm-request-toggle'));
    fireEvent.change(screen.getByTestId('acm-request-domain'), {
      target: { value: 'example.test' },
    });
    fireEvent.click(screen.getByTestId('acm-request-submit'));

    await waitFor(() =>
      expect(requestAcmCertificateMock).toHaveBeenCalledWith({
        domainName: 'example.test',
        validationMethod: 'DNS',
        subjectAlternativeNames: [],
      }),
    );
  });

  it('shows an error message when the request fails', async () => {
    requestAcmCertificateMock.mockRejectedValue(new Error('boom'));
    await renderReady();

    fireEvent.click(screen.getByTestId('acm-request-toggle'));
    fireEvent.change(screen.getByTestId('acm-request-domain'), {
      target: { value: 'example.test' },
    });
    fireEvent.click(screen.getByTestId('acm-request-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('acm-request-error')).toBeInTheDocument(),
    );
  });
});
