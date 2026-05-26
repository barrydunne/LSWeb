import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { SsmParameterStoreDetailView } from './SsmParameterStoreDetailView';
import { getParameterHistory, getParameterValue, updateParameterValue } from '../../api/client';
import type { ParameterHistoryResult, ParameterValueResult } from '../../api/client';

vi.mock('../../api/client');

const getParameterValueMock = vi.mocked(getParameterValue);
const updateParameterValueMock = vi.mocked(updateParameterValue);
const getParameterHistoryMock = vi.mocked(getParameterHistory);

const maskedValue: ParameterValueResult = {
  name: '/app/db/password',
  type: 'SecureString',
  version: 3,
  value: '********',
  isSensitive: true,
  revealAllowed: true,
};

const historyResult: ParameterHistoryResult = {
  name: '/app/db/password',
  revealAllowed: true,
  entries: [
    {
      type: 'SecureString',
      version: 3,
      value: '********',
      lastModifiedDate: '2024-05-06T07:08:09Z',
      lastModifiedUser: 'arn:user/admin',
      isSensitive: true,
    },
    {
      type: 'SecureString',
      version: 2,
      value: '********',
      lastModifiedDate: '2024-04-01T01:02:03Z',
      lastModifiedUser: 'arn:user/admin',
      isSensitive: true,
    },
  ],
};

function renderView() {
  return render(
    <SsmParameterStoreDetailView serviceKey="ssm-parameter-store" resourceId="/app/db/password" />,
  );
}

