import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { IamAccountPanel } from './IamAccountPanel';
import {
  IamNotSupportedError,
  createIamAccountAlias,
  deleteIamAccountAlias,
  deleteIamAccountPasswordPolicy,
  getIamAccountAliases,
  getIamAccountPasswordPolicy,
  getIamAccountSummary,
  updateIamAccountPasswordPolicy,
} from '../../api/client';
import type { IamPasswordPolicy } from '../../api/client';

vi.mock('../../api/client', async (importActual) => {
  const actual = await importActual<typeof import('../../api/client')>();
  return {
    ...actual,
    getIamAccountSummary: vi.fn(),
    getIamAccountPasswordPolicy: vi.fn(),
    updateIamAccountPasswordPolicy: vi.fn(),
    deleteIamAccountPasswordPolicy: vi.fn(),
    getIamAccountAliases: vi.fn(),
    createIamAccountAlias: vi.fn(),
    deleteIamAccountAlias: vi.fn(),
  };
});

const getSummaryMock = vi.mocked(getIamAccountSummary);
const getPolicyMock = vi.mocked(getIamAccountPasswordPolicy);
const updatePolicyMock = vi.mocked(updateIamAccountPasswordPolicy);
const deletePolicyMock = vi.mocked(deleteIamAccountPasswordPolicy);
const getAliasesMock = vi.mocked(getIamAccountAliases);
const createAliasMock = vi.mocked(createIamAccountAlias);
const deleteAliasMock = vi.mocked(deleteIamAccountAlias);

const samplePolicy: IamPasswordPolicy = {
  minimumPasswordLength: 14,
  requireSymbols: true,
  requireNumbers: true,
  requireUppercaseCharacters: true,
  requireLowercaseCharacters: true,
  allowUsersToChangePassword: true,
  expirePasswords: true,
  maxPasswordAge: 90,
  passwordReusePrevention: 5,
  hardExpiry: false,
};

function renderView() {
  return render(<IamAccountPanel serviceKey="iam" />);
}

function confirmFirstDelete(container: HTMLElement) {
  fireEvent.click(within(container).getByTestId('confirm-trigger'));
  fireEvent.click(screen.getByTestId('confirm-accept'));
}

