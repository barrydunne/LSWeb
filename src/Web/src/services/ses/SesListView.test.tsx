import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SesListView } from './SesListView';
import { getSesIdentities } from '../../api/client';
import type { SesIdentityListResult } from '../../api/client';

vi.mock('../../api/client');

const getSesIdentitiesMock = vi.mocked(getSesIdentities);

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
  });

  afterEach(() => {
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
});
