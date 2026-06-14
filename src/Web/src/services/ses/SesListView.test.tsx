import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SesListView } from './SesListView';
import { deleteSesIdentity, getSesIdentities, verifySesDomainIdentity, verifySesEmailIdentity } from '../../api/client';
import type { SesIdentityListResult } from '../../api/client';

vi.mock('../../api/client');

const getSesIdentitiesMock = vi.mocked(getSesIdentities);
const verifySesEmailIdentityMock = vi.mocked(verifySesEmailIdentity);
const verifySesDomainIdentityMock = vi.mocked(verifySesDomainIdentity);
const deleteSesIdentityMock = vi.mocked(deleteSesIdentity);

const result: SesIdentityListResult = {
  identities: [
    {
      identity: 'sender@example.com',
      identityType: 'EmailAddress',
      verificationStatus: 'Success',
    },
    {
      identity: 'example.com',
      identityType: 'Domain',
      verificationStatus: 'Pending',
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <SesListView serviceKey="ses" />
    </MemoryRouter>,
  );
}

describe('SesListView', () => {
  beforeEach(() => {
    getSesIdentitiesMock.mockResolvedValue(result);
    verifySesEmailIdentityMock.mockResolvedValue();
    verifySesDomainIdentityMock.mockResolvedValue();
    deleteSesIdentityMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before identities arrive', () => {
    getSesIdentitiesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('ses-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSesIdentitiesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ses-list-error')).toBeInTheDocument(),
    );
  });

  it('renders a row per identity', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ses-list-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('data-list-row-sender@example.com')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-example.com')).toBeInTheDocument();
  });

  it('shows the identity, type and verification status', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ses-list-view')).toBeInTheDocument(),
    );

    const identities = screen.getAllByTestId('ses-list-identity');
    const types = screen.getAllByTestId('ses-list-type');
    const verifications = screen.getAllByTestId('ses-list-verification');
    expect(identities[0]).toHaveTextContent('sender@example.com');
    expect(types[0]).toHaveTextContent('EmailAddress');
    expect(verifications[0]).toHaveTextContent('Success');
    expect(verifications[1]).toHaveTextContent('Pending');
  });

  it('reloads the identities when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('ses-list-view')).toBeInTheDocument(),
      );
      expect(getSesIdentitiesMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getSesIdentitiesMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });

  it('requests verification of a valid email identity', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('ses-verify-toggle'));
    fireEvent.change(screen.getByTestId('ses-verify-email'), {
      target: { value: 'new@example.com' },
    });
    fireEvent.click(screen.getByTestId('ses-verify-submit'));

    await waitFor(() =>
      expect(verifySesEmailIdentityMock).toHaveBeenCalledWith('new@example.com'),
    );
  });

  it('blocks verification when the email is invalid', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('ses-verify-toggle'));
    fireEvent.change(screen.getByTestId('ses-verify-email'), {
      target: { value: 'not-an-email' },
    });
    fireEvent.click(screen.getByTestId('ses-verify-submit'));

    expect(screen.getByTestId('ses-verify-error')).toBeInTheDocument();
    expect(verifySesEmailIdentityMock).not.toHaveBeenCalled();
  });

  it('shows an error when the verification request fails', async () => {
    verifySesEmailIdentityMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('ses-verify-toggle'));
    fireEvent.change(screen.getByTestId('ses-verify-email'), {
      target: { value: 'new@example.com' },
    });
    fireEvent.click(screen.getByTestId('ses-verify-submit'));

    await waitFor(() => expect(screen.getByTestId('ses-verify-error')).toBeInTheDocument());
  });

  it('hides the verify form when toggled off', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('ses-verify-toggle'));
    expect(screen.getByTestId('ses-verify-form')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('ses-verify-toggle'));
    expect(screen.queryByTestId('ses-verify-form')).not.toBeInTheDocument();
  });

  it('deletes an identity after confirmation', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    const triggers = screen.getAllByTestId('confirm-trigger');
    fireEvent.click(triggers[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(deleteSesIdentityMock).toHaveBeenCalledWith('sender@example.com'),
    );
  });

  it('shows an error when deleting an identity fails', async () => {
    deleteSesIdentityMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    const triggers = screen.getAllByTestId('confirm-trigger');
    fireEvent.click(triggers[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('ses-list-error')).toBeInTheDocument());
  });

  it('initiates verification of a valid domain identity', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('ses-verify-domain-toggle'));
    fireEvent.change(screen.getByTestId('ses-verify-domain'), {
      target: { value: 'new-domain.com' },
    });
    fireEvent.click(screen.getByTestId('ses-verify-domain-submit'));

    await waitFor(() =>
      expect(verifySesDomainIdentityMock).toHaveBeenCalledWith('new-domain.com'),
    );
  });

  it('blocks domain verification when the domain is invalid', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('ses-verify-domain-toggle'));
    fireEvent.change(screen.getByTestId('ses-verify-domain'), {
      target: { value: 'nodot' },
    });
    fireEvent.click(screen.getByTestId('ses-verify-domain-submit'));

    expect(screen.getByTestId('ses-verify-domain-error')).toBeInTheDocument();
    expect(verifySesDomainIdentityMock).not.toHaveBeenCalled();
  });

  it('shows an error when the domain verification request fails', async () => {
    verifySesDomainIdentityMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('ses-verify-domain-toggle'));
    fireEvent.change(screen.getByTestId('ses-verify-domain'), {
      target: { value: 'new-domain.com' },
    });
    fireEvent.click(screen.getByTestId('ses-verify-domain-submit'));

    await waitFor(() => expect(screen.getByTestId('ses-verify-domain-error')).toBeInTheDocument());
  });

  it('hides the verify domain form when toggled off', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('ses-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('ses-verify-domain-toggle'));
    expect(screen.getByTestId('ses-verify-domain-form')).toBeInTheDocument();
    fireEvent.click(screen.getByTestId('ses-verify-domain-toggle'));
    expect(screen.queryByTestId('ses-verify-domain-form')).not.toBeInTheDocument();
  });
});