describe('IamAccountPanel', () => {
  beforeEach(() => {
    getSummaryMock.mockResolvedValue({ entries: { Users: 3, Groups: 1 } });
    getPolicyMock.mockResolvedValue(null);
    updatePolicyMock.mockResolvedValue();
    deletePolicyMock.mockResolvedValue();
    getAliasesMock.mockResolvedValue({ aliases: [] });
    createAliasMock.mockResolvedValue();
    deleteAliasMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows loading states before data arrives', () => {
    getSummaryMock.mockReturnValue(new Promise(() => {}));
    getPolicyMock.mockReturnValue(new Promise(() => {}));
    getAliasesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('iam-account-summary-loading')).toBeInTheDocument();
    expect(screen.getByTestId('iam-account-policy-loading')).toBeInTheDocument();
    expect(screen.getByTestId('iam-account-aliases-loading')).toBeInTheDocument();
  });

  it('renders summary cards from the account entries', async () => {
    renderView();

    await screen.findByTestId('iam-account-summary-cards');
    expect(screen.getByTestId('iam-account-summary-card-Users')).toHaveTextContent('3');
    expect(screen.getByTestId('iam-account-summary-card-Groups')).toHaveTextContent('1');
  });

  it('shows an empty summary state when there are no entries', async () => {
    getSummaryMock.mockResolvedValue({ entries: {} });

    renderView();

    await screen.findByTestId('iam-account-summary-empty');
  });

  it('shows a not-supported state for the summary', async () => {
    getSummaryMock.mockRejectedValue(new IamNotSupportedError('nope'));

    renderView();

    await screen.findByTestId('iam-account-summary-unsupported');
  });

  it('shows an error state for the summary', async () => {
    getSummaryMock.mockRejectedValue(new Error('boom'));

    renderView();

    await screen.findByTestId('iam-account-summary-error');
  });

  it('shows the not-set state and creates a new password policy', async () => {
    renderView();

    await screen.findByTestId('iam-account-policy-not-set');
    fireEvent.click(screen.getByTestId('iam-account-policy-edit'));

    fireEvent.change(screen.getByTestId('iam-account-policy-min-length'), {
      target: { value: '12' },
    });
    fireEvent.click(screen.getByTestId('iam-account-policy-require-symbols'));
    fireEvent.click(screen.getByTestId('iam-account-policy-require-numbers'));
    fireEvent.click(screen.getByTestId('iam-account-policy-require-uppercase'));
    fireEvent.click(screen.getByTestId('iam-account-policy-require-lowercase'));
    fireEvent.click(screen.getByTestId('iam-account-policy-allow-change'));
    fireEvent.click(screen.getByTestId('iam-account-policy-expire'));
    fireEvent.change(screen.getByTestId('iam-account-policy-max-age'), {
      target: { value: '45' },
    });
    fireEvent.change(screen.getByTestId('iam-account-policy-reuse'), {
      target: { value: '3' },
    });
    fireEvent.click(screen.getByTestId('iam-account-policy-hard-expiry'));

    fireEvent.click(screen.getByTestId('iam-account-policy-save'));

    await waitFor(() => expect(updatePolicyMock).toHaveBeenCalledTimes(1));
    expect(updatePolicyMock).toHaveBeenCalledWith({
      minimumPasswordLength: 12,
      requireSymbols: true,
      requireNumbers: true,
      requireUppercaseCharacters: true,
      requireLowercaseCharacters: true,
      allowUsersToChangePassword: false,
      maxPasswordAge: 45,
      passwordReusePrevention: 3,
      hardExpiry: true,
    });
  });

  it('renders an existing policy summary and updates it', async () => {
    getPolicyMock.mockResolvedValue(samplePolicy);

    renderView();

    await screen.findByTestId('iam-account-policy-summary');
    fireEvent.click(screen.getByTestId('iam-account-policy-edit'));
    fireEvent.click(screen.getByTestId('iam-account-policy-save'));

    await waitFor(() => expect(updatePolicyMock).toHaveBeenCalledTimes(1));
    expect(updatePolicyMock).toHaveBeenCalledWith(
      expect.objectContaining({ minimumPasswordLength: 14, maxPasswordAge: 90 }),
    );
  });

  it('renders a not-set numeric fallback when an existing policy omits ages', async () => {
    getPolicyMock.mockResolvedValue({
      ...samplePolicy,
      requireSymbols: false,
      requireNumbers: false,
      requireUppercaseCharacters: false,
      requireLowercaseCharacters: false,
      allowUsersToChangePassword: false,
      hardExpiry: true,
      maxPasswordAge: null,
      passwordReusePrevention: null,
      expirePasswords: false,
    });

    renderView();

    const summary = await screen.findByTestId('iam-account-policy-summary');
    expect(summary).toHaveTextContent('Never expires');
    expect(summary).toHaveTextContent('Off');

    fireEvent.click(screen.getByTestId('iam-account-policy-edit'));
    fireEvent.click(screen.getByTestId('iam-account-policy-save'));

    await waitFor(() => expect(updatePolicyMock).toHaveBeenCalledTimes(1));
    expect(updatePolicyMock).toHaveBeenCalledWith(
      expect.objectContaining({ maxPasswordAge: null, passwordReusePrevention: null }),
    );
  });

  it('cancels the policy editor without saving', async () => {
    renderView();

    await screen.findByTestId('iam-account-policy-not-set');
    fireEvent.click(screen.getByTestId('iam-account-policy-edit'));
    fireEvent.click(screen.getByTestId('iam-account-policy-cancel'));

    expect(screen.queryByTestId('iam-account-policy-form')).not.toBeInTheDocument();
    expect(updatePolicyMock).not.toHaveBeenCalled();
  });

  it('shows a save error when the policy update fails', async () => {
    updatePolicyMock.mockRejectedValue(new Error('boom'));

    renderView();

    await screen.findByTestId('iam-account-policy-not-set');
    fireEvent.click(screen.getByTestId('iam-account-policy-edit'));
    fireEvent.click(screen.getByTestId('iam-account-policy-save'));

    await screen.findByTestId('iam-account-policy-save-error');
  });

  it('deletes an existing password policy', async () => {
    getPolicyMock.mockResolvedValue(samplePolicy);

    renderView();

    const section = await screen.findByTestId('iam-account-password-policy');
    confirmFirstDelete(section);

    await waitFor(() => expect(deletePolicyMock).toHaveBeenCalledTimes(1));
  });

  it('shows an error when deleting the policy fails', async () => {
    getPolicyMock.mockResolvedValue(samplePolicy);
    deletePolicyMock.mockRejectedValue(new Error('boom'));

    renderView();

    const section = await screen.findByTestId('iam-account-password-policy');
    confirmFirstDelete(section);

    await screen.findByTestId('iam-account-policy-error');
  });

  it('shows a not-supported state for the password policy', async () => {
    getPolicyMock.mockRejectedValue(new IamNotSupportedError('nope'));

    renderView();

    await screen.findByTestId('iam-account-policy-unsupported');
  });

  it('shows an error state for the password policy', async () => {
    getPolicyMock.mockRejectedValue(new Error('boom'));

    renderView();

    await screen.findByTestId('iam-account-policy-error');
  });

  it('shows an empty aliases state', async () => {
    renderView();

    await screen.findByTestId('iam-account-aliases-empty');
  });

  it('creates an account alias', async () => {
    renderView();

    await screen.findByTestId('iam-account-alias-form');
    fireEvent.change(screen.getByTestId('iam-account-alias-input'), {
      target: { value: 'my-alias' },
    });
    fireEvent.click(screen.getByTestId('iam-account-alias-create'));

    await waitFor(() => expect(createAliasMock).toHaveBeenCalledWith('my-alias'));
  });

  it('ignores an empty alias submission', async () => {
    renderView();

    await screen.findByTestId('iam-account-alias-form');
    fireEvent.click(screen.getByTestId('iam-account-alias-create'));

    expect(createAliasMock).not.toHaveBeenCalled();
  });

  it('shows an error when alias creation fails', async () => {
    createAliasMock.mockRejectedValue(new Error('boom'));

    renderView();

    await screen.findByTestId('iam-account-alias-form');
    fireEvent.change(screen.getByTestId('iam-account-alias-input'), {
      target: { value: 'my-alias' },
    });
    fireEvent.click(screen.getByTestId('iam-account-alias-create'));

    await screen.findByTestId('iam-account-alias-error');
  });

  it('renders existing aliases and deletes one', async () => {
    getAliasesMock.mockResolvedValue({ aliases: ['acme'] });

    renderView();

    const row = await screen.findByTestId('iam-account-alias-acme');
    confirmFirstDelete(row);

    await waitFor(() => expect(deleteAliasMock).toHaveBeenCalledWith('acme'));
  });

  it('shows an error when alias deletion fails', async () => {
    getAliasesMock.mockResolvedValue({ aliases: ['acme'] });
    deleteAliasMock.mockRejectedValue(new Error('boom'));

    renderView();

    const row = await screen.findByTestId('iam-account-alias-acme');
    confirmFirstDelete(row);

    await screen.findByTestId('iam-account-aliases-error');
  });

  it('shows a not-supported state for aliases', async () => {
    getAliasesMock.mockRejectedValue(new IamNotSupportedError('nope'));

    renderView();

    await screen.findByTestId('iam-account-aliases-unsupported');
  });

  it('shows an error state for aliases', async () => {
    getAliasesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await screen.findByTestId('iam-account-aliases-error');
  });
});
