import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties, FormEvent } from 'react';
import { ConfirmationHost } from '../../components/ConfirmationHost';
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
import type { IamAccountSummary, IamPasswordPolicy } from '../../api/client';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  marginBottom: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const headingStyle: CSSProperties = { fontSize: 14, fontWeight: 600 };

const messageStyle: CSSProperties = { fontSize: 14 };

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const checkboxRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 6,
  fontSize: 13,
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
  maxWidth: 200,
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const cardsStyle: CSSProperties = {
  display: 'grid',
  gridTemplateColumns: 'repeat(auto-fill, minmax(160px, 1fr))',
  gap: 8,
};

const cardStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
  padding: 10,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const cardValueStyle: CSSProperties = { fontSize: 18, fontWeight: 600 };

const aliasRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
  justifyContent: 'space-between',
};

type SummaryState =
  | { kind: 'loading' }
  | { kind: 'ready'; summary: IamAccountSummary }
  | { kind: 'notSupported' }
  | { kind: 'error' };

type PolicyState =
  | { kind: 'loading' }
  | { kind: 'ready'; policy: IamPasswordPolicy | null }
  | { kind: 'notSupported' }
  | { kind: 'error' };

type AliasState =
  | { kind: 'loading' }
  | { kind: 'ready'; aliases: string[] }
  | { kind: 'notSupported' }
  | { kind: 'error' };

interface PasswordPolicyForm {
  minimumPasswordLength: number;
  requireSymbols: boolean;
  requireNumbers: boolean;
  requireUppercaseCharacters: boolean;
  requireLowercaseCharacters: boolean;
  allowUsersToChangePassword: boolean;
  expirePasswords: boolean;
  maxPasswordAge: number;
  passwordReusePrevention: number;
  hardExpiry: boolean;
}

const defaultPolicyForm: PasswordPolicyForm = {
  minimumPasswordLength: 8,
  requireSymbols: false,
  requireNumbers: false,
  requireUppercaseCharacters: false,
  requireLowercaseCharacters: false,
  allowUsersToChangePassword: true,
  expirePasswords: false,
  maxPasswordAge: 90,
  passwordReusePrevention: 0,
  hardExpiry: false,
};

/**
 * Maps an existing password policy onto the editable form, defaulting numeric fields when the
 * backend reports them as not configured.
 */
function policyToForm(policy: IamPasswordPolicy): PasswordPolicyForm {
  return {
    minimumPasswordLength: policy.minimumPasswordLength,
    requireSymbols: policy.requireSymbols,
    requireNumbers: policy.requireNumbers,
    requireUppercaseCharacters: policy.requireUppercaseCharacters,
    requireLowercaseCharacters: policy.requireLowercaseCharacters,
    allowUsersToChangePassword: policy.allowUsersToChangePassword,
    expirePasswords: policy.expirePasswords,
    maxPasswordAge: policy.maxPasswordAge ?? 90,
    passwordReusePrevention: policy.passwordReusePrevention ?? 0,
    hardExpiry: policy.hardExpiry,
  };
}

interface IamAccountPanelProps {
  serviceKey: string;
}

/**
 * Account-level IAM console section showing entity-count summary cards, a password-policy editor
 * (view/create/update/delete) and an account-aliases manager (list/create/delete). Each area
 * surfaces a clear "not set" or "not supported by LocalStack" state where applicable.
 */
