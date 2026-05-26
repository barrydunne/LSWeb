import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { SsmParameterStoreListView } from './SsmParameterStoreListView';
import { getParameters, createParameter, deleteParameter } from '../../api/client';
import type { ParameterListResult } from '../../api/client';

vi.mock('../../api/client');

const getParametersMock = vi.mocked(getParameters);
const createParameterMock = vi.mocked(createParameter);
const deleteParameterMock = vi.mocked(deleteParameter);

const listResult: ParameterListResult = {
  path: '/',
  parameters: [
    {
      name: '/app/config/db-host',
      type: 'String',
      version: 3,
      lastModifiedDate: '2024-01-01T00:00:00Z',
      arn: 'arn:db-host',
    },
    {
      name: '/app/config/api-key',
      type: 'SecureString',
      version: 1,
      lastModifiedDate: null,
      arn: 'arn:api-key',
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <SsmParameterStoreListView serviceKey="ssm-parameter-store" />
    </MemoryRouter>,
  );
}

describe('SsmParameterStoreListView', () => {
  beforeEach(() => {
    getParametersMock.mockResolvedValue(listResult);
    createParameterMock.mockResolvedValue();
    deleteParameterMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before parameters arrive', () => {
    getParametersMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('ssm-parameter-store-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getParametersMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-error')).toBeInTheDocument(),
    );
  });

  it('requests the full hierarchy from the root path recursively', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );

    expect(getParametersMock).toHaveBeenCalledWith('/', true, expect.anything());
  });

  it('renders a row per parameter with links to the detail view', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );

    const links = screen.getAllByTestId('ssm-parameter-store-list-link');
    expect(links[0]).toHaveTextContent('/app/config/db-host');
    expect(links[0]).toHaveAttribute(
      'href',
      '/services/ssm-parameter-store/%2Fapp%2Fconfig%2Fdb-host',
    );
    expect(links[1]).toHaveTextContent('/app/config/api-key');
    expect(links[1]).toHaveAttribute(
      'href',
      '/services/ssm-parameter-store/%2Fapp%2Fconfig%2Fapi-key',
    );
  });

  it('toggles the create form', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('ssm-parameter-store-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-toggle'));
    expect(screen.getByTestId('ssm-parameter-store-create-form')).toBeInTheDocument();
  });

  it('creates a parameter with a trimmed description and shows a status message', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-toggle'));

    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-name'), {
      target: { value: '/app/config/new' },
    });
    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-type'), {
      target: { value: 'SecureString' },
    });
    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-value'), {
      target: { value: 's3cr3t' },
    });
    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-description'), {
      target: { value: '  primary config  ' },
    });
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-create-status')).toBeInTheDocument(),
    );
    expect(createParameterMock).toHaveBeenCalledWith({
      name: '/app/config/new',
      type: 'SecureString',
      value: 's3cr3t',
      description: 'primary config',
    });
  });

  it('creates a parameter without a description as null', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-toggle'));

    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-name'), {
      target: { value: '/app/config/new' },
    });
    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-value'), {
      target: { value: 'plain' },
    });
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-create-status')).toBeInTheDocument(),
    );
    expect(createParameterMock).toHaveBeenCalledWith({
      name: '/app/config/new',
      type: 'String',
      value: 'plain',
      description: null,
    });
  });

  it('shows a saving label while the create is in flight', async () => {
    let resolveCreate: (() => void) | undefined;
    createParameterMock.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveCreate = resolve;
      }),
    );
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-toggle'));
    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-name'), {
      target: { value: '/app/config/new' },
    });
    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-value'), {
      target: { value: 'plain' },
    });
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-submit'));

    expect(screen.getByTestId('ssm-parameter-store-create-submit')).toBeDisabled();
    expect(screen.getByTestId('ssm-parameter-store-create-submit')).toHaveTextContent('Creating');

    resolveCreate?.();
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-create-status')).toBeInTheDocument(),
    );
  });

  it('shows an error when parameter creation fails', async () => {
    createParameterMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-toggle'));

    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-name'), {
      target: { value: '/app/config/new' },
    });
    fireEvent.change(screen.getByTestId('ssm-parameter-store-create-value'), {
      target: { value: 'plain' },
    });
    fireEvent.click(screen.getByTestId('ssm-parameter-store-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-create-error')).toBeInTheDocument(),
    );
  });

  it('deletes a parameter after confirmation', async () => {
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteParameterMock).toHaveBeenCalledWith('/app/config/db-host'));
  });

  it('shows an error when parameter deletion fails', async () => {
    deleteParameterMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-view')).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByTestId('confirm-trigger')[0]);
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-list-error')).toBeInTheDocument(),
    );
  });
});
