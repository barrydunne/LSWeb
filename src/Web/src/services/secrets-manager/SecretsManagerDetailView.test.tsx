import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { SecretsManagerDetailView } from './SecretsManagerDetailView';
import { getSecretValue, getSecretVersions, putSecretValue } from '../../api/client';
import type { SecretValueResult, SecretVersionListResult } from '../../api/client';

vi.mock('../../api/client');

const getSecretValueMock = vi.mocked(getSecretValue);
const getSecretVersionsMock = vi.mocked(getSecretVersions);
const putSecretValueMock = vi.mocked(putSecretValue);

const maskedValue: SecretValueResult = {
  name: 'db-password',
  arn: 'arn:db-password',
  versionId: 'v1',
  value: '********',
  revealAllowed: true,
};

const versionList: SecretVersionListResult = {
  name: 'db-password',
  arn: 'arn:db-password',
  versions: [
    { versionId: 'v2', stages: ['AWSCURRENT', 'custom'], createdDate: null, lastAccessedDate: null },
    { versionId: 'v1', stages: ['AWSPREVIOUS'], createdDate: null, lastAccessedDate: null },
  ],
};

function renderView() {
  return render(<SecretsManagerDetailView serviceKey="secrets-manager" resourceId="db-password" />);
}

describe('SecretsManagerDetailView', () => {
  beforeEach(() => {
    getSecretValueMock.mockResolvedValue(maskedValue);
    getSecretVersionsMock.mockResolvedValue(versionList);
    putSecretValueMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the secret arrives', () => {
    getSecretValueMock.mockReturnValue(new Promise(() => {}));
    getSecretVersionsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('secrets-manager-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSecretValueMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-error')).toBeInTheDocument(),
    );
  });

  it('renders the secret with a masked value by default', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-view')).toBeInTheDocument(),
    );

    expect(getSecretValueMock).toHaveBeenCalledWith('db-password', false, expect.anything());
    expect(screen.getByTestId('secrets-manager-detail-name')).toHaveTextContent('db-password');
    expect(screen.getByTestId('secrets-manager-detail-arn')).toHaveTextContent('arn:db-password');
    expect(screen.getByTestId('secrets-manager-detail-versionId')).toHaveTextContent('v1');
    expect(screen.getByTestId('secrets-manager-detail-value')).toHaveTextContent('********');
    expect(screen.getByTestId('secrets-manager-detail-reveal')).toHaveTextContent('Reveal value');
  });

  it('shows an em dash when the version is null', async () => {
    getSecretValueMock.mockResolvedValue({ ...maskedValue, versionId: null });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('secrets-manager-detail-versionId')).toHaveTextContent('\u2014');
  });

  it('hides the reveal control when revealing is not allowed', async () => {
    getSecretValueMock.mockResolvedValue({ ...maskedValue, revealAllowed: false });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-view')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('secrets-manager-detail-reveal')).not.toBeInTheDocument();
  });

  it('refetches with the reveal flag toggled', async () => {
    const user = userEvent.setup();
    getSecretValueMock
      .mockResolvedValueOnce(maskedValue)
      .mockResolvedValueOnce({ ...maskedValue, value: 's3cr3t' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('secrets-manager-detail-reveal'));

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-value')).toHaveTextContent('s3cr3t'),
    );
    expect(getSecretValueMock).toHaveBeenLastCalledWith('db-password', true, undefined);
    expect(screen.getByTestId('secrets-manager-detail-reveal')).toHaveTextContent('Hide value');
  });

  it('saves a new value after confirmation and refetches', async () => {
    const user = userEvent.setup();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-view')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('secrets-manager-detail-edit'), 'new-value');
    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-save-status')).toBeInTheDocument(),
    );
    expect(putSecretValueMock).toHaveBeenCalledWith('db-password', { secretString: 'new-value' });
    await waitFor(() => expect(getSecretValueMock).toHaveBeenCalledTimes(2));
  });

  it('shows a save error when the update fails', async () => {
    const user = userEvent.setup();
    putSecretValueMock.mockRejectedValue(new Error('write boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-view')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('secrets-manager-detail-edit'), 'new-value');
    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-save-error')).toBeInTheDocument(),
    );
  });

  it('renders each version with its stage labels', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-versions-list')).toBeInTheDocument(),
    );

    expect(getSecretVersionsMock).toHaveBeenCalledWith('db-password', expect.anything());
    const rows = screen.getAllByTestId('secrets-manager-detail-version');
    expect(rows).toHaveLength(2);
    expect(rows[0]).toHaveTextContent('v2');
    const stages = screen.getAllByTestId('secrets-manager-detail-version-stage');
    expect(stages.map((stage) => stage.textContent)).toEqual([
      'AWSCURRENT',
      'custom',
      'AWSPREVIOUS',
    ]);
  });

  it('shows a loading state while the version stages are fetched', async () => {
    getSecretVersionsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-versions-loading')).toBeInTheDocument(),
    );
  });

  it('shows an error when the version stages fail to load', async () => {
    getSecretVersionsMock.mockRejectedValue(new Error('versions boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-versions-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty state when there are no staged versions', async () => {
    getSecretVersionsMock.mockResolvedValue({ ...versionList, versions: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-versions-empty')).toBeInTheDocument(),
    );
  });

  it('reloads the version stages after saving a new value', async () => {
    const user = userEvent.setup();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-detail-view')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('secrets-manager-detail-edit'), 'new-value');
    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(getSecretVersionsMock).toHaveBeenCalledTimes(2));
  });
});
