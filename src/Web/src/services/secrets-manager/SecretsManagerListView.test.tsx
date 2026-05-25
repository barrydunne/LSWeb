import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SecretsManagerListView } from './SecretsManagerListView';
import { getSecrets, createSecret, deleteSecret } from '../../api/client';
import type { SecretListResult } from '../../api/client';

vi.mock('../../api/client');

const getSecretsMock = vi.mocked(getSecrets);
const createSecretMock = vi.mocked(createSecret);
const deleteSecretMock = vi.mocked(deleteSecret);

const listResult: SecretListResult = {
  secrets: [
    {
      name: 'db-password',
      arn: 'arn:db-password',
      description: 'primary db',
      createdDate: null,
      lastChangedDate: null,
    },
    {
      name: 'api-key',
      arn: 'arn:api-key',
      description: null,
      createdDate: null,
      lastChangedDate: null,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <SecretsManagerListView serviceKey="secrets-manager" />
    </MemoryRouter>,
  );
}

describe('SecretsManagerListView', () => {
  beforeEach(() => {
    getSecretsMock.mockResolvedValue(listResult);
    createSecretMock.mockResolvedValue();
    deleteSecretMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before secrets arrive', () => {
    getSecretsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('secrets-manager-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getSecretsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-error')).toBeInTheDocument(),
    );
  });

  it('renders a row per secret with links to the detail view', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-view')).toBeInTheDocument(),
    );

    const links = screen.getAllByTestId('secrets-manager-list-link');
    expect(links[0]).toHaveTextContent('db-password');
    expect(links[0]).toHaveAttribute('href', '/services/secrets-manager/db-password');
    expect(links[1]).toHaveTextContent('api-key');
    expect(links[1]).toHaveAttribute('href', '/services/secrets-manager/api-key');
  });

  it('toggles the create form', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-view')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('secrets-manager-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('secrets-manager-create-toggle'));
    expect(screen.getByTestId('secrets-manager-create-form')).toBeInTheDocument();
  });

  it('creates a secret with a trimmed description and shows a status message', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('secrets-manager-create-toggle'));

    fireEvent.change(screen.getByTestId('secrets-manager-create-name'), {
      target: { value: 'new-secret' },
    });
    fireEvent.change(screen.getByTestId('secrets-manager-create-description'), {
      target: { value: '  primary db  ' },
    });
    fireEvent.change(screen.getByTestId('secrets-manager-create-value'), {
      target: { value: 's3cr3t' },
    });
    fireEvent.click(screen.getByTestId('secrets-manager-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-create-status')).toBeInTheDocument(),
    );
    expect(createSecretMock).toHaveBeenCalledWith({
      name: 'new-secret',
      description: 'primary db',
      secretString: 's3cr3t',
    });
  });

  it('creates a secret without a description as null', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('secrets-manager-create-toggle'));

    fireEvent.change(screen.getByTestId('secrets-manager-create-name'), {
      target: { value: 'new-secret' },
    });
    fireEvent.change(screen.getByTestId('secrets-manager-create-value'), {
      target: { value: 's3cr3t' },
    });
    fireEvent.click(screen.getByTestId('secrets-manager-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-create-status')).toBeInTheDocument(),
    );
    expect(createSecretMock).toHaveBeenCalledWith({
      name: 'new-secret',
      description: null,
      secretString: 's3cr3t',
    });
  });

  it('shows a saving label while the create is in flight', async () => {
    let resolveCreate: (() => void) | undefined;
    createSecretMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveCreate = resolve;
      }),
    );
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('secrets-manager-create-toggle'));
    fireEvent.change(screen.getByTestId('secrets-manager-create-name'), {
      target: { value: 'new-secret' },
    });
    fireEvent.change(screen.getByTestId('secrets-manager-create-value'), {
      target: { value: 's3cr3t' },
    });
    fireEvent.click(screen.getByTestId('secrets-manager-create-submit'));

    expect(screen.getByTestId('secrets-manager-create-submit')).toBeDisabled();
    expect(screen.getByTestId('secrets-manager-create-submit')).toHaveTextContent('Creating');

    resolveCreate?.();
    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-create-status')).toBeInTheDocument(),
    );
  });

  it('shows an error when secret creation fails', async () => {
    createSecretMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('secrets-manager-create-toggle'));

    fireEvent.change(screen.getByTestId('secrets-manager-create-name'), {
      target: { value: 'new-secret' },
    });
    fireEvent.change(screen.getByTestId('secrets-manager-create-value'), {
      target: { value: 's3cr3t' },
    });
    fireEvent.click(screen.getByTestId('secrets-manager-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-create-error')).toBeInTheDocument(),
    );
  });

  it('deletes a secret after confirmation', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteSecretMock).toHaveBeenCalledWith('db-password'));
  });

  it('shows an error when secret deletion fails', async () => {
    deleteSecretMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('secrets-manager-list-error')).toBeInTheDocument(),
    );
  });
});
