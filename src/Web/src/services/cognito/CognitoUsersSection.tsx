import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import {
  createCognitoUser,
  deleteCognitoUser,
  getCognitoUser,
  getCognitoUsers,
  setCognitoUserEnabled,
  setCognitoUserPassword,
} from '../../api/client';
import type { CognitoUserDetailResult, CognitoUserSummaryItem } from '../../api/client';
import { ConfirmationHost } from '../../components/ConfirmationHost';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const checkboxRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 6,
};

const listStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const rowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const actionsStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
};

const detailRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
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

type UsersState =
  | { kind: 'loading' }
  | { kind: 'ready'; users: CognitoUserSummaryItem[] }
  | { kind: 'error' };

type SaveState = 'idle' | 'saving' | 'error';

export interface CognitoUsersSectionProps {
  poolId: string;
}

function formatAttribute(value: string): string {
  return value === '' ? '—' : value;
}

export function CognitoUsersSection({ poolId }: CognitoUsersSectionProps) {
  const [usersState, setUsersState] = useState<UsersState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  const [showCreate, setShowCreate] = useState(false);
  const [createUsername, setCreateUsername] = useState('');
  const [createEmail, setCreateEmail] = useState('');
  const [createTemporaryPassword, setCreateTemporaryPassword] = useState('');
  const [createState, setCreateState] = useState<SaveState>('idle');

  const [selectedUser, setSelectedUser] = useState<CognitoUserDetailResult | null>(null);
  const [selectedState, setSelectedState] = useState<'idle' | 'loading' | 'error'>('idle');

  const [passwordTarget, setPasswordTarget] = useState<string | null>(null);
  const [passwordValue, setPasswordValue] = useState('');
  const [passwordPermanent, setPasswordPermanent] = useState(true);
  const [passwordState, setPasswordState] = useState<SaveState>('idle');

  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    setUsersState({ kind: 'loading' });
    getCognitoUsers(poolId, controller.signal)
      .then((result) => setUsersState({ kind: 'ready', users: result.users }))
      .catch(() => setUsersState({ kind: 'error' }));
    return () => controller.abort();
  }, [poolId, reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createCognitoUser(poolId, {
      username: createUsername,
      attributes: createEmail === '' ? [] : [{ name: 'email', value: createEmail }],
      temporaryPassword: createTemporaryPassword === '' ? null : createTemporaryPassword,
    })
      .then(() => {
        setCreateState('idle');
        setCreateUsername('');
        setCreateEmail('');
        setCreateTemporaryPassword('');
        setShowCreate(false);
        refresh();
      })
      .catch(() => setCreateState('error'));
  };

  const handleSelect = useCallback(
    (username: string) => {
      setSelectedState('loading');
      getCognitoUser(poolId, username)
        .then((user) => {
          setSelectedUser(user);
          setSelectedState('idle');
        })
        .catch(() => setSelectedState('error'));
    },
    [poolId],
  );

  const handleDelete = useCallback(
    (username: string) => {
      deleteCognitoUser(poolId, username)
        .then(() => {
          setSelectedUser((current) =>
            current && current.username === username ? null : current,
          );
          refresh();
        })
        .catch(() => setActionError(`Unable to delete ${username}.`));
    },
    [poolId, refresh],
  );

  const handleToggleEnabled = (user: CognitoUserSummaryItem) => {
    setActionError(null);
    setCognitoUserEnabled(poolId, user.username, !user.enabled)
      .then(() => refresh())
      .catch(() => setActionError(`Unable to update ${user.username}.`));
  };

  const handleStartPassword = (username: string) => {
    setPasswordTarget(username);
    setPasswordValue('');
    setPasswordPermanent(true);
    setPasswordState('idle');
  };

  const handlePasswordSubmit = (username: string) => {
    setPasswordState('saving');
    setCognitoUserPassword(poolId, username, {
      password: passwordValue,
      permanent: passwordPermanent,
    })
      .then(() => {
        setPasswordState('idle');
        setPasswordTarget(null);
      })
      .catch(() => setPasswordState('error'));
  };

  return (
    <div data-testid="cognito-users-section" style={sectionStyle}>
      <Heading as="h3" style={{ fontSize: 16 }}>
        Users
      </Heading>
      <button
        type="button"
        data-testid="cognito-user-create-toggle"
        style={buttonStyle}
        onClick={() => setShowCreate((current) => !current)}
      >
        {showCreate ? 'Cancel' : 'Create user'}
      </button>
      {showCreate ? (
        <div data-testid="cognito-user-create-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-user-create-username">
              Username
            </label>
            <input
              id="cognito-user-create-username"
              type="text"
              data-testid="cognito-user-create-username"
              style={inputStyle}
              value={createUsername}
              onChange={(event) => setCreateUsername(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-user-create-email">
              Email (optional)
            </label>
            <input
              id="cognito-user-create-email"
              type="text"
              data-testid="cognito-user-create-email"
              style={inputStyle}
              value={createEmail}
              onChange={(event) => setCreateEmail(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-user-create-password">
              Temporary password (optional)
            </label>
            <input
              id="cognito-user-create-password"
              type="text"
              data-testid="cognito-user-create-password"
              style={inputStyle}
              value={createTemporaryPassword}
              onChange={(event) => setCreateTemporaryPassword(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="cognito-user-create-submit"
            style={buttonStyle}
            disabled={createState === 'saving'}
            onClick={handleCreate}
          >
            {createState === 'saving' ? 'Creating\u2026' : 'Create'}
          </button>
          {createState === 'error' ? (
            <p data-testid="cognito-user-create-error" style={messageStyle}>
              Unable to create the user.
            </p>
          ) : null}
        </div>
      ) : null}

      {usersState.kind === 'loading' ? (
        <p data-testid="cognito-users-loading" style={messageStyle}>
          Loading users&hellip;
        </p>
      ) : null}
      {usersState.kind === 'error' ? (
        <p data-testid="cognito-users-error" style={messageStyle}>
          Unable to load users.
        </p>
      ) : null}
      {usersState.kind === 'ready' && usersState.users.length === 0 ? (
        <p data-testid="cognito-users-empty" style={messageStyle}>
          No users found for this user pool.
        </p>
      ) : null}
      {usersState.kind === 'ready' && usersState.users.length > 0 ? (
        <div data-testid="cognito-users-list" style={listStyle}>
          {usersState.users.map((user) => (
            <div key={user.username} data-testid="cognito-user-row" style={rowStyle}>
              <span style={valueStyle}>
                {user.username} ({user.status}
                {user.enabled ? '' : ', disabled'})
              </span>
              <div style={actionsStyle}>
                <button
                  type="button"
                  data-testid="cognito-user-view"
                  style={buttonStyle}
                  onClick={() => handleSelect(user.username)}
                >
                  View
                </button>
                <button
                  type="button"
                  data-testid="cognito-user-toggle-enabled"
                  style={buttonStyle}
                  onClick={() => handleToggleEnabled(user)}
                >
                  {user.enabled ? 'Disable' : 'Enable'}
                </button>
                <button
                  type="button"
                  data-testid="cognito-user-reset-password"
                  style={buttonStyle}
                  onClick={() => handleStartPassword(user.username)}
                >
                  Reset password
                </button>
                <ConfirmationHost
                  actionLabel="Delete"
                  prompt={`Delete ${user.username}?`}
                  confirmLabel="Confirm"
                  onConfirm={() => handleDelete(user.username)}
                />
              </div>
            </div>
          ))}
        </div>
      ) : null}

      {actionError !== null ? (
        <p data-testid="cognito-users-action-error" style={messageStyle}>
          {actionError}
        </p>
      ) : null}

      {passwordTarget !== null ? (
        <div data-testid="cognito-user-password-form" style={formStyle}>
          <span style={labelStyle}>Set password for {passwordTarget}</span>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="cognito-user-password-value">
              New password
            </label>
            <input
              id="cognito-user-password-value"
              type="text"
              data-testid="cognito-user-password-value"
              style={inputStyle}
              value={passwordValue}
              onChange={(event) => setPasswordValue(event.target.value)}
            />
          </div>
          <label style={checkboxRowStyle}>
            <input
              type="checkbox"
              data-testid="cognito-user-password-permanent"
              checked={passwordPermanent}
              onChange={(event) => setPasswordPermanent(event.target.checked)}
            />
            <span style={labelStyle}>Permanent</span>
          </label>
          <button
            type="button"
            data-testid="cognito-user-password-submit"
            style={buttonStyle}
            disabled={passwordState === 'saving'}
            onClick={() => handlePasswordSubmit(passwordTarget)}
          >
            {passwordState === 'saving' ? 'Saving\u2026' : 'Set password'}
          </button>
          {passwordState === 'error' ? (
            <p data-testid="cognito-user-password-error" style={messageStyle}>
              Unable to set the password.
            </p>
          ) : null}
        </div>
      ) : null}

      {selectedState === 'loading' ? (
        <p data-testid="cognito-user-detail-loading" style={messageStyle}>
          Loading user&hellip;
        </p>
      ) : null}
      {selectedState === 'error' ? (
        <p data-testid="cognito-user-detail-error" style={messageStyle}>
          Unable to load the user.
        </p>
      ) : null}
      {selectedState === 'idle' && selectedUser !== null ? (
        <div data-testid="cognito-user-detail" style={formStyle}>
          <div style={detailRowStyle}>
            <span style={labelStyle}>Username</span>
            <span data-testid="cognito-user-detail-username" style={valueStyle}>
              {selectedUser.username}
            </span>
          </div>
          <div style={detailRowStyle}>
            <span style={labelStyle}>Status</span>
            <span data-testid="cognito-user-detail-status" style={valueStyle}>
              {selectedUser.status}
            </span>
          </div>
          <div style={detailRowStyle}>
            <span style={labelStyle}>Enabled</span>
            <span data-testid="cognito-user-detail-enabled" style={valueStyle}>
              {selectedUser.enabled ? 'Yes' : 'No'}
            </span>
          </div>
          <div style={detailRowStyle}>
            <span style={labelStyle}>Attributes</span>
            {selectedUser.attributes.length === 0 ? (
              <span data-testid="cognito-user-detail-attributes-empty" style={valueStyle}>
                —
              </span>
            ) : (
              <div data-testid="cognito-user-detail-attributes" style={listStyle}>
                {selectedUser.attributes.map((attribute) => (
                  <span key={attribute.name} style={valueStyle}>
                    {attribute.name}: {formatAttribute(attribute.value)}
                  </span>
                ))}
              </div>
            )}
          </div>
        </div>
      ) : null}
    </div>
  );
}

export default CognitoUsersSection;
