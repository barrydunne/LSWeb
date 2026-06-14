import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { SesDomainSetupPanel } from './SesDomainSetupPanel';
import { enableSesDomainDkim, getSesDomainSetup } from '../../api/client';
import type { SesDomainSetupResult } from '../../api/client';

vi.mock('../../api/client');

const getSesDomainSetupMock = vi.mocked(getSesDomainSetup);
const enableSesDomainDkimMock = vi.mocked(enableSesDomainDkim);

const setup: SesDomainSetupResult = {
  domain: 'example.com',
  verificationStatus: 'Pending',
  verificationToken: 'verify-token',
  dkimVerificationStatus: 'Success',
  dkimTokens: ['token-a', 'token-b'],
};

function renderPanel() {
  return render(<SesDomainSetupPanel domain="example.com" />);
}

describe('SesDomainSetupPanel', () => {
  beforeEach(() => {
    getSesDomainSetupMock.mockResolvedValue(setup);
    enableSesDomainDkimMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before the setup arrives', () => {
    getSesDomainSetupMock.mockReturnValue(new Promise(() => {}));

    renderPanel();

    expect(screen.getByTestId('ses-domain-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSesDomainSetupMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('ses-domain-error')).toBeInTheDocument());
  });

  it('shows the TXT verification record and DKIM CNAME records', async () => {
    renderPanel();

    await waitFor(() => expect(screen.getByTestId('ses-domain-setup')).toBeInTheDocument());

    expect(screen.getByTestId('ses-domain-verification-status')).toHaveTextContent('Pending');
    expect(screen.getByTestId('ses-domain-txt-value')).toHaveTextContent('verify-token');
    expect(screen.getByTestId('ses-domain-dkim-status')).toHaveTextContent('Success');
    expect(screen.getAllByTestId('ses-domain-dkim-record')).toHaveLength(2);
  });

  it('shows empty states when no tokens are present', async () => {
    getSesDomainSetupMock.mockResolvedValue({
      ...setup,
      verificationToken: '',
      dkimTokens: [],
    });

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('ses-domain-setup')).toBeInTheDocument());
    expect(screen.getByTestId('ses-domain-txt-empty')).toBeInTheDocument();
    expect(screen.getByTestId('ses-domain-dkim-empty')).toBeInTheDocument();
  });

  it('enables DKIM and reloads the setup', async () => {
    renderPanel();

    await waitFor(() => expect(screen.getByTestId('ses-domain-setup')).toBeInTheDocument());
    const before = getSesDomainSetupMock.mock.calls.length;

    fireEvent.click(screen.getByTestId('ses-domain-enable-dkim'));

    await waitFor(() => expect(enableSesDomainDkimMock).toHaveBeenCalledWith('example.com'));
    await waitFor(() =>
      expect(getSesDomainSetupMock.mock.calls.length).toBeGreaterThan(before),
    );
  });

  it('shows an error when enabling DKIM fails', async () => {
    enableSesDomainDkimMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await waitFor(() => expect(screen.getByTestId('ses-domain-setup')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('ses-domain-enable-dkim'));

    await waitFor(() => expect(screen.getByTestId('ses-domain-dkim-error')).toBeInTheDocument());
  });
});
