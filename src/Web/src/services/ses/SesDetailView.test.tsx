import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SesDetailView } from './SesDetailView';
import { deleteSesIdentity, getSesDomainSetup, getSesIdentityDetail } from '../../api/client';
import type { SesIdentityDetailResult } from '../../api/client';

const navigateMock = vi.fn();

vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>();
  return { ...actual, useNavigate: () => navigateMock };
});

vi.mock('../../api/client');

const getSesIdentityDetailMock = vi.mocked(getSesIdentityDetail);
const deleteSesIdentityMock = vi.mocked(deleteSesIdentity);
const getSesDomainSetupMock = vi.mocked(getSesDomainSetup);

const detail: SesIdentityDetailResult = {
  identity: 'sender@example.com',
  identityType: 'EmailAddress',
  verificationStatus: 'Pending',
};

function renderView() {
  return render(
    <MemoryRouter>
      <SesDetailView serviceKey="ses" resourceId="sender@example.com" />
    </MemoryRouter>,
  );
}

describe('SesDetailView', () => {
  beforeEach(() => {
    getSesIdentityDetailMock.mockResolvedValue(detail);
    deleteSesIdentityMock.mockResolvedValue();
    getSesDomainSetupMock.mockResolvedValue({
      domain: 'example.com',
      verificationStatus: 'Pending',
      verificationToken: 'token-123',
      dkimVerificationStatus: 'NotStarted',
      dkimTokens: [],
    });
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before the detail arrives', () => {
    getSesIdentityDetailMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('ses-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSesIdentityDetailMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('ses-detail-error')).toBeInTheDocument());
  });

  it('shows the identity, type, status and lifecycle guidance', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('ses-detail-view')).toBeInTheDocument());

    expect(screen.getByTestId('ses-detail-identity')).toHaveTextContent('sender@example.com');
    expect(screen.getByTestId('ses-detail-type')).toHaveTextContent('EmailAddress');
    expect(screen.getByTestId('ses-detail-status')).toHaveTextContent('Pending');
    expect(screen.getByTestId('ses-detail-guidance')).toHaveTextContent('verification request is in progress');
  });

  it('falls back to a generic guidance for an unknown status', async () => {
    getSesIdentityDetailMock.mockResolvedValue({ ...detail, verificationStatus: 'Weird' });

    renderView();

    await waitFor(() => expect(screen.getByTestId('ses-detail-view')).toBeInTheDocument());
    expect(screen.getByTestId('ses-detail-guidance')).toHaveTextContent(
      'Verification status reported by the backend.',
    );
  });

  it('refreshes the detail when the refresh button is clicked', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('ses-detail-view')).toBeInTheDocument());
    const before = getSesIdentityDetailMock.mock.calls.length;

    fireEvent.click(screen.getByTestId('ses-detail-refresh'));

    await waitFor(() =>
      expect(getSesIdentityDetailMock.mock.calls.length).toBeGreaterThan(before),
    );
  });

  it('deletes the identity and navigates back to the list', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('ses-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteSesIdentityMock).toHaveBeenCalledWith('sender@example.com'));
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/services/ses'));
  });

  it('shows the domain setup panel for a domain identity', async () => {
    getSesIdentityDetailMock.mockResolvedValue({
      identity: 'example.com',
      identityType: 'Domain',
      verificationStatus: 'Pending',
    });

    render(
      <MemoryRouter>
        <SesDetailView serviceKey="ses" resourceId="example.com" />
      </MemoryRouter>,
    );

    await waitFor(() => expect(screen.getByTestId('ses-domain-setup')).toBeInTheDocument());
  });

  it('does not show the domain setup panel for an email identity', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('ses-detail-view')).toBeInTheDocument());
    expect(screen.queryByTestId('ses-domain-setup')).not.toBeInTheDocument();
  });

  it('shows an error when deleting fails', async () => {
    deleteSesIdentityMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('ses-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('ses-detail-delete-error')).toBeInTheDocument());
    expect(navigateMock).not.toHaveBeenCalled();
  });
});