export function IamAccountPanel({ serviceKey }: IamAccountPanelProps) {
  const [summaryState, setSummaryState] = useState<SummaryState>({ kind: 'loading' });
  const [policyState, setPolicyState] = useState<PolicyState>({ kind: 'loading' });
  const [aliasState, setAliasState] = useState<AliasState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  const [form, setForm] = useState<PasswordPolicyForm>(defaultPolicyForm);
  const [showEditor, setShowEditor] = useState(false);
  const [saveError, setSaveError] = useState(false);

  const [newAlias, setNewAlias] = useState('');
  const [aliasError, setAliasError] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    setSummaryState({ kind: 'loading' });
    getIamAccountSummary(controller.signal)
      .then((summary) => setSummaryState({ kind: 'ready', summary }))
      .catch((error) =>
        setSummaryState(
          error instanceof IamNotSupportedError ? { kind: 'notSupported' } : { kind: 'error' },
        ),
      );
    return () => controller.abort();
  }, [reloadToken]);

  useEffect(() => {
    const controller = new AbortController();
    setPolicyState({ kind: 'loading' });
    getIamAccountPasswordPolicy(controller.signal)
      .then((policy) => {
        setPolicyState({ kind: 'ready', policy });
        setForm(policy ? policyToForm(policy) : defaultPolicyForm);
      })
      .catch((error) =>
        setPolicyState(
          error instanceof IamNotSupportedError ? { kind: 'notSupported' } : { kind: 'error' },
        ),
      );
    return () => controller.abort();
  }, [reloadToken]);

  useEffect(() => {
    const controller = new AbortController();
    setAliasState({ kind: 'loading' });
    getIamAccountAliases(controller.signal)
      .then((result) => setAliasState({ kind: 'ready', aliases: result.aliases }))
      .catch((error) =>
        setAliasState(
          error instanceof IamNotSupportedError ? { kind: 'notSupported' } : { kind: 'error' },
        ),
      );
    return () => controller.abort();
  }, [reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleSavePolicy = (event: FormEvent) => {
    event.preventDefault();
    setSaveError(false);
    updateIamAccountPasswordPolicy({
      minimumPasswordLength: form.minimumPasswordLength,
      requireSymbols: form.requireSymbols,
      requireNumbers: form.requireNumbers,
      requireUppercaseCharacters: form.requireUppercaseCharacters,
      requireLowercaseCharacters: form.requireLowercaseCharacters,
      allowUsersToChangePassword: form.allowUsersToChangePassword,
      maxPasswordAge: form.expirePasswords ? form.maxPasswordAge : null,
      passwordReusePrevention:
        form.passwordReusePrevention > 0 ? form.passwordReusePrevention : null,
      hardExpiry: form.hardExpiry,
    })
      .then(() => {
        setShowEditor(false);
        refresh();
      })
      .catch(() => setSaveError(true));
  };

  const handleDeletePolicy = useCallback(() => {
    deleteIamAccountPasswordPolicy()
      .then(() => refresh())
      .catch(() => setPolicyState({ kind: 'error' }));
  }, [refresh]);

  const handleCreateAlias = (event: FormEvent) => {
    event.preventDefault();
    const trimmed = newAlias.trim();
    if (trimmed === '') {
      return;
    }
    setAliasError(false);
    createIamAccountAlias(trimmed)
      .then(() => {
        setNewAlias('');
        refresh();
      })
      .catch(() => setAliasError(true));
  };

  const handleDeleteAlias = useCallback(
    (alias: string) => {
      deleteIamAccountAlias(alias)
        .then(() => refresh())
        .catch(() => setAliasState({ kind: 'error' }));
    },
    [refresh],
  );

  const updateField = <K extends keyof PasswordPolicyForm>(
    key: K,
    value: PasswordPolicyForm[K],
  ) => {
    setForm((current) => ({ ...current, [key]: value }));
  };

  return (
    <div data-testid={`${serviceKey}-account-panel`}>
      <section data-testid="iam-account-summary" style={sectionStyle}>
        <span style={headingStyle}>Account summary</span>
        {summaryState.kind === 'loading' ? (
          <p data-testid="iam-account-summary-loading" style={messageStyle}>
            Loading account summary&hellip;
          </p>
        ) : null}
        {summaryState.kind === 'notSupported' ? (
          <p data-testid="iam-account-summary-unsupported" style={messageStyle}>
            Account summary is not supported by the current backend.
          </p>
        ) : null}
        {summaryState.kind === 'error' ? (
          <p data-testid="iam-account-summary-error" style={messageStyle}>
            Unable to load the account summary.
          </p>
        ) : null}
        {summaryState.kind === 'ready' ? (
          Object.keys(summaryState.summary.entries).length === 0 ? (
            <p data-testid="iam-account-summary-empty" style={messageStyle}>
              No account summary entries reported.
            </p>
          ) : (
            <div data-testid="iam-account-summary-cards" style={cardsStyle}>
              {Object.entries(summaryState.summary.entries).map(([key, value]) => (
                <div key={key} data-testid={`iam-account-summary-card-${key}`} style={cardStyle}>
                  <span style={labelStyle}>{key}</span>
                  <span style={cardValueStyle}>{value}</span>
                </div>
              ))}
            </div>
          )
        ) : null}
      </section>

      <section data-testid="iam-account-password-policy" style={sectionStyle}>
        <span style={headingStyle}>Password policy</span>
        {policyState.kind === 'loading' ? (
          <p data-testid="iam-account-policy-loading" style={messageStyle}>
            Loading password policy&hellip;
          </p>
        ) : null}
        {policyState.kind === 'notSupported' ? (
          <p data-testid="iam-account-policy-unsupported" style={messageStyle}>
            Password policy is not supported by the current backend.
          </p>
        ) : null}
        {policyState.kind === 'error' ? (
          <p data-testid="iam-account-policy-error" style={messageStyle}>
            Unable to load the password policy.
          </p>
        ) : null}
        {policyState.kind === 'ready' && policyState.policy === null && !showEditor ? (
          <p data-testid="iam-account-policy-not-set" style={messageStyle}>
            No password policy is set on this account.
          </p>
        ) : null}
        {policyState.kind === 'ready' && policyState.policy !== null && !showEditor ? (
          <dl data-testid="iam-account-policy-summary" style={{ margin: 0, fontSize: 13 }}>
            <div>Minimum length: {policyState.policy.minimumPasswordLength}</div>
            <div>Require symbols: {policyState.policy.requireSymbols ? 'Yes' : 'No'}</div>
            <div>Require numbers: {policyState.policy.requireNumbers ? 'Yes' : 'No'}</div>
            <div>
              Require uppercase: {policyState.policy.requireUppercaseCharacters ? 'Yes' : 'No'}
            </div>
            <div>
              Require lowercase: {policyState.policy.requireLowercaseCharacters ? 'Yes' : 'No'}
            </div>
            <div>
              Users can change password:{' '}
              {policyState.policy.allowUsersToChangePassword ? 'Yes' : 'No'}
            </div>
            <div>
              Max password age:{' '}
              {policyState.policy.maxPasswordAge === null
                ? 'Never expires'
                : `${policyState.policy.maxPasswordAge} days`}
            </div>
            <div>
              Reuse prevention:{' '}
              {policyState.policy.passwordReusePrevention === null
                ? 'Off'
                : policyState.policy.passwordReusePrevention}
            </div>
            <div>Hard expiry: {policyState.policy.hardExpiry ? 'Yes' : 'No'}</div>
          </dl>
        ) : null}
        {policyState.kind === 'ready' && !showEditor ? (
          <div style={{ display: 'flex', gap: 8 }}>
            <button
              type="button"
              data-testid="iam-account-policy-edit"
              style={buttonStyle}
              onClick={() => setShowEditor(true)}
            >
              {policyState.policy === null ? 'Create policy' : 'Edit policy'}
            </button>
            {policyState.policy !== null ? (
              <ConfirmationHost
                actionLabel="Delete"
                prompt="Delete the account password policy?"
                confirmLabel="Confirm"
                onConfirm={handleDeletePolicy}
              />
            ) : null}
          </div>
        ) : null}
        {policyState.kind === 'ready' && showEditor ? (
          <form data-testid="iam-account-policy-form" style={fieldRowStyle} onSubmit={handleSavePolicy}>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="iam-account-policy-min-length">
                Minimum password length
              </label>
              <input
                id="iam-account-policy-min-length"
                type="number"
                data-testid="iam-account-policy-min-length"
                style={inputStyle}
                value={form.minimumPasswordLength}
                onChange={(event) =>
                  updateField('minimumPasswordLength', Number(event.target.value))
                }
              />
            </div>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="iam-account-policy-require-symbols"
                checked={form.requireSymbols}
                onChange={(event) => updateField('requireSymbols', event.target.checked)}
              />
              Require symbols
            </label>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="iam-account-policy-require-numbers"
                checked={form.requireNumbers}
                onChange={(event) => updateField('requireNumbers', event.target.checked)}
              />
              Require numbers
            </label>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="iam-account-policy-require-uppercase"
                checked={form.requireUppercaseCharacters}
                onChange={(event) =>
                  updateField('requireUppercaseCharacters', event.target.checked)
                }
              />
              Require uppercase characters
            </label>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="iam-account-policy-require-lowercase"
                checked={form.requireLowercaseCharacters}
                onChange={(event) =>
                  updateField('requireLowercaseCharacters', event.target.checked)
                }
              />
              Require lowercase characters
            </label>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="iam-account-policy-allow-change"
                checked={form.allowUsersToChangePassword}
                onChange={(event) =>
                  updateField('allowUsersToChangePassword', event.target.checked)
                }
              />
              Allow users to change their own password
            </label>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="iam-account-policy-expire"
                checked={form.expirePasswords}
                onChange={(event) => updateField('expirePasswords', event.target.checked)}
              />
              Expire passwords
            </label>
            {form.expirePasswords ? (
              <div style={fieldRowStyle}>
                <label style={labelStyle} htmlFor="iam-account-policy-max-age">
                  Maximum password age (days)
                </label>
                <input
                  id="iam-account-policy-max-age"
                  type="number"
                  data-testid="iam-account-policy-max-age"
                  style={inputStyle}
                  value={form.maxPasswordAge}
                  onChange={(event) => updateField('maxPasswordAge', Number(event.target.value))}
                />
              </div>
            ) : null}
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="iam-account-policy-reuse">
                Prevent password reuse (0 = off)
              </label>
              <input
                id="iam-account-policy-reuse"
                type="number"
                data-testid="iam-account-policy-reuse"
                style={inputStyle}
                value={form.passwordReusePrevention}
                onChange={(event) =>
                  updateField('passwordReusePrevention', Number(event.target.value))
                }
              />
            </div>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="iam-account-policy-hard-expiry"
                checked={form.hardExpiry}
                onChange={(event) => updateField('hardExpiry', event.target.checked)}
              />
              Prevent password reset after expiry (hard expiry)
            </label>
            {saveError ? (
              <p data-testid="iam-account-policy-save-error" style={messageStyle}>
                Unable to save the password policy.
              </p>
            ) : null}
            <div style={{ display: 'flex', gap: 8 }}>
              <button type="submit" data-testid="iam-account-policy-save" style={buttonStyle}>
                Save policy
              </button>
              <button
                type="button"
                data-testid="iam-account-policy-cancel"
                style={buttonStyle}
                onClick={() => {
                  setShowEditor(false);
                  setSaveError(false);
                }}
              >
                Cancel
              </button>
            </div>
          </form>
        ) : null}
      </section>

      <section data-testid="iam-account-aliases" style={sectionStyle}>
        <span style={headingStyle}>Account aliases</span>
        {aliasState.kind === 'loading' ? (
          <p data-testid="iam-account-aliases-loading" style={messageStyle}>
            Loading account aliases&hellip;
          </p>
        ) : null}
        {aliasState.kind === 'notSupported' ? (
          <p data-testid="iam-account-aliases-unsupported" style={messageStyle}>
            Account aliases are not supported by the current backend.
          </p>
        ) : null}
        {aliasState.kind === 'error' ? (
          <p data-testid="iam-account-aliases-error" style={messageStyle}>
            Unable to load the account aliases.
          </p>
        ) : null}
        {aliasState.kind === 'ready' ? (
          aliasState.aliases.length === 0 ? (
            <p data-testid="iam-account-aliases-empty" style={messageStyle}>
              No account aliases are configured.
            </p>
          ) : (
            <ul data-testid="iam-account-aliases-list" style={{ margin: 0, paddingLeft: 0, listStyle: 'none' }}>
              {aliasState.aliases.map((alias) => (
                <li key={alias} data-testid={`iam-account-alias-${alias}`} style={aliasRowStyle}>
                  <span>{alias}</span>
                  <ConfirmationHost
                    actionLabel="Delete"
                    prompt={`Delete alias ${alias}?`}
                    confirmLabel="Confirm"
                    onConfirm={() => handleDeleteAlias(alias)}
                  />
                </li>
              ))}
            </ul>
          )
        ) : null}
        {aliasState.kind === 'ready' ? (
          <form data-testid="iam-account-alias-form" style={aliasRowStyle} onSubmit={handleCreateAlias}>
            <input
              type="text"
              data-testid="iam-account-alias-input"
              style={inputStyle}
              placeholder="new-account-alias"
              value={newAlias}
              onChange={(event) => setNewAlias(event.target.value)}
            />
            <button type="submit" data-testid="iam-account-alias-create" style={buttonStyle}>
              Add alias
            </button>
          </form>
        ) : null}
        {aliasError ? (
          <p data-testid="iam-account-alias-error" style={messageStyle}>
            Unable to create the account alias.
          </p>
        ) : null}
      </section>
    </div>
  );
}

export default IamAccountPanel;