describe('SsmParameterStoreDetailView', () => {
  beforeEach(() => {
    getParameterValueMock.mockResolvedValue(maskedValue);
    updateParameterValueMock.mockResolvedValue();
    getParameterHistoryMock.mockResolvedValue(historyResult);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before the parameter arrives', () => {
    getParameterValueMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('ssm-parameter-store-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getParameterValueMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-error')).toBeInTheDocument(),
    );
  });

  it('renders the parameter with a masked value by default', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    expect(getParameterValueMock).toHaveBeenCalledWith('/app/db/password', false, expect.anything());
    expect(screen.getByTestId('ssm-parameter-store-detail-name')).toHaveTextContent('/app/db/password');
    expect(screen.getByTestId('ssm-parameter-store-detail-type')).toHaveTextContent('SecureString');
    expect(screen.getByTestId('ssm-parameter-store-detail-version')).toHaveTextContent('3');
    expect(screen.getByTestId('ssm-parameter-store-detail-value')).toHaveTextContent('********');
    expect(screen.getByTestId('ssm-parameter-store-detail-reveal')).toHaveTextContent('Reveal value');
  });

  it('hides the reveal control when revealing is not allowed', async () => {
    getParameterValueMock.mockResolvedValue({ ...maskedValue, revealAllowed: false });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    expect(screen.queryByTestId('ssm-parameter-store-detail-reveal')).not.toBeInTheDocument();
  });

  it('refetches with the reveal flag toggled', async () => {
    const user = userEvent.setup();
    getParameterValueMock
      .mockResolvedValueOnce(maskedValue)
      .mockResolvedValueOnce({ ...maskedValue, value: 's3cr3t' });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-reveal'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-value')).toHaveTextContent('s3cr3t'),
    );
    expect(getParameterValueMock).toHaveBeenLastCalledWith('/app/db/password', true, undefined);
    expect(screen.getByTestId('ssm-parameter-store-detail-reveal')).toHaveTextContent('Hide value');
  });

  it('saves a new value after confirmation and refetches', async () => {
    const user = userEvent.setup();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('ssm-parameter-store-detail-edit'), 'new-value');
    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-save-status')).toBeInTheDocument(),
    );
    expect(updateParameterValueMock).toHaveBeenCalledWith('/app/db/password', 'new-value');
    await waitFor(() => expect(getParameterValueMock).toHaveBeenCalledTimes(2));
  });

  it('shows a save error when the update fails', async () => {
    const user = userEvent.setup();
    updateParameterValueMock.mockRejectedValue(new Error('write boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('ssm-parameter-store-detail-edit'), 'new-value');
    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-save-error')).toBeInTheDocument(),
    );
  });

  it('does not request history until the panel is shown', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    expect(getParameterHistoryMock).not.toHaveBeenCalled();
    expect(screen.queryByTestId('ssm-parameter-store-detail-history')).not.toBeInTheDocument();
  });

  it('loads and renders the history when the panel is shown', async () => {
    const user = userEvent.setup();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-history')).toBeInTheDocument(),
    );
    expect(getParameterHistoryMock).toHaveBeenCalledWith('/app/db/password', false, undefined);
    expect(screen.getAllByTestId('ssm-parameter-store-detail-history-entry')).toHaveLength(2);
    expect(screen.getAllByTestId('ssm-parameter-store-detail-history-version')[0]).toHaveTextContent(
      'Version 3',
    );
    expect(screen.getAllByTestId('ssm-parameter-store-detail-history-user')[0]).toHaveTextContent(
      'arn:user/admin',
    );
    expect(screen.getAllByTestId('ssm-parameter-store-detail-history-date')[0]).toHaveTextContent(
      '2024-05-06T07:08:09Z',
    );
    expect(screen.getAllByTestId('ssm-parameter-store-detail-history-value')[0]).toHaveTextContent(
      '********',
    );
    expect(screen.getByTestId('ssm-parameter-store-detail-history-toggle')).toHaveTextContent(
      'Hide history',
    );
  });

  it('hides the history panel when toggled off', async () => {
    const user = userEvent.setup();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-history')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));

    expect(screen.queryByTestId('ssm-parameter-store-detail-history')).not.toBeInTheDocument();
    expect(getParameterHistoryMock).toHaveBeenCalledTimes(1);
  });

  it('shows a loading state while the history is being fetched', async () => {
    const user = userEvent.setup();
    getParameterHistoryMock.mockReturnValue(new Promise(() => {}));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));

    expect(screen.getByTestId('ssm-parameter-store-detail-history-loading')).toBeInTheDocument();
  });

  it('shows an error when the history request fails', async () => {
    const user = userEvent.setup();
    getParameterHistoryMock.mockRejectedValue(new Error('history boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-history-error')).toBeInTheDocument(),
    );
  });

  it('shows an empty state when there is no history', async () => {
    const user = userEvent.setup();
    getParameterHistoryMock.mockResolvedValue({ ...historyResult, entries: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-history-empty')).toBeInTheDocument(),
    );
  });

  it('renders an em dash when history metadata is missing', async () => {
    const user = userEvent.setup();
    getParameterHistoryMock.mockResolvedValue({
      name: '/app/db/password',
      revealAllowed: true,
      entries: [
        {
          type: 'String',
          version: 1,
          value: 'enabled',
          lastModifiedDate: null,
          lastModifiedUser: '',
          isSensitive: false,
        },
      ],
    });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-history')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('ssm-parameter-store-detail-history-user')).toHaveTextContent('\u2014');
    expect(screen.getByTestId('ssm-parameter-store-detail-history-date')).toHaveTextContent('\u2014');
  });

  it('reloads the history with the reveal flag when revealing while shown', async () => {
    const user = userEvent.setup();
    getParameterHistoryMock
      .mockResolvedValueOnce(historyResult)
      .mockResolvedValueOnce({
        ...historyResult,
        entries: historyResult.entries.map((entry) => ({ ...entry, value: 's3cr3t' })),
      });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-history')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-reveal'));

    await waitFor(() =>
      expect(getParameterHistoryMock).toHaveBeenLastCalledWith('/app/db/password', true, undefined),
    );
    await waitFor(() =>
      expect(screen.getAllByTestId('ssm-parameter-store-detail-history-value')[0]).toHaveTextContent(
        's3cr3t',
      ),
    );
  });

  it('reloads the history after a successful save when shown', async () => {
    const user = userEvent.setup();

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-view')).toBeInTheDocument(),
    );

    await user.click(screen.getByTestId('ssm-parameter-store-detail-history-toggle'));
    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-history')).toBeInTheDocument(),
    );

    await user.type(screen.getByTestId('ssm-parameter-store-detail-edit'), 'new-value');
    await user.click(screen.getByTestId('confirm-trigger'));
    await user.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('ssm-parameter-store-detail-save-status')).toBeInTheDocument(),
    );
    await waitFor(() => expect(getParameterHistoryMock).toHaveBeenCalledTimes(2));
  });
});
